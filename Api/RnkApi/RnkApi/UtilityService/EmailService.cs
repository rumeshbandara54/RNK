﻿using login.model.Dto;
using MimeKit;
using MailKit.Net.Smtp;

namespace login.UtilityService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;  
        public EmailService(IConfiguration configuration) 
        
        { 
            _config = configuration;
        }

        //email massage create  (send email) 
        public void SendEmail(EmailModel emailModel)
        {
            var emailMessage = new MimeMessage();
            var from = _config["EmailSettings:From"];
            emailMessage.From.Add(new MailboxAddress("Lets Program", from));
            emailMessage.To.Add(new MailboxAddress(emailModel.To, emailModel.To));
            emailMessage.Subject = emailModel.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = string.Format(emailModel.Content)
            };
            //using krddi "SmtpClient" eka using MailKit.Net.Smtp; enn use krnna 
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_config["EmailSettings:SmtpServer"], 465, true);
                    client.Authenticate(_config["EmailSettings:From"], _config["EmailSettings:Password"]);
                    client.Send(emailMessage);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

       
    }
}
