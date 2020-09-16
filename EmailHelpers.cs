using System;
using System.Net;
using System.Net.Mail;
using Polly;

namespace websitechangenotifier
{
    public class EmailHelpers
    {
        public void SendEmail(string subject, string body)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential("mosoftwareenterprisesemails@gmail.com", "kctboibbaivumdev");

            Policy
              .Handle<Exception>()
              .WaitAndRetry(new[]
              {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
              }).Execute(() => smtp.Send("mosoftwareenterprisesemails+noreply@gmail.com", "bigmansbro@gmail.com", subject, body));
        }
    }
}