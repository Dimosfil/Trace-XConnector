using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace Trace_XConnector
{
    public class HttpManager
    {
        public static HttpManager Instance => instance;

        private static HttpManager instance;
        public static void Init()
        {
            if(instance == null)
                instance = new HttpManager();
        }

        public string GetJsonRequest()
        {
            //string site = "http://www.professorweb.ru";
            string site = "http://193.232.150.26:4545/wamba/text.php";
            string site2 = "http://193.232.150.26:4545/wamba/text2.php";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(site);
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