//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRepositoryTest.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsApplianceSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Moq;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class SmartDetectorRepositoryTest
    {
        private SmartDetectorRepository smartDetectorRepository;
        private Mock<ICloudBlobContainerWrapper> blobContainerMock;

        [TestInitialize]
        public void Setup()
        {
            this.blobContainerMock = new Mock<ICloudBlobContainerWrapper>();
            var storageProviderFactoryMock = new Mock<ICloudStorageProviderFactory>();
            storageProviderFactoryMock.Setup(m => m.GetSmartDetectorGlobalStorageContainer()).Returns(this.blobContainerMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.smartDetectorRepository = new SmartDetectorRepository(tracerMock.Object, storageProviderFactoryMock.Object);
        }

        [TestMethod]
        public async Task WhenStorageExceptionIsThrownWhenReadingAllManifestsThenCorrectExceptionIsThrown()
        {
            this.blobContainerMock.Setup(m => m.ListBlobsAsync(string.Empty, true, BlobListingDetails.Metadata, It.IsAny<CancellationToken>())).Throws(new StorageException());

            try
            {
                await this.smartDetectorRepository.ReadAllSmartDetectorsManifestsAsync(CancellationToken.None);
            }
            catch (SmartDetectorRepositoryException e)
            {
                if (e.InnerException is StorageException)
                {
                    return;
                }

                Assert.Fail("Exception from the blob storage should cause a repository exception with an inner exception of StorageException");
            }

            Assert.Fail("Exception from the blob storage should cause a repository exception");
        }

        [TestMethod]
        public async Task WhenStorageExceptionIsThrownWhenReadingSmartDetectorPackageThenCorrectExceptionIsThrown()
        {
            const string SmartDetectorId = "someId";
            this.blobContainerMock.Setup(m => m.ListBlobsAsync($"{SmartDetectorId}/", true, BlobListingDetails.Metadata, It.IsAny<CancellationToken>())).Throws(new StorageException());

            try
            {
                await this.smartDetectorRepository.ReadSmartDetectorPackageAsync(SmartDetectorId, CancellationToken.None);
            }
            catch (SmartDetectorRepositoryException e)
            {
                if (e.InnerException is StorageException)
                {
                    return;
                }

                Assert.Fail("Exception from the blob storage should cause a repository exception with an inner exception of StorageException");
            }

            Assert.Fail("Exception from the blob storage should cause a repository exception");
        }

        [TestMethod]
        public async Task WhenReadingAllSmartDetectorsManifestsThenLatestVersionsManifestsAreReturned()
        {
            const string FirstSmartDetectorId = "smartDetectorId1";
            const string SecondSmartDetectorId = "smartDetectorId2";

            var firstSmartDetectorOldVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{FirstSignalId}/signal.v1.package"));
            var firstSmartDetectorNewVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{FirstSignalId}/signal.v2.package"));
            var secondSmartDetectorOldVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{SecondSignalId}/signal.v1.package"));
            var secondSmartdetectorNewVersion = new CloudBlockBlob(new Uri("https://storage.blob.core.windows.net/container/{SecondSignalId}/signal.v2.package"));

            var newVersionSupportedResourceTypes = new List<ResourceType> { ResourceType.ApplicationInsights };
            var oldVersionSupportedResourceTypes = new List<ResourceType> { ResourceType.ApplicationInsights, ResourceType.LogAnalytics };
            var firstSmartDetectorNewVersionMetadata = this.GenerateMetadata(FirstSmartDetectorId, "2.0", newVersionSupportedResourceTypes);
            var firstSmartDetectorOldVersionMetadata = this.GenerateMetadata(FirstSmartDetectorId, "1.0", oldVersionSupportedResourceTypes);
            var secondSmartDetecotrNewMetadata = this.GenerateMetadata(SecondSmartDetectorId, "1.1.0.1", newVersionSupportedResourceTypes);
            var secondmartDetectorOldMetadata = this.GenerateMetadata(SecondSmartDetectorId, "1.1", oldVersionSupportedResourceTypes);

            this.AssignBlobMetadata(firstSmartDetectorNewVersion, firstSmartDetectorNewVersionMetadata);
            this.AssignBlobMetadata(firstSmartDetectorOldVersion, firstSmartDetectorOldVersionMetadata);
            this.AssignBlobMetadata(secondSmartdetectorNewVersion, secondSmartDetecotrNewMetadata);
            this.AssignBlobMetadata(secondSmartDetectorOldVersion, secondmartDetectorOldMetadata);

            var blobs = new List<IListBlobItem>
            {
                firstSmartDetectorOldVersion,
                firstSmartDetectorNewVersion,
                secondSmartDetectorOldVersion,
                secondSmartdetectorNewVersion
            };

            this.blobContainerMock.Setup(m => m.ListBlobsAsync(string.Empty, true, BlobListingDetails.Metadata, It.IsAny<CancellationToken>())).ReturnsAsync(blobs);

            var smartDetectorsManifests = await this.smartDetectorRepository.ReadAllSmartDetectorsManifestsAsync(CancellationToken.None);
            Assert.AreEqual(2, smartDetectorsManifests.Count);

            this.AssertMetadata(smartDetectorsManifests.First(), firstSmartDetectorNewVersionMetadata);
            this.AssertMetadata(smartDetectorsManifests.Last(), secondSmartDetecotrNewMetadata);
        }

        private void AssignBlobMetadata(CloudBlockBlob blockBlob, Dictionary<string, string> metadata)
        {
            // Setting the block blob properties in reflection since it has no setter
            object attributes = blockBlob.GetType().GetField("attributes", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(blockBlob);
            attributes?.GetType().GetProperty("Metadata")?.SetValue(attributes, metadata, null);
        }

        private Dictionary<string, string> GenerateMetadata(string smartDetectorId, string version, List<ResourceType> supportedresourcetypes)
        {
            return new Dictionary<string, string>
            {
                { "id", smartDetectorId },
                { "name", "smartDetectorName" },
                { "version", version },
                { "description", "Smart Detector description" },
                { "assemblyName", "assembly" },
                { "className", "class" },
                { "supportedCadencesInMinutes", "[5,60]" },
                { "supportedResourceTypes", $"[\"{string.Join("\",\"", supportedresourcetypes)}\"]" }
            };
        }

        private void AssertMetadata(SmartDetectorManifest smartDetectorManifest, Dictionary<string, string> expectedMetadata)
        {
            var supportedResourceTypes = JArray.Parse(expectedMetadata["supportedResourceTypes"])
                .Select(jtoken => (ResourceType)Enum.Parse(typeof(ResourceType), jtoken.ToString(), true))
                .ToList();

            var supportedCadencesInMinutes = JArray.Parse(expectedMetadata["supportedCadencesInMinutes"])
                .Select(jToken => int.Parse(jToken.ToString()))
                .ToList();

            Assert.AreEqual(expectedMetadata["id"], smartDetectorManifest.Id);
            Assert.AreEqual(expectedMetadata["name"], smartDetectorManifest.Name);
            Assert.AreEqual(expectedMetadata["version"], smartDetectorManifest.Version.ToString());
            Assert.AreEqual(expectedMetadata["description"], smartDetectorManifest.Description);
            Assert.AreEqual(expectedMetadata["assemblyName"], smartDetectorManifest.AssemblyName);
            Assert.AreEqual(expectedMetadata["className"], smartDetectorManifest.ClassName);

            Assert.AreEqual(supportedResourceTypes.Count, smartDetectorManifest.SupportedResourceTypes.Count);
            foreach (var supportedResourceType in supportedResourceTypes)
            {
                Assert.IsTrue(smartDetectorManifest.SupportedResourceTypes.Contains(supportedResourceType));
            }

            Assert.AreEqual(supportedCadencesInMinutes.Count, smartDetectorManifest.SupportedCadencesInMinutes.Count);
            foreach (var supportedCadence in supportedCadencesInMinutes)
            {
                Assert.IsTrue(smartDetectorManifest.SupportedCadencesInMinutes.Contains(supportedCadence));
            }
        }
    }
}
