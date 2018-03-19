//-----------------------------------------------------------------------
// <copyright file="ListAlertsResponse.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class represents the GET Management API operation for listing alerts.
    /// </summary>
    public class ListAlertsResponse
    {
        /// <summary>
        /// Gets or sets the alerts list
        /// </summary>
        [JsonProperty("alerts")]
        public IList<ContractsAlert> Alerts { get; set; }
    }
}
