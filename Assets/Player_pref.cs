using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Player_pref : MonoBehaviour

{
    public TMP_InputField CreateUserInputField;
    public static PlayerPrefsPlus CurrentPlayer;
    public static string CurrentPlayerName;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void CreateUser()
    {
        PlayerPrefsSetup playerprefssetup = new PlayerPrefsSetup();
        playerprefssetup.AddPlayer(CreateUserInputField.text);
        CurrentPlayerName = CreateUserInputField.text;
        playerprefssetup.Close();

        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus(CreateUserInputField.text);
        playerprefsplus.SetDefaults();
        playerprefsplus.Close();

    }

    public static Dictionary<string, object> GetPlayerData(string PlayerName) 
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus(PlayerName);
        Dictionary<string, object> playerprefs = playerprefsplus.Get();
        playerprefsplus.Close();
        return playerprefs;
    }
        

    // Update is called once per frame
    void Update()
    {
        
    }
}
