using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MimeKit;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading.Tasks;
using System.Linq;
using TMPro;

public class Emailer : MonoBehaviour
{
    private UserManager user;
    private List<string[]> playerData = new List<string[]>();
    private string tempCachePath;

    private string adminEmail;
    private string userEmail;

    public TextMeshProUGUI userSentEmailText;
    public TextMeshProUGUI instructorSentEmailText;
    private void Start()
    {
        GameObject userMan = GameObject.Find("UserManager");
        if (userMan != null)
        {
            user = userMan.GetComponent<UserManager>();
        }
        tempCachePath = Application.temporaryCachePath;

    }

    public void CreateCSV()
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus();
        playerprefsplus_player[] players;
        playerprefsplus.GetAllPlayers(out players);
        string[] csvTitles = new string[] { "Name", "Number Of Office Attempts", "OfficeScores", "BestOfficeScore", "AverageOfficeScore", "Total Possible Evidence", "Total Gathered Evidence", "Password" };
        playerData.Add(csvTitles);
        foreach (playerprefsplus_player p in players)
        {
            if (p.Title != "Default Values")
            {
                playerprefsplus.GetPlayerByName(p.Title);
                Dictionary<string, object> playerprefs = playerprefsplus.Get();

                //Get all values
                string name = p.Title;
                int numOfOfficeRuns = (int)playerprefs["NumberOfOfficeRuns"]; 
                float bestOfficeScore = (float)playerprefs["BestOfficeScore"];
                float averageOfficeScore = (float)playerprefs["AverageOfficeScore"];
                string password = (string)playerprefs["Password"];
                int totalEvidence = (int)playerprefs["TotalEvidence"];
                int evidenceScore = (int)playerprefs["EvidenceScore"];

                string officeScores = "";
                if (numOfOfficeRuns <= 1)
                {
                    float firstOfficeScore = (float)playerprefs["OfficeScores"];
                    officeScores = firstOfficeScore.ToString();
                }
                else
                {
                    object[] tempOfficeScores = (object[])playerprefs["OfficeScores"];      //get array of all Office Scores
                    float[] allOfficeScores = tempOfficeScores.OfType<float>().ToArray();    //convert to float array
                    officeScores += "[";
                    foreach (float val in allOfficeScores)
                    {
                        officeScores += val.ToString() + "|";
                    }
                    officeScores += "]";
                }

                string[] tempPlayerData = new string[] {name, numOfOfficeRuns.ToString(), officeScores, bestOfficeScore.ToString(), averageOfficeScore.ToString(), totalEvidence.ToString(), evidenceScore.ToString(), password};
                playerData.Add(tempPlayerData);
            }
        }
        playerprefsplus.Close();
        string filePath = Application.temporaryCachePath + "/PlayerData.csv";
        WriteCsv(playerData, filePath);
        Debug.Log("CSV Created");
    }

    public static void WriteCsv(List<string[]> data, string filePath)
    {
        using (StreamWriter file = new StreamWriter(filePath))
        {
            foreach (var row in data)
            {
                string line = string.Join(",", row);
                file.WriteLine(line);
            }
        }
    }

    public void SendAllUserDataEmail()
    {
        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus("Admin");
        adminEmail = (string)playerprefsplus.Get("Email");
        playerprefsplus.Close();

        CreateCSV();
        instructorSentEmailText.text = "Email Sent";
        Task.Run(() => SendAllUserData())
                    .ContinueWith(task =>
                    {
                        if (task.Exception != null)
                            Debug.LogError("Failed to send email: " + task.Exception);
                        else
                            Debug.Log("Email sent successfully!");
                    }, TaskScheduler.FromCurrentSynchronizationContext()); // Ensure Unity context for logging
    }

    public async Task SendAllUserData()
    {
        await Task.Delay(1);



        if (adminEmail != "")
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("VRDF", "vrdf443@outlook.com"));
            message.To.Add(new MailboxAddress("Professor", adminEmail));
            message.Subject = "VRDF Data Export";

            var multipartBody = new Multipart("mixed");
            {
                var textPart = new TextPart("plain")
                {
                    Text = @"Here are all of your player's Data"
                };
                multipartBody.Add(textPart);

                string filePath = tempCachePath + "/PlayerData.csv";
                var csvPart = new MimePart("text", "csv")
                {
                    Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(filePath)
                };
                multipartBody.Add(csvPart);
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
            }
        }
    }

    public void SendUserDataEmail()
    {

        PlayerPrefsPlus playerprefsplus = new PlayerPrefsPlus(user.currentUser);
        userEmail = (string)playerprefsplus.Get("Email");
        userSentEmailText.text = "Email Sent";
        playerprefsplus.Close();

        Task.Run(() => SendUserData())
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                        Debug.LogError("Failed to send email: " + task.Exception);
                    else
                        Debug.Log("Email sent successfully!");
                }, TaskScheduler.FromCurrentSynchronizationContext()); // Ensure Unity context for logging
    }

    public async Task SendUserData()
    {
        await Task.Delay(1);


        var message = new MimeMessage();
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
            string fulldirectoryPath = tempCachePath + directoryPath;
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
