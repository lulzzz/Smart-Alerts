//-----------------------------------------------------------------------
// <copyright file="AnalysisRequest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;

    /// <summary>
    /// Represents a single analysis request sent to the Smart Detector. This is the main parameter sent to the 
    /// <see cref="ISmartDetector.AnalyzeResourcesAsync"/> method.
    /// </summary>
    public struct AnalysisRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisRequest"/> struct.
        /// </summary>
        /// <param name="targetResources">The list of resource identifiers to analyze.</param>
        /// <param name="dataEndTime">The data end time to query</param>
        /// <param name="analysisCadence">The analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.</param>
        /// <param name="alertRuleResourceId">The alert rule resource ID.</param>
        /// <param name="analysisServicesFactory">The analysis services factory to be used for querying the resources telemetry.</param>
        /// <param name="stateRepository">The persistent state repository for storing state between analysis runs</param>
        public AnalysisRequest(List<ResourceIdentifier> targetResources, DateTime dataEndTime, TimeSpan analysisCadence, string alertRuleResourceId, IAnalysisServicesFactory analysisServicesFactory, IStateRepository stateRepository)
        {
            // Parameter validations
            if (targetResources == null)
            {
                throw new ArgumentNullException(nameof(targetResources));
            }
            else if (!targetResources.Any())
            {
                throw new ArgumentException("Analysis request must have at least one target resource", nameof(targetResources));
            }

            if (dataEndTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Data end time must be specified in UTC", nameof(dataEndTime));
            }
            else if (dataEndTime >= DateTime.UtcNow)
            {
                throw new ArgumentException("Data end time cannot be in the future", nameof(dataEndTime));
            }

            if (analysisCadence <= TimeSpan.Zero)
            {
                throw new ArgumentException("Analysis cadence must represent a positive time span", nameof(analysisCadence));
            }

            if (analysisServicesFactory == null)
            {
                throw new ArgumentNullException(nameof(analysisServicesFactory));
            }

            this.TargetResources = targetResources;
            this.DataEndTime = dataEndTime;
            this.AnalysisCadence = analysisCadence;
            this.AlertRuleResourceId = alertRuleResourceId;
            this.AnalysisServicesFactory = analysisServicesFactory;
            this.StateRepository = stateRepository;
        }

        /// <summary>
        /// Gets the list of resource identifiers to analyze.
        /// <para>
        /// The scope of analysis depends on the resource's type, so that for resources with types that represent 
        /// a container resource (such as <see cref="ResourceType.Subscription"/> or <see cref="ResourceType.ResourceGroup"/>),
        /// the Smart Detector is expected to analyze all relevant resources contained in that container.</para>
        /// </summary>
        public List<ResourceIdentifier> TargetResources { get; }

        /// <summary>
        /// Gets the data end time to query.
        /// </summary>
        public DateTime DataEndTime { get; }

        /// <summary>
        /// Gets the analysis cadence defined in the Alert Rule which initiated the Smart Detector's analysis.
        /// </summary>
        public TimeSpan AnalysisCadence { get; }

        /// <summary>
        /// Gets the alert rule resource ID.
        /// </summary>
        public string AlertRuleResourceId { get; }

        /// <summary>
        /// Gets the analysis services factory to be used for querying the resources telemetry.
        /// </summary>
        public IAnalysisServicesFactory AnalysisServicesFactory { get; }

        /// <summary>
        /// Gets the persistent state repository for storing state between analysis runs.
        /// </summary>
        public IStateRepository StateRepository { get; }
    }
}
