﻿//-----------------------------------------------------------------------
// <copyright file="ServiceType.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Mdm
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of all resource services supported by Smart Detectors.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceType
    {
        /// <summary>
        /// Blob service of AzureStorage
        /// </summary>
        AzureStorageBlob,

        /// <summary>
        /// Table service of AzureStorage
        /// </summary>
        AzureStorageTable,

        /// <summary>
        /// Queue service of AzureStorage
        /// </summary>
        AzureStorageQueue,

        /// <summary>
        /// File service of AzureStorage
        /// </summary>
        AzureStorageFile
    }
}
