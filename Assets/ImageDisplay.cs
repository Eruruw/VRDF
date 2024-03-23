using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ImageDisplay : MonoBehaviour
{
    public GameObject imagePrefab; // Prefab for displaying images
    public Transform contentPanel; // Panel where images will be displayed

    void Start()
    {
        // Call this method periodically or whenever a new image is captured
        UpdateImageDisplay();
    }

    void UpdateImageDisplay()
    {
        // Clear existing images from the panel
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Load captured images from folder (you would need to adjust this depending on how you're storing the images)
        string[] imagePaths = Directory.GetFiles(Application.persistentDataPath, "*.png");

        // Instantiate and display images in the panel
        foreach (string path in imagePaths)
        {
            Texture2D texture = LoadTextureFromFile(path); // Implement this method to load a Texture2D from file
            if (texture != null)
            {
                GameObject imageObject = Instantiate(imagePrefab, contentPanel);
                imageObject.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // Load image data into the texture
        return texture;
    }
}