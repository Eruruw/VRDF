using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

[System.Serializable]
public class Player_pref : MonoBehaviour

{
    public TMP_InputField CreateUserInputField;
    public TMP_InputField EmailInputField;
    public TMP_InputField UserPasswordField;
    public TMP_InputField PasswordVerifyField;

    public static PlayerPrefsPlus CurrentPlayer { get; set; }
    public static string CurrentPlayerName { get; set; }
    public GameObject Main_Menu_Canvas;
    public GameObject New_Player_Screen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool IsInputValid(string input)
    {
        // Use a regular expression to check if the input contains only alphabetical characters
        // Adjust the regular expression pattern based on your validation criteria
        return Regex.IsMatch(input, "^[a-zA-Z]+$");
    }

    public void CreateUser()
    {
        if (CreateUserInputField != null && !string.IsNullOrEmpty(CreateUserInputField.text))
        {
            if (UserPasswordField.text == PasswordVerifyField.text)
            {


                // Validate the input using a regular expression
                if (IsInputValid(CreateUserInputField.text))
                {

                    PlayerPrefsSetup playerprefssetup = new PlayerPrefsSetup();
                    playerprefssetup.AddPlayer(CreateUserInputField.text);
                    CurrentPlayerName = CreateUserInputField.text;
                    playerprefssetup.Close();

                    PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus(CreateUserInputField.text);
                    playerprefsplus.SetDefaults();
                    playerprefsplus.Set("Email", EmailInputField.text);
                    playerprefsplus.Set("Password", PasswordVerifyField.text);
                    playerprefsplus.Close();
                    New_Player_Screen.SetActive(false);
                    Main_Menu_Canvas.SetActive(true);


                }
                else
                {
                    Debug.Log("Invalid input! Only alphabetical characters are allowed.");
                }
            }
        }
        else
        {
            Debug.Log("Input field is empty");
        }



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
