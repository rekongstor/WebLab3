using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebLab3Server
{
    class Data
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8080/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        //static private SortedList<string, string> map;
        static private Data dataServer;


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                String reqData = "";

                if (req.HasEntityBody)
                {
                    using (System.IO.Stream body = req.InputStream) // here we have data
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(body, req.ContentEncoding))
                        {
                            reqData = reader.ReadToEnd();
                        }
                    }
                }

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine("BODY: " + reqData);
                Console.WriteLine();

                string jsResp = "";
                Data jss;
                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                switch (req.HttpMethod)
                {
                    case "POST":
                        try
                        {
                            jss = JsonSerializer.Deserialize<Data>(reqData, new JsonSerializerOptions());

                            if (dataServer == null)
                                dataServer = new Data();

                            if (jss.name != null)
                                dataServer.name = jss.name;
                            if (jss.value != null)
                                dataServer.value = jss.value;
                            resp.StatusCode = 200;
                        }
                        catch
                        {
                            resp.StatusCode = 400;
                        }
                        break;
                    case "GET":
                        jsResp = JsonSerializer.Serialize<Data>(dataServer);
                        resp.StatusCode = 200;
                        break;
                    case "PUT":
                        if (dataServer == null)
                        {
                            resp.StatusCode = 400;
                            break;
                        }
                        try
                        {
                            jss = JsonSerializer.Deserialize<Data>(reqData, new JsonSerializerOptions());
                            if (jss.name != null)
                                dataServer.name = jss.name;
                            if (jss.value != null)
                                dataServer.value = jss.value;
                            resp.StatusCode = 200;
                        }
                        catch
                        {
                            resp.StatusCode = 400;
                        }
                        break;
                    case "PATCH":
                        try
                        {
                            jss = JsonSerializer.Deserialize<Data>(reqData, new JsonSerializerOptions());
                            if (jss.name != null)
                                dataServer.name = jss.name;
                            if (jss.value != null)
                                dataServer.value = jss.value;
                            resp.StatusCode = 200;
                        }
                        catch
                        {
                            resp.StatusCode = 400;
                        }
                        break;
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(jsResp);
                resp.ContentType = "text/text";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }


        public static void Main(string[] args)
        {
            //map = new SortedList<string, string>();
            dataServer = null;
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}
