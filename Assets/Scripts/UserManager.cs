using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;


public class UserManager : MonoBehaviour
{
    public string currentUser;
    public string nextSceneToLoad;
    public static UserManager instance = null;

    void Awake()
    {
        // check if instance already exists
        if (instance == null)
        {
            instance = this;
        }
        //if instace does not equal this, destroy it 
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

    }

    private void OnApplicationQuit()
    {
        ClearTempFiles();
    }

    void ClearTempFiles()
    {
        string tempDirectory = Application.temporaryCachePath;
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(tempDirectory);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
            Debug.Log("Temporary files cleared.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error clearing temporary files: " + e.Message);
        }
    }
}
