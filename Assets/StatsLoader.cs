using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class StatsLoader : MonoBehaviour
{
    public TextMeshProUGUI overallRatingText;
    public TextMeshProUGUI totalScenariosText;
    public TextMeshProUGUI flawlessExecutionsText;

    public TextMeshProUGUI officeRatingText;
    public TextMeshProUGUI officeAttemptsText;
    public TextMeshProUGUI evidenceScoreText;

    private UserManager userManager;
    private string user;
    private float OfficeScore;
    // Start is called before the first frame update

    public string CalculateScore(float average)
    {
        string grade = "";
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

        return grade;
    }
    public void LoadStats() 
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            userManager = userMan.GetComponent<UserManager>();
            user = userManager.currentUser;

            PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
            playerprefsplus.GetPlayerByName(user);                                  //load user prefs
            Dictionary<string, object> playerprefs = playerprefsplus.Get();         //get dictionary of player prefs
            if (playerprefsplus.HasKey("NumberOfOfficeRuns"))                       //check if player has required prefs
            {
                int numOfOfficeRuns = (int)playerprefs["NumberOfOfficeRuns"];       //get number of runs for office
                int evidenceScore = (int)playerprefs["EvidenceScore"];
                int totalEvidenceScore = (int)playerprefs["TotalEvidence"];

                totalScenariosText.text = $"Total Scenarios Completed: {numOfOfficeRuns}";
                officeAttemptsText.text = numOfOfficeRuns.ToString();
                evidenceScoreText.text = $"Total Evidence Gathered: {evidenceScore}/{totalEvidenceScore}";


                if (numOfOfficeRuns <= 1)
                {
                    //set ratings
                    float OfficeScore = (float)playerprefs["OfficeScores"];
                    string rating = CalculateScore(OfficeScore);
                    overallRatingText.text = $"Overall Rating: {rating}";
                    officeRatingText.text = rating;

                    //set flawless score
                    if (OfficeScore == 100)
                    {
                        flawlessExecutionsText.text = "Flawless Executions: 1";
                    }
                }
                else
                {
                    object[] tempOfficeScores = (object[])playerprefs["OfficeScores"];      //get array of all Office Scores
                    float[] allOfficeScores = tempOfficeScores.OfType<float>().ToArray();   //convert to float array

                    string rating = CalculateScore(allOfficeScores.Average());               //get rating from average of all office scores
                    overallRatingText.text = $"Overall Rating: {rating}";                    //set rating
                    officeRatingText.text = rating;
                    int flawlessExecutions = 0;

                    foreach (float value in allOfficeScores)
                    {
                        if (value == 100)
                        {
                            flawlessExecutions++;
                        }
                    }

                    flawlessExecutionsText.text = $"Flawless Executions: {flawlessExecutions}";
                }
            }
        }
        else
        {
            Debug.Log("Could not find UserManager");
        }
    }

}
