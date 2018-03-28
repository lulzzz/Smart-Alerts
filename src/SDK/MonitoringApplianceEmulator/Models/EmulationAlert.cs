//-----------------------------------------------------------------------
// <copyright file="EmulationAlert.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// Wrapper for <see cref="ContractsAlert"/> with additional emulation details.
    /// </summary>
    public class EmulationAlert : ObservableObject
    {
        private ContractsAlert contractsAlert;

        private ResourceIdentifier resourceIdentifier;

        private DateTime emulationIterationDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmulationAlert"/> class
        /// </summary>
        /// <param name="contractsAlert">The alert presentation object</param>
        /// <param name="resourceIdentifier">The alert's resource identifier</param>
        /// <param name="emulationIterationDate">The timestamp of the emulation iteration</param>
        public EmulationAlert(ContractsAlert contractsAlert, ResourceIdentifier resourceIdentifier, DateTime emulationIterationDate)
        {
            this.ContractsAlert = contractsAlert;
            this.ResourceIdentifier = resourceIdentifier;
            this.EmulationIterationDate = emulationIterationDate;
        }

        /// <summary>
        /// Gets or sets the alert presentation.
        /// </summary>
        public ContractsAlert ContractsAlert
        {
            get
            {
                return this.contractsAlert;
            }

            set
            {
                this.contractsAlert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the alert's resource identifier.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier
        {
            get
            {
                return this.resourceIdentifier;
            }

            set
            {
                this.resourceIdentifier = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the timestamp of the emulation iteration.
        /// </summary>
        public DateTime EmulationIterationDate
        {
            get
            {
                return this.emulationIterationDate;
            }

            set
            {
                this.emulationIterationDate = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the alert's severity.
        /// </summary>
        public string Severity => "SEV 3";

        /// <summary>
        /// Gets the alert's type.
        /// </summary>
        public string Type => "Smart Detector";

        /// <summary>
        /// Gets the alert's status.
        /// </summary>
        public string Status => "Unresolved";

        /// <summary>
        /// Gets the alert's monitor service.
        /// </summary>
        public string MonitorService => "Azure monitor";
    }
}