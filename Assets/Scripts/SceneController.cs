using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void OfficeScene()
    {
        SceneManager.LoadScene("Office");
    }

    public void TrafficScene()
    {
        SceneManager.LoadScene("Traffic Collision");
    }
}
