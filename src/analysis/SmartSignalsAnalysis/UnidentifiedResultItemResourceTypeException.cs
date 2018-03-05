//-----------------------------------------------------------------------
// <copyright file="UnidentifiedResultItemResourceTypeException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Analysis
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// This exception is thrown when a signals' result item resource type is not one of the types supported by the signal.
    /// </summary>
    [Serializable]
    public class UnidentifiedResultItemResourceTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedResultItemResourceTypeException"/> class
        /// with the specified result item resource.
        /// </summary>
        /// <param name="resourceIdentifier">The result item resource</param>
        public UnidentifiedResultItemResourceTypeException(ResourceIdentifier resourceIdentifier)
            : base($"Received a result item for resource \"{resourceIdentifier.ResourceName}\", of type {resourceIdentifier.ResourceType}, which did not match any of the resource types supported by the signal")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedResultItemResourceTypeException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected UnidentifiedResultItemResourceTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}