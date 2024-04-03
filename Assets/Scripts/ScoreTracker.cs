using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreTracker : MonoBehaviour
{
    public int score;
    public int numberOfBagsInCart;
    private int total;
    private float calc;
    private string username;
    private GameObject[] evidence;
    private WarrantManager manager;
    private string warrantDesk;
    private List<string> tagsList;
    public bool warrantGrabbed { get; set; }
    public int numberOfPicturesTaken { get; set; }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Office")
        {
            warrantGrabbed = false;
            evidence = GameObject.FindGameObjectsWithTag("evidence");
            GameObject userMan = GameObject.Find("UserManager");
            UserManager user = userMan.GetComponent<UserManager>();
            username = user.currentUser;
            GameObject warrant = GameObject.FindWithTag("manager");
            manager = warrant.GetComponent<WarrantManager>();
            warrantDesk = manager.exactDesk;
            tagsList = manager.validEvidenceList;
            foreach (GameObject item in evidence)
            {
                EvidenceID ID = item.GetComponent<EvidenceID>();
                if (ID.desk == warrantDesk)
                {
                    if (tagsList.Contains(ID.type))
                        total++;
                }
            }
        }
    }
    void Update()
    {
    }

    public void WarrantGrabbbed()
    {
        warrantGrabbed = true;
    }

    void OnDisable()
    {

        if (SceneManager.GetActiveScene().name == "Office")
        {
            if (total != 0)
                calc = score / total;
            else
                calc = 0;

            PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
            playerprefsplus.GetPlayerByName(username);
            if (playerprefsplus.HasKey("OfficeScore"))
            {
                Dictionary<string, object> playerprefs = playerprefsplus.Get();
                float prev = (float)playerprefs["OfficeScore"];
                float best = Mathf.Max(calc, prev);
                playerprefsplus.Set("OfficeScore", best);
            }
            playerprefsplus.Save();
        }
    }
}
