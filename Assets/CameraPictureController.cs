using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPictureController : MonoBehaviour
{
    private UserManager user;
    private ScoreTracker scoreTracker;
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
    public Camera cameraToCapture; // Assign the camera you want to capture from in the inspector
    public RenderTexture renderTexture; // Assign your RenderTexture here

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

        // Encode the texture into PNG format
        byte[] imageBytes = texture.EncodeToPNG();

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
        System.IO.File.WriteAllBytes(filePath, imageBytes);
        
        Debug.Log("Captured Image Saved to: " + filePath);
        scoreTracker.numberOfPicturesTaken += 1;
    }
}
