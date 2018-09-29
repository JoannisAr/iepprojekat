using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace IepProject0
{
    public static class Mail
    {
        public static void Send(string to ,string subject,string body)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("iepaukcije@gmail.com", "iepprojekat123");
            mail.Subject = subject;
            mail.Body = body;
            mail.From = new MailAddress("iepaukcije@gmail.com");
            mail.To.Add(to);
            SmtpServer.Send(mail);
        }
    }
}