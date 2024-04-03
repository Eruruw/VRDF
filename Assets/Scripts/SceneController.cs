using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneController : MonoBehaviour
{
    public float loadingDelay = 5f; // Delay time in seconds
    public Slider loadingSlider;
    private UserManager user;
   

    private void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        user = userMan.GetComponent<UserManager>();
        if (SceneManager.GetActiveScene().name == "Loading scene")
        {
            StartCoroutine(LoadSceneAsync(user.nextSceneToLoad));
        }
    }

    public void LoadOfficeAsync()
    {
        user.nextSceneToLoad = "Office";
        SceneManager.LoadScene("Loading scene");
    }

    public void LoadMainMenuAsync()
    {
        user.nextSceneToLoad = "Main Menu";
        SceneManager.LoadScene("Loading scene");
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        float startTime = Time.time;
        Debug.Log("Loading Scene Async");
        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // While the scene loads, update the slider
        while (!asyncLoad.isDone)
        {
            float elapsedTime = Time.time - startTime;
            float remainingTime = loadingDelay - elapsedTime;

            // Unity's load progress goes up to 0.9, so we scale it to 1 for the slider.
            float progress = Mathf.Clamp01(remainingTime / loadingDelay);
            loadingSlider.value = 1 - progress;
            if (remainingTime <= 0)
            {
                asyncLoad.allowSceneActivation = true; // Allow the next scene to activate
            }

            yield return null; // Wait a frame before continuing the loop
        }

        // Optional: actions to perform after the scene is fully loaded
    }















    public void OfficeSceneWithDelay()
    {
        // Ensure the game object is active before starting the coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(LoadOfficeWithDelay());
    }

    public void MainMenuSceneWithDelay()
    {
        // Ensure the game object is active before starting the coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

   
        StartCoroutine(LoadMainMenuWithDelay());
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

    IEnumerator LoadMainMenuWithDelay()
    {
        Debug.Log("Starting delay...");
        // Load the loading scene
        SceneManager.LoadScene("Loading scene");

        // Wait for the specified delay
        yield return new WaitForSeconds(loadingDelay);
        Debug.Log("Delay completed, loading office scene...");

        // Load the office scene after the delay
        SceneManager.LoadScene("Main Menu");
    }

    public void StatScene()
    {
        SceneManager.LoadScene("Main Menu");
    }
}