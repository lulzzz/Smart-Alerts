//-----------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for DateTime objects
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Format the given datetime to the time format which accepts by Application Insights query.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The date time in the required query format.</returns>
        public static string ToQueryTimeFormat(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
