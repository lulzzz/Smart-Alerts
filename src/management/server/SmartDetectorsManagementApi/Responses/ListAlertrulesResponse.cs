//-----------------------------------------------------------------------
// <copyright file="ListAlertRulesResponse.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses
{ 
    using System.Collections.Generic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the GET alert rules operation.
    /// </summary>
    public class ListAlertRulesResponse
    {
        /// <summary>
        /// Gets or sets the alert rules
        /// </summary>
        [JsonProperty("alertRules")]
        public List<AlertRuleApiEntity> AlertRules { get; set; }
    }
}
