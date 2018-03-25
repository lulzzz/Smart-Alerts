//-----------------------------------------------------------------------
// <copyright file="Alerts.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// This class is the entry point for the Alerts endpoint.
    /// </summary>
    public static class Alerts
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Alerts"/> class.
        /// </summary>
        static Alerts()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<ICloudStorageProviderFactory, CloudStorageProviderFactory>()
                .RegisterType<IApplicationInsightsClientFactory, ApplicationInsightsClientFactory>()
                .RegisterType<IAlertsApi, AlertsApi>();
        }

        /// <summary>
        /// Gets all the Alerts.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The Alerts.</returns>
        [FunctionName("GetAlerts")]
        public static async Task<HttpResponseMessage> GetAllAlerts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alerts")]HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var alertsApi = childContainer.Resolve<IAlertsApi>();

                try
                {
                    ListAlertsResponse alertsResponse;

                    // Extract the url parameters
                    NameValueCollection queryParameters = req.RequestUri.ParseQueryString();

                    DateTime startTime;
                    if (!DateTime.TryParse(queryParameters.Get("startTime"), out startTime))
                    {
                        return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Given start time is not in valid format");
                    }

                    // Endtime parameter is optional
                    DateTime endTime = new DateTime();
                    string endTimeValue = queryParameters.Get("endTime");
                    bool hasEndTime = false;

                    // Check if we have an 'endTime' value in the url
                    if (!string.IsNullOrWhiteSpace(endTimeValue))
                    {
                        // Check value is a legal datetime
                        if (!DateTime.TryParse(endTimeValue, out endTime))
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Given end time is not in valid format");
                        }

                        hasEndTime = true;
                    }

                    // Get all the Alerts based on the given time range
                    if (hasEndTime)
                    {
                        alertsResponse = await alertsApi.GetAllAlertsAsync(startTime, endTime, cancellationToken);
                    }
                    else
                    {
                        alertsResponse = await alertsApi.GetAllAlertsAsync(startTime, cancellationToken: cancellationToken);
                    }

                    return req.CreateResponse(alertsResponse);
                }
                catch (SmartDetectorsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get Alerts due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get Smart Detectors", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get Alerts due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get Smart Detectors", e);
                }
            }
        }
    }
}
