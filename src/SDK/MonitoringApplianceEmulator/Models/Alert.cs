//-----------------------------------------------------------------------
// <copyright file="Alert.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    /// <summary>
    /// Represents alert.
    /// </summary>
    public class Alert : ObservableObject
    {
        private ContractsAlert contractsAlert;

        private ResourceIdentifier resourceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class
        /// </summary>
        /// <param name="contractsAlert">The alert presentation object</param>
        /// <param name="resourceIdentifier">The alert's resource identifier</param>
        public Alert(ContractsAlert contractsAlert, ResourceIdentifier resourceIdentifier)
        {
            this.ContractsAlert = contractsAlert;
            this.ResourceIdentifier = resourceIdentifier;
        }

        /// <summary>
        /// Gets the alert presentation.
        /// </summary>
        public ContractsAlert ContractsAlert
        {
            get
            {
                return this.contractsAlert;
            }

            private set
            {
                this.contractsAlert = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the alert's resource identifier.
        /// </summary>
        public ResourceIdentifier ResourceIdentifier
        {
            get
            {
                return this.resourceIdentifier;
            }

            private set
            {
                this.resourceIdentifier = value;
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