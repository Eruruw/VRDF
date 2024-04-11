using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeScore : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject player = GameObject.Find("Player");
        ScoreTracker tracker = player.GetComponent<ScoreTracker>();
        tracker.TallyScores();
        tracker.UpdateStats();
    }
}