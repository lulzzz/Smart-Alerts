//-----------------------------------------------------------------------
// <copyright file="InvalidAlertPresentationException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Presentation
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when the presentation information returned by an
    /// alert is invalid
    /// </summary>
    [Serializable]
    public class InvalidAlertPresentationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="message">The error message</param>
        public InvalidAlertPresentationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAlertPresentationException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected InvalidAlertPresentationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}