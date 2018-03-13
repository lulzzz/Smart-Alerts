//-----------------------------------------------------------------------
// <copyright file="ISmartSignalResultPublisher.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An interface for publishing Smart Signal results
    /// </summary>
    public interface ISmartSignalResultPublisher
    {
        /// <summary>
        /// Publish Smart Signal result items as events to Application Insights
        /// </summary>
        /// <param name="signalId">The signal ID</param>
        /// <param name="alerts">The Alerts to publish</param>
        /// <returns>A <see cref="Task"/> object, running the current operation</returns>
        Task PublishSignalResultItemsAsync(string signalId, IList<ContractsAlert> alerts);
    }
}
