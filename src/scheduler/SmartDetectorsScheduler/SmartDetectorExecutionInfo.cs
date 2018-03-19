//-----------------------------------------------------------------------
// <copyright file="SmartDetectorExecutionInfo.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;

    /// <summary>
    /// Smart Detector execution information
    /// </summary>
    public class SmartDetectorExecutionInfo
    {
        /// <summary>
        /// Gets or sets the alert rule of the execution
        /// </summary>
        public AlertRule AlertRule { get; set; }
        
        /// <summary>
        /// Gets or sets the current execution time
        /// </summary>
        public DateTime CurrentExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the last execution time
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }
    }
}
