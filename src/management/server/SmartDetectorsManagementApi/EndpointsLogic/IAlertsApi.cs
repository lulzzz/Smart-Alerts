//-----------------------------------------------------------------------
// <copyright file="IAlertsApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;

    /// <summary>
    /// This class is the logic for the /alerts endpoint.
    /// </summary>
    public interface IAlertsApi
    {
        /// <summary>
        /// Gets all the Alerts.
        /// </summary>
        /// <param name="startTime">The query start time.</param>
        /// <param name="endTime">(optional) The query end time.</param>
        /// <param name="cancellationToken">(optional) The cancellation token.</param>
        /// <returns>The Alerts response.</returns>
        Task<ListAlertsResponse> GetAllAlertsAsync(DateTime startTime, DateTime? endTime = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
