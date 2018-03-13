//-----------------------------------------------------------------------
// <copyright file="AlertRule.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.FunctionApp.Authorization;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
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
                .RegisterType<IAlertRuleApi, AlertRuleApi>()
                .RegisterType<IAlertRuleStore, AlertRuleStore>()
                .RegisterType<ICloudStorageProviderFactory, CloudStorageProviderFactory>();
        }

        /// <summary>
        /// Add the given alert rule to the alert rules store.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>200 if request was successful, 500 if not.</returns>
        [FunctionName("AddAlertRule")]
        public static async Task<HttpResponseMessage> AddAlertRule([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alertRule")] HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var alertRuleApi = childContainer.Resolve<IAlertRuleApi>();
                var authorizationManagementClient = childContainer.Resolve<IAuthorizationManagementClient>();

                try
                {
                    // Read given parameters from body
                    var alertRuleEntityToAdd = await req.Content.ReadAsAsync<AlertRuleApiEntity>(cancellationToken);

                    // Check authorization
                    bool isAuthorized = await authorizationManagementClient.IsAuthorizedAsync(req, cancellationToken);
                    if (!isAuthorized)
                    {
                        return req.CreateErrorResponse(HttpStatusCode.Forbidden, "The client is not authorized to perform this action");
                    }

                    await alertRuleApi.AddAlertRuleAsync(alertRuleEntityToAdd, cancellationToken);

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

        /// <summary>
        /// Get the alert rules.
        /// </summary>
        /// <param name="req">The incoming request.</param>
        /// <param name="log">The logger.</param>
        /// <param name="cancellationToken">A cancellation token to control the function's execution.</param>
        /// <returns>200 if request was successful, 500 if not.</returns>
        [FunctionName("GetAlertRule")]
        public static async Task<HttpResponseMessage> GetAlertRules([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alertRule")] HttpRequestMessage req, TraceWriter log, CancellationToken cancellationToken)
        {
            using (IUnityContainer childContainer = Container.CreateChildContainer().WithTracer(log, true))
            {
                ITracer tracer = childContainer.Resolve<ITracer>();
                var alertRuleApi = childContainer.Resolve<IAlertRuleApi>();
                var authorizationManagementClient = childContainer.Resolve<IAuthorizationManagementClient>();

                try
                {
                    // Check authorization
                    bool isAuthorized = await authorizationManagementClient.IsAuthorizedAsync(req, cancellationToken);
                    if (!isAuthorized)
                    {
                        return req.CreateErrorResponse(HttpStatusCode.Forbidden, "The client is not authorized to perform this action");
                    }

                    var alertRules = await alertRuleApi.GetAlertRulesAsync();

                    // Convert the alert rules to alert rules entities that the alert rules API entities
                    List<AlertRuleApiEntity> alertRuleApiEntities = alertRules.Select(alertRule => new AlertRuleApiEntity
                    {
                        ResourceId = alertRule.ResourceId,
                        SignalId = alertRule.SmartDetectorId,
                        Name = alertRule.Name,
                        Description = alertRule.Description,
                        CadenceInMinutes = (int)alertRule.Cadence.TotalMinutes,
                        EmailRecipients = alertRule.EmailRecipients
                    }).ToList();

                    return req.CreateResponse(new ListAlertRulesResponse
                    {   
                        AlertRules = alertRuleApiEntities
                    });
                }
                catch (SmartSignalsManagementApiException e)
                {
                    tracer.TraceError($"Failed to get alert rules due to managed exception: {e}");

                    return req.CreateErrorResponse(e.StatusCode, "Failed to add the given alert rule", e);
                }
                catch (Exception e)
                {
                    tracer.TraceError($"Failed to get alert rules due to un-managed exception: {e}");

                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to add the given alert rule", e);
                }
            }
        }
    }
}
