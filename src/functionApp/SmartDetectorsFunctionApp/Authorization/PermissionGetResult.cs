//-----------------------------------------------------------------------
// <copyright file="PermissionGetResult.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp.Authorization
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the permissions information. This is the result from the RBAC GET API.
    /// </summary>
    public class PermissionGetResult
    {
        /// <summary>
        /// Gets or sets the role definition permissions
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public IList<Permission> Permissions { get; set; }
    }
}
