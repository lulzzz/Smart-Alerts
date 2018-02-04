﻿//-----------------------------------------------------------------------
// <copyright file="AlertRule.cs" company="Microsoft Corporation">
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
    using Microsoft.Azure.Monitoring.SmartSignals.FunctionApp.Authorization;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Unity;

    /// <summary>
    /// This class is the entry point for the alert rule endpoint.
    /// </summary>
    public static class AlertRule
    {
        private static readonly IUnityContainer Container;

        /// <summary>
        /// Initializes static members of the <see cref="AlertRule"/> class.
        /// </summary>
        static AlertRule()
        {
            // To increase Azure calls performance we increase default connection limit (default is 2) and ThreadPool minimum threads to allow more open connections
            ServicePointManager.DefaultConnectionLimit = 100;
            ThreadPool.SetMinThreads(100, 100);

            Container = DependenciesInjector.GetContainer()
                .RegisterType<IAuthorizationManagementClient, AuthorizationManagementClient>()
                .RegisterType<ISmartSignalRepository, SmartSignalRepository>()
                .RegisterType<IAlertRuleApi, AlertRuleApi>();
        }

        /// <summary>
        /// Add the given alert rule to the alert rules store..
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>200 if request was successful, 500 if not.</returns>
        [FunctionName("alertRule")]
        public static async Task<HttpResponseMessage> AddAlertRule([HttpTrigger(AuthorizationLevel.Anonymous, "put")] HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var alertRuleApi = childContainer.Resolve<IAlertRuleApi>();
                var authorizationManagementClient = childContainer.Resolve<IAuthorizationManagementClient>();

                // Read given parameters from body
                var addAlertRule = await req.Content.ReadAsAsync<AddAlertRule>(cancellationToken);

                try
                {
                    // Check authorization
                    bool isAuthorized = await authorizationManagementClient.IsAuthorizedAsync(req, cancellationToken);
                    if (!isAuthorized)
                    {
                        return req.CreateErrorResponse(HttpStatusCode.Forbidden, "The client is not authorized to perform this action");
                    }

                    await alertRuleApi.AddAlertRuleAsync(addAlertRule, cancellationToken);

                    return req.CreateResponse(HttpStatusCode.OK);
                }
                catch (SmartSignalsManagementApiException e)
                {
                    tracer.TraceError($"Failed to add alert rule due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to add the given alert rule", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to add alert rule due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to add the given alert rule", e);
                }
            }
        }
    }
}
