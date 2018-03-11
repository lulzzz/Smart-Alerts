//-----------------------------------------------------------------------
// <copyright file="Alert.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliancEmulator.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;

    /// <summary>
    /// Represents alert.
    /// </summary>
    public class Alert : ObservableObject
    {
        private AlertPresentation alertPresentation;

        private ResourceIdentifier resourceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class
        /// </summary>
        /// <param name="alertPresentation">The alert presentation object</param>
        /// <param name="resourceIdentifier">The alert's resource identifier</param>
        public Alert(AlertPresentation alertPresentation, ResourceIdentifier resourceIdentifier)
        {
            this.AlertPresentation = alertPresentation;
            this.ResourceIdentifier = resourceIdentifier;
        }

        /// <summary>
        /// Gets the alert presentation.
        /// </summary>
        public AlertPresentation AlertPresentation
        {
            get
            {
                return this.alertPresentation;
            }

            private set
            {
                this.alertPresentation = value;
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