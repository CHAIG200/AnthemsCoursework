using AnthemsCoursework.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using TagLib;

namespace AnthemsCoursework
{
    public partial class _Default : System.Web.UI.Page
    {
        private BlobStorageService _blobStorageService = new BlobStorageService();
        private CloudQueueService _queueStorageService = new CloudQueueService();

        private CloudTable table;
        private CloudTableClient tClient;
        private CloudStorageAccount storageAcc;

        public _Default()
        {
            storageAcc = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tClient = storageAcc.CreateCloudTableClient();
            table = tClient.GetTableReference("Samples");
        }


        private CloudBlobContainer getAudioContainer()
        {
            return _blobStorageService.getCloudBlobContainer();
        }

        private CloudQueue getAudioMakerQueue()
        {
            return _queueStorageService.getCloudQueue();
        }

        //Returns the mime type of a filename passed in
        private string GetMimeType(string Filename)
        {
            try
            {
                string ext = Path.GetExtension(Filename).ToLowerInvariant();
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (key != null)
                {
                    string contentType = key.GetValue("Content Type") as String;
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
            }
            return "application/octet-stream";
        }

        // User clicked the "Submit" button
        protected void submitButton_Click(object sender, EventArgs e)
        {
            if (upload.HasFile)
            {
                // Get the file ext specified by the file. 
                var ext = Path.GetExtension(upload.FileName);

                // assign it a new Guid as the filename
                var name = string.Format("{0}{1}", Guid.NewGuid(), ext);
                // Path to the new location mp3/name.extension
                String path = "mp3/" + name;

                var blob = getAudioContainer().GetBlockBlobReference(path);


                //Sets the mime type of the file that is being uploaded
                blob.Properties.ContentType = GetMimeType(upload.FileName);

                // Upload the data to the blob and set the title metadata to the filename 
                blob.UploadFromStream(upload.FileContent);

                blob.FetchAttributes();
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(upload.FileName,
                         upload.FileContent, upload.FileContent));

                var tags = tagFile.GetTag(TagTypes.Id3v2);
                var artistMeta = String.Join(",", tags.Performers);

                blob.Metadata["Title"] = Path.GetFileNameWithoutExtension(upload.FileName);
                blob.Metadata["Created"] = DateTime.UtcNow.ToString();
                blob.Metadata["mp3Blob"] = path;

                if (artistMeta == "")
                {
                    blob.Metadata["Artist"] = "Unkown Arists";
                }
                else
                {
                    blob.Metadata["Artist"] = artistMeta;
                }
                blob.SetMetadata();

                SampleEntity sampleEntity = new SampleEntity()
                {
                    RowKey = GetNewMaxRowKeyValue(),
                    PartitionKey = "Samples_Partition_1",
                    Title = blob.Metadata["Title"],
                    Artist = blob.Metadata["Artist"],
                    Mp3Blob = path,
                    SampleDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    FileName = name

                };


                var insertOperation = TableOperation.Insert(sampleEntity);
                table.Execute(insertOperation);


                /* Place a message in the queue to tell the worker role that a new audio blob exists, which will 
                  cause it to create a sample blob of that audio
                */
                getAudioMakerQueue().AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(sampleEntity)));

                System.Diagnostics.Trace.WriteLine(String.Format("*** WebRole: Enqueued '{0}'", path));
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                /*                
                Look at the blob container that contains the samples generated by the worker role and query all of the contents and return 
                a list of all of the blobs that begins with samples and returns an enumerator of the URLs and then place it into the data source.
                 */

                AnthemDisplayControl.DataSource = from o in getAudioContainer().GetDirectoryReference("samples").ListBlobs().OfType<CloudBlockBlob>() select new { Url = o.Uri, Title = getMetadata(o.Uri) };

                //Display all the samples 
                AnthemDisplayControl.DataBind();
            }
            catch (Exception)
            {
            }

        }

        /*
        This function grabs the metadate of the uri that is passed it and returns the title
        */
        protected String getMetadata(Uri uri)
        {
            CloudBlockBlob blob = new CloudBlockBlob(uri);
            blob.FetchAttributes();
            return blob.Metadata["Title"];
        }

        /*
         * Function for refreshing the page.
         */
        protected void refresh_Click(object sender, EventArgs e)
        {
            Page.Response.Redirect(Page.Request.Url.ToString(), false);
        }

        public String GetNewMaxRowKeyValue()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Samples_Partition_1"));

            int maxRowKeyValue = 0;
            foreach (SampleEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }
    }
}
