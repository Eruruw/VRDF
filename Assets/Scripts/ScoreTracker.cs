using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System;

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
    public TextMeshProUGUI evidenceScoreText;
    public TextMeshProUGUI pictureScoreText;
    public TextMeshProUGUI warrantScoreText;
    public TextMeshProUGUI overallScoreText;
    public TextMeshProUGUI gradeText;
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
    private bool done = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Office" || SceneManager.GetActiveScene().name == "Tutorial")
        {
            Debug.Log("GOT IT");
            warrantGrabbed = false;
            evidence = GameObject.FindGameObjectsWithTag("evidence");
            GameObject userMan = GameObject.Find("UserManager");
            UserManager user = userMan.GetComponent<UserManager>();
            username = user.currentUser;
            GameObject warrant = GameObject.FindWithTag("manager");
            manager = warrant.GetComponentInChildren<WarrantManager>();
            warrantDesk = manager.exactDesk;
            tagsList = manager.validEvidenceList;
            cartObject = GameObject.FindWithTag("CartTrigger");
            cartTransform = cartObject.GetComponent<Transform>();
            Debug.Log(warrantDesk);
            Debug.Log(tagsList);
            
            StartCoroutine(Names());
        }
    }

    IEnumerator Names() {
        yield return new WaitForSeconds(.4f);
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

    public void WarrantGrabbbed()
    {
        warrantGrabbed = true;
    }

    public void TallyScores()
    {
        if (done == false)
        {
            done = true;
            foreach (Transform childTransform in cartTransform)
            {
                GameObject childObject = childTransform.gameObject;
                if (childObject.CompareTag("Interactable"))
                {
                    Transform socketTransform = childTransform.Find("Bag Socket");
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
            if (overallTotal > 0)
                average = (overallScore / overallTotal) * 100;
            else
                average = 0f;
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
    }

    public void UpdateStats()
    {
        evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}";
        pictureScoreText.text = $"Picture Score: {pictureScore} / {pictureTotal}";
        warrantScoreText.text = $"Picked up Warrant?: {warrantGrabbed}";
        overallScoreText.text = $"Overall Score: {overallScore} / {overallTotal}";
        gradeText.text = $"Grade: {grade}";
        //SaveScores();
    }

    public void SaveScores()
    {
        if (SceneManager.GetActiveScene().name == "Office")
        {

            PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
            Debug.Log("Username: " + username);
            playerprefsplus.GetPlayerByName(username);
            if (playerprefsplus.HasKey("NumberOfOfficeRuns"))
            {
                Dictionary<string, object> playerprefs = playerprefsplus.Get();
                int prevNumOfRuns = (int)playerprefs["NumberOfOfficeRuns"]; //get number of runs for office
                int updatedNumOfRuns = prevNumOfRuns + 1;
                playerprefsplus.Set("NumberOfOfficeRuns", updatedNumOfRuns);
                if (prevNumOfRuns == 0)
                {
                    playerprefsplus.Set("OfficeScores", average);
                    playerprefsplus.Set("BestOfficeScore", average);
                    playerprefsplus.Set("AverageOfficeScore", average);
                }
                else if (prevNumOfRuns == 1)
                {
                    float firstOfficeScore = (float)playerprefs["OfficeScores"];
                    float[] twoOfficeScores = { firstOfficeScore, average };
                    playerprefsplus.Set("OfficeScores", twoOfficeScores);
                    playerprefsplus.Set("BestOfficeScore", twoOfficeScores.Max());
                    playerprefsplus.Set("AverageOfficeScore", twoOfficeScores.Average());
                }
                else 
                {
                    object[] tempOfficeScores = (object[])playerprefs["OfficeScores"];      //get array of all Office Scores
                    float[] allOfficeScores = tempOfficeScores.OfType<float>().ToArray();   //convert to float array
                    Array.Resize(ref allOfficeScores, allOfficeScores.Length + 1);

                    allOfficeScores[allOfficeScores.Length - 1] = average;                  //add current score to array
                    foreach (float value in allOfficeScores)
                    {
                        Debug.Log($"Float value: {value}");
                    }
                    playerprefsplus.Set("OfficeScores", allOfficeScores);                   //set the updated array
                    playerprefsplus.Set("BestOfficeScore", allOfficeScores.Max());          //set best score pref
                    playerprefsplus.Set("AverageOfficeScore", allOfficeScores.Average());   //set average score
                }
                Debug.Log("Stats saved");
            }
            playerprefsplus.Save();
            playerprefsplus.Close();
        }
    }
}
