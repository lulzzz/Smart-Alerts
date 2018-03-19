//-----------------------------------------------------------------------
// <copyright file="AlertRuleApiEntity.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the returned model of the /alertRule endpoint.
    /// </summary>
    public class AlertRuleApiEntity
    {
        /// <summary>
        /// Gets or sets the alert rule name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alert rule description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector ID.
        /// </summary>
        [JsonProperty("smartDetectorId")]
        public string SmartDetectorId { get; set; }

        /// <summary>
        /// Gets or sets the resource to be analyzed by the Smart Detector.
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the Smart Detector's execution cadence in minutes.
        /// </summary>
        [JsonProperty("cadenceInMinutes")]
        public int CadenceInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the email recipients for the Alerts
        /// </summary>
        [JsonProperty("emailRecipients")]
        public IList<string> EmailRecipients { get; set; }
    }
}
