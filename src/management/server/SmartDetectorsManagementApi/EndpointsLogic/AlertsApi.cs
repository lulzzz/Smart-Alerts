//-----------------------------------------------------------------------
// <copyright file="AlertsApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class contains the logic for the /alerts endpoint.
    /// </summary>
    public class AlertsApi : IAlertsApi
    {
        /// <summary>
        /// The event name in the Alerts store (Application Insights) that contains the Alerts
        /// </summary>
        private const string EventName = "Alerts";

        private readonly IApplicationInsightsClient applicationInsightsClient;
        private readonly ICloudBlobContainerWrapper alertsStorageContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsApi"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The storage provider factory.</param>
        /// <param name="applicationInsightsClientFactory">The Application Insights client factory.</param>
        public AlertsApi(ICloudStorageProviderFactory storageProviderFactory, IApplicationInsightsClientFactory applicationInsightsClientFactory)
        {
            this.alertsStorageContainer = storageProviderFactory.GetAlertsStorageContainer();
            this.applicationInsightsClient = applicationInsightsClientFactory.GetApplicationInsightsClient();
        }

        /// <summary>
        /// Gets all the Alerts.
        /// </summary>
        /// <param name="startTime">The query start time.</param>
        /// <param name="endTime">The query end time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Alerts response.</returns>
        public async Task<ListAlertsResponse> GetAllAlertsAsync(DateTime startTime, DateTime? endTime = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Get the custom events from 
                IEnumerable<ApplicationInsightsEvent> events = await this.applicationInsightsClient.GetCustomEventsAsync(EventName, startTime, endTime, cancellationToken);

                // Take all the blobs uris that contains the Alerts
                IEnumerable<string> alertsBlobsUri = events.Where(result => result.CustomDimensions.ContainsKey("AlertBlobUri"))
                                                                  .Select(result => result.CustomDimensions["AlertBlobUri"]);

                // Get the blobs content (as we are getting blob uri, we are creating new CloudBlockBlob for each and extracting the blob name 
                var blobsContent = await Task.WhenAll(alertsBlobsUri.Select(blobUri => this.alertsStorageContainer
                                                                                              .DownloadBlobContentAsync(new CloudBlockBlob(new Uri(blobUri)).Name)));
                
                // Deserialize the blobs content to alert
                IEnumerable<ContractsAlert> alerts = blobsContent.Select(JsonConvert.DeserializeObject<ContractsAlert>);

                return new ListAlertsResponse
                {
                    Alerts = alerts.ToList()
                };
            }
            catch (ApplicationInsightsClientException e)
            {
                throw new SmartDetectorsManagementApiException("Failed to query Alerts due to an exception from Application Insights", e, HttpStatusCode.InternalServerError);
            }
            catch (JsonException e)
            {
                throw new SmartDetectorsManagementApiException("Failed to de-serialize Alerts", e, HttpStatusCode.InternalServerError);
            }
            catch (StorageException e)
            {
                throw new SmartDetectorsManagementApiException("Failed to get Alerts from storage", e, HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new SmartDetectorsManagementApiException("Failed to get Alerts from storage", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}