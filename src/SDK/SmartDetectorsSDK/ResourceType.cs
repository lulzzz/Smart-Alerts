//-----------------------------------------------------------------------
// <copyright file="ResourceType.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of all resource types supported by Smart Detectors.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResourceType
    {
        /// <summary>
        /// The Subscription resource type.
        /// </summary>
        Subscription,

        /// <summary>
        /// The Resource Group resource type.
        /// </summary>
        ResourceGroup,

        /// <summary>
        /// The Virtual Machine resource type.
        /// </summary>
        VirtualMachine,

        /// <summary>
        /// The Virtual Machine Scale Set resource type.
        /// </summary>
        VirtualMachineScaleSet,

        /// <summary>
        /// The Application Instance resource type.
        /// </summary>
        ApplicationInsights,

        /// <summary>
        /// The log analytics workspace resource type.
        /// </summary>
        LogAnalytics
    }
}
