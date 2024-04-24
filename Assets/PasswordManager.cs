using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PasswordManager : MonoBehaviour

{    
    public TMP_InputField UserPasswordField;

    public GameObject Main_Menu_Canvas;
    public GameObject Password_Canvas;
    
    private UserManager user;
    // Start is called before the first frame update
    void Start()
    {
        //New_Pla.SetActive(false);
        //Main_Menu_Canvas.SetActive(true);
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }

        // Update is called once per frame
    }

    public void CheckPassword()
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
        playerprefsplus.GetPlayerByName(user.currentUser);

        string playerPassword = (string)playerprefsplus.Get("Password");

        if (playerPassword == UserPasswordField.text)
        {
            Password_Canvas.SetActive(false);
            Main_Menu_Canvas.SetActive(true);
            UserPasswordField.text = "";
        }
    }

}
