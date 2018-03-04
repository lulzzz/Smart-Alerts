//-----------------------------------------------------------------------
// <copyright file="MdmClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Extensions;
    using Microsoft.Azure.Monitoring.SmartSignals.Mdm;
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
        /// A dictionary, mapping <see cref="MdmResourceType"/> enumeration values to matching ARM string
        /// </summary>
        private static readonly ReadOnlyDictionary<ServiceType, string> MapAzureResourceServiceToString =
            new ReadOnlyDictionary<ServiceType, string>(
                new Dictionary<ServiceType, string>()
                {
                    [ServiceType.AzureStorageBlob] = "blobServices/default",
                    [ServiceType.AzureStorageTable] = "tableServices/default",
                    [ServiceType.AzureStorageQueue] = "queueServices/default",
                    [ServiceType.AzureStorageFile] = "fileServices/default",
                });

        private readonly ResourceIdentifier resourceIdentifier;
        private readonly ServiceClientCredentials credentials;
        private readonly ITracer tracer;
        private readonly Policy retryPolicy;

        private MonitorManagementClient monitorManagementClient;

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
            this.resourceIdentifier = resourceIdentifier;

            this.credentials = credentialsFactory.Create("https://management.azure.com/");
            this.monitorManagementClient = new MonitorManagementClient(this.credentials);
            this.monitorManagementClient.SubscriptionId = resourceIdentifier.SubscriptionId;
            this.tracer = tracer;
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Get the resource metric values from MDM
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryParameters">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        public async Task<IEnumerable<MdmQueryResult>> GetResourceMetrics(string resourceFullUri, QueryParameters queryParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.tracer.TraceInformation($"Running GetResourceMetrics with an instance of {this.GetType().Name}, with params: {queryParameters.ToString()}");
            ResponseInner metrics = await this.retryPolicy.RunAndTrackDependencyAsync(
                this.tracer, 
                DependencyName, 
                "GetResourceMetrics", 
                () => this.monitorManagementClient.Metrics.ListAsync(
                    resourceUri: resourceFullUri, 
                    timespan: queryParameters.TimeRange, 
                    interval: queryParameters.Interval, 
                    metric: queryParameters.MetricName, 
                    aggregation: queryParameters.Aggregations != null ? string.Join(",", queryParameters.Aggregations) : null,
                    top: queryParameters.Top, 
                    orderby: queryParameters.Orderby, 
                    filter: queryParameters.Filter, 
                    resultType: null, 
                    cancellationToken: cancellationToken));

            this.tracer.TraceInformation($"Running GetResourceMetrics completed. Total Metrics: {metrics.Value.Count}.");
            IList<MdmQueryResult> result = this.ConvertMdmResponseToQueryResult(metrics);

            return result;
        }

        /// <summary>
        /// Get the resource metric values from MDM, based on the Azure Resource Service (e.g - Azure storage queues service)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="queryParameters">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        public async Task<IEnumerable<MdmQueryResult>> GetResourceMetrics(ServiceType azureResourceService, QueryParameters queryParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            string resourceFullUri = this.GetResourceFullUri(azureResourceService);
            return await this.GetResourceMetrics(resourceFullUri, queryParameters, cancellationToken); 
        }

        /// <summary>
        /// Converts an MDM query response to an internal DTO and returns it
        /// </summary>
        /// <param name="mdmQueryResponse">The MDM query response as returned by Azure Monitoring</param>
        /// <returns>A list of MDM query results</returns>
        private IList<MdmQueryResult> ConvertMdmResponseToQueryResult(ResponseInner mdmQueryResponse)
        {
            var mdmQueryResults = new List<MdmQueryResult>();

            // Convert each metric (a single metric is created per metric name)
            foreach (Metric metric in mdmQueryResponse.Value)
            {
                var mdmQueryResult = new MdmQueryResult()
                {
                    Name = metric.Name.Value,
                    Unit = metric.Unit.ToString(),
                    Timeseries = new List<MetricTimeSeries>()
                };

                // Convert the time series. A time series is created per filtered dimension. 
                // The info regarding the relevant dimension is set int he MetaData field
                foreach (TimeSeriesElement timeSeries in metric.Timeseries)
                {
                    MetricTimeSeries metricTimeSeries = new MetricTimeSeries()
                    {
                        Data = new List<MetricValues>(),
                        MetaData = new List<string>(),
                    };

                    // Convert all metric values 
                    foreach (MetricValue metricValue in timeSeries.Data)
                    {
                        var values = new MetricValues()
                        {
                            TimeStamp = metricValue.TimeStamp,
                            Average = metricValue.Average,
                            Count = metricValue.Count,
                            Maximum = metricValue.Maximum,
                            Minimum = metricValue.Minimum,
                            Total = metricValue.Total
                        };

                        metricTimeSeries.Data.Add(values);
                    }

                    foreach (MetadataValue metadata in timeSeries.Metadatavalues)
                    {
                        metricTimeSeries.MetaData.Add($"{metadata.Name.Value}={metadata.Value}");
                    }
                }

                mdmQueryResults.Add(mdmQueryResult);
                int? firstSeriesLength = mdmQueryResult.Timeseries.Count > 0 ? mdmQueryResult.Timeseries[0].Data.Count : 0;
                this.tracer.TraceInformation($"Metric converted successully. Name: {mdmQueryResult.Name}, Timeseries count: {mdmQueryResult.Timeseries.Count}, First time series length: {firstSeriesLength}");
            }

            return mdmQueryResults;
        }

        /// <summary>
        /// Builds the full Resource metrics Uri based on <see cref="ServiceType"/>.
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <returns>The full Resource metrics Uri</returns>
        private string GetResourceFullUri(ServiceType azureResourceService)
        {
            return $"{this.resourceIdentifier.ToResourceId()}/{MapAzureResourceServiceToString[azureResourceService]}";
        }
    }
}