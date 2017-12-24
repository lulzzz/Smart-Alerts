﻿namespace Microsoft.Azure.Monitoring.SmartSignals.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request for a smart signal execution
    /// </summary>
    public class SmartSignalRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the signal</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="lastExecutionTime">The last execution of the signal</param>
        /// <param name="cadence">The signal configured cadence</param>
        /// <param name="settings">The analysis settings</param>
        public SmartSignalRequest(IList<string> resourceIds, string signalId, DateTime? lastExecutionTime, TimeSpan cadence, SmartSignalSettings settings)
        {
            this.ResourceIds = resourceIds;
            this.SignalId = signalId;
            this.LastExecutionTime = lastExecutionTime;
            this.Settings = settings;
            this.Cadence = cadence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRequest"/> class
        /// </summary>
        /// <param name="resourceIds">The resource IDs on which to run the signal</param>
        /// <param name="signalId">The signal ID</param>
        /// <param name="analysisStartTime">The start of the time range for analysis</param>
        /// <param name="analysisEndTime">The end of the time range for analysis</param>
        /// <param name="settings">The analysis settings</param>
        public SmartSignalRequest(IList<string> resourceIds, string signalId, DateTime analysisStartTime, DateTime analysisEndTime, SmartSignalSettings settings)
        {
            // TODO: Remove this constructor once SDK will work with execution time and cadence
            this.ResourceIds = resourceIds;
            this.SignalId = signalId;
            this.Settings = settings;
            this.AnalysisStartTime = analysisStartTime;
            this.AnalysisEndTime = analysisEndTime;
        }

        /// <summary>
        /// Gets the resource IDs on which to run the signal
        /// </summary>
        public IList<string> ResourceIds { get; }

        /// <summary>
        /// Gets the signal ID
        /// </summary>
        public string SignalId { get; }

        /// <summary>
        /// Gets the last execution time
        /// </summary>
        public DateTime? LastExecutionTime { get; }

        /// <summary>
        /// Gets the signal configured cadence
        /// </summary>
        public TimeSpan Cadence { get; }

        /// <summary>
        /// Gets the start of the time range for analysis
        /// TODO: Delete this after SDK will work with execution time and cadence
        /// </summary>
        public DateTime AnalysisStartTime { get; }

        /// <summary>
        /// Gets the end time of the analysis
        /// TODO: Delete this after SDK will work with execution time and cadence
        /// </summary>
        public DateTime AnalysisEndTime { get; }

        /// <summary>
        /// Gets the analysis settings
        /// </summary>
        public SmartSignalSettings Settings { get; }
    }
}
