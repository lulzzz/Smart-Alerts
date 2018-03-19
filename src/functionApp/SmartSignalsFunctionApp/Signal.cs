//-----------------------------------------------------------------------
// <copyright file="Signal.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// This class is the entry point for the signal endpoint.
    /// </summary>
    public static class Signal
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="Signal"/> class.
        /// </summary>
        static Signal()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<ISmartDetectorApi, SmartDetectorApi>()
                .RegisterType<IAlertRuleApi, AlertRuleApi>()
                .RegisterType<IAlertsApi, AlertsApi>();
        }

        /// <summary>
        /// Gets all the smart signals.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>The smart signals encoded as JSON.</returns>
        [FunctionName("GetSignal")]
        public static async Task<HttpResponseMessage> GetAllSmartSignals([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "signal")] HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            // TODO: Only allow calls from Smart-Alert rule engine
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var signalApi = childContainer.Resolve<ISmartDetectorApi>();

                try
                {
                    ListSmartDetectorsResponse smartSignals = await signalApi.GetAllSmartDetectorsAsync(cancellationToken);

                    return req.CreateResponse(smartSignals);
                }
                catch (SmartDetectorsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get smart signals due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to get smart signals", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get smart signals due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to get smart signals", e);
                }
            }
        }
    }
}
