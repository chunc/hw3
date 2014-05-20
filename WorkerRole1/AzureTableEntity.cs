using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    class AzureTableEntity : TableEntity
    {
        public AzureTableEntity(string PartitionName)
        {
            string rowKeyToUse = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.PartitionKey = PartitionName;
            //this.RowKey = Guid.NewGuid().ToString();
            this.RowKey = rowKeyToUse;
        }

        public AzureTableEntity() { }
        public string url { get; set; }
        public string PageTitle { get; set; }
    }
}
