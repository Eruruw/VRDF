using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Load_User_Data : MonoBehaviour
{
    public TextMeshProUGUI PlayerListText;
    public TextMeshProUGUI CurrentPlayerText;
    public GameObject textPrefab;
    public Transform spawnLocation;
    public GameObject parentObject;

    private List<GameObject> createdTextObjects = new List<GameObject>();

    // Start is called before the first frame update
    [SerializeField]
    private static string CurrentPlayer = Player_pref.CurrentPlayerName;

    void Start()
    {

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
                GameObject textObject = new GameObject(item.Title);
                TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
                textMesh.text = item.Title;
                textMesh.fontSize = 150;
                textMesh.autoSizeTextContainer = true;
                textObject.transform.SetParent(parentObject.transform, false);
                //playerListBuilder.AppendLine(name);
                createdTextObjects.Add(textObject);
            }

        }

        //PlayerListText.text = playerListBuilder.ToString();
        playerprefsplus.Close();
    }

    public void ClearTextObjects()
    {
        foreach (GameObject textObj in createdTextObjects) 
        {
            Destroy(textObj);
        }
    }
    public void ShowPlayerSelection()
    {
        //PlayerListText.text = Player_pref.CurrentPlayerName;
    }

    public void LoadCurrentPlayerText()
    {
        CurrentPlayerText.text = Player_pref.CurrentPlayerName;

    }

}

