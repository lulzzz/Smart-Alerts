//-----------------------------------------------------------------------
// <copyright file="ScheduleFlow.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class is responsible for discovering which Smart Detector should be executed and sends them to the analysis flow
    /// </summary>
    public class ScheduleFlow
    {
        private readonly ITracer tracer;
        private readonly IAlertRuleStore alertRulesStore;
        private readonly ISmartDetectorRunsTracker smartDetectorRunsTracker;
        private readonly IAnalysisExecuter analysisExecuter;
        private readonly IAlertsPublisher alertsPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFlow"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="alertRulesStore">The alert rules store repository</param>
        /// <param name="smartDetectorRunsTracker">The Smart Detector run tracker</param>
        /// <param name="analysisExecuter">The analysis executer instance</param>
        /// <param name="alertsPublisher">The Alerts publisher instance</param>
        public ScheduleFlow(
            ITracer tracer,
            IAlertRuleStore alertRulesStore,
            ISmartDetectorRunsTracker smartDetectorRunsTracker,
            IAnalysisExecuter analysisExecuter,
            IAlertsPublisher alertsPublisher)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.alertRulesStore = Diagnostics.EnsureArgumentNotNull(() => alertRulesStore);
            this.smartDetectorRunsTracker = Diagnostics.EnsureArgumentNotNull(() => smartDetectorRunsTracker);
            this.analysisExecuter = Diagnostics.EnsureArgumentNotNull(() => analysisExecuter);
            this.alertsPublisher = Diagnostics.EnsureArgumentNotNull(() => alertsPublisher);
        }

        /// <summary>
        /// Starting point of the schedule flow
        /// </summary>
        /// <returns>A <see cref="Task"/> running the asynchronous operation.</returns>
        public async Task RunAsync()
        {
            IList<AlertRule> alertRules = await this.alertRulesStore.GetAllAlertRulesAsync();
            IList<SmartDetectorExecutionInfo> smartDetectorsToRun = await this.smartDetectorRunsTracker.GetSmartDetectorsToRunAsync(alertRules);

            foreach (SmartDetectorExecutionInfo smartDetectorExecution in smartDetectorsToRun)
            {
                try
                {
                    IList<ContractsAlert> alerts = await this.analysisExecuter.ExecuteSmartDetectorAsync(smartDetectorExecution, new List<string> { smartDetectorExecution.AlertRule.ResourceId });
                    this.tracer.TraceInformation($"Found {alerts.Count} alerts");
                    await this.alertsPublisher.PublishAlertsAsync(smartDetectorExecution.AlertRule.SmartDetectorId, alerts);
                    await this.smartDetectorRunsTracker.UpdateSmartDetectorRunAsync(smartDetectorExecution);
                }
                catch (Exception exception)
                {
                    this.tracer.TraceError($"Failed executing smart detector {smartDetectorExecution.AlertRule.SmartDetectorId} with exception: {exception}");
                    this.tracer.ReportException(exception);
                }
            }
        }
    }
}
