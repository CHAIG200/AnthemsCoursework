using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using AnthemsCoursework.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace AnthemsCoursework.Controllers
{
    public class SamplesController : ApiController
    {

        public static String partitionName = "Samples_Partition_1";

        private CloudStorageAccount storageAcc;
        private CloudTableClient tClient;
        private CloudTable table;

        public SamplesController()
        {
            storageAcc = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tClient = storageAcc.CreateCloudTableClient();
            table = tClient.GetTableReference("Samples");
        }


        /// <summary>
        /// Get all Samples
        /// </summary>
        /// <returns></returns>
        // GET: api/Samples
        public IEnumerable<SampleDTO> Get()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<SampleEntity> entityList = new List<SampleEntity>(table.ExecuteQuery(query));
            IEnumerable<SampleDTO> SampleList = from e in entityList
                                                select new SampleDTO()
                                                {
                                                    SampleID = e.RowKey,
                                                    Title = e.Title,
                                                    Artist = e.Artist,
                                                    Mp3Blob = e.Mp3Blob,
                                                    SampleMp3Blob = e.SampleMp3Blob,
                                                    SampleDate = e.SampleDate,
                                                    CreatedDate = e.CreatedDate,
                                                    SampleMp3Url = e.SampleMp3Url
                                                   
                                                };
            return SampleList;
        }

        // GET: api/Samples/5
        /// <summary>
        /// Get one Sample
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(SampleDTO))]
        public IHttpActionResult GetSample(string id)
        {
            // Create a retrieve operation that takes a Sample entity.
            TableOperation getOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult getOperationResult = table.Execute(getOperation);

            // Construct response including a new DTO as apprporiatte
            if (getOperationResult.Result == null) return NotFound();
            else
            {
                SampleEntity sampleEntity = (SampleEntity)getOperationResult.Result;
                SampleDTO s = new SampleDTO()
                {
                    SampleID = sampleEntity.RowKey,
                    Title = sampleEntity.Title,
                    Artist = sampleEntity.Artist,
                    Mp3Blob = sampleEntity.Mp3Blob,
                    SampleMp3Blob = sampleEntity.SampleMp3Blob,
                    SampleDate = sampleEntity.SampleDate,
                    CreatedDate = sampleEntity.CreatedDate,
                    SampleMp3Url = sampleEntity.SampleMp3Url,
                    FileName = sampleEntity.FileName
                };
                return Ok(s);
            }
        }

        // POST: api/Samples
        /// <summary>
        /// Create a new Sample
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [ResponseType(typeof(SampleDTO))]
        public IHttpActionResult PostSample(SampleDTO sampleDTO)
        {
            SampleEntity sampleEntity = new SampleEntity()
            {
                RowKey = getNewMaxRowKeyValue(),
                PartitionKey = partitionName,
                Title = sampleDTO.Title,
                Artist = sampleDTO.Artist,
                Mp3Blob = sampleDTO.Mp3Blob,
                SampleMp3Blob = sampleDTO.SampleMp3Blob,
                SampleDate = sampleDTO.SampleDate,
                CreatedDate = sampleDTO.CreatedDate,
                SampleMp3Url = sampleDTO.SampleMp3Url,
                FileName =  sampleDTO.FileName
            };

            // Create the TableOperation that inserts the Sample entity.
            var insertOperation = TableOperation.Insert(sampleEntity);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return CreatedAtRoute("DefaultApi", new { id = sampleEntity.RowKey }, sampleEntity);
        }

        // PUT: api/Samples/5
        /// <summary>
        /// Update a Sample
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sample"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSample(string id, SampleDTO sample)
        {
            if (id != sample.SampleID)
            {
                return BadRequest();
            }

            // Create a retrieve operation that takes a Sample entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a SampleEntity object.
            SampleEntity updateEntity = (SampleEntity)retrievedResult.Result;

            deleteOldBlobs(updateEntity);

            updateEntity.Title = sample.Title;
            updateEntity.Artist = sample.Artist;
            updateEntity.Mp3Blob = sample.Mp3Blob;
            updateEntity.SampleMp3Blob = sample.SampleMp3Blob;
            updateEntity.SampleDate = sample.SampleDate;
            updateEntity.CreatedDate = sample.CreatedDate;
            updateEntity.SampleMp3Url = sample.SampleMp3Url;
            updateEntity.FileName = sample.FileName;

            // Create the TableOperation that inserts the product entity.
            var updateOperation = TableOperation.InsertOrReplace(updateEntity);

            // Execute the insert operation.
            table.Execute(updateOperation);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Samples/5
        /// <summary>
        /// Delete a Sample
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(SampleDTO))]
        public IHttpActionResult DeleteSample(string id)
        {
            // Create a retrieve operation that takes a sample entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<SampleEntity>(partitionName, id);
            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result == null) return NotFound();
            else
            {
                SampleEntity deleteEntity = (SampleEntity)retrievedResult.Result;
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                deleteOldBlobs(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                return Ok(retrievedResult.Result);
            }
        }

        private String getNewMaxRowKeyValue()
        {
            TableQuery<SampleEntity> query = new TableQuery<SampleEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

            int maxRowKeyValue = 0;
            foreach (SampleEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }

        private void deleteOldBlobs(SampleEntity something)
        {
            // Retrieve storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");

            // Create a blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a created container.
            CloudBlobContainer container = blobClient.GetContainerReference("audiogallery");

            // Retrieve reference to blob.
            CloudBlockBlob blockBlobSample = container.GetBlockBlobReference(something.SampleMp3Blob);
            CloudBlockBlob blockBlobTrack = container.GetBlockBlobReference(something.Mp3Blob);

            blockBlobSample.DeleteIfExists();
            blockBlobTrack.DeleteIfExists();
        }
    }
}
