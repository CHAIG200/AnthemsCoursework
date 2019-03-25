using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using System.Diagnostics;

namespace AnthemsCoursework
{
    public class CloudQueueService
    {
        public CloudQueue getCloudQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse
                 (ConfigurationManager.ConnectionStrings["AzureStorage"].ToString());

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue audioQueue = queueClient.GetQueueReference("audiomaker");
            audioQueue.CreateIfNotExists();

            Trace.TraceInformation("Queue initialized");

            return audioQueue;
        }
    }
}

