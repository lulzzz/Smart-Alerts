//-----------------------------------------------------------------------
// <copyright file="TrackSmartDetectorRunEntity.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.SmartDetectorRunTracker
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the last successful run of a Smart Detector job.
    /// The rule ID is the row key.
    /// </summary>
    public class TrackSmartDetectorRunEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the Smart Detector ID
        /// </summary>
        public string SmartDetectorId { get; set; }

        /// <summary>
        /// Gets or sets the last successful run execution time
        /// </summary>
        public DateTime LastSuccessfulExecutionTime { get; set; }
    }
}
