//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunner.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An observable class that runs a Smart Detector.
    /// </summary>
    public class SmartDetectorRunner : ObservableObject
    {
        private readonly ISmartDetector smartDetector;

        private readonly IAnalysisServicesFactory analysisServicesFactory;

        private readonly IQueryRunInfoProvider queryRunInfoProvider;

        private readonly SmartDetectorManifest smartDetectorManifes;

        private ObservableCollection<Alert> alerts;

        private ITracer tracer;

        private bool isSmartDetectorRunning;

        private CancellationTokenSource cancellationTokenSource;

        private IStateRepositoryFactory stateRepositoryFactory;

        private string smartDetectorId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorRunner"/> class.
        /// </summary>
        /// <param name="smartDetector">The Smart Detector.</param>
        /// <param name="analysisServicesFactory">The analysis services factory.</param>
        /// <param name="queryRunInfoProvider">The query run information provider.</param>
        /// <param name="smartDetectorManifes">The Smart Detector manifest.</param>
        /// <param name="stateRepositoryFactory">The state repository factory</param>
        /// <param name="smartDetectorId">The id of the Smart Detector</param>
        /// <param name="tracer">The tracer.</param>
        public SmartDetectorRunner(
            ISmartDetector smartDetector, 
            IAnalysisServicesFactory analysisServicesFactory,
            IQueryRunInfoProvider queryRunInfoProvider,
            SmartDetectorManifest smartDetectorManifes,
            IStateRepositoryFactory stateRepositoryFactory,
            string smartDetectorId,
            ITracer tracer)
        {
            this.smartDetector = smartDetector;
            this.analysisServicesFactory = analysisServicesFactory;
            this.queryRunInfoProvider = queryRunInfoProvider;
            this.smartDetectorManifes = smartDetectorManifes;
            this.Tracer = tracer;
            this.IsSmartDetectorRunning = false;
            this.Alerts = new ObservableCollection<Alert>();
            this.stateRepositoryFactory = stateRepositoryFactory;
            this.smartDetectorId = smartDetectorId;
        }

        /// <summary>
        /// Gets the Smart Detector run's alerts.
        /// </summary>
        public ObservableCollection<Alert> Alerts
        {
            get
            {
                return this.alerts;
            }

            private set
            {
                this.alerts = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tracer used by the Smart Detector runner.
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
        /// Gets or sets a value indicating whether the Smart Detector is running.
        /// </summary>
        public bool IsSmartDetectorRunning
        {
            get
            {
                return this.isSmartDetectorRunning;
            }

            set
            {
                this.isSmartDetectorRunning = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Runs the Smart Detector.
        /// </summary>
        /// <param name="resources">The resources which the Smart Detector should run on.</param>
        /// <param name="analysisCadence">The analysis cadence.</param>
        /// <returns>A task that runs the Smart Detector.</returns>
        public async Task RunAsync(List<ResourceIdentifier> resources, TimeSpan analysisCadence)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Alerts.Clear();
            IStateRepository stateRepository = this.stateRepositoryFactory.Create(this.smartDetectorId);

            var analysisRequest = new AnalysisRequest(resources, DateTime.UtcNow.AddMinutes(-20), analysisCadence, null, this.analysisServicesFactory, stateRepository);

            try
            {
                // Run Smart Detector
                this.IsSmartDetectorRunning = true;

                List<SmartDetectors.Alert> alerts = await this.smartDetector.AnalyzeResourcesAsync(
                    analysisRequest,
                    this.Tracer,
                    this.cancellationTokenSource.Token);

                // Create alerts
                List<Alert> results = new List<Alert>();
                foreach (var alert in alerts)
                {
                    // Create alert presentation 
                    var resourceIds = resources.Select(resource => resource.ResourceName).ToList();
                    var smartDetectorSettings = new SmartDetectorSettings();
                    var smartDetectorExecutionRequest = new SmartDetectorExecutionRequest
                    {
                        ResourceIds = resourceIds,
                        SmartDetectorId = this.smartDetectorManifes.Id,
                        Cadence = analysisCadence,
                        DataEndTime = DateTime.UtcNow.AddMinutes(-20)
                    };
                    QueryRunInfo queryRunInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(new List<ResourceIdentifier>() { alert.ResourceIdentifier }, this.cancellationTokenSource.Token);
                    ContractsAlert contractsAlert = alert.CreateContractsAlert(smartDetectorExecutionRequest, this.smartDetectorManifes.Name, queryRunInfo);

                    // Create Azure resource identifier 
                    ResourceIdentifier resourceIdentifier = ResourceIdentifier.CreateFromResourceId(contractsAlert.ResourceId);

                    results.Add(new Alert(contractsAlert, resourceIdentifier));
                }

                this.Alerts = new ObservableCollection<Alert>(results);
                this.tracer.TraceInformation($"Found {this.Alerts.Count} alerts");
            }
            catch (OperationCanceledException)
            {
                this.Tracer.TraceError("Smart Detector run was canceled.");
            }
            catch (Exception e)
            {
                this.Tracer.ReportException(e);
            }
            finally
            {
                this.IsSmartDetectorRunning = false;
                this.cancellationTokenSource?.Dispose();
            }
        }

        /// <summary>
        /// Cancel Smart Detector run.
        /// </summary>
        public void CancelSmartDetectorRun()
        {
            this.cancellationTokenSource?.Cancel();
        }
    }
}
