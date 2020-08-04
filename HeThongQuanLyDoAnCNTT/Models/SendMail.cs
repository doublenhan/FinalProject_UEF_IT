using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace HeThongQuanLyDoAnCNTT.Models
{
    public class Mail
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Mail(string to, string subject, string body)
        {
            this.To = to;
            this.Subject = subject;
            this.Body = body;
        }
        public void SendMail()
        {
            MailMessage NewMail = new MailMessage(System.Configuration.ConfigurationManager.AppSettings["Email"].ToString(), To);
            NewMail.Subject = Subject;
            NewMail.Body = Body;
            NewMail.IsBodyHtml = false;
            SmtpClient Client = new SmtpClient("smtp.gmail.com", 587);
            Client.Timeout = 600000;
            Client.EnableSsl = true;
            Client.DeliveryMethod = SmtpDeliveryMethod.Network;
            NetworkCredential Credential = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["Email"].ToString()
                                                                , System.Configuration.ConfigurationManager.AppSettings["Password"].ToString());
            Client.UseDefaultCredentials = false;
            Client.Credentials = Credential;
            Client.Send(NewMail);
        }
    }
}