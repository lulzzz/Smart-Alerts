//-----------------------------------------------------------------------
// <copyright file="SmartDetectorCustomException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception wraps an exception thrown by a Smart Detector.
    /// </summary>
    [Serializable]
    public class SmartDetectorCustomException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorCustomException"/> class
        /// </summary>
        /// <param name="smartDetectorExceptionType">The original Smart Detector exception type</param>
        /// <param name="smartDetectorExceptionMessage">The original Smart Detector exception message</param>
        /// <param name="smartDetectorExceptionStackTrace">The original Smart Detector exception stack trace</param>
        public SmartDetectorCustomException(string smartDetectorExceptionType, string smartDetectorExceptionMessage, string smartDetectorExceptionStackTrace) : base(smartDetectorExceptionMessage)
        {
            this.SmartDetectorExceptionType = smartDetectorExceptionType;
            this.SmartDetectorExceptionMessage = smartDetectorExceptionMessage;
            this.SmartDetectorExceptionStackTrace = smartDetectorExceptionStackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorCustomException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected SmartDetectorCustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.SmartDetectorExceptionType = info.GetString(nameof(this.SmartDetectorExceptionType));
            this.SmartDetectorExceptionMessage = info.GetString(nameof(this.SmartDetectorExceptionMessage));
            this.SmartDetectorExceptionStackTrace = info.GetString(nameof(this.SmartDetectorExceptionStackTrace));
        }

        /// <summary>
        /// Gets the original Smart Detector exception type name
        /// </summary>
        public string SmartDetectorExceptionType { get; }

        /// <summary>
        /// Gets the original Smart Detector exception message
        /// </summary>
        public string SmartDetectorExceptionMessage { get; }

        /// <summary>
        /// Gets the original Smart Detector exception stack trace
        /// </summary>
        public string SmartDetectorExceptionStackTrace { get; }

        /// <summary>
        /// Gets the object data from the serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SeraizliationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.SmartDetectorExceptionType), this.SmartDetectorExceptionType);
            info.AddValue(nameof(this.SmartDetectorExceptionMessage), this.SmartDetectorExceptionMessage);
            info.AddValue(nameof(this.SmartDetectorExceptionStackTrace), this.SmartDetectorExceptionStackTrace);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a string representation of this exception, including the original exception's details.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return this.Message + Environment.NewLine +
                $"OriginalExceptionType: {this.SmartDetectorExceptionType}" + Environment.NewLine +
                $"OriginalExceptionStackTrace: {this.SmartDetectorExceptionStackTrace}" + Environment.NewLine +
                $"StackTrace: {this.StackTrace}";
        }
    }
}