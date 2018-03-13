//-----------------------------------------------------------------------
// <copyright file="SignalRunsTracker.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.SignalRunTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Tracking the signal job runs - Responsible to determine whether the signal job should run.
    /// Gets the last run for each signal from the tracking table and updates it after a successful run.
    /// </summary>
    public class SignalRunsTracker : ISignalRunsTracker
    {
        private const string TableName = "signaltracking";
        private const string PartitionKey = "tracking";

        private readonly ICloudTableWrapper trackingTable;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the<see cref="SignalRunsTracker"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The Azure Storage provider factory</param>
        /// <param name="tracer">Log wrapper</param>
        public SignalRunsTracker(ICloudStorageProviderFactory storageProviderFactory, ITracer tracer)
        {
            this.tracer = tracer;

            // create the cloud table instance
            ICloudTableClientWrapper tableClient = storageProviderFactory.GetSmartDetectorStorageTableClient();
            this.trackingTable = tableClient.GetTableReference(TableName);
            this.trackingTable.CreateIfNotExists();
        }

        /// <summary>
        /// Returns the execution information of the signals that needs to be executed based on their last execution times and the alert rules
        /// </summary>
        /// <param name="alertRules">The alert rules</param>
        /// <returns>A list of signal execution times of the signals to execute</returns>
        public async Task<IList<SignalExecutionInfo>> GetSignalsToRunAsync(IEnumerable<AlertRule> alertRules)
        {
            this.tracer.TraceVerbose("getting signals to run");

            // get all last signal runs from table storage
            var signalsLastRuns = await this.trackingTable.ReadPartitionAsync<TrackSignalRunEntity>(PartitionKey);

            // create a dictionary from rule ID to signal execution for faster lookup
            var ruleIdToLastRun = signalsLastRuns.ToDictionary(x => x.RowKey, x => x);

            // for each rule check if needs to run based on its cadence and its last execution time
            var signalsToRun = new List<SignalExecutionInfo>();
            foreach (var alertRule in alertRules)
            {
                bool signalWasExecutedBefore = ruleIdToLastRun.TryGetValue(alertRule.Id, out TrackSignalRunEntity ruleLastRun);
                DateTime lastExecutionTime = signalWasExecutedBefore ? ruleLastRun.LastSuccessfulExecutionTime : DateTime.MinValue;
                DateTime signalNextRun = lastExecutionTime.Add(alertRule.Cadence);
                if (signalNextRun <= DateTime.UtcNow)
                {
                    this.tracer.TraceInformation($"rule {alertRule.Id} for smart detector {alertRule.SmartDetectorId} is marked to run");
                    signalsToRun.Add(new SignalExecutionInfo
                    {
                        AlertRule = alertRule,
                        LastExecutionTime = ruleLastRun?.LastSuccessfulExecutionTime,
                        CurrentExecutionTime = DateTime.UtcNow
                    });
                }
            }

            return signalsToRun;
        }

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="signalExecutionInfo">The current signal execution information</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation</returns>
        public async Task UpdateSignalRunAsync(SignalExecutionInfo signalExecutionInfo)
        {
            // Execute the update operation
            this.tracer.TraceVerbose($"updating run for: {signalExecutionInfo.AlertRule.Id}");
            var operation = TableOperation.InsertOrReplace(new TrackSignalRunEntity
            {
                PartitionKey = PartitionKey,
                RowKey = signalExecutionInfo.AlertRule.Id,
                SignalId = signalExecutionInfo.AlertRule.SmartDetectorId,
                LastSuccessfulExecutionTime = signalExecutionInfo.CurrentExecutionTime
            });
            await this.trackingTable.ExecuteAsync(operation, CancellationToken.None);
        }
    }
}
