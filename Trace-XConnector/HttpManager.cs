﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public string PostOrderDataRequest()
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