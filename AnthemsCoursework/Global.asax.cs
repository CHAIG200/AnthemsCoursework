using AnthemsCoursework.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Mvc;

namespace AnthemsCoursework
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitialiseSamples.go();
            BlobStorageService blobStorageService = new BlobStorageService();
            blobStorageService.getCloudBlobContainer();
            CloudQueueService cloudQueueService = new CloudQueueService();
            cloudQueueService.getCloudQueue();

        }
    }
}