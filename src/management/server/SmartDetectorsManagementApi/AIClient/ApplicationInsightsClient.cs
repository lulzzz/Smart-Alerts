//-----------------------------------------------------------------------
// <copyright file="ApplicationInsightsClient.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is responsible for querying Application Insights via Rest API
    /// </summary>
    public class ApplicationInsightsClient : IApplicationInsightsClient
    {
        /// <summary>
        /// The Application Insights resource id (used for getting credentials)
        /// </summary>
        private const string ApplicationInsightsResource = "https://api.applicationinsights.io";

        private readonly IHttpClientWrapper httpClient;
        private readonly ServiceClientCredentials credentials;
        private readonly string applicationId;
        private readonly Uri applicationInsightUri = new Uri(ApplicationInsightsResource);

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClient"/> class.
        /// </summary>
        /// <param name="applicationId">The AI application id.</param>
        /// <param name="credentialsFactory">The credentials factory (for AI authentication)</param>
        public ApplicationInsightsClient(string applicationId, ICredentialsFactory credentialsFactory)
        {
            this.applicationId = applicationId;
            this.credentials = credentialsFactory.Create(ApplicationInsightsResource);
            this.httpClient = new HttpClientWrapper();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsClient"/> class.
        /// We are using this constructor for UTs.
        /// </summary>
        /// <param name="applicationId">The AI application id.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="credentialsFactory">The credentials factory (for AI authentication)</param>
        internal ApplicationInsightsClient(string applicationId, IHttpClientWrapper httpClient, ICredentialsFactory credentialsFactory)
        {
            this.applicationId = applicationId;
            this.httpClient = httpClient;
            this.credentials = credentialsFactory.Create(ApplicationInsightsResource);
        }

        /// <summary>
        /// Gets all the custom events from Application Insights for the configured application by the given filtering.
        /// </summary>
        /// <param name="eventName">The custom event name.</param>
        /// <param name="startTime">(optional) filtering by start time.</param>
        /// <param name="endTime">(optional) filtering by end time.</param>
        /// <param name="cancellationToken">(optional) The cancellation token.</param>
        /// <returns>The Application Insights events.</returns>
        public async Task<IEnumerable<ApplicationInsightsEvent>> GetCustomEventsAsync(
            string eventName,
            DateTime? startTime = null,
            DateTime? endTime = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Diagnostics.EnsureStringNotNullOrWhiteSpace(() => eventName);
            Diagnostics.EnsureArgument(startTime.HasValue && endTime.HasValue ? startTime <= endTime : true, () => startTime, "End time must be after start time");

            try
            {
                var appInsightsRelativeUrl = $"/v1/apps/{this.applicationId}/events/customEvents";

                // Filter by event name
                appInsightsRelativeUrl += $"?$filter=customEvent/name eq '{eventName}'";

                // Add timestamp filters in case it's required
                if (startTime.HasValue && endTime.HasValue)
                {
                    appInsightsRelativeUrl += $" and timestamp ge {startTime.Value.ToQueryTimeFormat()} and timestamp le {endTime.Value.ToQueryTimeFormat()}";
                }
                else if (startTime.HasValue)
                {
                    appInsightsRelativeUrl += $" and timestamp ge {startTime.Value.ToQueryTimeFormat()}";
                }
                else if (endTime.HasValue)
                {
                    appInsightsRelativeUrl += $" and timestamp le {endTime.Value.ToQueryTimeFormat()}";
                }

                // Send the AI Rest API request
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(this.applicationInsightUri, appInsightsRelativeUrl)))
                {
                    // Add the authorization token
                    await this.credentials.ProcessHttpRequestAsync(httpRequestMessage, cancellationToken);

                    // Add the authorization token
                    using (HttpResponseMessage response = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken))
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new ApplicationInsightsClientException($"Failed to query AI endpoint. Status code: {response.StatusCode}, response: {responseContent}");
                        }
                        
                        JObject appInsightsEventsAsJson = JObject.Parse(responseContent);

                        return JsonConvert.DeserializeObject<IEnumerable<ApplicationInsightsEvent>>(appInsightsEventsAsJson["value"].ToString());
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new ApplicationInsightsClientException("Failed to query AI endpoint", e);
            }
            catch (JsonException e)
            {
                throw new ApplicationInsightsClientException($"Failed to de-serialize the returned AI data", e);
            }
        }
    }
}
