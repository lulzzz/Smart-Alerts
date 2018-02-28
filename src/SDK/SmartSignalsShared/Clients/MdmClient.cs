//-----------------------------------------------------------------------
// <copyright file="MdmClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Clients
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Extensions;
    using Microsoft.Azure.Monitoring.SmartSignals.Tools;
    using Microsoft.Rest;
    using Polly;

    /// <summary>
    /// An MDM client that implements <see cref="IMdm"/>.
    /// </summary>
    public class MdmClient : IMdmClient
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "MDM";

        /// <summary>
        /// A dictionary, mapping <see cref="AzureResourceService"/> enumeration values to matching ARM string
        /// </summary>
        private static readonly ReadOnlyDictionary<AzureResourceService, string> MapAzureResourceServiceToString =
            new ReadOnlyDictionary<AzureResourceService, string>(
                new Dictionary<AzureResourceService, string>()
                {
                    [AzureResourceService.AzureStorageBlob] = "blobServices/default",
                    [AzureResourceService.AzureStorageTable] = "tableServices/default",
                    [AzureResourceService.AzureStorageQueue] = "queueServices/default",
                    [AzureResourceService.AzureStorageFile] = "fileServices/default",
                });

        private readonly ServiceClientCredentials credentials;
        private readonly ITracer tracer;
        private readonly Policy retryPolicy;

        private MonitorManagementClient monitorManagementClient;
        private ResourceIdentifier resourceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdmClient"/> class 
        /// </summary>
        /// <param name="tracer">The tracer</param>
        /// <param name="credentialsFactory">The credentials factory</param>
        /// <param name="resourceIdentifier">The resource for which we want to fetch data from MDM</param>
        public MdmClient(ITracer tracer, ICredentialsFactory credentialsFactory, ResourceIdentifier resourceIdentifier)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            Diagnostics.EnsureArgumentNotNull(() => credentialsFactory);
            //// TODO: should I ensure resourceIdentifier is initialized? (it's a struct)
            this.resourceIdentifier = resourceIdentifier;

            this.credentials = credentialsFactory.Create("https://management.azure.com/");
            this.monitorManagementClient = new MonitorManagementClient(this.credentials);
            this.monitorManagementClient.SubscriptionId = resourceIdentifier.SubscriptionId;
            this.tracer = tracer;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Get the resource metric definitions from MDM
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metric definitions list</returns>
        public async Task<IEnumerable<MetricDefinition>> GetMetricDefinitions(string resourceFullUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.tracer.TraceInformation($"Running GetMetricDefinitions with an instance of {this.GetType().Name}, with Uri: {resourceFullUri}");
            IEnumerable<MetricDefinition> metricDefinitions = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer,
                DependencyName,
                "GetMetricDefinitions",
                () => this.monitorManagementClient.MetricDefinitions.ListAsync(
                    resourceUri: resourceFullUri, 
                    cancellationToken: cancellationToken));
            this.tracer.TraceInformation($"Running GetMetricDefinitions completed");

            return metricDefinitions;
        }

        /// <summary>
        /// Get the resource metric definitions from MDM, based on the Azure Resource Service (e.g - Azure storage queues service)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metric definitions list</returns>
        public async Task<IEnumerable<MetricDefinition>> GetMetricDefinitions(AzureResourceService azureResourceService, CancellationToken cancellationToken = default(CancellationToken))
        {
            string resourceFullUri = this.GetResourceFullUri(azureResourceService);
            IEnumerable<MetricDefinition> metricDefinitions = await this.GetMetricDefinitions(resourceFullUri, cancellationToken);
            return metricDefinitions;
        }

        /// <summary>
        /// Get the resource metric values from MDM
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metrics list</returns>
        public async Task<IEnumerable<Metric>> GetResourceMetrics(string resourceFullUri, MdmQueryParameters queryProperties, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.tracer.TraceInformation($"Running GetResourceMetrics with an instance of {this.GetType().Name}, with params: {queryProperties.ToString()}");
            ResponseInner metrics = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer, 
                DependencyName, 
                "GetResourceMetrics", 
                () => this.monitorManagementClient.Metrics.ListAsync(
                    resourceUri: resourceFullUri, 
                    timespan: queryProperties.TimeRange, 
                    interval: queryProperties.Interval, 
                    metric: queryProperties.MetricName, 
                    aggregation: queryProperties.Aggregation?.ToString(), 
                    top: queryProperties.Top, 
                    orderby: queryProperties.Orderby, 
                    filter: queryProperties.Filter, 
                    resultType: queryProperties.ResultType, 
                    cancellationToken: cancellationToken));
            this.tracer.TraceInformation($"Running GetResourceMetrics completed");

            return metrics.Value;
        }

        /// <summary>
        /// Get the resource metric values from MDM, based on the Azure Resource Service (e.g - Azure storage queues service)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metrics list</returns>
        public async Task<IEnumerable<Metric>> GetResourceMetrics(AzureResourceService azureResourceService, MdmQueryParameters queryProperties, CancellationToken cancellationToken = default(CancellationToken))
        {
            string resourceFullUri = this.GetResourceFullUri(azureResourceService);
            return await this.GetResourceMetrics(resourceFullUri, queryProperties, cancellationToken); 
        }

        /// <summary>
        /// Builds the full Resource metrics Uri based on <see cref="AzureResourceService"/>.
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <returns>The full Resource metrics Uri</returns>
        private string GetResourceFullUri(AzureResourceService azureResourceService)
        {
            return $"{this.resourceIdentifier.ToResourceId()}/{MapAzureResourceServiceToString[azureResourceService]}";
        }
    }
}