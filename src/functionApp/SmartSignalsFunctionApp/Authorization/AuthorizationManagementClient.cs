//-----------------------------------------------------------------------
// <copyright file="AuthorizationManagementClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.FunctionApp.Authorization
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals.Clients;
    using Microsoft.Azure.Monitoring.SmartSignals.Extensions;
    using Microsoft.Azure.Monitoring.SmartSignals.Tools;
    using Newtonsoft.Json;
    using Polly;

    /// <summary>
    /// This class is responsible to manage authorization for the SiRA
    /// </summary>
    public class AuthorizationManagementClient : IAuthorizationManagementClient
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "RBAC";

        private readonly ITracer tracer;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationManagementClient"/> class.
        /// </summary>
        /// <param name="tracer">Log wrapper</param>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        public AuthorizationManagementClient(ITracer tracer, IHttpClientWrapper httpClientWrapper)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);
        }

        /// <summary>
        /// Verifying if the HTTP request message is authorized to access the SiRA
        /// </summary>
        /// <param name="req">The HTTP request message</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the client is authorized to access the SiRA, false otherwise</returns>
        public async Task<bool> IsAuthorizedAsync(HttpRequestMessage req, CancellationToken cancellationToken)
        {
            string jwtToken = req.Headers.Authorization?.Parameter;
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                this.tracer.TraceWarning("No JWT Token was provided");
                return false;
            }

            string subscriptionId = ConfigurationReader.ReadConfig("SubscriptionId", true);
            string resourceGroupName = ConfigurationReader.ReadConfig("ResourceGroupName", true);
            string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/Microsoft.Authorization/permissions?api-version=2015-07-01";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            // Add the authentication header
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            HttpResponseMessage response = await this.retryPolicy.RunAndTrackDependencyAsync(this.tracer, DependencyName, "Permissions", () => this.httpClientWrapper.SendAsync(requestMessage, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    this.tracer.TraceWarning("The client doesn't have access to the resource group");
                }
                else
                {
                    string content = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                    var message = $"Failed trying to get access permissions. Fail StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}. Content: {content}.";
                    this.tracer.TraceError(message);
                }

                return false;
            }

            string permissionsString = await response.Content.ReadAsStringAsync();
            PermissionGetResult permissionGetResult = JsonConvert.DeserializeObject<PermissionGetResult>(permissionsString);

            if (permissionGetResult.Permissions == null || !permissionGetResult.Permissions.Any() || !permissionGetResult.Permissions.Any(p => p.Actions.Any()))
            {
                this.tracer.TraceWarning("No permissions were found for the token");
                return false;
            }

            // For now, for every permission action allowed on the resource group we allow accessing the SiRA.
            return true;
        }
    }
}
