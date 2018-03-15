//-----------------------------------------------------------------------
// <copyright file="UnidentifiedAlertResourceTypeException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// This exception is thrown when an alert resource type is not one of the types supported by the Smart Detector.
    /// </summary>
    [Serializable]
    public class UnidentifiedAlertResourceTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// with the specified alert resource.
        /// </summary>
        /// <param name="resourceIdentifier">The alert resource</param>
        public UnidentifiedAlertResourceTypeException(ResourceIdentifier resourceIdentifier)
            : base($"Received an alert for resource \"{resourceIdentifier.ResourceName}\", of type {resourceIdentifier.ResourceType}, which did not match any of the resource types supported by the Smart Detector")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedAlertResourceTypeException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected UnidentifiedAlertResourceTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}