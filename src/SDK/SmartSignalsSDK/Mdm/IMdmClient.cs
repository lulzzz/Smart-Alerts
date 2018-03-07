//-----------------------------------------------------------------------
// <copyright file="IMdmClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Mdm
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for fetching metrics from MDM for a specific azure resource.
    /// <see href="https://github.com/Azure-Samples/monitor-dotnet-metrics-api/blob/master/Program.cs">.NET MDM API example</see>
    /// <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-metrics-in-azure-monitor">Azure storage metrics example</see>
    /// <see href="https://docs.microsoft.com/en-us/powershell/module/azurerm.insights/get-azurermmetricdefinition?view=azurermps-5.4.0">Use Get-AzureRmMetricDefinition, to fetch for available metric names, granularity, etc.</see>
    /// </summary>
    public interface IMdmClient
    {
        /// <summary>
        /// Get the resource metric values from MDM
        /// </summary>
        /// <param name="resourceFullUri">The Uri to the resource metrics API. 
        ///                           E.g. for queues: "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageName}/queueServices/default"</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        Task<IEnumerable<MdmQueryResult>> GetResourceMetricsAsync(string resourceFullUri, QueryParameters queryProperties, CancellationToken cancellationToken);

        /// <summary>
        /// Get the resource metric values from MDM, based on the Azure Resource Service (For example: if the resource is a storage account, possible services are BLOB, Queue, and Table)
        /// </summary>
        /// <param name="azureResourceService">Specific azure resource for which we want to fetch metrics</param>
        /// <param name="queryProperties">MDM properties to be used when fetching data from MDM. All fields are optional</param>
        /// <param name="cancellationToken">Cancelation Token for the async operation</param>
        /// <returns>A <see cref="Task{TResult}"/> object that represents the asynchronous operation, returning the list metrics</returns>
        Task<IEnumerable<MdmQueryResult>> GetResourceMetricsAsync(ServiceType azureResourceService, QueryParameters queryProperties, CancellationToken cancellationToken);
    }
}