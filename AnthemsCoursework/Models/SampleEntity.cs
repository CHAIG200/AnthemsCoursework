using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace AnthemsCoursework.Models
{
    public class SampleEntity : TableEntity 
    {       
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Mp3Blob { get; set; }
        public string SampleMp3Blob { get; set; }
        public Nullable<DateTime> SampleDate { get; set; }
        public string FileName { get; set; }
        public string SampleMp3Url { get; set; }
        public SampleEntity(string partitionKey, string sampleID)
        {
            PartitionKey = partitionKey;
            RowKey = sampleID;
        }

        public SampleEntity() { }
        
    }
}