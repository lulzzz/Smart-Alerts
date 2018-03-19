//-----------------------------------------------------------------------
// <copyright file="SignalApiTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SignalApiTests
    {
        private Mock<ISmartDetectorRepository> smartDetectorRepository;

        private ISmartDetectorApi signalsLogic;

        [TestInitialize]
        public void Initialize()
        {
            this.smartDetectorRepository = new Mock<ISmartDetectorRepository>();
            this.signalsLogic = new SmartDetectorApi(this.smartDetectorRepository.Object);
        }

        #region Getting Signals Tests

        [TestMethod]
        public async Task WhenGettingAllSignalsHappyFlow()
        {
            this.smartDetectorRepository.Setup(repository => repository.ReadAllSmartDetectorsManifestsAsync(It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(() => new List<SmartDetectorManifest>()
                {
                    new SmartDetectorManifest("someId", "someName", "someDescription", Version.Parse("1.0"), "someAssemblyName", "someClassName", new List<ResourceType> { ResourceType.ResourceGroup }, new List<int> { 60 })
                });

            ListSmartDetectorsResponse response = await this.signalsLogic.GetAllSmartDetectorsAsync(CancellationToken.None);

            Assert.AreEqual(1, response.SmartDetectors.Count);
            Assert.AreEqual("someId", response.SmartDetectors.First().Id);
            Assert.AreEqual("someName", response.SmartDetectors.First().Name);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsButSignalsRepositoryThrowsExceptionThenThrowsWrappedException()
        {
            this.smartDetectorRepository.Setup(repository => repository.ReadAllSmartDetectorsManifestsAsync(It.IsAny<CancellationToken>()))
                                       .ThrowsAsync(new AlertRuleStoreException("some message", new Exception()));

            try
            {
                await this.signalsLogic.GetAllSmartDetectorsAsync(CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException)
            {
                return;
            }

            Assert.Fail("Exception from the signals store should cause to management API exception");
        }

        #endregion
    }
}
