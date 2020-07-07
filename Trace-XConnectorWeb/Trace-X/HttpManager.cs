using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Trace_XConnectorWeb.Trace_X
{
    public class EmailXmlConfig
    {
        public string ServerMail = "trinadfil@mail.ru";
        public string ServerMailPass = "vagitidu";
        public string SmtpServer = "smtp.mail.ru";
        public int SmtpServerPort = 25;
        public string SupportMail = "dimosfil@gmail.com";
    }

    public class HttpManager
    {
        public static HttpManager Instance => instance;

        private static HttpManager instance;

        private static EmailXmlConfig mailXmlConfig;
        public static void Init()
        {
            if (instance == null)
                instance = new HttpManager();


            mailXmlConfig = new EmailXmlConfig();
        }

        public string GetJsonRequest()
        {
            //string site = "http://www.professorweb.ru";
            //string site = "http://193.232.150.26:4545/wamba/text.php";
            string site = "http://10.57.104.2:5555/prosalex/product/api/orderdata";
            //string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);
            //HttpWebResponse resp = (HttpWebResponse)req.();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            string text = String.Empty;
            var httpResp = resp.GetResponseStream();
            if (httpResp != null)
            {
                using StreamReader stream = new StreamReader(httpResp, Encoding.UTF8);
                text = stream.ReadToEnd();
            }

            return text;
        }

        public void SendMailAsync(string message)
        {
            Task.Run((() => SendMail(message)));
        }

        private void SendMail(string message)
        {
            try
            {
                using (MailMessage mm = new MailMessage(mailXmlConfig.ServerMail, mailXmlConfig.SupportMail))
                {
                    mm.Subject = "ProsalexReport";
                    mm.Body = message;
                    mm.IsBodyHtml = false;
                    using (SmtpClient sc = new SmtpClient(mailXmlConfig.SmtpServer, mailXmlConfig.SmtpServerPort))
                    {
                        sc.EnableSsl = true;
                        sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                        sc.UseDefaultCredentials = false;
                        sc.Credentials = new NetworkCredential(mailXmlConfig.ServerMail, mailXmlConfig.ServerMailPass);
                        sc.Send(mm);
                        Program.logger.Debug("SendMail Complite");
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logger.Debug("SendMailInfo: ex: " + ex.ToString());
            }
        }

        public void SendMessageSmtpClient(string message)
        {
            SmtpClient client = new SmtpClient("mysmtpserver");
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("username", "password");

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("whoever@me.com");
            mailMessage.To.Add("receiver@me.com");
            mailMessage.Body = "body";
            mailMessage.Subject = "subject";
            client.Send(mailMessage);
        }

        public string PostOrderDataRequest()
        {
            string site = "http://10.57.104.2:5555/prosalex/product/api/orderdata";
            //string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);

            req.Method = "POST";

            //HttpWebResponse resp = (HttpWebResponse)req.();
            HttpWebResponse resp = (HttpWebResponse) req.GetResponse();

            string text = String.Empty;
            var httpResp = resp.GetResponseStream();
            if (httpResp != null)
            {
                using StreamReader stream = new StreamReader(httpResp, Encoding.UTF8);
                text = stream.ReadToEnd();
            }

            return text;
        }

        public string PostOrderInProductionRequest()
        {
            string site = "http://10.57.104.2:5555/prosalex/product/api/orderdata";
            //string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);

            req.Method = "POST";

            //HttpWebResponse resp = (HttpWebResponse)req.();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            string text = String.Empty;
            var httpResp = resp.GetResponseStream();
            if (httpResp != null)
            {
                using StreamReader stream = new StreamReader(httpResp, Encoding.UTF8);
                text = stream.ReadToEnd();
            }

            return text;
        }

        public string PostOrderInProductionRequest(OrderInProductionRequest orderInProduction)
        {
            string site = "http://10.57.104.2:5555/prosalex/product/api/orderinproduction";
            //string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);

            req.Method = "POST";
            req.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(orderInProduction);
                streamWriter.Write(json);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            string text = String.Empty;
            var httpResp = resp.GetResponseStream();
            if (httpResp != null)
            {
                using StreamReader stream = new StreamReader(httpResp, Encoding.UTF8);
                text = stream.ReadToEnd();
            }

            return text;
        }

        public string PostOrderExportRequest(JsonOrderExportData jsonOrderExportData)
        {
            string site = "http://10.57.104.2:5555/prosalex/product/api/orderexport";
            //string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);

            req.Method = "POST";
            req.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(jsonOrderExportData);
                streamWriter.Write(json);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            string text = String.Empty;
            var httpResp = resp.GetResponseStream();
            if (httpResp != null)
            {
                using StreamReader stream = new StreamReader(httpResp, Encoding.UTF8);
                text = stream.ReadToEnd();
            }

            return text;
        }

    }
}
