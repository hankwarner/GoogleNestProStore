using System;
using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;

namespace ItemFulfillmentReconciliation.Controllers
{
    public class EWSHelper
    {
        public static ExchangeService service = GetEWSService();

        public static void SendErrorEmail(string subject, string body, string recipient)
        {
            try
            {
                EmailMessage message = new EmailMessage(service);
                message.Subject = subject;
                message.Body = body;
                message.ToRecipients.Add(recipient);
                message.Send();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendErrorEmail(string subject, string body, List<string> recipients)
        {
            try
            {
                EmailMessage message = new EmailMessage(service);
                message.Subject = subject;
                message.Body = body;

                foreach (var recipient in recipients)
                {
                    message.ToRecipients.Add(recipient);
                }

                message.Send();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ExchangeService GetEWSService()
        {
            string[] creds_ews = Environment.GetEnvironmentVariable("CREDS_EWS").Split(';');
            string ews_un = creds_ews[0];
            string ews_pw = creds_ews[1];

            // Set up Exchange Web Service credentials
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013);
            service.Credentials = new WebCredentials(ews_un, ews_pw, "DS");
            service.Url = new Uri("https://outlook.office365.com/ews/Exchange.asmx");

            return service;
        }
    }
}
