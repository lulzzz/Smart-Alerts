//-----------------------------------------------------------------------
// <copyright file="IMdmClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;

    /// <summary>
    /// An interface for fetching metrics from MDM for a specific azure resource.
    /// <see href="https://github.com/Azure-Samples/monitor-dotnet-metrics-api/blob/master/Program.cs">.NET MDM API example</see>
    /// <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-metrics-in-azure-monitor">Azure storage metrics example</see>
    /// </summary>
    public interface IMdmClient
    {
        /// <summary>
        /// Get the resource metric definitions from MDM props
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metric definitions list</returns>
        Task<IEnumerable<MetricDefinition>> GetMetricDefinitions(string resourceFullUri, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric definitions from MDM, based on the Azure Resource Service (e.g - Azure storage queues service)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metric definitions list</returns>
        Task<IEnumerable<MetricDefinition>> GetMetricDefinitions(AzureResourceService azureResourceService, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric values from MDM
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metrics list</returns>
        Task<IEnumerable<Metric>> GetResourceMetrics(string resourceFullUri, MdmQueryParameters queryProperties, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric values from MDM, based on the Azure Resource Service (e.g - Azure storage queues service)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>The resource metrics list</returns>
        Task<IEnumerable<Metric>> GetResourceMetrics(AzureResourceService azureResourceService, MdmQueryParameters queryProperties, CancellationToken cancellationToken);
    }
}