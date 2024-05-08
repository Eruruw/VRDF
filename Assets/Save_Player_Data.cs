using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Save_Player_Data : MonoBehaviour
{
    public TMP_Dropdown mainMenuMovementSelectDropdown;
    public TMP_Dropdown pauseMenuMovementSelectDropdown;

    public Slider mainMenuVolumeSlider;
    public Slider pauseMenuVolumeSlider;
    private UserManager user;

    // Start is called before the first frame update
    void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void SaveSettings()
    {
        PlayerPrefsPlus currentPlayerPrefs = new PlayerPrefsPlus(user.currentUser);
        currentPlayerPrefs.AutoSave(false);
        if (mainMenuMovementSelectDropdown != null)
        {
            Debug.Log(user.currentUser + "Saved");
            if (mainMenuMovementSelectDropdown.value == 1)
            {
                currentPlayerPrefs.Set("ContinousMovementDefault", false);
            }
            else 
            {
                currentPlayerPrefs.Set("ContinousMovementDefault", true);
            }
        }

        if (mainMenuVolumeSlider != null)
        {
            currentPlayerPrefs.Set("Volume", mainMenuVolumeSlider.value);
        }
        currentPlayerPrefs.Save();
        currentPlayerPrefs.Close();
    }

    public void SavePauseMenuSettings()
    {
        PlayerPrefsPlus currentPlayerPrefs = new PlayerPrefsPlus(user.currentUser);
        currentPlayerPrefs.AutoSave(false);
        if (pauseMenuMovementSelectDropdown != null)
        {
            Debug.Log(user.currentUser + "Saved");
            if (pauseMenuMovementSelectDropdown.value == 1)
            {
                currentPlayerPrefs.Set("ContinousMovementDefault", false);
            }
            else
            {
                currentPlayerPrefs.Set("ContinousMovementDefault", true);
            }
        }

        if (pauseMenuVolumeSlider != null)
        {
            currentPlayerPrefs.Set("Volume", pauseMenuVolumeSlider.value);
        }
        currentPlayerPrefs.Save();
        currentPlayerPrefs.Close();
    }
}
