//-----------------------------------------------------------------------
// <copyright file="SmartDetector.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// This class is the entry point for the Smart Detector endpoint.
    /// </summary>
    public static class SmartDetector
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="SmartDetector"/> class.
        /// </summary>
        static SmartDetector()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<ISmartDetectorApi, SmartDetectorApi>();
        }

        /// <summary>
        /// Gets all the Smart Detectors.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The Smart Detectors encoded as JSON.</returns>
        [FunctionName("GetSmartDetector")]
        public static async Task<HttpResponseMessage> GetAllSmartDetectors([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "smartDetector")] HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var smartDetectorApi = childContainer.Resolve<ISmartDetectorApi>();

                try
                {
                    ListSmartDetectorsResponse smartDetectors = await smartDetectorApi.GetAllSmartDetectorsAsync(cancellationToken);

                    return req.CreateResponse(smartDetectors);
                }
                catch (SmartDetectorsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get Smart Detectors due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get Smart Detectors", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get Smart Detectors due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get Smart Detectors", e);
                }
            }
        }
    }
}
