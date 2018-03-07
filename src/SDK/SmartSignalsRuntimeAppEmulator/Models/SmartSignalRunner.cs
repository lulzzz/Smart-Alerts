//-----------------------------------------------------------------------
// <copyright file="SmartSignalRunner.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;

    /// <summary>
    /// An observable class that runs a smart signal.
    /// </summary>
    public class SmartSignalRunner : ObservableObject
    {
        private readonly ISmartDetector smartDetector;

        private readonly IAnalysisServicesFactory analysisServicesFactory;

        private readonly IQueryRunInfoProvider queryRunInfoProvider;

        private readonly SmartDetectorManifest smartDetectorManifes;

        private ObservableCollection<SignalResultItem> results;

        private ITracer tracer;

        private bool isSignalRunning;

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartSignalRunner"/> class.
        /// </summary>
        /// <param name="smartDetector">The Smart Detector.</param>
        /// <param name="analysisServicesFactory">The analysis services factory.</param>
        /// <param name="queryRunInfoProvider">The query run information provider.</param>
        /// <param name="smartSignalManifes">The smart detector manifest.</param>
        /// <param name="tracer">The tracer.</param>
        public SmartSignalRunner(
            ISmartDetector smartDetector, 
            IAnalysisServicesFactory analysisServicesFactory,
            IQueryRunInfoProvider queryRunInfoProvider,
            SmartDetectorManifest smartSignalManifes,
            ITracer tracer)
        {
            this.smartDetector = smartDetector;
            this.analysisServicesFactory = analysisServicesFactory;
            this.queryRunInfoProvider = queryRunInfoProvider;
            this.smartDetectorManifes = smartSignalManifes;
            this.Tracer = tracer;
            this.IsSignalRunning = false;
            this.Results = new ObservableCollection<SignalResultItem>();
        }

        /// <summary>
        /// Gets the signal run's result.
        /// </summary>
        public ObservableCollection<SignalResultItem> Results
        {
            get
            {
                return this.results;
            }

            private set
            {
                this.results = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tracer used by the signal runner.
        /// </summary>
        public ITracer Tracer
        {
            get
            {
                return this.tracer;
            }

            set
            {
                this.tracer = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the signal is running.
        /// </summary>
        public bool IsSignalRunning
        {
            get
            {
                return this.isSignalRunning;
            }

            set
            {
                this.isSignalRunning = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Runs the smart signal.
        /// </summary>
        /// <param name="resources">The resources which the signal should run on.</param>
        /// <param name="analysisCadence">The analysis cadence.</param>
        /// <returns>A task that runs the smart signal.</returns>
        public async Task RunAsync(List<ResourceIdentifier> resources, TimeSpan analysisCadence)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Results.Clear();
            var analysisRequest = new AnalysisRequest(resources, null, analysisCadence, this.analysisServicesFactory);
            try
            {
                // Run Signal
                this.IsSignalRunning = true;

                List<Alert> alerts = await this.smartDetector.AnalyzeResourcesAsync(
                    analysisRequest,
                    this.Tracer,
                    this.cancellationTokenSource.Token);

                // Create signal result items
                List<SignalResultItem> signalResultItems = new List<SignalResultItem>();
                foreach (var alert in alerts)
                {
                    // Create alert presentation 
                    var resourceIds = resources.Select(resource => resource.ResourceName).ToList();
                    var smartDetectorSettings = new SmartDetectorSettings();
                    var smartDetectorRequest = new SmartDetectorRequest(resourceIds, this.smartDetectorManifes.Id, null, analysisCadence, smartDetectorSettings);
                    QueryRunInfo queryRunInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(new List<ResourceIdentifier>() { alert.ResourceIdentifier }, this.cancellationTokenSource.Token);
                    AlertPresentation alertPresentation = AlertPresentation.CreateFromAlert(
                        smartDetectorRequest, this.smartDetectorManifes.Name, alert, queryRunInfo);

                    // Create Azure resource identifier 
                    ResourceIdentifier resourceIdentifier = ResourceIdentifier.CreateFromResourceId(alertPresentation.ResourceId);

                    signalResultItems.Add(new SignalResultItem(alertPresentation, resourceIdentifier));
                }

                this.Results = new ObservableCollection<SignalResultItem>(signalResultItems);
                this.tracer.TraceInformation($"Found {this.Results.Count} results");
            }
            catch (OperationCanceledException)
            {
                this.Tracer.TraceError("Signal run was canceled.");
            }
            catch (Exception e)
            {
                this.Tracer.ReportException(e);
            }
            finally
            {
                this.IsSignalRunning = false;
                this.cancellationTokenSource?.Dispose();
            }
        }

        /// <summary>
        /// Cancel signal run.
        /// </summary>
        public void CancelSignalRun()
        {
            this.cancellationTokenSource?.Cancel();
        }
    }
}
