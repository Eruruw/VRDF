using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Load_User_Data : MonoBehaviour
{
    public TextMeshProUGUI PlayerListText;

    // Start is called before the first frame update
    [SerializeField]
    private static string CurrentPlayer = Player_pref.CurrentPlayerName;

    void Start()
    {
        PlayerListText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void LoadPlayerNames()
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
        playerprefsplus_player[] players;
        int count = playerprefsplus.GetAllPlayers(out players);
        System.Text.StringBuilder playerListBuilder = new System.Text.StringBuilder();

        foreach (playerprefsplus_player item in players)
        {
           
            if (!(item.Title == "Default Values"))
            {
                string name = item.Title;
                Debug.Log(name);
                playerListBuilder.AppendLine(name);
            }
            
        }

        PlayerListText.text = playerListBuilder.ToString();
        playerprefsplus.Close();
    }
    public void ShowPlayerSelection()
    {
        //PlayerListText.text = Player_pref.CurrentPlayerName;
    }
}