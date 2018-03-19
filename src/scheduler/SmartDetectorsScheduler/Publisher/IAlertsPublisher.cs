//-----------------------------------------------------------------------
// <copyright file="IAlertsPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Publisher
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An interface for publishing Alerts
    /// </summary>
    public interface IAlertsPublisher
    {
        /// <summary>
        /// Publish Alerts as events to Application Insights
        /// </summary>
        /// <param name="smartDetectorId">The Smart Detector ID</param>
        /// <param name="alerts">The Alerts to publish</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        Task PublishAlertsAsync(string smartDetectorId, IList<ContractsAlert> alerts);
    }
}
