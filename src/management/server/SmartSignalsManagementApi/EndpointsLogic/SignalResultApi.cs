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
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.AIClient;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.AzureStorage;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

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
        private readonly ICloudBlobContainerWrapper signalResultStorageContainer;
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultApi"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The storage provider factory.</param>
        /// <param name="applicationInsightsClientFactory">The Application Insights client factory.</param>
        /// <param name="tracer">The tracer.</param>
        public SignalResultApi(ICloudStorageProviderFactory storageProviderFactory, IApplicationInsightsClientFactory applicationInsightsClientFactory, ITracer tracer)
        {
            this.signalResultStorageContainer = storageProviderFactory.GetSmartSignalResultStorageContainer();
            this.applicationInsightsClient = applicationInsightsClientFactory.GetApplicationInsightsClient();
            this.tracer = tracer;
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
                IEnumerable<string> blobsContent = await Task.WhenAll(signalResultsBlobsUri.Select(blobUri => this.GetBlobContentWithoutThrowing(new CloudBlockBlob(new Uri(blobUri)).Name)));

                // Remove blobs with empty content
                blobsContent = blobsContent.Where(content => !string.IsNullOrWhiteSpace(content));

                // Deserialize the blobs content to alert
                IEnumerable<AlertPresentation> alerts = blobsContent.Select(JsonConvert.DeserializeObject<AlertPresentation>);

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

        /// <summary>
        /// Download a blob from the signal result container without throwing in case of <see cref="StorageException"/>.
        /// We are only logging the exception and not throwing because we don't want the user not getting any signal in case there is 
        /// an issue with Azure Storage.
        /// </summary>
        /// <param name="blobName">The blob's name.</param>
        /// <returns>The blob's content.</returns>
        private async Task<string> GetBlobContentWithoutThrowing(string blobName)
        {
            try
            {
                return await this.signalResultStorageContainer.DownloadBlobContentAsync(blobName);
            }
            catch (StorageException e)
            {
                this.tracer.ReportException(e);
                this.tracer.TraceError($"Failed to download blob ${blobName} due to exception: {e}");

                return null;
            }
        }
    }
}