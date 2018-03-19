//-----------------------------------------------------------------------
// <copyright file="AlertRuleApiTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.AlertRules;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.ManagementApi.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AlertRuleApiTests
    {
        private Mock<IAlertRuleStore> alertRuleStoreMock;

        private IAlertRuleApi alertRuleApi;

        [TestInitialize]
        public void Initialize()
        {
            this.alertRuleStoreMock = new Mock<IAlertRuleStore>();
            this.alertRuleApi = new AlertRuleApi(this.alertRuleStoreMock.Object);
        }

        #region Adding New Alert Rule Tests

        [TestMethod]
        public async Task WhenAddingSignalHappyFlow()
        {
            var addSignalModel = new AlertRuleApiEntity()
            {
                SmartDetectorId = Guid.NewGuid().ToString(),
                ResourceId = "resourceId",
                CadenceInMinutes = 1440
            };

            this.alertRuleStoreMock.Setup(s => s.AddOrReplaceAlertRuleAsync(It.IsAny<AlertRule>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // This shouldn't throw any exception
            await this.alertRuleApi.AddAlertRuleAsync(addSignalModel, CancellationToken.None);
        }

        [TestMethod]
        public async Task WhenAddingSignalButModelIsInvalidBecauseSignalIdIsEmptyThenThrowException()
        {
            var addSignalModel = new AlertRuleApiEntity()
            {
                SmartDetectorId = string.Empty,
                ResourceId = "resourceId",
                CadenceInMinutes = 1440
            };

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
                return;
            }

            Assert.Fail("Invalid model should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButCadenceValueIsInvalidCronValueThenThrowException()
        {
            var addSignalModel = new AlertRuleApiEntity()
            {
                SmartDetectorId = Guid.NewGuid().ToString(),
                ResourceId = "resourceId",
                CadenceInMinutes = 0
            };

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
                return;
            }

            Assert.Fail("Invalid CRON value should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButStoreThrowsExceptionThenThrowTheWrappedException()
        {
            var addSignalModel = new AlertRuleApiEntity()
            {
                SmartDetectorId = Guid.NewGuid().ToString(),
                ResourceId = "resourceId",
                CadenceInMinutes = 1440
            };

            this.alertRuleStoreMock.Setup(s => s.AddOrReplaceAlertRuleAsync(It.IsAny<AlertRule>(), It.IsAny<CancellationToken>()))
                                                  .ThrowsAsync(new AlertRuleStoreException(string.Empty, new Exception()));

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel, CancellationToken.None);
            }
            catch (SmartDetectorsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.StatusCode);
                return;
            }

            Assert.Fail("Exception coming from the Signals store should cause to an exception from the controller");
        }

        #endregion
    }
}
