//-----------------------------------------------------------------------
// <copyright file="EmulationStatusControlViewModel.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.ViewModels
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Controls;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Unity.Attributes;

    /// <summary>
    /// The view model class for the <see cref="EmulationStatusControl"/> control.
    /// </summary>
    public class EmulationStatusControlViewModel : ObservableObject
    {
        private SmartSignalRunner signalRunner;

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
        /// <param name="signalRunner">The smart signal runner.</param>
        /// <param name="tracer">The tracer.</param>
        [InjectionConstructor]
        public EmulationStatusControlViewModel(SmartSignalRunner signalRunner, ITracer tracer)
        {
            this.SignalRunner = signalRunner;
            this.Tracer = tracer;
        }

        #region Binded Properties

        /// <summary>
        /// Gets the signal runner.
        /// </summary>
        public SmartSignalRunner SignalRunner
        {
            get
            {
                return this.signalRunner;
            }

            private set
            {
                this.signalRunner = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the tracer used by the signal runner.
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
        /// Gets the command that cancel the signal run.
        /// </summary>
        public CommandHandler CancelSignalRunCommand => new CommandHandler(() => this.SignalRunner.CancelSignalRun());

        #endregion
    }
}
