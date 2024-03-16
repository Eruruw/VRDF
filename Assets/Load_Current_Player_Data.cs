using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class Load_Current_Player_Data : MonoBehaviour
{
    public LocomotionManager locomotionManager;
    public ActivateTeleportationRay activateTeleportationRay;
    public TMP_Dropdown pauseSettingsMovementDropdown;
    public TMP_Dropdown mainMenuSettingsMovementDropdown;
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
        currentPlayerPrefs.Close();
    }
}
