using System.Net;
using System.Net.Mail;

namespace websitechangenotifier
{
    public class EmailHelpers
    {
        public void SendEmail(string subject, string body)
        {
            SmtpClient smtp = new SmtpClient();
            {
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("mosoftwareenterprisesemails@gmail.com", "kctboibbaivumdev");
                // send the email
                smtp.Send("noreply@mosoftwareenterprises.co.uk", "bigmansbro@gmail.com", subject, body);
            }
        }
    }
}