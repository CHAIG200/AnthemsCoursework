using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AnthemsCoursework.Models
{
    public class SampleDTO
    {
        /// <summary>
        /// Sample ID
        /// </summary>
        [Key]
        public string SampleID{ get; set; }

        /// <summary>
        /// Title of sample
        /// </summary>
        public string Title { get; set; }        

        /// <summary>
        /// Name of the Artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// creation date/time of entity
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Name of uploaded blob in blob storag
        /// </summary>
        public string Mp3Blob { get; set; }

        /// <summary>
        /// Name of sample blob in blob storage
        /// </summary>
        public string SampleMp3Blob { get; set; }

        /// <summary>
        /// Web service resource URL of mp3 sample
        /// </summary>
        public string SampleMp3Url { get; set; }

        /// <summary>
        /// FileName for Item
        /// </summary>
        public string FileName{ get; set; }

        /// <summary>
        /// creation date/time of sample blob
        /// </summary>
        public Nullable<DateTime> SampleDate { get; set; }
    }
}