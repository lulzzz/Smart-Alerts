//-----------------------------------------------------------------------
// <copyright file="AlertRule.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds an alert rule
    /// </summary>
    public class AlertRule
    {
        /// <summary>
        /// Gets or sets the rule ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the alert rule name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alert rule description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector ID.
        /// </summary>
        public string SmartDetectorId { get; set; }

        /// <summary>
        /// Gets or sets the resource to be analyzed by the Smart Detector.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector's execution cadence.
        /// </summary>
        public TimeSpan Cadence { get; set; }

        /// <summary>
        /// Gets or sets the email recipients for the alerts
        /// </summary>
        public IList<string> EmailRecipients { get; set; }
    }
}