//-----------------------------------------------------------------------
// <copyright file="SignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;

    /// <summary>
    /// Represents signal result item.
    /// </summary>
    public class SignalResultItem : ObservableObject
    {
        private AlertPresentation alertPresentation;

        private ResourceIdentifier resourceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultItem"/> class
        /// </summary>
        /// <param name="alertPresentation">The alert presentation object</param>
        /// <param name="resourceIdentifier">The signal result's resource identifier</param>
        public SignalResultItem(AlertPresentation alertPresentation, ResourceIdentifier resourceIdentifier)
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
        /// Gets the signal result's resource identifier.
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
        /// Gets the signal result's severity.
        /// </summary>
        public string Severity => "SEV 3";

        /// <summary>
        /// Gets the signal result's type.
        /// </summary>
        public string Type => "Smart Signal";

        /// <summary>
        /// Gets the signal result's status.
        /// </summary>
        public string Status => "Unresolved";

        /// <summary>
        /// Gets the signal result's monitor service.
        /// </summary>
        public string MonitorService => "Azure monitor";
    }
}