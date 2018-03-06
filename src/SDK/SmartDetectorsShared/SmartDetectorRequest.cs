//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRequest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request for a Smart Detector execution
    /// </summary>
    public class SmartDetectorRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the Smart Detector</param>
        /// <param name="smartDetectorId">The Smart Detector ID</param>
        /// <param name="lastExecutionTime">The last execution of the Smart Detector. This can be null if the Smart Detector never ran.</param>
        /// <param name="cadence">The Smart Detector configured cadence</param>
        /// <param name="settings">The analysis settings</param>
        public SmartDetectorRequest(IList<string> resourceIds, string smartDetectorId, DateTime? lastExecutionTime, TimeSpan cadence, SmartDetectorSettings settings)
        {
            this.ResourceIds = resourceIds;
            this.SmartDetectorId = smartDetectorId;
            this.LastExecutionTime = lastExecutionTime;
            this.Settings = settings;
            this.Cadence = cadence;
        }

        /// <summary>
        /// Gets the resource IDs on which to run the Smart Detector
        /// </summary>
        public IList<string> ResourceIds { get; }

        /// <summary>
        /// Gets the Smart Detector ID
        /// </summary>
        public string SmartDetectorId { get; }

        /// <summary>
        /// Gets the last execution time
        /// </summary>
        public DateTime? LastExecutionTime { get; }

        /// <summary>
        /// Gets the Smart Detector configured cadence
        /// </summary>
        public TimeSpan Cadence { get; }

        /// <summary>
        /// Gets the analysis settings
        /// </summary>
        public SmartDetectorSettings Settings { get; }
    }
}
