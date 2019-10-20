using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace FrictionlessRSS
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 1 && args[0] != "help")
            {
                Scraper theScraper = new Scraper(args[0]);
                do
                {
                    Thread.Sleep(10000);
                    theScraper.ReadTopics(theScraper.ScrapeForNew());
                } while (true);
            } else
            {
                Console.WriteLine("Usage: One argument, the path to where you want the bin file keeping track of topics to be put. Example: 'ConsoleGame2 C:/topics.bin'");
                return;
            }
        }
    }


    class Scraper
    {
        private string outputFile;
        /// <summary>
        /// Scraper class. Pass in the path to the file you want to sore the scraped data in.
        /// </summary>
        public Scraper(string of)
        {
            outputFile = of;
        }

        /// <summary>This function scrapes for new topics from the sitemap. It compares the topics to a binary file (the path is passed in to in the constructor) and adds any new topics it finds to that file as well as returns any new topics it finds.
        /// </summary>
        public Dictionary<string, string> ScrapeForNew()
        {

            XmlReader xmlReader = XmlReader.Create("https://friction-less.net/rss");
            List<String> listBox1 = new List<string>();
            /// <summary>
            /// Topics as we'll see, are currently stored as a dictionary where the title is the key, and url 
            /// is the value.
            /// </summary>
            Dictionary<string, string> topics = File.Exists(outputFile) ? ReadFromBinaryFile<Dictionary<string, string>>(outputFile) : new Dictionary<string, string>();
            Dictionary<string, string> newTopics = new Dictionary<string, string>();

            bool loc = false;
            bool lm = false;
            string locP = "";
            string lmP = "";

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if(xmlReader.Name == "title")
                        {
                            loc = true; 
                            if(locP.Length > 0)
                            {
                                if (!topics.ContainsKey(locP))
                                {
                                    topics.Add(locP, lmP);
                                    newTopics.Add(locP, lmP);
                                }
                                locP = "";
                                lmP = "";
                            }
                        }
                        if (xmlReader.Name == "link")
                        {
                            lm = true;
                        }
                        break;
                    case XmlNodeType.Text:
                        if(loc == true)
                        {
                            locP = xmlReader.Value;
                            loc = false;
                        }
                        if(lm == true) {
                            lmP = xmlReader.Value;
                            lm = false;
                        }
                        break;
                }
            }
            if (!topics.ContainsKey(locP))
            {
                topics.Add(locP, lmP);
                newTopics.Add(locP, lmP);
            }
            WriteToBinaryFile<Dictionary<string, string>>(outputFile, topics);
            return newTopics;
        }

        /// <summary>This function reads to the console the passed in topics.
        /// </summary>
        public void ReadTopics(Dictionary<string, string> dict)
        {
            foreach (var item in dict.OrderBy(i => i.Key))
            {
                Console.WriteLine(item);
            }
        }


        /// <summary>
        /// This function was taken from https://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="objectToWrite"></param>
        /// <param name="append"></param>
        private static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        /// <summary>
        /// This function was taken from https://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static T ReadFromBinaryFile<T>(string filePath)
        {

            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }

        }
    }
}
