//-----------------------------------------------------------------------
// <copyright file="ScheduleFlow.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.SignalRunTracker;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class is responsible for discovering which signal should be executed and sends them to the analysis flow
    /// </summary>
    public class ScheduleFlow
    {
        private readonly ITracer tracer;
        private readonly IAlertRuleStore alertRulesStore;
        private readonly ISignalRunsTracker signalRunsTracker;
        private readonly IAnalysisExecuter analysisExecuter;
        private readonly ISmartSignalResultPublisher smartSignalResultPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFlow"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="alertRulesStore">The alert rules store repository</param>
        /// <param name="signalRunsTracker">The signal run tracker</param>
        /// <param name="analysisExecuter">The analysis executer instance</param>
        /// <param name="smartSignalResultPublisher">The signal results publisher instance</param>
        public ScheduleFlow(
            ITracer tracer,
            IAlertRuleStore alertRulesStore,
            ISignalRunsTracker signalRunsTracker,
            IAnalysisExecuter analysisExecuter,
            ISmartSignalResultPublisher smartSignalResultPublisher)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.alertRulesStore = Diagnostics.EnsureArgumentNotNull(() => alertRulesStore);
            this.signalRunsTracker = Diagnostics.EnsureArgumentNotNull(() => signalRunsTracker);
            this.analysisExecuter = Diagnostics.EnsureArgumentNotNull(() => analysisExecuter);
            this.smartSignalResultPublisher = Diagnostics.EnsureArgumentNotNull(() => smartSignalResultPublisher);
        }

        /// <summary>
        /// Starting point of the schedule flow
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task RunAsync()
        {
            IList<AlertRule> alertRules = await this.alertRulesStore.GetAllAlertRulesAsync();
            IList<SignalExecutionInfo> signalsToRun = await this.signalRunsTracker.GetSignalsToRunAsync(alertRules);

            foreach (SignalExecutionInfo signalExecution in signalsToRun)
            {
                try
                {
                    IList<ContractsAlert> alerts = await this.analysisExecuter.ExecuteSignalAsync(signalExecution, new List<string> { signalExecution.AlertRule.ResourceId });
                    this.tracer.TraceInformation($"Found {alerts.Count} alerts");
                    await this.smartSignalResultPublisher.PublishSignalResultItemsAsync(signalExecution.AlertRule.SignalId, alerts);
                    await this.signalRunsTracker.UpdateSignalRunAsync(signalExecution);
                }
                catch (Exception exception)
                {
                    this.tracer.TraceError($"Failed executing signal {signalExecution.AlertRule.SignalId} with exception: {exception}");
                    this.tracer.ReportException(exception);
                }
            }
        }
    }
}
