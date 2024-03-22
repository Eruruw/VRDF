using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPictureController : MonoBehaviour
{
    private UserManager user;
    private ScoreTracker scoreTracker;

    public Camera cameraToCapture; // Assign the camera you want to capture from in the inspector
    public RenderTexture renderTexture; // Assign your RenderTexture here
    public GameObject pictureDisplayPrefab; // Prefab of the UI image to display pictures
    public Transform pictureDisplayParent; // Parent transform for the picture display UI elements

    private void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            scoreTracker = player.GetComponent<ScoreTracker>();
        }
    }

    public void TakePicture()
    {
        StartCoroutine(CaptureAndSave());
    }

    IEnumerator CaptureAndSave()
    {
        string pictureNumber = scoreTracker.numberOfPicturesTaken.ToString();
        // Wait for the end of the frame to ensure all rendering is complete
        yield return new WaitForEndOfFrame();

        // Save the current RenderTexture so we can restore it after the capture
        RenderTexture currentRT = RenderTexture.active;

        // Set the RenderTexture to be the one from the camera
        RenderTexture.active = cameraToCapture.targetTexture ?? renderTexture;

        // Create a Texture2D to copy the RenderTexture into
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Restore the original RenderTexture
        RenderTexture.active = currentRT;
        string directoryPath = "/Captures" + "/" + user.currentUser;
        // Create directory
        string fulldirectoryPath = Application.temporaryCachePath + directoryPath;
        // Ensure the directory exists
        System.IO.Directory.CreateDirectory(fulldirectoryPath); // No exception if it already exists

        // Define the file path including the directory
        string filePath = fulldirectoryPath + "/Capture" + pictureNumber + ".png";
        // Save the image
        System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());

        Debug.Log("Captured Image Saved to: " + filePath);
        scoreTracker.numberOfPicturesTaken += 1;

        // Display the captured picture
        DisplayPicture(filePath);
    }

    void DisplayPicture(string filePath)
    {
        // Instantiate a new picture display prefab
        GameObject pictureDisplay = Instantiate(pictureDisplayPrefab, pictureDisplayParent);

        // Load the image from file and assign it to the picture display UI element
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        pictureDisplay.GetComponent<Image>().sprite = sprite;

        // Adjust the position of the instantiated picture display
        RectTransform rt = pictureDisplay.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); // Set anchor to top-center
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f); // Set pivot to top-center

        // Calculate maximum allowable vertical position
        float canvasHeight = 728f;
        float imageHeight = 100f;
        float maxVerticalPosition = -(canvasHeight / 2f) + (imageHeight / 2f);

        // Adjust vertical position to ensure it stays within canvas bounds
        float yOffset = Mathf.Clamp(-(imageHeight + 10) * scoreTracker.numberOfPicturesTaken, maxVerticalPosition, 0f);
        rt.anchoredPosition = new Vector2(0, yOffset);
    }
}