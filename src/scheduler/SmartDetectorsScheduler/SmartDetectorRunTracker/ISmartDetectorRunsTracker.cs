//-----------------------------------------------------------------------
// <copyright file="ISmartDetectorRunsTracker.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler;

    /// <summary>
    /// Tracking the Smart Detector job runs - Responsible to determine whether the Smart Detector job should run.
    /// Gets the last run for each Smart Detector from the tracking table and updates it after a successful run.
    /// </summary>
    public interface ISmartDetectorRunsTracker
    {
        /// <summary>
        /// Returns the execution information of the Smart Detectors that needs to be executed based on their last execution times and the alert rules
        /// </summary>
        /// <param name="alertRules">The alert rules</param>
        /// <returns>A list of Smart Detector execution times of the detectors to execute</returns>
        Task<IList<SmartDetectorExecutionInfo>> GetSmartDetectorsToRunAsync(IEnumerable<AlertRule> alertRules);

        /// <summary>
        /// Updates a successful run in the tracking table.
        /// </summary>
        /// <param name="smartDetectorExecutionInfo">The current Smart Detector execution information</param>
        /// <returns>A <see cref="Task"/> running the asynchronous operation</returns>
        Task UpdateSmartDetectorRunAsync(SmartDetectorExecutionInfo smartDetectorExecutionInfo);
    }
}
