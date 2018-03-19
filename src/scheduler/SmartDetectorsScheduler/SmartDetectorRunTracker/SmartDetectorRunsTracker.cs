//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunsTracker.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Tracking the Smart Detector job runs - Responsible to determine whether the Smart Detector job should run.
    /// Gets the last run for each Smart Detector from the tracking table and updates it after a successful run.
    /// </summary>
    public class SmartDetectorRunsTracker : ISmartDetectorRunsTracker
    {
        private const string TableName = "signaltracking";
        private const string PartitionKey = "tracking";

        private readonly ICloudTableWrapper trackingTable;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the<see cref="SmartDetectorRunsTracker"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The Azure Storage provider factory</param>
        /// <param name="tracer">Log wrapper</param>
        public SmartDetectorRunsTracker(ICloudStorageProviderFactory storageProviderFactory, ITracer tracer)
        {
            this.tracer = tracer;

            // create the cloud table instance
            ICloudTableClientWrapper tableClient = storageProviderFactory.GetSmartDetectorStorageTableClient();
            this.trackingTable = tableClient.GetTableReference(TableName);
            this.trackingTable.CreateIfNotExists();
        }

        /// <summary>
        /// Returns the execution information of the Smart Detectors that needs to be executed based on their last execution times and the alert rules
        /// </summary>
        /// <param name="alertRules">The alert rules</param>
        /// <returns>A list of Smart Detector execution times of the detectors to execute</returns>
        public async Task<IList<SmartDetectorExecutionInfo>> GetSmartDetectorsToRunAsync(IEnumerable<AlertRule> alertRules)
        {
            this.tracer.TraceVerbose("getting Smart Detectors to run");

            // get all last Smart Detector runs from table storage
            var smartDetectorsLastRuns = await this.trackingTable.ReadPartitionAsync<TrackSmartDetectorRunEntity>(PartitionKey);

            // create a dictionary from rule ID to Smart Detector execution for faster lookup
            var ruleIdToLastRun = smartDetectorsLastRuns.ToDictionary(x => x.RowKey, x => x);

            // for each rule check if needs to run based on its cadence and its last execution time
            var smartDetectorsToRun = new List<SmartDetectorExecutionInfo>();
            foreach (var alertRule in alertRules)
            {
                bool smartDetectorWasExecutedBefore = ruleIdToLastRun.TryGetValue(alertRule.Id, out TrackSmartDetectorRunEntity ruleLastRun);
                DateTime lastExecutionTime = smartDetectorWasExecutedBefore ? ruleLastRun.LastSuccessfulExecutionTime : DateTime.MinValue;
                DateTime smartDetectorNextRun = lastExecutionTime.Add(alertRule.Cadence);
                if (smartDetectorNextRun <= DateTime.UtcNow)
                {
                    this.tracer.TraceInformation($"rule {alertRule.Id} for smart detector {alertRule.SmartDetectorId} is marked to run");
                    smartDetectorsToRun.Add(new SmartDetectorExecutionInfo
                    {
                        AlertRule = alertRule,
                        LastExecutionTime = ruleLastRun?.LastSuccessfulExecutionTime,
                        CurrentExecutionTime = DateTime.UtcNow
                    });
                }
            }

            return smartDetectorsToRun;
        }

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="smartDetectorExecutionInfo">The current Smart Detector execution information</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation</returns>
        public async Task UpdateSmartDetectorRunAsync(SmartDetectorExecutionInfo smartDetectorExecutionInfo)
        {
            // Execute the update operation
            this.tracer.TraceVerbose($"updating run for: {smartDetectorExecutionInfo.AlertRule.Id}");
            var operation = TableOperation.InsertOrReplace(new TrackSmartDetectorRunEntity
            {
                PartitionKey = PartitionKey,
                RowKey = smartDetectorExecutionInfo.AlertRule.Id,
                SmartDetectorId = smartDetectorExecutionInfo.AlertRule.SmartDetectorId,
                LastSuccessfulExecutionTime = smartDetectorExecutionInfo.CurrentExecutionTime
            });
            await this.trackingTable.ExecuteAsync(operation, CancellationToken.None);
        }
    }
}
