using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebServiceMain
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebServiceMain : System.Web.Services.WebService
    {

        public static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        public static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        
        
        /// <summary>
        /// Initiates web crawling by sending message through a command queue
        /// </summary>
        [WebMethod]
        public void startCrawl()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("commandq");
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage("start");
            queue.AddMessage(message);
        }

        /// <summary>
        /// Stops crawling via a "stop" message sent through command queue
        /// </summary>
        [WebMethod]
        public void stopCrawl()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("commandq");
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage("stop");
            queue.AddMessage(message);
        }

        [WebMethod]
        public string getCPU()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("performancetable");
            TableQuery<StatTest123> query = new TableQuery<StatTest123>().Select(new string[] { "cpu" });

            foreach (StatTest123 entity in table.ExecuteQuery(query))
            {
                return entity.cpu;
            }

            return "Nothing found";
        }
        
        /// <summary>
        /// Gets CPU utilization stored in azure performance table
        /// </summary>
        /// <returns>string CPU utilization %</returns>
        [WebMethod]
        public string getRAM()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("performancetable");
            TableQuery<StatTest123> query = new TableQuery<StatTest123>().Select(new string[] { "ram" });

            foreach (StatTest123 entity in table.ExecuteQuery(query))
            {
                return entity.ram;
            }

            return "Nothing found";
        }

        /// <summary>
        /// Gets link queue size from azure performance table
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public int? getQueueLength()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("linkq");

            queue.FetchAttributes();
            //queue.
            int? cachedMessageCount = queue.ApproximateMessageCount;
            return cachedMessageCount;
        }

        /// <summary>
        /// Returns the number of URLs that were crawled from Azure Performance table
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string getIndexSize()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("performancetable");
            TableQuery<StatTest123> query = new TableQuery<StatTest123>().Select(new string[] { "count" });
            query.Take(10);

            foreach (StatTest123 entity in table.ExecuteQuery(query))
            {
                return entity.count;
            }

            return "Nothing found";
        }

        /// <summary>
        /// Linq query where it fetches the last 10 url stored in azure url table
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getTenURL()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            string rowKeyToUse = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            CloudTable table = tableClient.GetTableReference("urltable");
            var query = (from urltable in table.CreateQuery<CustomerEntity>()
                        where urltable.PartitionKey == "URL Partition"
                        && urltable.RowKey.CompareTo(rowKeyToUse) > 0
                        select urltable).Take(10);
            
            List<string> list = new List<string>();
            try
            {
                foreach (CustomerEntity entity in query)
                {
                    string url = entity.url;
                    string title = entity.PageTitle;
                    list.Add(url + ";;;" + title);

                }
                return new JavaScriptSerializer().Serialize(list);
            }
            catch
            {
                return "nothing";
            }
        }
        
    }
}
