using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class Load_Current_Player_Data : MonoBehaviour
{
    public LocomotionManager locomotionManager;
    public ActivateTeleportationRay activateTeleportationRay;
    public TMP_Dropdown pauseSettingsMovementDropdown;
    public TMP_Dropdown mainMenuSettingsMovementDropdown;

    public Slider mainMenuVolumeSlider;
    public Slider pauseMenuVolumeSlider;

    private VolumeController volumeController;
    private UserManager user;
    // Start is called before the first frame update
    void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }

        if (SceneManager.GetActiveScene().name == "Office")
        {
            LoadCurrentPlayerData();
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            volumeController = player.GetComponent<VolumeController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadCurrentPlayerData()
    {
        Debug.Log(user.currentUser);
        PlayerPrefsPlus currentPlayerPrefs = new PlayerPrefsPlus(user.currentUser);
        Dictionary<string, object> playerprefs = currentPlayerPrefs.Get();
        bool continuiousMovement = (bool)playerprefs["ContinousMovementDefault"];
        if (continuiousMovement == false)
        {
            locomotionManager.SwitchLocomotion(1);
            activateTeleportationRay.SwitchLocomotion(1);
            pauseSettingsMovementDropdown.value = 1;
        }
        else
        {
            locomotionManager.SwitchLocomotion(0);
            activateTeleportationRay.SwitchLocomotion(0);
            pauseSettingsMovementDropdown.value = 0;
        }

        if (currentPlayerPrefs.HasKey("Volume"))
        {
            float volume = (float)playerprefs["Volume"];
            pauseMenuVolumeSlider.value = volume;
            if (volumeController != null)
            {
                volumeController.SetVolume(volume);
            }

        }

        currentPlayerPrefs.Close();
    }

    public void LoadCurrentPlayerDataForMainMenu() 
    {
        Debug.Log(user.currentUser);
        PlayerPrefsPlus currentPlayerPrefs = new PlayerPrefsPlus(user.currentUser);
        Dictionary<string, object> playerprefs = currentPlayerPrefs.Get();
        bool continuiousMovement = (bool)playerprefs["ContinousMovementDefault"];
        if (continuiousMovement == false)
        {
            locomotionManager.SwitchLocomotion(1);
            activateTeleportationRay.SwitchLocomotion(1);
            mainMenuSettingsMovementDropdown.value = 1;
        }
        else
        {
            locomotionManager.SwitchLocomotion(0);
            activateTeleportationRay.SwitchLocomotion(0);
            mainMenuSettingsMovementDropdown.value = 0;
        }

        //load volume
        if (currentPlayerPrefs.HasKey("Volume"))
        {
            float volume = (float)playerprefs["Volume"];
            mainMenuVolumeSlider.value = volume;
            if (volumeController != null)
            {
                volumeController.SetVolume(volume);
            }

        }
        currentPlayerPrefs.Close();
    }
}
