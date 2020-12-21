using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebLab3Client
{
    class HttpClient
    {
        static private System.Net.WebClient client;

        static void Send1(String data, String method)
        {
            client.UploadData("http://localhost:8080/", method, Encoding.UTF8.GetBytes(data));
        }

        static void Send(String postData, String method)
        {
            var data = Encoding.ASCII.GetBytes(postData);


            WebRequest request = WebRequest.Create("http://localhost:8080/");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = method;
            request.ContentLength = data.Length;

            if (data.Length > 0)
                request.GetRequestStream().Write(data, 0, data.Length);

            // Get the response.

            try
            {
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                // Display the status.
                Console.WriteLine("Status: " + response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(responseFromServer);
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch
            {
                Console.WriteLine("Unhandled exception");
            }
        }

        public static void Main(string[] args)
        {

            client = new WebClient();

            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            while (true)
            {
                String line = Console.ReadLine();
                if (line == null)
                    continue;

                int space = line.IndexOf(' ');
                String request;
                if (space == -1)
                    request = line.ToUpper();
                else
                    request = line.Substring(0, space).ToUpper();

                switch (request)
                {
                    case "PUT":
                        Send(line.Substring(space + 1), "PUT");
                        break;
                    case "POST":
                        Send(line.Substring(space + 1), "POST");
                        break;
                    case "GET":
                        Send("", "GET");
                        break;
                    case "PATCH":
                        Send(line.Substring(space + 1), "PATCH");
                        break;
                    case "QUIT":
                        return;
                }
            }
        }
    }
}
