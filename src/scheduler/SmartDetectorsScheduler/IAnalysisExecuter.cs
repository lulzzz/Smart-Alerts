//-----------------------------------------------------------------------
// <copyright file="IAnalysisExecuter.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An interface responsible for executing Smart Detectors via the analysis flow
    /// </summary>
    public interface IAnalysisExecuter
    {
        /// <summary>
        /// Executes the Smart Detector via the analysis flow
        /// </summary>
        /// <param name="smartDetectorExecutionInfo">The Smart Detector execution information</param>
        /// <param name="resourceIds">The resource IDs used by the Smart Detector</param>
        /// <returns>A list of Alerts</returns>
        Task<IList<ContractsAlert>> ExecuteSmartDetectorAsync(SmartDetectorExecutionInfo smartDetectorExecutionInfo, IList<string> resourceIds);
    }
}
