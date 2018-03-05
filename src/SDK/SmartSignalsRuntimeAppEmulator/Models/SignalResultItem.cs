//-----------------------------------------------------------------------
// <copyright file="SignalResultItem.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models
{
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartSignals.SignalResultPresentation;

    /// <summary>
    /// Represents signal result item.
    /// </summary>
    public class SignalResultItem : ObservableObject
    {
        private SmartSignalResultItemPresentation smartSignalResultItemPresentation;

        private ResourceIdentifier resourceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalResultItem"/> class
        /// </summary>
        /// <param name="smartSignalResultItemPresentation">The signal result item presentation object</param>
        /// <param name="resourceIdentifier">The signal result's resource identifier</param>
        public SignalResultItem(SmartSignalResultItemPresentation smartSignalResultItemPresentation, ResourceIdentifier resourceIdentifier)
        {
            this.ResultItemPresentation = smartSignalResultItemPresentation;
            this.ResourceIdentifier = resourceIdentifier;
        }

        /// <summary>
        /// Gets the signal result presentation item.
        /// </summary>
        public SmartSignalResultItemPresentation ResultItemPresentation
        {
            get
            {
                return this.smartSignalResultItemPresentation;
            }

            private set
            {
                this.smartSignalResultItemPresentation = value;
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