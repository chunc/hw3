using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        //Add To Queue
        [WebMethod]
        public void WorkerRoleCalculateSum(int num1, int num2, int num3)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("numq");
            queue.CreateIfNotExists();

            string numlist = num1.ToString() + ',' + num2.ToString() + ',' + num3.ToString();
            CloudQueueMessage message1 = new CloudQueueMessage(numlist);
            queue.AddMessage(message1);
        }

        [WebMethod]
        public bool QaddURLtoQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("linkq");
            queue.CreateIfNotExists();

            //string url = "http://students.washington.edu/changcc/vcs-master-v3";
            string url = "http://htmlagilitypack.codeplex.com/discussions/438284";
            CloudQueueMessage message = new CloudQueueMessage(url);
            queue.AddMessage(message);

            return true;
        }

        [WebMethod]
        public bool QclearQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("linkq");

            queue.Clear();

            return true;
            
        }

        [WebMethod]
        public string QgetQmessage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("numq");

            CloudQueueMessage peekedMessage = queue.PeekMessage();
            return peekedMessage.AsString;
        }

        [WebMethod]
        public void QdeleteNextQ()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("numq");

            CloudQueueMessage message = queue.GetMessage(TimeSpan.FromMinutes(5));
            //return message.AsString;
            queue.DeleteMessage(message);
        }

        //Parses a string
        [WebMethod]
        public string parseNum()
        {
            //string numString = "1, 2, 5";
            string numString = QgetQmessage();

            char[] seperator = {','};
            string[] numlist = numString.Split(seperator);
            
            return numlist[1];
        }


        [WebMethod]
        public string GetWebText()
        {
            //string url = "http://students.washington.edu/changcc/vcs-master-v3";
            string url = "http://htmlagilitypack.codeplex.com/discussions/438284";
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();
            return htmlText;
        }

        [WebMethod]
        public List<string> GetLinksFromHtml()
        {
            string content = GetWebText();
            //string regex = @"(<a.*?>.*?</a>)";    //Extracts <a> tags
            string regex = @"href=\""(.*?)\""";     //Extracts href or url content
            //string regex = @"(?<=<title.*>)([\s\S]*)(?=</title>)";      //Extracts the title of webpage
            var matches = Regex.Matches(content, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var links = new List<string>();

            foreach (Match item in matches)
            {
                string link = item.Groups[1].Value;
                links.Add(link);
            }

            return links;
        }

        [WebMethod]
        public bool stopWorkerRole()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue stopQueue = queueClient.GetQueueReference("stopq");
            stopQueue.CreateIfNotExists();

            CloudQueueMessage stop_message = new CloudQueueMessage("stop");
            stopQueue.AddMessage(stop_message);

            return true;
        }

        [WebMethod]
        public bool TableInsertURL()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("people");
            table.CreateIfNotExists();

            // Create a new customer entity.
            CustomerEntity customer1 = new CustomerEntity("Partition A", "totoro");
            customer1.PageTitle = "brand new entry";
            

            // Create the TableOperation that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(customer1);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return true;

        }
        
        [WebMethod]
        public string TableCheckURL()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("people");

            string partition = "Partition A";
            string url = "www.yahoo.com";
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>(partition, url);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
                return "It exists!";
            else
                return "Nothing found...";
        }

        [WebMethod]
        public List<string> TableQueryPageTitle()
        {
            List <string> title = new List<string>();
           
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("people");

           
            
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Select(new string[] { "PageTitle" });
            query.Take(2);

            // Print the fields for each customer.
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                title.Add(entity.PageTitle);
            }

            return title;
        }

        [WebMethod]
        public bool IsAWebPage(string url)
        {
            return url.Contains(".htm");
        }

        [WebMethod]
        public bool DateCheck(string datestring)
        {
            
            //string datestring = "2014-05-21";

            DateTime today = DateTime.Today;
            
            char[] seperator = { '-' };
            string[] dateArray = datestring.Split(seperator);
            DateTime webpageDate = new DateTime(Convert.ToInt16(dateArray[0]), Convert.ToInt16(dateArray[1]), Convert.ToInt16(dateArray[2]),0,0,0);
            TimeSpan time = today - webpageDate;
            int days = (int)Math.Round((today - webpageDate).TotalDays);

            if (days < 60)
            {
                return true;
            }
            
            return false;
 
        }

        [WebMethod]
        public List<string> DateMatchTEST(string url)
        {
            //string url = "http://sportsillustrated.cnn.com/mlb/news/20140513/jose-fernandez-miami-marlins-injury/?eref=sihp";
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();

            string regex = @"(\d\d\d\d-\d\d-\d\d)";      //Extracts Date webpage
            //string regex = @"(content..\d\d\d\d-\d\d-\d\d)";
            var matches = Regex.Matches(htmlText, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var links = new List<string>();

            foreach (Match item in matches)
            {
                string link = item.Groups[1].Value;
                links.Add(link);
                break;
            }
            
            return links;
        }

        [WebMethod]
        public bool checkNBA(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();

            string regex = @"(<meta.*?.*?\/>)";      
            var matches = Regex.Matches(htmlText, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var links = new List<string>();

            foreach (Match item in matches)
            {
                string link = item.Groups[1].Value;
                if (link.ToLower().Contains("nba"))
                {
                    return true;     
                }
            }
            return false;    
        }

        [WebMethod]
        public string getPageSource(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();

            return htmlText;
        }

        [WebMethod]
        public List<string> parseRobot(string url,bool disallow)
        {
            //string cnn ="http://www.cnn.com/robots.txt";
            //string si = "http://sportsillustrated.cnn.com/robots.txt";

            string robot = getPageSource(url);
            List<string> List = new List<string>();
            String[] lines = Regex.Split(robot,"\n");
            string delimiter;
            string delimiter2;
            if (disallow == true)
            {
                delimiter = "Disallow:";
                delimiter2 = ":";
            } else 
            {
                delimiter = "Sitemap:";
                delimiter2 = " ";
            }

            foreach (String item in lines)
            {
                if (item.Contains(delimiter))
                {
                    
                    String[] text = Regex.Split(item, delimiter2);
                    List.Add(text[1]);
                    
                }
            }

            return List;            
        }

        [WebMethod]
        public string ZZisDisallowedAlphaTest(string url)
        {
            string regex_root = @"http:\/\/.*cnn.com";
            var root = Regex.Match(url, regex_root, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return root.ToString();
        }
        
        [WebMethod]
        public string CPU22222()
        {
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;

            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            return "CPU: " + cpuCounter.NextValue() + "% " + "RAM: " + ramCounter.NextValue() + "MB"; ;
        }

        
        [WebMethod]
        public bool insertorreplace999000000()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("performancetable");

            // Create a retrieve operation that takes a customer entity.
            
            TableOperation retrieveOperation = TableOperation.Retrieve<StatTest123>("counter", "one");

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity object.
            //CustomerEntity updateEntity = (CustomerEntity)retrievedResult.Result;
            StatTest123 updateEntity = (StatTest123)retrievedResult.Result;

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity.cpu = "15%";
                updateEntity.ram = "879 MB";

                // Create the InsertOrReplace TableOperation
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity);

                // Execute the operation.
                table.Execute(insertOrReplaceOperation);

                Console.WriteLine("Entity was updated.");
            }

            else
                Console.WriteLine("Entity could not be retrieved.");
            
            return true;
        }
      

        
        







    }
}
