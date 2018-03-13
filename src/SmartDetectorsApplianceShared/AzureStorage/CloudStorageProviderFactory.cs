//-----------------------------------------------------------------------
// <copyright file="CloudStorageProviderFactory.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// An implementation of the <see cref="ICloudStorageProviderFactory"/> interface.
    /// </summary>
    public class CloudStorageProviderFactory : ICloudStorageProviderFactory
    {
        /// <summary>
        /// Creates an Azure Storage table client for the Smart Detector storage
        /// </summary>
        /// <returns>A <see cref="ICloudTableClientWrapper"/> for the Smart Detector storage</returns>
        public ICloudTableClientWrapper GetSmartDetectorStorageTableClient()
        {
            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudTableClient();

            return new CloudTableClientWrapper(cloudTableClient);
        }

        /// <summary>
        /// Creates an Azure Storage container client for the alerts storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Alerts storage container</returns>
        public ICloudBlobContainerWrapper GetAlertsStorageContainer()
        {
            var storageConnectionString = ConfigurationReader.ReadConfigConnectionString("StorageConnectionString", true);
            CloudBlobClient cloudBlobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("alerts");
            cloudBlobContainer.CreateIfNotExists();

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }

        /// <summary>
        /// Creates an Azure Storage container client for the global Smart Detector storage container
        /// </summary>
        /// <returns>A <see cref="ICloudBlobContainerWrapper"/> for the Smart Detector storage container</returns>
        public ICloudBlobContainerWrapper GetSmartDetectorGlobalStorageContainer()
        {
            var cloudBlobContainerUri = new Uri(ConfigurationReader.ReadConfig("GlobalSmartDetectorContainerUri", required: true));
            var cloudBlobContainer = new CloudBlobContainer(cloudBlobContainerUri);

            return new CloudBlobContainerWrapper(cloudBlobContainer);
        }
    }
}
