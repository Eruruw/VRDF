using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public float loadingDelay = 5f; // Delay time in seconds

    public void OfficeSceneWithDelay()
    {
        // Ensure the game object is active before starting the coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(LoadOfficeWithDelay());
    }

    IEnumerator LoadOfficeWithDelay()
    {
        Debug.Log("Starting delay...");
        // Load the loading scene
        SceneManager.LoadScene("Loading scene");

        // Wait for the specified delay
        yield return new WaitForSeconds(loadingDelay);
        Debug.Log("Delay completed, loading office scene...");

        // Load the office scene after the delay
        SceneManager.LoadScene("Office");
    }

    public void TrafficScene()
    {
        SceneManager.LoadScene("Traffic Collision");
    }

    public void MainMenuScene()
    {
        SceneManager.LoadScene("Main Menu");
    }
}