using AnthemsCoursework.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace AnthemsCoursework.Migrations
{
    public class InitialiseSamples
    {
        public static void go()
        {
            const String partitionName = "Samples_Partition_1";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("Samples");

            if (!table.Exists())
            {
                // Create table if it doesn't exist already
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                var path = HttpContext.Current.Server.MapPath(@"~/InitSamples.json");

                //Loop through all of the objects inside the initSamples.json file and add 
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    List<SampleDTO> samples = JsonConvert.DeserializeObject<List<SampleDTO>>(json);
                    int x = 1;
                    foreach (var sample in samples)
                    {
                        SampleEntity sampleE = new SampleEntity(partitionName, x.ToString());
                        sampleE.Title = sample.Title;
                        sampleE.Artist = sample.Artist;
                        sampleE.CreatedDate = DateTime.Now;
                        sampleE.Mp3Blob = sample.Mp3Blob;
                        sampleE.SampleMp3Blob = sample.SampleMp3Blob;
                        sampleE.SampleMp3Url = sample.SampleMp3Url;
                        sampleE.SampleDate = sample.SampleDate;
                        batchOperation.Insert(sampleE);
                        x++;
                    }
                }

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }

        }

    }
}