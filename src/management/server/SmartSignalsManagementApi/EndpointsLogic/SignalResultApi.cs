//-----------------------------------------------------------------------
// <copyright file="SignalResultApi.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class contains the logic for the /signalResult endpoint.
    /// </summary>
    public class SignalResultApi : ISignalResultApi
    {
        /// <summary>
        /// The event name in the Signal Result store (Application Insights) that contains the signal result
        /// </summary>
        private const string EventName = "SmartSignalResult";

        private readonly IApplicationInsightsClient applicationInsightsClient;
        private readonly ICloudBlobContainerWrapper alertsStorageContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultApi"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The storage provider factory.</param>
        /// <param name="applicationInsightsClientFactory">The Application Insights client factory.</param>
        public SignalResultApi(ICloudStorageProviderFactory storageProviderFactory, IApplicationInsightsClientFactory applicationInsightsClientFactory)
        {
            this.alertsStorageContainer = storageProviderFactory.GetAlertsStorageContainer();
            this.applicationInsightsClient = applicationInsightsClientFactory.GetApplicationInsightsClient();
        }

        /// <summary>
        /// Gets all the Smart Signals results.
        /// </summary>
        /// <param name="startTime">The query start time.</param>
        /// <param name="endTime">The query end time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Smart Signals results response.</returns>
        public async Task<ListSmartSignalsResultsResponse> GetAllSmartSignalResultsAsync(DateTime startTime, DateTime? endTime = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Get the custom events from 
                IEnumerable<ApplicationInsightsEvent> events = await this.applicationInsightsClient.GetCustomEventsAsync(EventName, startTime, endTime, cancellationToken);

                // Take all the blobs uris that contains the signals results items
                IEnumerable<string> signalResultsBlobsUri = events.Where(result => result.CustomDimensions.ContainsKey("ResultItemBlobUri"))
                                                                  .Select(result => result.CustomDimensions["ResultItemBlobUri"]);

                // Get the blobs content (as we are getting blob uri, we are creating new CloudBlockBlob for each and extracting the blob name 
                var blobsContent = await Task.WhenAll(signalResultsBlobsUri.Select(blobUri => this.alertsStorageContainer
                                                                                              .DownloadBlobContentAsync(new CloudBlockBlob(new Uri(blobUri)).Name)));
                
                // Deserialize the blobs content to alert
                IEnumerable<ContractsAlert> alerts = blobsContent.Select(JsonConvert.DeserializeObject<ContractsAlert>);

                return new ListSmartSignalsResultsResponse
                {
                    Alerts = alerts.ToList()
                };
            }
            catch (ApplicationInsightsClientException e)
            {
                throw new SmartSignalsManagementApiException("Failed to query smart signals results due to an exception from Application Insights", e, HttpStatusCode.InternalServerError);
            }
            catch (JsonException e)
            {
                throw new SmartSignalsManagementApiException("Failed to de-serialize signals results items", e, HttpStatusCode.InternalServerError);
            }
            catch (StorageException e)
            {
                throw new SmartSignalsManagementApiException("Failed to get signals results items from storage", e, HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                throw new SmartSignalsManagementApiException("Failed to get signals results items from storage", e, HttpStatusCode.InternalServerError);
            }
        }
    }
}