//-----------------------------------------------------------------------
// <copyright file="AnalysisExecuter.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Newtonsoft.Json;
    using Polly;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// This class is responsible for executing Smart Detectors via the analysis flow
    /// </summary>
    public class AnalysisExecuter : IAnalysisExecuter
    {
        /// <summary>
        /// The dependency name, for telemetry
        /// </summary>
        private const string DependencyName = "Analysis";

        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ITracer tracer;
        private readonly string analysisUrl;
        private readonly Policy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisExecuter"/> class.
        /// </summary>
        /// <param name="httpClientWrapper">The HTTP client wrapper</param>
        /// <param name="tracer">Log wrapper</param>
        public AnalysisExecuter(IHttpClientWrapper httpClientWrapper, ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
            this.httpClientWrapper = Diagnostics.EnsureArgumentNotNull(() => httpClientWrapper);
            this.retryPolicy = PolicyExtensions.CreateDefaultPolicy(this.tracer, DependencyName);

            var functionAppBaseUrl = ConfigurationReader.ReadConfig("FunctionBaseUrl", true);
            this.analysisUrl = $"{functionAppBaseUrl}/api/Analyze";
        }

        /// <summary>
        /// Executes the Smart Detector via the analysis flow
        /// </summary>
        /// <param name="smartDetectorExecutionInfo">The Smart Detector execution information</param>
        /// <param name="resourceIds">The resource IDs used by the Smart Detector</param>
        /// <returns>A list of Alerts</returns>
        public async Task<IList<ContractsAlert>> ExecuteSmartDetectorAsync(SmartDetectorExecutionInfo smartDetectorExecutionInfo, IList<string> resourceIds)
        {
            var analysisRequest = new SmartDetectorExecutionRequest
            {
                ResourceIds = resourceIds,
                SmartDetectorId = smartDetectorExecutionInfo.AlertRule.SmartDetectorId,
                Cadence = smartDetectorExecutionInfo.AlertRule.Cadence,
                DataEndTime = DateTime.UtcNow.AddMinutes(-20)
            };
            return await this.SendToAnalysisAsync(analysisRequest);
        }

        /// <summary>
        /// Sends an HTTP request to the analysis function with the Smart Detector request
        /// </summary>
        /// <param name="analysisRequest">The request to send to the analysis function</param>
        /// <returns>A list of Alerts</returns>
        private async Task<IList<ContractsAlert>> SendToAnalysisAsync(SmartDetectorExecutionRequest analysisRequest)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, this.analysisUrl);
            string requestBody = JsonConvert.SerializeObject(analysisRequest);
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            this.tracer.TraceVerbose($"Sending analysis request {requestBody}");

            // Send the request
            var response = await this.retryPolicy.RunAndTrackDependencyAsync(this.tracer, DependencyName, analysisRequest.SmartDetectorId, () => this.httpClientWrapper.SendAsync(requestMessage, default(CancellationToken)));

            if (!response.IsSuccessStatusCode)
            {
                string content = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                var message = $"Failed to execute Smart Detector {analysisRequest.SmartDetectorId}. Fail StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}. Content: {content}.";
                throw new AnalysisExecutionException(message);
            }

            var httpAnalysisResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IList<ContractsAlert>>(httpAnalysisResult);
        }
    }
}
