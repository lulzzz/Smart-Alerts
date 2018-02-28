//-----------------------------------------------------------------------
// <copyright file="AzureResourceService.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of all resource services supported by Smart Signals.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AzureResourceService
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
