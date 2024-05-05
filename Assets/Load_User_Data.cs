using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Load_User_Data : MonoBehaviour
{
    public TextMeshProUGUI CurrentPlayerText;
    public GameObject buttonPrefab;
    public Transform spawnLocation;
    public GameObject parentObject;
    public GameObject Password_Check;
    public GameObject Create_User_Canvas;
    public LocomotionManager locomotionManager;
    public ActivateTeleportationRay activateTeleportationRay;

    public GameObject setInstructorButton;
    public GameObject sendUserDataButton;

    private List<GameObject> createdButtonObjects = new List<GameObject>();
    private UserManager user;
    private Load_Current_Player_Data loadcurrentplayerData;

    void Start()
    {
        // Call LoadPlayerNames() or any other initialization if needed
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            loadcurrentplayerData = player.GetComponent<Load_Current_Player_Data>();
        }
    }

    void Update()
    {
        // Update logic if needed
    }

    public void LoadPlayerNames()
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
        playerprefsplus_player[] players;
        int count = playerprefsplus.GetAllPlayers(out players);
        ClearButtonObjects();
        foreach (playerprefsplus_player item in players)
        {
            if (!(item.Title == "Default Values"))
            {
                string name = item.Title;
               

                GameObject buttonObject = Instantiate(buttonPrefab, parentObject.transform);
                Button buttonComponent = buttonObject.GetComponent<Button>();
                buttonObject.SetActive(true);
                TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();

                buttonText.text = name;
                buttonComponent.onClick.AddListener(() => OnButtonClick(name));

                createdButtonObjects.Add(buttonObject);
            }
        }

        playerprefsplus.Close();
    }

    public void ClearButtonObjects()
    {
        foreach (GameObject buttonObj in createdButtonObjects)
        {
            Destroy(buttonObj);
        }

        createdButtonObjects.Clear();
    }

    public void OnButtonClick(string playerName)
    {
        // Handle button click for the selected player (you can modify this method as needed)
        Password_Check.SetActive(true);
        Create_User_Canvas.SetActive(false);
        CurrentPlayerText.text = playerName;
        user.currentUser = playerName;

        if (playerName == "Admin")
        {
            //show button
            setInstructorButton.SetActive(true);
            sendUserDataButton.SetActive(true);
        }
        else
        {
            //hide button
            setInstructorButton.SetActive(false);
            sendUserDataButton.SetActive(false);
        }
        
        Player_pref.CurrentPlayerName = playerName;

        LoadCurrentPLayerSettings();

    }

    public void ShowPlayerSelection()
    {
        // Update UI or perform any actions related to player selection
    }

    public void LoadCurrentPlayerText()
    {
        CurrentPlayerText.text = Player_pref.CurrentPlayerName;
    }

    public void LoadCurrentPLayerSettings()
    {
        loadcurrentplayerData.LoadCurrentPlayerDataForMainMenu();

        /*
        Debug.Log(Player_pref.CurrentPlayerName);
        PlayerPrefsPlus currentPlayerPrefs = new PlayerPrefsPlus(Player_pref.CurrentPlayerName);
        Dictionary<string, object> playerprefs = currentPlayerPrefs.Get();
        bool continuiousMovement = (bool)playerprefs["ContinousMovementDefault"];
        if (continuiousMovement == false)
        {
            locomotionManager.SwitchLocomotion(1);
            activateTeleportationRay.SwitchLocomotion(1);
            movementDropdown.value = 1;
        }
        else {
            locomotionManager.SwitchLocomotion(0);
            activateTeleportationRay.SwitchLocomotion(0);
            movementDropdown.value = 0;
        }
        currentPlayerPrefs.Close();
        */
    }

    public void DeleteCurrentUser()
    {
        PlayerPrefsSetup playerprefssetup = new PlayerPrefsSetup();
        playerprefssetup.DeletePlayer(user.currentUser);
        playerprefssetup.Close();
    }
}