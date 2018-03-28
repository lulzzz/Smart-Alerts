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
    using State;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// An observable class that runs a Smart Detector.
    /// </summary>
    public class SmartDetectorRunner : ObservableObject
    {
        private readonly IStateRepositoryFactory stateRepositoryFactory;

        private readonly string smartDetectorId;

        private readonly ISmartDetector smartDetector;

        private readonly IAnalysisServicesFactory analysisServicesFactory;

        private readonly IQueryRunInfoProvider queryRunInfoProvider;

        private readonly SmartDetectorManifest smartDetectorManifes;

        private ObservableCollection<EmulationAlert> alerts;

        private ITracer tracer;

        private bool isSmartDetectorRunning;

        private CancellationTokenSource cancellationTokenSource;

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
            this.Alerts = new ObservableCollection<EmulationAlert>();
            this.stateRepositoryFactory = stateRepositoryFactory;
            this.smartDetectorId = smartDetectorId;
        }

        /// <summary>
        /// Gets or sets the Smart Detector run's alerts.
        /// </summary>
        public ObservableCollection<EmulationAlert> Alerts
        {
            get
            {
                return this.alerts;
            }

            set
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
        /// <param name="resources">The resources which the Smart Detector should run on</param>
        /// <param name="analysisCadence">The analysis cadence</param>
        /// <param name="startTimeRange">The start time</param>
        /// <param name="endTimeRange">The end time</param>
        /// <returns>A task that runs the Smart Detector</returns>
        public async Task RunAsync(List<ResourceIdentifier> resources, TimeSpan analysisCadence, DateTime startTimeRange, DateTime endTimeRange)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            IStateRepository stateRepository = this.stateRepositoryFactory.Create(this.smartDetectorId);

            List<string> resourceIds = resources.Select(resource => resource.ResourceName).ToList();
            this.Alerts.Clear();
            try
            {
                // Run Smart Detector
                this.IsSmartDetectorRunning = true;

                int totalRunsAmount = (int)((endTimeRange.Subtract(startTimeRange).Ticks / analysisCadence.Ticks) + 1);
                int currentRunNumber = 1;
                for (var currentTime = startTimeRange; currentTime <= endTimeRange; currentTime = currentTime.Add(analysisCadence))
                {
                    this.tracer.TraceInformation($"Start analysis, end of time range: {currentTime}");

                    var analysisRequest = new AnalysisRequest(resources, currentTime, analysisCadence, null, this.analysisServicesFactory, stateRepository);
                    var newAlerts = await this.smartDetector.AnalyzeResourcesAsync(
                        analysisRequest,
                        this.Tracer,
                        this.cancellationTokenSource.Token);

                    var smartDetectorExecutionRequest = new SmartDetectorExecutionRequest
                    {
                        ResourceIds = resourceIds,
                        SmartDetectorId = this.smartDetectorManifes.Id,
                        Cadence = analysisCadence,
                        DataEndTime = currentTime
                    };

                    // Show the alerts that were found in this iteration
                    newAlerts.ForEach(async newAlert =>
                    {
                        QueryRunInfo queryRunInfo = await this.queryRunInfoProvider.GetQueryRunInfoAsync(new List<ResourceIdentifier>() { newAlert.ResourceIdentifier }, this.cancellationTokenSource.Token);
                        ContractsAlert contractsAlert = newAlert.CreateContractsAlert(smartDetectorExecutionRequest, this.smartDetectorManifes.Name, queryRunInfo);

                        // Create Azure resource identifier 
                        ResourceIdentifier resourceIdentifier = ResourceIdentifier.CreateFromResourceId(contractsAlert.ResourceId);

                        this.Alerts.Add(new EmulationAlert(contractsAlert, resourceIdentifier, currentTime));
                    });

                    this.tracer.TraceInformation($"completed {currentRunNumber} of {totalRunsAmount} runs");
                    currentRunNumber++;
                }

                string separator = "=====================================================================================================";
                this.tracer.TraceInformation($"Total alerts found: {this.Alerts.Count} {Environment.NewLine} {separator}");
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
