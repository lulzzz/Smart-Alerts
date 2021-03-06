﻿//-----------------------------------------------------------------------
// <copyright file="SmartDetectorsManagementApiException.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents an exception thrown from the management API logic
    /// </summary>
    public class SmartDetectorsManagementApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// with a specified error message and status code.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="statusCode">The HTTP status code that represents the exception.</param>
        public SmartDetectorsManagementApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorsManagementApiException"/> class
        /// with a specified error message and status code.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="exception">The inner exception.</param>
        /// <param name="statusCode">The HTTP status code that represents the exception.</param>
        public SmartDetectorsManagementApiException(string message, Exception exception, HttpStatusCode statusCode) : base(message, exception)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the status code that represents the exception
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
