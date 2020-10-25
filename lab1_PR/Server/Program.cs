using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            string access_token = String.Empty;
            string homeURL = "/home";
            // sending/receiving the HTTP requests/responses from a URL
            using (var httpClient = new HttpClient())
            {
                string responseBody = httpClient.GetStringAsync("http://localhost:5000/register").Result;
                JObject json = JObject.Parse(responseBody);
                access_token = json["access_token"].ToString();
                timer.Start();
                SendRequest(homeURL, access_token);
            }
            Console.ReadLine();
           

            // connect to the server
            string sURL;
            sURL = "http://localhost:5000/";

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);

            // get json response
            Stream objStream;
            // access URL
            objStream = wrGETURL.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            // read content from filewrite
            string sLine = objReader.ReadLine();
            Console.WriteLine(sLine);

            // deserialize json into a dynamic object
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = serializer.Deserialize(sLine, typeof(object));

            // parse json response to get the authentication ticket
            string registerURL = String.Format("http://localhost:5000{0}", obj.register.link);
            wrGETURL = WebRequest.Create(registerURL);

            objStream = wrGETURL.GetResponse().GetResponseStream();
            DateTime dt_start = DateTime.Now;

            objReader = new StreamReader(objStream);

            sLine = objReader.ReadLine();
            Console.WriteLine("\n" + sLine);

            serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

            obj = serializer.Deserialize(sLine, typeof(object));

            string accessToken = obj.access_token;
            homeURL = obj.link;

            //access server with authentication ticket to get the response
            sURL = String.Format("http://localhost:5000{0}", homeURL);
            wrGETURL = WebRequest.Create(sURL);
            // access token - header, specification for the server
            wrGETURL.Headers.Add("X-Access-Token", accessToken);

            objStream = wrGETURL.GetResponse().GetResponseStream();

            objReader = new StreamReader(objStream);

            sLine = objReader.ReadLine();
            Console.WriteLine("\n" + sLine);

            serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

            obj = serializer.Deserialize(sLine, typeof(object));

            Dictionary<string, string> nodes = new Dictionary<string, string>();

            // put the JSON object here
            JObject rootObject = JObject.Parse(sLine);
            ParseJson(rootObject, nodes);
            // nodes dictionary contains xpath-like node locations
            Console.WriteLine("");
            Console.WriteLine("JSON:");

            Dictionary<string, string> routes = new Dictionary<string, string>();
            foreach (string key in nodes.Keys)
            {
                if (key.Contains("link"))
                {
                    string newURL = nodes[key];

                    Console.WriteLine(sURL = String.Format("http://localhost:5000{0}", newURL));
                    wrGETURL = WebRequest.Create(sURL);
                    wrGETURL.Headers.Add("X-Access-Token", accessToken);
                    objStream = wrGETURL.GetResponse().GetResponseStream();

                    objReader = new StreamReader(objStream);

                    sLine = objReader.ReadLine();

                    routes[key] = sLine;
                }
            }

            foreach (string key in nodes.Keys)
            {
                Console.WriteLine(key + " = " + nodes[key]);
                if (key.Contains("link"))
                {
                    string newURL = nodes[key];

                    Console.WriteLine(sURL = String.Format("http://localhost:5000{0}", newURL));
                    wrGETURL = WebRequest.Create(sURL);
                    wrGETURL.Headers.Add("X-Access-Token", accessToken);
                    objStream = wrGETURL.GetResponse().GetResponseStream();

                    objReader = new StreamReader(objStream);

                    sLine = objReader.ReadLine();
                    Dictionary<string, string> subnodes = new Dictionary<string, string>();
                    rootObject = JObject.Parse(sLine);
                    ParseJson(rootObject, subnodes);
                    // nodes dictionary contains xpath-like node locations
                    Console.WriteLine("");
                    Console.WriteLine("JSON:");
                    foreach (string subKey in subnodes.Keys)
                    {
                        Console.WriteLine(subKey + " = " + subnodes[subKey]);
                        if (subKey.Contains("link"))
                        {
                            string newSubURL = subnodes[subKey];

                            Console.WriteLine(sURL = String.Format("http://localhost:5000{0}", newSubURL));
                            wrGETURL = WebRequest.Create(sURL);
                            wrGETURL.Headers.Add("X-Access-Token", accessToken);
                            objStream = wrGETURL.GetResponse().GetResponseStream();

                            objReader = new StreamReader(objStream);

                            sLine = objReader.ReadLine();
                            Dictionary<string, string> sub2nodes = new Dictionary<string, string>();
                            rootObject = JObject.Parse(sLine);
                            ParseJson(rootObject, sub2nodes);
                            // nodes dictionary contains xpath-like node locations
                            Console.WriteLine("");
                            Console.WriteLine("JSON:");
                            foreach (string sub2Key in sub2nodes.Keys)
                            {
                                Console.WriteLine(sub2Key + " = " + sub2nodes[sub2Key]);
                            }
                        }
                    }
                }
                Console.WriteLine(DateTime.Now - dt_start);
            }
            Console.ReadLine();
        }

        static bool ParseJson(JToken token, Dictionary<string, string> nodes, string parentLocation = "")
        {
            if (token.HasValues)
            {
                foreach (JToken child in token.Children())
                {
                    if (token.Type == JTokenType.Property)
                    {
                        if (parentLocation == "")
                        {
                            parentLocation = ((JProperty)token).Name;
                        }
                        else
                        {
                            parentLocation += "." + ((JProperty)token).Name;
                        }
                    }
                    ParseJson(child, nodes, parentLocation);
                }

                return true;
            }
            else
            {
                // leaf of the tree
                if (nodes.ContainsKey(parentLocation))
                {
                    // array
                    nodes[parentLocation] += "|" + token.ToString();
                }
                else
                {
                    // single property
                    nodes.Add(parentLocation, token.ToString());
                }

                return false;
            }
        }

        public static List<TableData> fetchedResult = new List<TableData>();
        public static int isDone = 0;
        public static List<string> finalResult = new List<string>();
        public static Stopwatch timer = new Stopwatch();
      //  private static ServerSocket socket = new ServerSocket();

        public static void SendRequest(string route, string access_token)
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Accessing " + route + "...");
                httpClient.DefaultRequestHeaders.Add("X-Access-Token", access_token);
                string responseBody = httpClient.GetStringAsync("http://localhost:5000" + route).Result;
                JObject jsonResponse = JObject.Parse(responseBody);

                if (jsonResponse["link"] != null)
                {
                    JObject linkNodes = (JObject)jsonResponse["link"];
                    foreach (var item in linkNodes.Children())
                    {
                        SendRequestWithNewThread(item.First.ToString(), access_token);
                        //ThreadPool.QueueUserWorkItem(state => SendRequest(item.First.ToString(), access_token));

                    }
                }
                if (jsonResponse["data"] != null)
                {
                    fetchedResult.Add(TableDataMethods.GetTableData(jsonResponse));
                    finalResult.Add(jsonResponse["data"].ToString());
                }

            }
        }

        public static void SendRequestWithNewThread(string route, string access_token)
        {
            var thread = new Thread(() =>
            {
                isDone++;
                SendRequest(route, access_token);
                isDone--;
                if (isDone == 0)
                {
                    timer.Stop();
                    outputResponse();
                    //startServer();
                }
            });
            thread.Start();
        }

        public static void outputResponse()
        {
            Console.WriteLine("\n");
            finalResult.ForEach(output => Console.WriteLine(output + "\n"));
            Console.Write("Process done in " + timer.Elapsed.Seconds + " seconds.");
        }
    }
}