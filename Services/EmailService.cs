using System.Net;
using System.Net.Mail;

namespace Blog.Services;

public class EmailService {
  public bool Send(
    string toName,
    string toEmail,
    string subject,
    string body,
    string fromName = "Equipe balta.io",
    string fromEmail = "ricardo.passos.cr@gmail.com"
  ) {
    var smtpClient = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port);

    smtpClient.Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password);
    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
    smtpClient.EnableSsl = true;

    var mail = new MailMessage {
      From = new MailAddress(fromEmail, fromName), 
      Subject = subject,
      Body = body,
      IsBodyHtml = true,
    };

    mail.To.Add(new MailAddress(toEmail, toName));

    try {
      smtpClient.Send( mail );
      return true;
    } catch (Exception) {
      return false;
    }
  }
}
