using NAudio.Wave;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using AnthemsCoursework.Models;

namespace SampleMaker_WebJob
{
    public class Function
    {
        public static void addSample([QueueTrigger("audiomaker")] SampleEntity sampleInQueue, [Table("Samples", "{PartitionKey}", "{RowKey}")] SampleEntity sampleInTable, [Table("Samples")] CloudTable tableBinding, TextWriter logger)
        {
            logger.WriteLine("addsample() started...");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Samples");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer blobContainer = blobClient.GetContainerReference("audiogallery");
            if (blobContainer.CreateIfNotExists())
            {
                // Enable public access on the newly created "photogallery" container.
                blobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
            }

            var inputBlob = blobContainer.GetBlockBlobReference(sampleInQueue.Mp3Blob);
            sampleInQueue.SampleMp3Blob = "samples/" + sampleInQueue.FileName;
            var outputBlob = blobContainer.GetBlockBlobReference(sampleInQueue.SampleMp3Blob);

            GenerateSample(inputBlob, outputBlob, logger);

            var sampleEntityForTable = new SampleEntity()
            {
                PartitionKey = sampleInQueue.PartitionKey,
                RowKey = sampleInQueue.RowKey,
                Title = sampleInQueue.Title,
                Artist = sampleInQueue.Artist,
                Mp3Blob = sampleInQueue.Mp3Blob,
                SampleMp3Blob = sampleInQueue.SampleMp3Blob,
                CreatedDate = sampleInQueue.CreatedDate,
                SampleDate = sampleInQueue.SampleDate,
                FileName = sampleInQueue.FileName,
                SampleMp3Url = outputBlob.Uri.ToString()
            };
            TableOperation insertOperation = TableOperation.InsertOrMerge(sampleEntityForTable);
            tableBinding.Execute(insertOperation);
        }

        private static void GenerateSample(CloudBlockBlob inputBlob, CloudBlockBlob outputBlob, TextWriter logger)
        {

            logger.WriteLine("GenerateSample() started...");

            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                CreateSample(input, output, 15);
                outputBlob.Properties.ContentType = "audio/x-wav";
            }

            //Set some metadata
            var metadataHolder = inputBlob.Metadata["Title"];
            outputBlob.FetchAttributes();
            outputBlob.Metadata["Title"] = metadataHolder;
            outputBlob.SetMetadata();

            //Output to cmd window to inform user webjob is complete
            logger.WriteLine("GenerateSample() completed...");
        }

        private static void CreateSample(Stream input, Stream output, int duration)
        {
            using (var reader = new Mp3FileReader(input, wave => new NLayer.NAudioSupport.Mp3FrameDecompressor(wave)))
            {
                Mp3Frame frame;
                frame = reader.ReadNextFrame();
                int frameTimeLength = (int)(frame.SampleCount / (double)frame.SampleRate * 1000.0);
                int framesRequired = (int)(duration / (double)frameTimeLength * 1000.0);

                int frameNumber = 0;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    frameNumber++;

                    if (frameNumber <= framesRequired)
                    {
                        output.Write(frame.RawData, 0, frame.RawData.Length);
                    }
                    else break;
                }
            }
        }



    }
}



