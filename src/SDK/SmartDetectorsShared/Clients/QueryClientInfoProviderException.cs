//-----------------------------------------------------------------------
// <copyright file="QueryClientInfoProviderException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Clients
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is used to handle cases where query client information could not be created by the <see cref="IQueryRunInfoProvider"/>.
    /// </summary>
    [Serializable]
    public class QueryClientInfoProviderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryClientInfoProviderException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public QueryClientInfoProviderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryClientInfoProviderException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected QueryClientInfoProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}