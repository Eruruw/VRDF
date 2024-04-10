using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPictureController : MonoBehaviour
{
    private UserManager user;
    private ScoreTracker scoreTracker;
    private float initialYPosition = 0.0008468628f; // Initial Y position of the first picture
    private float yOffset = 70.0f; // Offset for Y position of each new picture
    private float maxYPosition = -6.94001f; // Maximum Y position
    private float newXPosition = 9.50f; // New X position when Y position exceeds the maximum
    private float initialXPosition; // Initial X position

    public Camera cameraToCapture;
    public RenderTexture renderTexture;
    public GameObject pictureDisplayPrefab;
    public Transform pictureDisplayParent;
    private bool pictureInProgress;
    private List<Collider> collidersInRange = new List<Collider>(); // List to store colliders in range

    private void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            scoreTracker = player.GetComponent<ScoreTracker>();
        }

        // Set initial X position
        initialXPosition = 8.392334e-05f; // Set the initial X position to 1.8
    }

    public void TakePicture()
    {
        pictureInProgress = true;
        gameObject.GetComponent<CapsuleCollider>().enabled = true;

        StartCoroutine(CaptureAndSave());
    }

    IEnumerator CaptureAndSave()
    {
        yield return new WaitForSeconds(.4f);
        string pictureNumber = scoreTracker.numberOfPicturesTaken.ToString();
        yield return new WaitForEndOfFrame();

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cameraToCapture.targetTexture ?? renderTexture;

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = currentRT;
        string directoryPath = "/Captures" + "/" + user.currentUser;
        string fulldirectoryPath = Application.temporaryCachePath + directoryPath;
        System.IO.Directory.CreateDirectory(fulldirectoryPath);

        string filePath = fulldirectoryPath + "/Capture" + pictureNumber + ".png";
        System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());

        Debug.Log("Captured Image Saved to: " + filePath);
        scoreTracker.numberOfPicturesTaken += 1;

        // Display the captured picture with adjusted Y position
        DisplayPicture(filePath);
        pictureInProgress = false;

        // Scan all colliders in range
        
        foreach (Collider collider in collidersInRange)
        {
            EvidenceID evidenceID = collider.gameObject.GetComponent<EvidenceID>();
            if (evidenceID != null)
            {
                evidenceID.picturetaken = true;
                Debug.Log("Evidence scanned: " + collider.gameObject.name);
            }
        }
        

        // Clear the list of colliders in range
        collidersInRange.Clear();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;

    }

    void DisplayPicture(string filePath)
    {
        GameObject pictureDisplay = Instantiate(pictureDisplayPrefab, pictureDisplayParent);

        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        pictureDisplay.GetComponent<Image>().sprite = sprite;

        RectTransform rt = pictureDisplay.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Calculate the Y position based on the number of pictures taken
        float newYPosition = initialYPosition - (scoreTracker.numberOfPicturesTaken - 1) * yOffset;

        // Set the picture to the calculated position
        rt.localPosition = new Vector3(initialXPosition, newYPosition, 75.61248f);

        // Force the scale to be 0.5630413 for all dimensions (X, Y, Z)
        rt.localScale = new Vector3(0.5784238f, 0.5784238f, 0.5784238f);
    }

    public void OnTriggerStay(Collider other)
    {
        Debug.Log("Colliding with " + other.gameObject);

        if (other.gameObject.tag == "Interactable")
        {
            // Add colliders in range to the list
            if (!collidersInRange.Contains(other))
                collidersInRange.Add(other);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        // Remove colliders from the list when they exit the trigger area
        if (other.gameObject.tag == "Interactable" && collidersInRange.Contains(other))
            collidersInRange.Remove(other);

        // Check if the collider is evidence and set PictureTaken if applicable
        EvidenceID evidenceID = other.gameObject.GetComponent<EvidenceID>();
        if (evidenceID != null)
        {
            evidenceID.picturetaken = true;
            Debug.Log("Evidence scanned");
        }

        gameObject.GetComponent<CapsuleCollider>().enabled = false;
    }
}