using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeScore : MonoBehaviour
{
    private void CalculateScore()
    {
        GameObject player = GameObject.Find("Player");
        ScoreTracker tracker = player.GetComponent<ScoreTracker>();
        tracker.TallyScores();
    }
}