//-----------------------------------------------------------------------
// <copyright file="AlertRuleEntity.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A row holds the alert rule for the Smart Detector.
    /// The rule ID is the row key.
    /// </summary>
    public class AlertRuleEntity : TableEntity
    {
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
        /// Gets or sets the Smart Detector's execution cadence in minutes.
        /// </summary>
        public int CadenceInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the email recipients for the alerts
        /// </summary>
        public string EmailRecipients { get; set; }
    }
}
