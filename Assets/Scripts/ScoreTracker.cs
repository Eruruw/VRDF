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
    public int digitalScore;
    public int digitalTotal;
    public int documentaryScore;
    public int documentaryTotal;
    public int personalScore;
    public int personalTotal;
    public int pictureTotal;
    public int overallTotal;
    public int incorrectTotal = 0;
    public float average;
    public string grade = "F";
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
    private UserManager user;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Office" || SceneManager.GetActiveScene().name == "Tutorial")
        {
            warrantGrabbed = false;
            evidence = GameObject.FindGameObjectsWithTag("evidence");
            GameObject userMan = GameObject.Find("UserManager");
            user = userMan.GetComponent<UserManager>();
            username = user.currentUser;
            GameObject warrant = GameObject.FindWithTag("manager");
            manager = warrant.GetComponentInChildren<WarrantManager>();
            warrantDesk = manager.exactDesk;
            tagsList = manager.validEvidenceList;
            cartObject = GameObject.FindWithTag("CartTrigger");
            cartTransform = cartObject.GetComponent<Transform>();
            
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
                    if (ID.type == "Digital")
                        digitalTotal++;
                    if (ID.type == "Documentary")
                        documentaryTotal++;
                    if (ID.type == "Personal")
                        personalTotal++;
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
                        if (socket.ID != null)
                        {
                            if (socket.ID.desk == warrantDesk)
                            {
                                if (socket.tagsList.Contains(socket.ID.type))
                                {
                                    evidenceScore++;
                                    if (socket.ID.type == "Digital")
                                        digitalScore++;
                                    if (socket.ID.type == "Documentary")
                                        documentaryScore++;
                                    if (socket.ID.type == "Personal")
                                        personalScore++;
                                }
                                else
                                    incorrectTotal++;
                            }
                            else
                                incorrectTotal++;
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
            overallScore = overallScore - incorrectTotal;
            if (overallTotal > 0)
                average = ((float)overallScore / (float)overallTotal) * 100f;
            else
                average = 0f;
            if (warrantGrabbed == false)
                average = average / 2;
            if (crimeCommitted)
                average = 0f;
            if (average >= 90f)
                grade = "A";
            if (average >= 75f && average < 90f)
                grade = "B";
            if (average >= 60f && average < 75f)
                grade = "C";
            if (average >= 50f && average < 60f)
                grade = "D";
        }
    }

    public void UpdateStats()
    {
        if (digitalTotal != 0 && documentaryTotal != 0 && personalTotal != 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDigital: {digitalScore} / {digitalTotal}\nDocumentary: {documentaryScore} / {documentaryTotal}\nPersonal: {personalScore} / {personalTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal != 0 && documentaryTotal != 0 && personalTotal == 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDigital: {digitalScore} / {digitalTotal}\nDocumentary: {documentaryScore} / {documentaryTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal != 0 && documentaryTotal == 0 && personalTotal == 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDigital: {digitalScore} / {digitalTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal != 0 && documentaryTotal == 0 && personalTotal != 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDigital: {digitalScore} / {digitalTotal}\nPersonal: {personalScore} / {personalTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal == 0 && documentaryTotal != 0 && personalTotal != 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDocumentary: {documentaryScore} / {documentaryTotal}\nPersonal: {personalScore} / {personalTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal == 0 && documentaryTotal != 0 && personalTotal == 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nDocumentary: {documentaryScore} / {documentaryTotal}\nNon-Evidence: {incorrectTotal}";
        if (digitalTotal == 0 && documentaryTotal == 0 && personalTotal != 0)
            evidenceScoreText.text = $"Evidence Score: {evidenceScore} / {evidenceTotal}\nPersonal: {personalScore} / {personalTotal}\nNon-Evidence: {incorrectTotal}";
        pictureScoreText.text = $"Picture Score: {pictureScore} / {pictureTotal}";
        warrantScoreText.text = $"Picked up Warrant?: {warrantGrabbed}";
        overallScoreText.text = $"Overall Score: {overallScore} / {overallTotal}";
        gradeText.text = $"Grade: {grade}";
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

                int prevTotalEvidence = (int)playerprefs["TotalEvidence"];
                int prevEvidenceScore = (int)playerprefs["EvidenceScore"];

                playerprefsplus.Set("TotalEvidence", prevTotalEvidence + evidenceTotal);
                playerprefsplus.Set("EvidenceScore", prevEvidenceScore + evidenceScore);
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

            user.ClearPlayerTempFiles();
        }
    }
}
