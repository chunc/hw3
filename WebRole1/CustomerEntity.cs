using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class CustomerEntity : TableEntity
    {
        public CustomerEntity(string PartitionName, string url)
        {
            string rowKeyToUse = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.PartitionKey = PartitionName;
            //this.RowKey = Guid.NewGuid().ToString();
            this.RowKey = rowKeyToUse;
        }

        public CustomerEntity() { }
        public string url { get; set; }
        public string PageTitle { get; set; }
    }
}