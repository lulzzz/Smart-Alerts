//-----------------------------------------------------------------------
// <copyright file="IAuthorizationManagementClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.FunctionApp.Authorization
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This interface is responsible to manage authorization for the Smart Detectors Monitoring Appliance
    /// </summary>
    public interface IAuthorizationManagementClient
    {
        /// <summary>
        /// Verifying if the HTTP request message is authorized to access the Smart Detectors Monitoring Appliance
        /// </summary>
        /// <param name="req">The HTTP request message</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the request is authorized to access the Smart Detectors Monitoring Appliance, false otherwise</returns>
        Task<bool> IsAuthorizedAsync(HttpRequestMessage req, CancellationToken cancellationToken);
    }
}
