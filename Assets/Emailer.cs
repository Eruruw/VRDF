using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MimeKit;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;

public class Emailer : MonoBehaviour
{
    private UserManager user;
    private void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
    }
    public void SendUserDataEmail()
    {
        StartCoroutine(SendUserData());
    }

    IEnumerator SendUserData()
    {
        yield return new WaitForSeconds(0.1f);

        //Get current users name then email 
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus(user.currentUser);
        string userEmail = (string)playerprefsplus.Get("Email");
        var message = new MimeMessage();
        playerprefsplus.Close();


        message.From.Add(new MailboxAddress("VRDF", "vrdf443@outlook.com"));
        message.To.Add(new MailboxAddress(user.currentUser, userEmail));
        message.Subject = "VRDF Data Export";

        var multipartBody = new Multipart("mixed");
        {
            var textPart = new TextPart("plain")
            {
                Text = @"Here are your pictures taken in the Scenerio"
            };
            multipartBody.Add(textPart);

            string directoryPath = "/Captures/" + user.currentUser;
            string fulldirectoryPath = Application.temporaryCachePath + directoryPath;
            if (Directory.Exists(fulldirectoryPath))
            {
                // Get all files in the folder
                string[] files = Directory.GetFiles(fulldirectoryPath);

                // Loop through each file
                foreach (string filePath in files)
                {
                    var pictures = new MimePart("image/png")
                    {
                        Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(filePath)
                    };
                    multipartBody.Add(pictures);
                }
            }
            else
            {
                Debug.Log("Directory does not exist.");
            }
        }
        message.Body = multipartBody;

        using (var client = new SmtpClient())
        {
            // This section must be changed based on your sender's email host
            // Do not use Gmail
            client.Connect("smtp-mail.outlook.com", 587, false);

            //client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate("vrdf443@outlook.com", "JNPS42024vrdf#");
            client.Send(message);
            client.Disconnect(true);
            Debug.Log("Sent email");
        }
    }
}
