using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
using MimeKit;
using MailKit.Net.Smtp;
*/

public class InstructorEmail : MonoBehaviour
{
/*
    public string senderEmail = "VRDF_LaTech@gmail.com";
    public string senderPassword = "443CSCvrdf443";
    public string recipientEmail = "nmj010@latech.edu";
    public string subject = "Test Email from Unity YOLO\nYOLO\n";
    public string body = "Hello, this is a test email from Unity!";
    // Start is called before the first frame update
    void Start()
    {
        Send_Email();
    }

    public void Send_Email()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("headset", senderEmail));
        message.To.Add(new MailboxAddress("That guy", recipientEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate(senderEmail, senderPassword);
            client.Send(message);
            client.Disconnect(true);
        }
    }
*/
}