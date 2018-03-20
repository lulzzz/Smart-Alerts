//-----------------------------------------------------------------------
// <copyright file="ICloudStorageProviderFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage
{
    /// <summary>
    /// An interface for exposing a factory that creates Azure Storage clients
    /// </summary>
    public interface ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage table client for the Smart Detector storage
        /// </summary>
        /// <returns>A <see cref="ICloudTableClientWrapper"/> for the Smart Detector storage</returns>
        ICloudTableClientWrapper GetSmartDetectorStorageTableClient();

        /// <summary>
        /// Creates an Azure Storage container client for the alerts storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the alerts storage container</returns>
        ICloudBlobContainerWrapper GetAlertsStorageContainer();

        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Detector storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector storage container</returns>
        ICloudBlobContainerWrapper GetSmartDetectorGlobalStorageContainer();
        
        /// <summary>
        /// Creates an Azure Storage container client for the Smart Detector state container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector state container</returns>
        ICloudBlobContainerWrapper GetSmartDetectorStateStorageContainer();
    }
}
