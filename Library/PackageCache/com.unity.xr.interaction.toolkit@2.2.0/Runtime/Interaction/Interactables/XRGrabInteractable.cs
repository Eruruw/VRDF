using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactable component that allows for basic grab functionality.
    /// When this behavior is selected (grabbed) by an Interactor, this behavior will follow it around
    /// and inherit velocity when released.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behavior is responsible for applying the position, rotation, and local scale calculated
    /// by one or more <see cref="IXRGrabTransformer"/> implementations. A default set of grab transformers
    /// are automatically added by Unity, but this functionality can be disabled to manually set those
    /// used by this behavior, allowing you to customize where this component should move and rotate to.
    /// </para>
    /// <para>
    /// Grab transformers are classified into two different types: Single and Multiple.
    /// Those added to the Single Grab Transformers list are used when there is a single interactor selecting this object.
    /// Those added to the Multiple Grab Transformers list are used when there are multiple interactors selecting this object.
    /// You can add multiple grab transformers in a category and they will each be processed in sequence.
    /// The Multiple Grab Transformers are given first opportunity to process when there are multiple grabs, and
    /// the Single Grab Transformer processing will be skipped if a Multiple Grab Transformer can process in that case.
    /// </para>
    /// <para>
    /// There are fallback rules that could allow a Single Grab Transformer to be processed when there are multiple grabs,
    /// and for a Multiple Grab Transformer to be processed when there is a single grab (though a grab transformer will never be
    /// processed if its <see cref="IXRGrabTransformer.canProcess"/> returns <see langword="false"/>).
    /// <list type="bullet">
    /// <item>
    /// <description>When there is a single interactor selecting this object, the Multiple Grab Transformer will be processed only
    ///  if the Single Grab Transformer list is empty or if all transformers in the Single Grab Transformer list return false during processing.</description>
    /// </item>
    /// <item>
    /// <description>When there are multiple interactors selecting this object, the Single Grab Transformer will be processed only
    /// if the Multiple Grab Transformer list is empty or if all transformers in the Multiple Grab Transformer list return false during processing.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IXRGrabTransformer"/>
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("XR/XR Grab Interactable", 11)]
    [HelpURL(XRHelpURLConstants.k_XRGrabInteractable)]
    public partial class XRGrabInteractable : XRBaseInteractable
    {
        const float k_DefaultTighteningAmount = 0.5f;
        const float k_DefaultSmoothingAmount = 5f;
        const float k_VelocityDamping = 1f;
        const float k_VelocityScale = 1f;
        const float k_AngularVelocityDamping = 1f;
        const float k_AngularVelocityScale = 1f;
        const int k_ThrowSmoothingFrameCount = 20;
        const float k_DefaultAttachEaseInTime = 0.15f;
        const float k_DefaultThrowSmoothingDuration = 0.25f;
        const float k_DefaultThrowVelocityScale = 1.5f;
        const float k_DefaultThrowAngularVelocityScale = 1f;
        const float k_DeltaTimeThreshold = 0.001f;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// </summary>
        /// <seealso cref="attachPointCompatibilityMode"/>
        public enum AttachPointCompatibilityMode
        {
            /// <summary>
            /// Use the default, correct method for calculating the target position of the object.
            /// </summary>
            Default,

            /// <summary>
            /// Use an additional offset from the center of mass when calculating the target position of the object.
            /// Also incorporate the scale of the Interactor's Attach Transform.
            /// Marked for deprecation.
            /// This is the backwards compatible support mode for projects that accounted for the
            /// unintended difference when using XR Interaction Toolkit prior to version <c>1.0.0-pre.4</c>.
            /// To have the effective attach position be the same between all <see cref="XRBaseInteractable.MovementType"/> values, use <see cref="Default"/>.
            /// </summary>
            Legacy,
        }

        [SerializeField]
        Transform m_AttachTransform;

        /// <summary>
        /// The attachment point Unity uses on this Interactable (will use this object's position if none set).
        /// </summary>
        public Transform attachTransform
        {
            get => m_AttachTransform;
            set => m_AttachTransform = value;
        }

        [SerializeField]
        bool m_UseDynamicAttach;

        /// <summary>
        /// The grab pose will be based on the pose of the Interactor when the selection is made.
        /// Unity will create a dynamic attachment point for each Interactor that selects this component.
        /// </summary>
        /// <remarks>
        /// A child GameObject will be created for each Interactor that selects this component to serve as the attachment point.
        /// These are cached and part of a shared pool used by all instances of <see cref="XRGrabInteractable"/>.
        /// Therefore, while a reference can be obtained by calling <see cref="GetAttachTransform"/> while selected,
        /// you should typically not add any components to that GameObject unless you remove them after being released
        /// since it won't always be used by the same Interactable.
        /// </remarks>
        /// <seealso cref="attachTransform"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        public bool useDynamicAttach
        {
            get => m_UseDynamicAttach;
            set => m_UseDynamicAttach = value;
        }

        [SerializeField]
        bool m_MatchAttachPosition = true;

        /// <summary>
        /// Match the position of the Interactor's attachment point when initializing the grab.
        /// This will override the position of <see cref="attachTransform"/>.
        /// </summary>
        /// <remarks>
        /// This will initialize the dynamic attachment point of this object using the position of the Interactor's attachment point.
        /// This value can be overridden for a specific interactor by overriding <see cref="ShouldMatchAttachPosition"/>.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="matchAttachRotation"/>
        public bool matchAttachPosition
        {
            get => m_MatchAttachPosition;
            set => m_MatchAttachPosition = value;
        }

        [SerializeField]
        bool m_MatchAttachRotation = true;

        /// <summary>
        /// Match the rotation of the Interactor's attachment point when initializing the grab.
        /// This will override the rotation of <see cref="attachTransform"/>.
        /// </summary>
        /// <remarks>
        /// This will initialize the dynamic attachment point of this object using the rotation of the Interactor's attachment point.
        /// This value can be overridden for a specific interactor by overriding <see cref="ShouldMatchAttachRotation"/>.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="matchAttachPosition"/>
        public bool matchAttachRotation
        {
            get => m_MatchAttachRotation;
            set => m_MatchAttachRotation = value;
        }

        [SerializeField]
        bool m_SnapToColliderVolume = true;

        /// <summary>
        /// Adjust the dynamic attachment point to keep it on or inside the Colliders that make up this object.
        /// </summary>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="ShouldSnapToColliderVolume"/>
        /// <seealso cref="Collider.ClosestPoint"/>
        public bool snapToColliderVolume
        {
            get => m_SnapToColliderVolume;
            set => m_SnapToColliderVolume = value;
        }

        [SerializeField]
        float m_AttachEaseInTime = k_DefaultAttachEaseInTime;

        /// <summary>
        /// Time in seconds Unity eases in the attach when selected (a value of 0 indicates no easing).
        /// </summary>
        public float attachEaseInTime
        {
            get => m_AttachEaseInTime;
            set => m_AttachEaseInTime = value;
        }

        [SerializeField]
        MovementType m_MovementType = MovementType.Instantaneous;

        /// <summary>
        /// Specifies how this object moves when selected, either through setting the velocity of the <see cref="Rigidbody"/>,
        /// moving the kinematic <see cref="Rigidbody"/> during Fixed Update, or by directly updating the <see cref="Transform"/> each frame.
        /// </summary>
        /// <seealso cref="XRBaseInteractable.MovementType"/>
        public MovementType movementType
        {
            get => m_MovementType;
            set
            {
                m_MovementType = value;

                if (isSelected)
                {
                    SetupRigidbodyDrop(m_Rigidbody);
                    UpdateCurrentMovementType();
                    SetupRigidbodyGrab(m_Rigidbody);
                }
            }
        }

        [SerializeField, Range(0f, 1f)]
        float m_VelocityDamping = k_VelocityDamping;

        /// <summary>
        /// Scale factor of how much to dampen the existing velocity when tracking the position of the Interactor.
        /// The smaller the value, the longer it takes for the velocity to decay.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackPosition"/>
        public float velocityDamping
        {
            get => m_VelocityDamping;
            set => m_VelocityDamping = value;
        }

        [SerializeField]
        float m_VelocityScale = k_VelocityScale;

        /// <summary>
        /// Scale factor Unity applies to the tracked velocity while updating the <see cref="Rigidbody"/>
        /// when tracking the position of the Interactor.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackPosition"/>
        public float velocityScale
        {
            get => m_VelocityScale;
            set => m_VelocityScale = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_AngularVelocityDamping = k_AngularVelocityDamping;

        /// <summary>
        /// Scale factor of how much Unity dampens the existing angular velocity when tracking the rotation of the Interactor.
        /// The smaller the value, the longer it takes for the angular velocity to decay.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackRotation"/>
        public float angularVelocityDamping
        {
            get => m_AngularVelocityDamping;
            set => m_AngularVelocityDamping = value;
        }

        [SerializeField]
        float m_AngularVelocityScale = k_AngularVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to the tracked angular velocity while updating the <see cref="Rigidbody"/>
        /// when tracking the rotation of the Interactor.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackRotation"/>
        public float angularVelocityScale
        {
            get => m_AngularVelocityScale;
            set => m_AngularVelocityScale = value;
        }

        [SerializeField]
        bool m_TrackPosition = true;

        /// <summary>
        /// Whether this object should follow the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackRotation"/>
        public bool trackPosition
        {
            get => m_TrackPosition;
            set => m_TrackPosition = value;
        }

        [SerializeField]
        bool m_SmoothPosition;

        /// <summary>
        /// Whether Unity applies smoothing while following the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothPositionAmount"/>
        /// <seealso cref="tightenPosition"/>
        public bool smoothPosition
        {
            get => m_SmoothPosition;
            set => m_SmoothPosition = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothPositionAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the position of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the position of the Interactor.
        /// </summary>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="tightenPosition"/>
        public float smoothPositionAmount
        {
            get => m_SmoothPositionAmount;
            set => m_SmoothPositionAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenPosition = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow position difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed position should remain to the position of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow distance,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="smoothPositionAmount"/>
        public float tightenPosition
        {
            get => m_TightenPosition;
            set => m_TightenPosition = value;
        }

        [SerializeField]
        bool m_TrackRotation = true;

        /// <summary>
        /// Whether this object should follow the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackPosition"/>
        public bool trackRotation
        {
            get => m_TrackRotation;
            set => m_TrackRotation = value;
        }

        [SerializeField]
        bool m_SmoothRotation;

        /// <summary>
        /// Apply smoothing while following the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothRotationAmount"/>
        /// <seealso cref="tightenRotation"/>
        public bool smoothRotation
        {
            get => m_SmoothRotation;
            set => m_SmoothRotation = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothRotationAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the rotation of the Interactor.
        /// </summary>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="tightenRotation"/>
        public float smoothRotationAmount
        {
            get => m_SmoothRotationAmount;
            set => m_SmoothRotationAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenRotation = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow rotation difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed rotation should remain to the rotation of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow rotation,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="smoothRotationAmount"/>
        public float tightenRotation
        {
            get => m_TightenRotation;
            set => m_TightenRotation = value;
        }

        [SerializeField]
        bool m_ThrowOnDetach = true;

        /// <summary>
        /// Whether this object inherits the velocity of the Interactor when released.
        /// </summary>
        public bool throwOnDetach
        {
            get => m_ThrowOnDetach;
            set => m_ThrowOnDetach = value;
        }

        [SerializeField]
        float m_ThrowSmoothingDuration = k_DefaultThrowSmoothingDuration;

        /// <summary>
        /// Time period to average thrown velocity over.
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public float throwSmoothingDuration
        {
            get => m_ThrowSmoothingDuration;
            set => m_ThrowSmoothingDuration = value;
        }

        [SerializeField]
        AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

        /// <summary>
        /// The curve to use to weight thrown velocity smoothing (most recent frames to the right).
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public AnimationCurve throwSmoothingCurve
        {
            get => m_ThrowSmoothingCurve;
            set => m_ThrowSmoothingCurve = value;
        }

        [SerializeField]
        float m_ThrowVelocityScale = k_DefaultThrowVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to this object's velocity inherited from the Interactor when released.
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public float throwVelocityScale
        {
            get => m_ThrowVelocityScale;
            set => m_ThrowVelocityScale = value;
        }

        [SerializeField]
        float m_ThrowAngularVelocityScale = k_DefaultThrowAngularVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to this object's angular velocity inherited from the Interactor when released.
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public float throwAngularVelocityScale
        {
            get => m_ThrowAngularVelocityScale;
            set => m_ThrowAngularVelocityScale = value;
        }

        [SerializeField, FormerlySerializedAs("m_GravityOnDetach")]
        bool m_ForceGravityOnDetach;

        /// <summary>
        /// Forces this object to have gravity when released
        /// (will still use pre-grab value if this is <see langword="false"/>).
        /// </summary>
        public bool forceGravityOnDetach
        {
            get => m_ForceGravityOnDetach;
            set => m_ForceGravityOnDetach = value;
        }

        [SerializeField]
        bool m_RetainTransformParent = true;

        /// <summary>
        /// Whether Unity sets the parent of this object back to its original parent this object was a child of after this object is dropped.
        /// </summary>
        public bool retainTransformParent
        {
            get => m_RetainTransformParent;
            set => m_RetainTransformParent = value;
        }

        [SerializeField]
        AttachPointCompatibilityMode m_AttachPointCompatibilityMode = AttachPointCompatibilityMode.Default;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// Use <see cref="AttachPointCompatibilityMode.Default"/> for consistent attach points
        /// between all <see cref="XRBaseInteractable.MovementType"/> values.
        /// Marked for deprecation, this property will be removed in a future version.
        /// </summary>
        /// <remarks>
        /// This is a backwards compatibility option in order to keep the old, incorrect method
        /// of calculating the attach point. Projects that already accounted for the difference
        /// can use the Legacy option to maintain the same attach positioning from older versions
        /// without needing to modify the Attach Transform position.
        /// </remarks>
        /// <seealso cref="AttachPointCompatibilityMode"/>
        public AttachPointCompatibilityMode attachPointCompatibilityMode
        {
            get => m_AttachPointCompatibilityMode;
            set => m_AttachPointCompatibilityMode = value;
        }

        [SerializeField]
        List<XRBaseGrabTransformer> m_StartingSingleGrabTransformers = new List<XRBaseGrabTransformer>();

        /// <summary>
        /// The grab transformers that this Interactable automatically links at startup (optional, may be empty).
        /// These are used when there is a single interactor selecting this object.
        /// </summary>
        /// <remarks>
        /// To modify the grab transformers used after startup,
        /// the <see cref="AddSingleGrabTransformer"/> or <see cref="RemoveSingleGrabTransformer"/> methods should be used instead.
        /// </remarks>
        /// <seealso cref="startingMultipleGrabTransformers"/>
        public List<XRBaseGrabTransformer> startingSingleGrabTransformers
        {
            get => m_StartingSingleGrabTransformers;
            set => m_StartingSingleGrabTransformers = value;
        }

        [SerializeField]
        List<XRBaseGrabTransformer> m_StartingMultipleGrabTransformers = new List<XRBaseGrabTransformer>();

        /// <summary>
        /// The grab transformers that this Interactable automatically links at startup (optional, may be empty).
        /// These are used when there are multiple interactors selecting this object.
        /// </summary>
        /// <remarks>
        /// To modify the grab transformers used after startup,
        /// the <see cref="AddMultipleGrabTransformer"/> or <see cref="RemoveMultipleGrabTransformer"/> methods should be used instead.
        /// </remarks>
        /// <seealso cref="startingSingleGrabTransformers"/>
        public List<XRBaseGrabTransformer> startingMultipleGrabTransformers
        {
            get => m_StartingMultipleGrabTransformers;
            set => m_StartingMultipleGrabTransformers = value;
        }

        [SerializeField]
        bool m_AddDefaultGrabTransformers = true;

        /// <summary>
        /// Whether Unity will add the default set of grab transformers if either the Single or Multiple Grab Transformers lists are empty.
        /// </summary>
        /// <remarks>
        /// Set this to <see langword="false"/> if you want to manually set the grab transformers used by populating
        /// <see cref="startingSingleGrabTransformers"/> and <see cref="startingMultipleGrabTransformers"/>.
        /// </remarks>
        /// <seealso cref="AddDefaultSingleGrabTransformer"/>
        /// <seealso cref="AddDefaultMultipleGrabTransformer"/>
        public bool addDefaultGrabTransformers
        {
            get => m_AddDefaultGrabTransformers;
            set => m_AddDefaultGrabTransformers = value;
        }

        /// <summary>
        /// The number of single grab transformers.
        /// These are the grab transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="AddSingleGrabTransformer"/>
        public int singleGrabTransformersCount => m_SingleGrabTransformers.flushedCount;

        /// <summary>
        /// The number of multiple grab transformers.
        /// These are the grab transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="AddMultipleGrabTransformer"/>
        public int multipleGrabTransformersCount => m_MultipleGrabTransformers.flushedCount;

        readonly SmallRegistrationList<IXRGrabTransformer> m_SingleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();
        readonly SmallRegistrationList<IXRGrabTransformer> m_MultipleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();

        List<IXRGrabTransformer> m_GrabTransformersAddedWhenGrabbed;
        bool m_GrabCountChanged;
        bool m_IsProcessingGrabTransformers;

        // World pose we are moving towards each frame (eventually will be at Interactor's attach point assuming default single grab algorithm)
        Pose m_TargetPose;
        Vector3 m_TargetLocalScale;

        float m_CurrentAttachEaseTime;
        MovementType m_CurrentMovementType;

        bool m_DetachInLateUpdate;
        Vector3 m_DetachVelocity;
        Vector3 m_DetachAngularVelocity;

        int m_ThrowSmoothingCurrentFrame;
        readonly float[] m_ThrowSmoothingFrameTimes = new float[k_ThrowSmoothingFrameCount];
        readonly Vector3[] m_ThrowSmoothingVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
        readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
        bool m_ThrowSmoothingFirstUpdate;
        Pose m_LastThrowReferencePose;

        Rigidbody m_Rigidbody;

        // Rigidbody's settings upon select, kept to restore these values when dropped
        bool m_WasKinematic;
        bool m_UsedGravity;
        float m_OldDrag;
        float m_OldAngularDrag;

        Transform m_OriginalSceneParent;

        // Account for teleportation to avoid throws with unintentionally high energy
        TeleportationMonitor m_TeleportationMonitor;

        readonly Dictionary<IXRSelectInteractor, Transform> m_DynamicAttachTransforms = new Dictionary<IXRSelectInteractor, Transform>();

        static readonly LinkedPool<Transform> s_DynamicAttachTransformPool = new LinkedPool<Transform>(OnCreatePooledItem, OnGetPooledItem, OnReleasePooledItem, OnDestroyPooledItem);

        static readonly ProfilerMarker s_ProcessGrabTransformersMarker = new ProfilerMarker("XRI.ProcessGrabTransformers");

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            m_TeleportationMonitor = new TeleportationMonitor();
            m_TeleportationMonitor.teleported += OnTeleported;

            m_CurrentMovementType = m_MovementType;
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
                Debug.LogError("XR Grab Interactable does not have a required Rigidbody.", this);

            if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Legacy)
            {
#pragma warning disable 618 // Adding deprecated component to help with backwards compatibility with existing user projects.
                var legacyGrabTransformer = GetOrAddComponent<XRLegacyGrabTransformer>();
#pragma warning restore 618
                legacyGrabTransformer.enabled = true;
                return;
            }

            // Load the starting grab transformers into the Play mode lists.
            // It is more efficient to add than move, but if there are existing items
            // use move to ensure the correct order dictated by the starting lists.
            if (m_SingleGrabTransformers.flushedCount > 0)
            {
                var index = 0;
                foreach (var transformer in m_StartingSingleGrabTransformers)
                {
                    if (transformer != null)
                        MoveSingleGrabTransformerTo(transformer, index++);
                }
            }
            else
            {
                foreach (var transformer in m_StartingSingleGrabTransformers)
                {
                    if (transformer != null)
                        AddSingleGrabTransformer(transformer);
                }
            }

            if (m_MultipleGrabTransformers.flushedCount > 0)
            {
                var index = 0;
                foreach (var transformer in m_StartingMultipleGrabTransformers)
                {
                    if (transformer != null)
                        MoveMultipleGrabTransformerTo(transformer, index++);
                }
            }
            else
            {
                foreach (var transformer in m_StartingMultipleGrabTransformers)
                {
                    if (transformer != null)
                        AddMultipleGrabTransformer(transformer);
                }
            }

            FlushRegistration();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            // Unlink this interactable from the grab transformers
            ClearSingleGrabTransformers();
            ClearMultipleGrabTransformers();
            base.OnDestroy();
        }

        /// <inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            // This is done here instead of Start since adding a Start method would be a breaking change.
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                AddDefaultGrabTransformers();

            FlushRegistration();

            switch (updatePhase)
            {
                // During Fixed update we want to apply any Rigidbody-based updates (e.g., Kinematic or VelocityTracking).
                case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                    if (isSelected)
                    {
                        if (m_CurrentMovementType == MovementType.Kinematic)
                            PerformKinematicUpdate(updatePhase);
                        else if (m_CurrentMovementType == MovementType.VelocityTracking)
                            PerformVelocityTrackingUpdate(updatePhase, Time.deltaTime);
                    }

                    break;

                // During Dynamic update and OnBeforeRender we want to update the target pose and apply any Transform-based updates (e.g., Instantaneous).
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    if (isSelected)
                    {
                        UpdateTarget(updatePhase, Time.deltaTime);

                        if (m_CurrentMovementType == MovementType.Instantaneous)
                            PerformInstantaneousUpdate(updatePhase);
                    }

                    break;

                // Late update is only used to handle detach as late as possible.
                case XRInteractionUpdateOrder.UpdatePhase.Late:
                    if (m_DetachInLateUpdate)
                    {
                        if (!isSelected)
                            Detach();
                        m_DetachInLateUpdate = false;
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            if (m_UseDynamicAttach && interactor is IXRSelectInteractor selectInteractor &&
                m_DynamicAttachTransforms.TryGetValue(selectInteractor, out var dynamicAttachTransform))
            {
                if (dynamicAttachTransform != null)
                    return dynamicAttachTransform;

                m_DynamicAttachTransforms.Remove(selectInteractor);
                Debug.LogWarning($"Dynamic Attach Transform created by {this} for {interactor} was destroyed after being created." +
                    " Continuing as if Use Dynamic Attach was disabled for this pair.", this);
            }

            return m_AttachTransform != null ? m_AttachTransform : base.GetAttachTransform(interactor);
        }

        /// <summary>
        /// Adds the given grab transformer to the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to add.</param>
        /// <seealso cref="AddMultipleGrabTransformer"/>
        public void AddSingleGrabTransformer(IXRGrabTransformer transformer) => AddGrabTransformer(transformer, m_SingleGrabTransformers);

        /// <summary>
        /// Adds the given grab transformer to the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to add.</param>
        /// <seealso cref="AddSingleGrabTransformer"/>
        public void AddMultipleGrabTransformer(IXRGrabTransformer transformer) => AddGrabTransformer(transformer, m_MultipleGrabTransformers);

        /// <summary>
        /// Removes the given grab transformer from the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="transformer"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if <paramref name="transformer"/> was not found in the list.
        /// </returns>
        /// <seealso cref="RemoveMultipleGrabTransformer"/>
        public bool RemoveSingleGrabTransformer(IXRGrabTransformer transformer) => RemoveGrabTransformer(transformer, m_SingleGrabTransformers);

        /// <summary>
        /// Removes the given grab transformer from the list of transformers used when there is are multiple interactors selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="transformer"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if <paramref name="transformer"/> was not found in the list.
        /// </returns>
        /// <seealso cref="RemoveSingleGrabTransformer"/>
        public bool RemoveMultipleGrabTransformer(IXRGrabTransformer transformer) => RemoveGrabTransformer(transformer, m_MultipleGrabTransformers);

        /// <summary>
        /// Removes all grab transformers from the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="ClearMultipleGrabTransformers"/>
        public void ClearSingleGrabTransformers() => ClearGrabTransformers(m_SingleGrabTransformers);

        /// <summary>
        /// Removes all grab transformers from the list of transformers used when there is are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="ClearSingleGrabTransformers"/>
        public void ClearMultipleGrabTransformers() => ClearGrabTransformers(m_MultipleGrabTransformers);

        /// <summary>
        /// Returns all transformers used when there is a single interactor selecting this object into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive grab transformers.</param>
        /// <remarks>
        /// This method populates the list with the grab transformers at the time the
        /// method is called. It is not a live view, meaning grab transformers
        /// added or removed afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetMultipleGrabTransformers"/>
        public void GetSingleGrabTransformers(List<IXRGrabTransformer> results) => GetGrabTransformers(m_SingleGrabTransformers, results);

        /// <summary>
        /// Returns all transformers used when there are multiple interactors selecting this object into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive grab transformers.</param>
        /// <remarks>
        /// This method populates the list with the grab transformers at the time the
        /// method is called. It is not a live view, meaning grab transformers
        /// added or removed afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetSingleGrabTransformers"/>
        public void GetMultipleGrabTransformers(List<IXRGrabTransformer> results) => GetGrabTransformers(m_MultipleGrabTransformers, results);

        /// <summary>
        /// Returns the grab transformer at <paramref name="index"/> in the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="index">Index of the grab transformer to return. Must be smaller than <see cref="singleGrabTransformersCount"/> and not negative.</param>
        /// <returns>Returns the grab transformer at the given index.</returns>
        /// <seealso cref="GetMultipleGrabTransformerAt"/>
        public IXRGrabTransformer GetSingleGrabTransformerAt(int index) => m_SingleGrabTransformers.GetRegisteredItemAt(index);

        /// <summary>
        /// Returns the grab transformer at <paramref name="index"/> in the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <param name="index">Index of the grab transformer to return. Must be smaller than <see cref="multipleGrabTransformersCount"/> and not negative.</param>
        /// <returns>Returns the grab transformer at the given index.</returns>
        /// <seealso cref="GetSingleGrabTransformerAt"/>
        public IXRGrabTransformer GetMultipleGrabTransformerAt(int index) => m_MultipleGrabTransformers.GetRegisteredItemAt(index);

        /// <summary>
        /// Moves the given grab transformer in the list of transformers used when there is a single interactor selecting this object.
        /// If the grab transformer is not in the list, this can be used to insert the grab transformer at the specified index.
        /// </summary>
        /// <param name="transformer">The grab transformer to move or add.</param>
        /// <param name="newIndex">New index of the grab transformer.</param>
        /// <seealso cref="MoveMultipleGrabTransformerTo"/>
        public void MoveSingleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex) => MoveGrabTransformerTo(transformer, newIndex, m_SingleGrabTransformers);

        /// <summary>
        /// Moves the given grab transformer in the list of transformers used when there are multiple interactors selecting this object.
        /// If the grab transformer is not in the list, this can be used to insert the grab transformer at the specified index.
        /// </summary>
        /// <param name="transformer">The grab transformer to move or add.</param>
        /// <param name="newIndex">New index of the grab transformer.</param>
        /// <seealso cref="MoveSingleGrabTransformerTo"/>
        public void MoveMultipleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex) => MoveGrabTransformerTo(transformer, newIndex, m_MultipleGrabTransformers);

        void AddGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            if (m_IsProcessingGrabTransformers)
                Debug.LogWarning($"{transformer} added while {name} is processing grab transformers. It won't be processed until the next process.", this);

            if (grabTransformers.Register(transformer))
                OnAddedGrabTransformer(transformer);
        }

        bool RemoveGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (grabTransformers.Unregister(transformer))
            {
                OnRemovedGrabTransformer(transformer);
                return true;
            }

            return false;
        }

        void ClearGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            for (var index = grabTransformers.flushedCount - 1; index >= 0; --index)
            {
                var transformer = grabTransformers.GetRegisteredItemAt(index);
                RemoveGrabTransformer(transformer, grabTransformers);
            }
        }

        static void GetGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers, List<IXRGrabTransformer> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            grabTransformers.GetRegisteredItems(results);
        }

        void MoveGrabTransformerTo(IXRGrabTransformer transformer, int newIndex, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            // BaseRegistrationList<T> does not yet support reordering with pending registration changes.
            if (m_IsProcessingGrabTransformers)
            {
                Debug.LogError($"Cannot move {transformer} while {name} is processing grab transformers.", this);
                return;
            }

            grabTransformers.Flush();
            if (grabTransformers.MoveItemImmediately(transformer, newIndex))
                OnAddedGrabTransformer(transformer);
        }

        void FlushRegistration()
        {
            m_SingleGrabTransformers.Flush();
            m_MultipleGrabTransformers.Flush();
        }

        void InvokeGrabTransformersOnGrab()
        {
            m_IsProcessingGrabTransformers = true;

            if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                {
                    if (m_SingleGrabTransformers.IsStillRegistered(transformer))
                        transformer.OnGrab(this);
                }
            }

            if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                {
                    if (m_MultipleGrabTransformers.IsStillRegistered(transformer))
                        transformer.OnGrab(this);
                }
            }

            m_IsProcessingGrabTransformers = false;
        }

        void InvokeGrabTransformersProcess(XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            m_IsProcessingGrabTransformers = true;

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
            using (s_ProcessGrabTransformersMarker.Auto())
            {
                // Let the transformers setup if the grab count changed.
                if (m_GrabCountChanged)
                {
                    if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
                    {
                        foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                        {
                            if (m_SingleGrabTransformers.IsStillRegistered(transformer))
                                transformer.OnGrabCountChanged(this, targetPose, localScale);
                        }
                    }

                    if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
                    {
                        foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                        {
                            if (m_MultipleGrabTransformers.IsStillRegistered(transformer))
                                transformer.OnGrabCountChanged(this, targetPose, localScale);
                        }
                    }

                    m_GrabCountChanged = false;
                    m_GrabTransformersAddedWhenGrabbed?.Clear();
                }
                else if (m_GrabTransformersAddedWhenGrabbed?.Count > 0)
                {
                    // Calling OnGrabCountChanged on just the grab transformers added when this was already grabbed
                    // avoids calling it needlessly on all linked grab transformers.
                    foreach (var transformer in m_GrabTransformersAddedWhenGrabbed)
                    {
                        transformer.OnGrabCountChanged(this, targetPose, localScale);
                    }

                    m_GrabTransformersAddedWhenGrabbed.Clear();
                }

                // Give the Multiple Grab Transformers first chance to process,
                // and if one actually does, skip the Single Grab Transformers.
                // Also let the Multiple Grab Transformers process if there aren't any Single Grab Transformers.
                // An empty Single Grab Transformers list is treated the same as a populated list where none can process.
                var processed = false;
                if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0 && (interactorsSelecting.Count > 1 || !CanProcessAnySingleGrabTransformer()))
                {
                    foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                    {
                        if (!m_MultipleGrabTransformers.IsStillRegistered(transformer))
                            continue;

                        if (transformer.canProcess)
                        {
                            transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                            processed = true;
                        }
                    }
                }

                if (!processed && m_SingleGrabTransformers.registeredSnapshot.Count > 0)
                {
                    foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                    {
                        if (!m_SingleGrabTransformers.IsStillRegistered(transformer))
                            continue;

                        if (transformer.canProcess)
                            transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                    }
                }
            }

            m_IsProcessingGrabTransformers = false;
        }

        /// <summary>
        /// Same check as Linq code for: <c>Any(t => t.canProcess)</c>.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the source list is not empty and at least
        /// one element passes the test; otherwise, <see langword="false"/>.</returns>
        bool CanProcessAnySingleGrabTransformer()
        {
            if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                {
                    if (!m_SingleGrabTransformers.IsStillRegistered(transformer))
                        continue;

                    if (transformer.canProcess)
                        return true;
                }
            }

            return false;
        }

        void OnAddedGrabTransformer(IXRGrabTransformer transformer)
        {
            transformer.OnLink(this);

            if (interactorsSelecting.Count == 0)
                return;

            // OnGrab is invoked immediately, but OnGrabCountChanged is only invoked right before Process so
            // it must be added to a list to maintain those that still need to have it invoked. It functions
            // like a setup method and users should be able to rely on it always being called at least once
            // when grabbed.
            transformer.OnGrab(this);

            if (m_GrabTransformersAddedWhenGrabbed == null)
                m_GrabTransformersAddedWhenGrabbed = new List<IXRGrabTransformer>();

            m_GrabTransformersAddedWhenGrabbed.Add(transformer);
        }

        void OnRemovedGrabTransformer(IXRGrabTransformer transformer)
        {
            transformer.OnUnlink(this);
            m_GrabTransformersAddedWhenGrabbed?.Remove(transformer);
        }

        void AddDefaultGrabTransformers()
        {
            if (!m_AddDefaultGrabTransformers)
                return;

            if (m_SingleGrabTransformers.flushedCount == 0)
                AddDefaultSingleGrabTransformer();

            // Avoid adding the multiple grab transformer component unnecessarily since it may never be needed.
            if (m_MultipleGrabTransformers.flushedCount == 0 && selectMode == InteractableSelectMode.Multiple && interactorsSelecting.Count > 1)
                AddDefaultMultipleGrabTransformer();
        }

        /// <summary>
        /// Adds the default grab transformer (if the Single Grab Transformers list is empty)
        /// to the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="addDefaultGrabTransformers"/>
        protected virtual void AddDefaultSingleGrabTransformer()
        {
            if (m_SingleGrabTransformers.flushedCount == 0)
            {
                var transformer = GetOrAddComponent<XRSingleGrabFreeTransformer>();
                AddSingleGrabTransformer(transformer);
            }
        }

        /// <summary>
        /// Adds the default grab transformer (if the Multiple Grab Transformers list is empty)
        /// to the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="addDefaultGrabTransformers"/>
        protected virtual void AddDefaultMultipleGrabTransformer()
        {
            if (m_MultipleGrabTransformers.flushedCount == 0)
            {
                var transformer = GetOrAddComponent<XRDualGrabFreeTransformer>();
                AddMultipleGrabTransformer(transformer);
            }
        }

        T GetOrAddComponent<T>() where T : Component
        {
            return TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();
        }

        void UpdateTarget(XRInteractionUpdateOrder.UpdatePhase updatePhase, float deltaTime)
        {
            var rawTargetPose = m_TargetPose;
            InvokeGrabTransformersProcess(updatePhase, ref rawTargetPose, ref m_TargetLocalScale);

            // Skip during OnBeforeRender since it doesn't require that high accuracy.
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Track the target pose before easing.
                // This avoids an unintentionally high detach velocity if grabbing with an XRRayInteractor
                // with Force Grab enabled causing the target pose to move very quickly between this transform's
                // initial position and the target pose after easing when the easing time is short.
                // By always tracking the target pose result from the grab transformers, it avoids the issue.
                StepThrowSmoothing(rawTargetPose, deltaTime);
            }

            // Apply easing and smoothing (if configured)
            StepSmoothing(rawTargetPose, deltaTime);
        }

        void StepSmoothing(Pose rawTargetPose, float deltaTime)
        {
            if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
            {
                var easePercent = m_CurrentAttachEaseTime / m_AttachEaseInTime;
                m_TargetPose.position = Vector3.Lerp(m_TargetPose.position, rawTargetPose.position, easePercent);
                m_TargetPose.rotation = Quaternion.Slerp(m_TargetPose.rotation, rawTargetPose.rotation, easePercent);
                m_CurrentAttachEaseTime += deltaTime;
            }
            else
            {
                if (m_SmoothPosition)
                {
                    m_TargetPose.position = Vector3.Lerp(m_TargetPose.position, rawTargetPose.position, m_SmoothPositionAmount * deltaTime);
                    m_TargetPose.position = Vector3.Lerp(m_TargetPose.position, rawTargetPose.position, m_TightenPosition);
                }
                else
                {
                    m_TargetPose.position = rawTargetPose.position;
                }

                if (m_SmoothRotation)
                {
                    m_TargetPose.rotation = Quaternion.Slerp(m_TargetPose.rotation, rawTargetPose.rotation, m_SmoothRotationAmount * deltaTime);
                    m_TargetPose.rotation = Quaternion.Slerp(m_TargetPose.rotation, rawTargetPose.rotation, m_TightenRotation);
                }
                else
                {
                    m_TargetPose.rotation = rawTargetPose.rotation;
                }
            }
        }

        void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                if (m_TrackPosition && m_TrackRotation)
                    transform.SetPositionAndRotation(m_TargetPose.position, m_TargetPose.rotation);
                else if (m_TrackPosition)
                    transform.position = m_TargetPose.position;
                else if (m_TrackRotation)
                    transform.rotation = m_TargetPose.rotation;

                transform.localScale = m_TargetLocalScale;
            }
        }

        void PerformKinematicUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                if (m_TrackPosition)
                {
                    var position = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default
                        ? m_TargetPose.position
                        : m_TargetPose.position - m_Rigidbody.worldCenterOfMass + m_Rigidbody.position;
                    m_Rigidbody.MovePosition(position);
                }

                if (m_TrackRotation)
                {
                    m_Rigidbody.MoveRotation(m_TargetPose.rotation);
                }

                transform.localScale = m_TargetLocalScale;
            }
        }

        void PerformVelocityTrackingUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase, float deltaTime)
        {
            // Skip velocity calculations if Time.deltaTime is too low due to a frame-timing issue on Quest
            if (deltaTime < k_DeltaTimeThreshold)
                return;

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                // Do velocity tracking
                if (m_TrackPosition)
                {
                    // Scale initialized velocity by prediction factor
                    m_Rigidbody.velocity *= (1f - m_VelocityDamping);
                    var positionDelta = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default
                        ? m_TargetPose.position - transform.position
                        : m_TargetPose.position - m_Rigidbody.worldCenterOfMass;
                    var velocity = positionDelta / deltaTime;
                    m_Rigidbody.velocity += (velocity * m_VelocityScale);
                }

                // Do angular velocity tracking
                if (m_TrackRotation)
                {
                    // Scale initialized velocity by prediction factor
                    m_Rigidbody.angularVelocity *= (1f - m_AngularVelocityDamping);
                    var rotationDelta = m_TargetPose.rotation * Quaternion.Inverse(transform.rotation);
                    rotationDelta.ToAngleAxis(out var angleInDegrees, out var rotationAxis);
                    if (angleInDegrees > 180f)
                        angleInDegrees -= 360f;

                    if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon)
                    {
                        var angularVelocity = (rotationAxis * (angleInDegrees * Mathf.Deg2Rad)) / deltaTime;
                        m_Rigidbody.angularVelocity += (angularVelocity * m_AngularVelocityScale);
                    }
                }

                transform.localScale = m_TargetLocalScale;
            }
        }

        void UpdateCurrentMovementType()
        {
            // Special case where the interactor will override this objects movement type (used for Sockets and other absolute interactors).
            // Iterates in reverse order so the most recent interactor with an override will win since that seems like it would
            // be the strategy most users would want by default.
            MovementType? movementTypeOverride = null;
            for (var index = interactorsSelecting.Count - 1; index >= 0; --index)
            {
                var baseInteractor = interactorsSelecting[index] as XRBaseInteractor;
                if (baseInteractor != null && baseInteractor.selectedInteractableMovementTypeOverride.HasValue)
                {
                    if (movementTypeOverride.HasValue)
                    {
                        Debug.LogWarning($"Multiple interactors selecting \"{name}\" have different movement type override values set" +
                            $" ({nameof(XRBaseInteractor.selectedInteractableMovementTypeOverride)})." +
                            $" Conflict resolved using {movementTypeOverride.Value} from the most recent interactor to select this object with an override.", this);
                        break;
                    }

                    movementTypeOverride = baseInteractor.selectedInteractableMovementTypeOverride.Value;
                }
            }

            m_CurrentMovementType = movementTypeOverride ?? m_MovementType;
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            // Setup the dynamic attach transform.
            // Done before calling the base method so the attach pose captured is the dynamic one.
            if (m_UseDynamicAttach)
            {
                var dynamicAttachTransform = CreateDynamicAttach(args.interactorObject);
                InitializeDynamicAttachPose(args.interactorObject, dynamicAttachTransform);
            }

            base.OnSelectEntering(args);

            m_GrabCountChanged = true;
            m_CurrentAttachEaseTime = 0f;

            // Reset the throw data every time the number of grabs increases since
            // each additional grab could cause a large change in target position,
            // making it throw at an unwanted velocity. It is not called when the number
            // of grabs decreases even though it would have the same issue, but doing so
            // would make it almost impossible to throw with both hands.
            ResetThrowSmoothing();

            if (interactorsSelecting.Count == 1)
            {
                Grab();
                InvokeGrabTransformersOnGrab();
            }

            // Add the default grab transformers if needed.
            // This will notify of the grab, so it is done after the above code so the transformer is not notified twice.
            AddDefaultGrabTransformers();

            SubscribeTeleportationProvider(args.interactorObject);
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);

            m_GrabCountChanged = true;
            m_CurrentAttachEaseTime = 0f;

            if (interactorsSelecting.Count == 0)
                Drop();

            UnsubscribeTeleportationProvider(args.interactorObject);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            // Skip checking m_UseDynamicAttach since it may have changed after being grabbed,
            // and we should ensure it is released. We instead check Count first as a faster way to avoid hashing
            // and the Dictionary lookup, which should handle when it was never enabled in the first place.
            if (m_DynamicAttachTransforms.Count > 0 && m_DynamicAttachTransforms.TryGetValue(args.interactorObject, out var dynamicAttachTransform))
            {
                if (dynamicAttachTransform != null)
                    s_DynamicAttachTransformPool.Release(dynamicAttachTransform);

                m_DynamicAttachTransforms.Remove(args.interactorObject);
            }
        }

        Transform CreateDynamicAttach(IXRSelectInteractor interactor)
        {
            Transform dynamicAttachTransform;

            do
            {
                dynamicAttachTransform = s_DynamicAttachTransformPool.Get();
            } while (dynamicAttachTransform == null);

            m_DynamicAttachTransforms.Remove(interactor);
            var staticAttachTransform = GetAttachTransform(interactor);
            m_DynamicAttachTransforms[interactor] = dynamicAttachTransform;

#if UNITY_EDITOR
            dynamicAttachTransform.name = $"[{interactor.transform.name}] Dynamic Attach";
#endif
            dynamicAttachTransform.SetParent(transform, false);

            // Base the initial pose on the Attach Transform.
            // Technically we could just do the final else statement, but setting the local position and rotation this way
            // keeps the position and rotation seen in the Inspector tidier by exactly matching instead of potentially having small
            // floating point offsets.
            if (staticAttachTransform == transform)
            {
                dynamicAttachTransform.localPosition = Vector3.zero;
                dynamicAttachTransform.localRotation = Quaternion.identity;
            }
            else if (staticAttachTransform.parent == transform)
            {
                dynamicAttachTransform.localPosition = staticAttachTransform.localPosition;
                dynamicAttachTransform.localRotation = staticAttachTransform.localRotation;
            }
            else
            {
                dynamicAttachTransform.SetPositionAndRotation(staticAttachTransform.position, staticAttachTransform.rotation);
            }

            return dynamicAttachTransform;
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="matchAttachPosition"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to match the position of the interactor's attachment point when initializing the grab.</returns>
        /// <seealso cref="matchAttachPosition"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldMatchAttachPosition(IXRSelectInteractor interactor)
        {
            if (!m_MatchAttachPosition)
                return false;

            // We assume the static pose should always be used for sockets.
            // For Ray Interactors that bring the object to hand (Force Grab enabled), we assume that property
            // takes precedence since otherwise this interactable wouldn't move if we copied the interactor's attach position,
            // which would violate the interactor's expected behavior.
            if (interactor is XRSocketInteractor ||
                interactor is XRRayInteractor rayInteractor && rayInteractor.useForceGrab)
                return false;

            return true;
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="matchAttachRotation"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to match the rotation of the interactor's attachment point when initializing the grab.</returns>
        /// <seealso cref="matchAttachRotation"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldMatchAttachRotation(IXRSelectInteractor interactor)
        {
            // We assume the static pose should always be used for sockets.
            // Unlike for position, we allow a Ray Interactor with Force Grab enabled to match the rotation
            // based on the property in this behavior.
            return m_MatchAttachRotation && !(interactor is XRSocketInteractor);
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="snapToColliderVolume"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to adjust the dynamic attachment point to keep it on or inside the Colliders that make up this object.</returns>
        /// <seealso cref="snapToColliderVolume"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldSnapToColliderVolume(IXRSelectInteractor interactor)
        {
            return m_SnapToColliderVolume;
        }

        /// <summary>
        /// Unity calls this method automatically when the interactor first initiates selection of this interactable.
        /// Override this method to set the pose of the dynamic attachment point. Before this method is called, the transform
        /// is already set as a child GameObject with inherited Transform values.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <param name="dynamicAttachTransform">The dynamic attachment Transform that serves as the attachment point for the given interactor.</param>
        /// <remarks>
        /// This method is only called when <see cref="useDynamicAttach"/> is enabled.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        protected virtual void InitializeDynamicAttachPose(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
        {
            var matchPosition = ShouldMatchAttachPosition(interactor);
            var matchRotation = ShouldMatchAttachRotation(interactor);
            if (!matchPosition && !matchRotation)
                return;

            // Copy the pose of the interactor's attach transform
            var interactorAttachTransform = interactor.GetAttachTransform(this);
            var position = interactorAttachTransform.position;
            var rotation = interactorAttachTransform.rotation;

            // Optionally constrain the position to within the Collider(s) of this Interactable
            if (matchPosition && ShouldSnapToColliderVolume(interactor) &&
                XRInteractableUtility.TryGetClosestPointOnCollider(this, position, out var distanceInfo))
            {
                position = distanceInfo.point;
            }

            if (matchPosition && matchRotation)
                dynamicAttachTransform.SetPositionAndRotation(position, rotation);
            else if (matchPosition)
                dynamicAttachTransform.position = position;
            else
                dynamicAttachTransform.rotation = rotation;
        }

        /// <summary>
        /// Updates the state of the object due to being grabbed.
        /// Automatically called when entering the Select state.
        /// </summary>
        /// <seealso cref="Drop"/>
        protected virtual void Grab()
        {
            var thisTransform = transform;
            m_OriginalSceneParent = thisTransform.parent;
            thisTransform.SetParent(null);

            UpdateCurrentMovementType();
            SetupRigidbodyGrab(m_Rigidbody);

            // Reset detach velocities
            m_DetachVelocity = Vector3.zero;
            m_DetachAngularVelocity = Vector3.zero;

            // Initialize target pose
            m_TargetPose.position = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default ? thisTransform.position : m_Rigidbody.worldCenterOfMass;
            m_TargetPose.rotation = thisTransform.rotation;
            m_TargetLocalScale = thisTransform.localScale;
        }

        /// <summary>
        /// Updates the state of the object due to being dropped and schedule to finish the detach during the end of the frame.
        /// Automatically called when exiting the Select state.
        /// </summary>
        /// <seealso cref="Detach"/>
        /// <seealso cref="Grab"/>
        protected virtual void Drop()
        {
            if (m_RetainTransformParent && m_OriginalSceneParent != null && !m_OriginalSceneParent.gameObject.activeInHierarchy)
            {
#if UNITY_EDITOR
                // Suppress the warning when exiting Play mode to avoid confusing the user
                var exitingPlayMode = UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                var exitingPlayMode = false;
#endif
                if (!exitingPlayMode)
                    Debug.LogWarning("Retain Transform Parent is set to true, and has a non-null Original Scene Parent. " +
                        "However, the old parent is deactivated so we are choosing not to re-parent upon dropping.", this);
            }
            else if (m_RetainTransformParent && gameObject.activeInHierarchy)
                transform.SetParent(m_OriginalSceneParent);

            SetupRigidbodyDrop(m_Rigidbody);

            m_CurrentMovementType = m_MovementType;
            m_DetachInLateUpdate = true;
            EndThrowSmoothing();
        }

        /// <summary>
        /// Updates the state of the object to finish the detach after being dropped.
        /// Automatically called during the end of the frame after being dropped.
        /// </summary>
        /// <remarks>
        /// This method updates the velocity of the Rigidbody if configured to do so.
        /// </remarks>
        /// <seealso cref="Drop"/>
        protected virtual void Detach()
        {
            if (m_ThrowOnDetach)
            {
                if (m_Rigidbody.isKinematic)
                {
                    Debug.LogWarning("Cannot throw a kinematic Rigidbody since updating the velocity and angular velocity of a kinematic Rigidbody is not supported. Disable Throw On Detach or Is Kinematic to fix this issue.", this);
                    return;
                }

                m_Rigidbody.velocity = m_DetachVelocity;
                m_Rigidbody.angularVelocity = m_DetachAngularVelocity;
            }
        }

        /// <summary>
        /// Setup the <see cref="Rigidbody"/> on this object due to being grabbed.
        /// Automatically called when entering the Select state.
        /// </summary>
        /// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
        /// <seealso cref="SetupRigidbodyDrop"/>
        // ReSharper disable once ParameterHidesMember
        protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
        {
            // Remember Rigidbody settings and setup to move
            m_WasKinematic = rigidbody.isKinematic;
            m_UsedGravity = rigidbody.useGravity;
            m_OldDrag = rigidbody.drag;
            m_OldAngularDrag = rigidbody.angularDrag;
            rigidbody.isKinematic = m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous;
            rigidbody.useGravity = false;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
        }

        /// <summary>
        /// Setup the <see cref="Rigidbody"/> on this object due to being dropped.
        /// Automatically called when exiting the Select state.
        /// </summary>
        /// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
        /// <seealso cref="SetupRigidbodyGrab"/>
        // ReSharper disable once ParameterHidesMember
        protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
        {
            // Restore Rigidbody settings
            rigidbody.isKinematic = m_WasKinematic;
            rigidbody.useGravity = m_UsedGravity;
            rigidbody.drag = m_OldDrag;
            rigidbody.angularDrag = m_OldAngularDrag;

            if (!isSelected)
                m_Rigidbody.useGravity |= m_ForceGravityOnDetach;
        }

        void ResetThrowSmoothing()
        {
            Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
            Array.Clear(m_ThrowSmoothingVelocityFrames, 0, m_ThrowSmoothingVelocityFrames.Length);
            Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
            m_ThrowSmoothingCurrentFrame = 0;
            m_ThrowSmoothingFirstUpdate = true;
        }

        void EndThrowSmoothing()
        {
            if (m_ThrowOnDetach)
            {
                // This can be potentially improved for multi-hand throws by ignoring the frames
                // after the first interactor releases if the second interactor also releases within
                // a short period of time. Since the target pose is tracked before easing, the most
                // recent frames might have been a large change.
                var smoothedVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingVelocityFrames);
                var smoothedAngularVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
                m_DetachVelocity = smoothedVelocity * m_ThrowVelocityScale;
                m_DetachAngularVelocity = smoothedAngularVelocity * m_ThrowAngularVelocityScale;
            }
        }

        void StepThrowSmoothing(Pose targetPose, float deltaTime)
        {
            // Skip velocity calculations if Time.deltaTime is too low due to a frame-timing issue on Quest
            if (deltaTime < k_DeltaTimeThreshold)
                return;

            if (m_ThrowSmoothingFirstUpdate)
            {
                m_ThrowSmoothingFirstUpdate = false;
            }
            else
            {
                m_ThrowSmoothingVelocityFrames[m_ThrowSmoothingCurrentFrame] = (targetPose.position - m_LastThrowReferencePose.position) / deltaTime;

                var rotationDiff = targetPose.rotation * Quaternion.Inverse(m_LastThrowReferencePose.rotation);
                var eulerAngles = rotationDiff.eulerAngles;
                var deltaAngles = new Vector3(Mathf.DeltaAngle(0f, eulerAngles.x),
                    Mathf.DeltaAngle(0f, eulerAngles.y),
                    Mathf.DeltaAngle(0f, eulerAngles.z));
                m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = (deltaAngles / deltaTime) * Mathf.Deg2Rad;
            }

            m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
            m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % k_ThrowSmoothingFrameCount;

            m_LastThrowReferencePose = targetPose;
        }

        Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
        {
            var calcVelocity = Vector3.zero;
            var totalWeights = 0f;
            for (var frameCounter = 0; frameCounter < k_ThrowSmoothingFrameCount; ++frameCounter)
            {
                var frameIdx = (((m_ThrowSmoothingCurrentFrame - frameCounter - 1) % k_ThrowSmoothingFrameCount) + k_ThrowSmoothingFrameCount) % k_ThrowSmoothingFrameCount;
                if (m_ThrowSmoothingFrameTimes[frameIdx] == 0f)
                    break;

                var timeAlpha = (Time.time - m_ThrowSmoothingFrameTimes[frameIdx]) / m_ThrowSmoothingDuration;
                var velocityWeight = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - timeAlpha, 0f, 1f));
                calcVelocity += velocityFrames[frameIdx] * velocityWeight;
                totalWeights += velocityWeight;
                if (Time.time - m_ThrowSmoothingFrameTimes[frameIdx] > m_ThrowSmoothingDuration)
                    break;
            }

            if (totalWeights > 0f)
                return calcVelocity / totalWeights;

            return Vector3.zero;
        }

        void SubscribeTeleportationProvider(IXRInteractor interactor)
        {
            m_TeleportationMonitor.AddInteractor(interactor);
        }

        void UnsubscribeTeleportationProvider(IXRInteractor interactor)
        {
            m_TeleportationMonitor.RemoveInteractor(interactor);
        }

        void OnTeleported(Pose offset)
        {
            var translated = offset.position;
            var rotated = offset.rotation;

            for (var frameIdx = 0; frameIdx < k_ThrowSmoothingFrameCount; ++frameIdx)
            {
                if (m_ThrowSmoothingFrameTimes[frameIdx] == 0f)
                    break;

                m_ThrowSmoothingVelocityFrames[frameIdx] = rotated * m_ThrowSmoothingVelocityFrames[frameIdx];
            }

            m_LastThrowReferencePose.position += translated;
            m_LastThrowReferencePose.rotation = rotated * m_LastThrowReferencePose.rotation;
        }

        static Transform OnCreatePooledItem()
        {
            var item = new GameObject().transform;
            item.localPosition = Vector3.zero;
            item.localRotation = Quaternion.identity;
            item.localScale = Vector3.one;

            return item;
        }

        static void OnGetPooledItem(Transform item)
        {
            if (item == null)
                return;

            item.hideFlags &= ~HideFlags.HideInHierarchy;
        }

        static void OnReleasePooledItem(Transform item)
        {
            if (item == null)
                return;

            // Don't clear the parent of the GameObject on release since there could be issues
            // with changing it while a parent GameObject is deactivating, which logs an error.
            // By keeping it under this interactable, it could mean that GameObjects in the pool
            // have a chance of being destroyed, but we check that the GameObject we obtain from the pool
            // has not been destroyed. This means potentially more creations of new GameObjects, but avoids
            // the issue with reparenting.

            // Hide the GameObject in the Hierarchy so it doesn't pollute this Interactable's hierarchy
            // when it is no longer used.
            item.hideFlags |= HideFlags.HideInHierarchy;
        }

        static void OnDestroyPooledItem(Transform item)
        {
            if (item == null)
                return;

            Destroy(item.gameObject);
        }
    }
}
