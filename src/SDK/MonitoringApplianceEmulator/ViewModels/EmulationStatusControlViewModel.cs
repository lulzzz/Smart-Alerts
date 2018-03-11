//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Controls;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="EmulationStatusControl"/> control.
    /// </summary>
    public class EmulationStatusControlViewModel : ObservableObject
    {
        private SmartDetectorRunner smartDetectorRunner;

        private ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStatusControlViewModel"/> class for design time only.
        /// </summary>
        public EmulationStatusControlViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationStatusControlViewModel"/> class.
        /// </summary>
        /// <param name="smartDetectorRunner">The Smart Detector runner.</param>
        /// <param name="tracer">The tracer.</param>
        [InjectionConstructor]
        public EmulationStatusControlViewModel(SmartDetectorRunner smartDetectorRunner, ITracer tracer)
        {
            this.SmartDetectorRunner = smartDetectorRunner;
            this.Tracer = tracer;
        }

        #region Binded Properties

        /// <summary>
        /// Gets the Smart Detector runner.
        /// </summary>
        public SmartDetectorRunner SmartDetectorRunner
        {
            get
            {
                return this.smartDetectorRunner;
            }

            private set
            {
                this.smartDetectorRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the tracer used by the Smart Detector runner.
        /// </summary>
        public ITracer Tracer
        {
            get
            {
                return this.tracer;
            }

            private set
            {
                this.tracer = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command that cancel the Smart Detector run.
        /// </summary>
        public CommandHandler CancelSmartDetectorRunCommand => new CommandHandler(() => this.SmartDetectorRunner.CancelSmartDetectorRun());

        #endregion
    }
}
