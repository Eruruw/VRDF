using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreTracker : MonoBehaviour
{
    public int evidenceScore;
    public int pictureScore;
    public int overallScore;
    public int evidenceTotal;
    public int pictureTotal;
    public int overallTotal;
    public float average;
    public string grade;
    public bool crimeCommitted;
    public int numberOfBagsInCart;
    private float calc;
    private string username;
    private GameObject[] evidence;
    private WarrantManager manager;
    private string warrantDesk;
    private List<string> tagsList;
    public bool warrantGrabbed { get; set; }
    public int numberOfPicturesTaken { get; set; }
    private GameObject cartObject;
    private Transform cartTransform;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Office" || SceneManager.GetActiveScene().name == "Tutorial")
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
            cartObject = GameObject.FindWithTag("CartTrigger");
            cartTransform = cartObject.GetComponent<Transform>();
            foreach (GameObject item in evidence)
            {
                item.gameObject.tag = "Interactable";
                EvidenceID ID = item.GetComponent<EvidenceID>();
                if (ID.desk == warrantDesk)
                {
                    if (tagsList.Contains(ID.type))
                    {
                        evidenceTotal++;
                        pictureTotal++;
                        overallTotal = evidenceTotal + pictureTotal;
                    }
                }
            }
        }
    }

    public void WarrantGrabbbed()
    {
        warrantGrabbed = true;
    }

    public void TallyScores()
    {
        foreach (Transform childTransform in cartTransform)
        {
            GameObject childObject = childTransform.gameObject;
            if (childObject.CompareTag("Interactable"))
            {
                Transform socketTransform = childTransform.Find("BagSocket");
                if (socketTransform != null)
                {
                    GameObject socketObject = socketTransform.gameObject;
                    BagSocket socket = socketObject.GetComponent<BagSocket>();
                    if (socket.ID.desk == warrantDesk)
                    {
                        if (socket.tagsList.Contains(socket.ID.type))
                        {
                            evidenceScore++;
                        }
                    }
                }
            }
        } 
        foreach (GameObject item in evidence)
        {
            EvidenceID ID = item.GetComponent<EvidenceID>();
            if (ID.desk == warrantDesk)
            {
                if (tagsList.Contains(ID.type))
                {
                    if (ID.picturetaken == true)
                    {
                        pictureScore++;
                    }
                }
            }
        }
        overallScore = evidenceScore + pictureScore;
        average = (overallScore / overallTotal) * 100;
        if (warrantGrabbed == false)
            average = average / 2;
        if (crimeCommitted)
            average = 0f;
        if (average >= 90)
            grade = "A";
        else if (average >= 80)
            grade = "B";
        else if (average >= 70)
            grade = "C";
        else if (average >= 60)
            grade = "D";
        else
            grade = "F";
    }

    public void SaveScores()
    {
        if (SceneManager.GetActiveScene().name == "Office")
        {
            PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
            playerprefsplus.GetPlayerByName(username);
            if (playerprefsplus.HasKey("OfficeScore"))
            {
                Dictionary<string, object> playerprefs = playerprefsplus.Get();
                float prev = (float)playerprefs["OfficeScore"];
                float best = Mathf.Max(average, prev);
                playerprefsplus.Set("OfficeScore", best);
            }
            playerprefsplus.Save();
        }
    }
}
