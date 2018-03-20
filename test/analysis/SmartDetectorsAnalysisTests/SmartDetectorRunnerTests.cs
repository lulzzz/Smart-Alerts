//-----------------------------------------------------------------------
// <copyright file="SmartDetectorRunnerTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Analysis;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Exceptions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.SmartDetectorLoader;
    using Microsoft.Azure.Monitoring.SmartDetectors.State;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Unity;
    using Alert = Microsoft.Azure.Monitoring.SmartDetectors.Alert;
    using ContractsAlert = Microsoft.Azure.Monitoring.SmartDetectors.MonitoringAppliance.Contracts.Alert;

    [TestClass]
    public class SmartDetectorRunnerTests
    {
        private SmartDetectorPackage smartDetectorPackage;
        private List<string> resourceIds;
        private SmartDetectorExecutionRequest request;
        private TestSmartDetector smartDetector;
        private IUnityContainer testContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            this.TestInitialize(ResourceType.VirtualMachine, ResourceType.VirtualMachine);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorThenTheCorrectAlertIsReturned()
        {
            // Run the Smart Detector and validate results
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.RunAsync(this.request, default(CancellationToken));
            Assert.IsNotNull(contractsAlerts, "Presentation list is null");
            Assert.AreEqual(1, contractsAlerts.Count);
            Assert.AreEqual("Test title", contractsAlerts.Single().Title);
        }

        [TestMethod]
        public void WhenRunningSmartDetectorThenCancellationIsHandledGracefully()
        {
            // Notify the Smart Detector that it should get stuck and wait for cancellation
            this.smartDetector.ShouldStuck = true;

            // Run the Smart Detector asynchronously
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.RunAsync(this.request, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.smartDetector.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            try
            {
                t.Wait(TimeSpan.FromSeconds(10));
            }
            catch (AggregateException e) when ((e.InnerExceptions.Single() as SmartDetectorCustomException).SmartDetectorExceptionType == typeof(TaskCanceledException).ToString())
            {
                Assert.IsTrue(this.smartDetector.WasCanceled, "The Smart Detector was not canceled!");
            }
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorThenExceptionsAreHandledCorrectly()
        {
            // Notify the Smart Detector that it should throw an exception
            this.smartDetector.ShouldThrow = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                await runner.RunAsync(this.request, default(CancellationToken));
            }
            catch (SmartDetectorCustomException e) when (e.SmartDetectorExceptionType == typeof(DivideByZeroException).ToString())
            {
                // Expected exception
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorCustomException))]
        public async Task WhenRunningSmartDetectorThenCustomExceptionsAreHandledCorrectly()
        {
            // Notify the Smart Detector that it should throw a custom exception
            this.smartDetector.ShouldThrowCustom = true;

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            await runner.RunAsync(this.request, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorithSupportedTypeThenTheCorrectResultsAreReturned()
        {
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.Subscription, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.ResourceGroup, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.Subscription, ResourceType.VirtualMachine, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.ResourceGroup, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.VirtualMachine, false);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.VirtualMachine, false);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorWithUnsupportedTypeThenAnExceptionIsThrown()
        {
            await this.RunSmartDetectorWithResourceTypes(ResourceType.ResourceGroup, ResourceType.Subscription, true);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.Subscription, true);
            await this.RunSmartDetectorWithResourceTypes(ResourceType.VirtualMachine, ResourceType.ResourceGroup, true);
        }

        [TestMethod]
        public async Task WhenRunningSmartDetectorThenStateRepositoryIsCreatedAndPassedToSmartDetector()
        {
            // Setup mocks
            var stateRepositoryMock = new Mock<IStateRepository>();
            var stateRepositoryFactoryMock = new Mock<IStateRepositoryFactory>();
            stateRepositoryFactoryMock.Setup(m => m.Create(It.IsAny<string>())).Returns(stateRepositoryMock.Object);
            this.testContainer.RegisterInstance<IStateRepositoryFactory>(stateRepositoryFactoryMock.Object);

            // Run the Smart Detector
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            List<ContractsAlert> contractsAlerts = await runner.RunAsync(this.request, default(CancellationToken));

            // Assert
            stateRepositoryFactoryMock.Verify(
                m => m.Create(It.IsAny<string>()));
            stateRepositoryMock.Verify(
                m => m.StoreStateAsync("test key", "test state", It.IsAny<CancellationToken>()));
        }

        private async Task RunSmartDetectorWithResourceTypes(ResourceType requestResourceType, ResourceType smartDetectorResourceType, bool shouldFail)
        {
            this.TestInitialize(requestResourceType, smartDetectorResourceType);
            ISmartDetectorRunner runner = this.testContainer.Resolve<ISmartDetectorRunner>();
            try
            {
                List<ContractsAlert> alertPresentations = await runner.RunAsync(this.request, default(CancellationToken));
                if (shouldFail)
                {
                    Assert.Fail("An exception should have been thrown - resource types are not compatible");
                }

                Assert.AreEqual(1, alertPresentations.Count);
            }
            catch (IncompatibleResourceTypesException)
            {
                if (!shouldFail)
                {
                    throw;
                }
            }
        }

        private void TestInitialize(ResourceType requestResourceType, ResourceType smartDetectorResourceType)
        {
            this.testContainer = new UnityContainer();

            this.testContainer.RegisterType<ISmartDetectorRunner, SmartDetectorRunner>();

            this.testContainer.RegisterInstance<ITracer>(new Mock<ITracer>().Object);

            ResourceIdentifier resourceId;
            switch (requestResourceType)
            {
                case ResourceType.Subscription:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", string.Empty, string.Empty);
                    break;
                case ResourceType.ResourceGroup:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", "resourceGroup", string.Empty);
                    break;
                default:
                    resourceId = new ResourceIdentifier(requestResourceType, "subscriptionId", "resourceGroup", "resourceName");
                    break;
            }

            this.resourceIds = new List<string>() { resourceId.ToResourceId() };
            this.request = new SmartDetectorExecutionRequest
            {
                ResourceIds = this.resourceIds,
                Cadence = TimeSpan.FromDays(1),
                DataEndTime = DateTime.UtcNow.AddMinutes(-20),
                SmartDetectorId = "1"
            };

            var smartDetectorManifest = new SmartDetectorManifest("1", "Test Smart Detector", "Test Smart Detector description", Version.Parse("1.0"), "assembly", "class", new List<ResourceType>() { smartDetectorResourceType }, new List<int> { 60 });
            this.smartDetectorPackage = new SmartDetectorPackage(smartDetectorManifest, new Dictionary<string, byte[]> { ["TestSmartDetectorLibrary"] = new byte[0] });

            var smartDetectorRepositoryMock = new Mock<ISmartDetectorRepository>();
            smartDetectorRepositoryMock
                .Setup(x => x.ReadSmartDetectorPackageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => this.smartDetectorPackage);
            this.testContainer.RegisterInstance<ISmartDetectorRepository>(smartDetectorRepositoryMock.Object);

            this.testContainer.RegisterInstance<IAnalysisServicesFactory>(new Mock<IAnalysisServicesFactory>().Object);

            this.smartDetector = new TestSmartDetector { ExpectedResourceType = smartDetectorResourceType };

            var smartDetectorLoaderMock = new Mock<ISmartDetectorLoader>();
            smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.smartDetectorPackage))
                .Returns(this.smartDetector);
            this.testContainer.RegisterInstance<ISmartDetectorLoader>(smartDetectorLoaderMock.Object);

            var azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourceGroupsInSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.ResourceGroup, subscriptionId, "resourceGroupName", string.Empty) });
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, "resourceGroupName", "resourceName") });
            azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInResourceGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, resourceGroupName, "resourceName") });
            this.testContainer.RegisterInstance<IAzureResourceManagerClient>(azureResourceManagerClientMock.Object);

            this.testContainer.RegisterInstance<IQueryRunInfoProvider>(new Mock<IQueryRunInfoProvider>().Object);

            var stateRepositoryMock = new Mock<IStateRepository>();
            var stateRepositoryFactoryMock = new Mock<IStateRepositoryFactory>();
            stateRepositoryFactoryMock.Setup(m => m.Create(It.IsAny<string>())).Returns(stateRepositoryMock.Object);
            this.testContainer.RegisterInstance<IStateRepositoryFactory>(stateRepositoryFactoryMock.Object);
        }

        private class TestSmartDetector : ISmartDetector
        {
            public bool ShouldStuck { private get; set; }

            public bool ShouldThrow { private get; set; }

            public bool ShouldThrowCustom { private get; set; }

            public bool IsRunning { get; private set; }

            public bool WasCanceled { get; private set; }

            public ResourceType ExpectedResourceType { private get; set; }

            public async Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                this.IsRunning = true;

                Assert.IsNotNull(analysisRequest.TargetResources, "Resources list is null");
                Assert.AreEqual(1, analysisRequest.TargetResources.Count);
                Assert.AreEqual(this.ExpectedResourceType, analysisRequest.TargetResources.Single().ResourceType);

                await analysisRequest.StateRepository.StoreStateAsync("test key", "test state", cancellationToken);

                if (this.ShouldStuck)
                {
                    try
                    {
                        await Task.Delay(int.MaxValue, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this.WasCanceled = true;
                        throw;
                    }
                }

                if (this.ShouldThrow)
                {
                    throw new DivideByZeroException();
                }

                if (this.ShouldThrowCustom)
                {
                    throw new CustomException();
                }

                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert(analysisRequest.TargetResources.First()));
                return await Task.FromResult(alerts);
            }

            private class CustomException : Exception
            {
            }
        }

        private class TestAlert : Alert
        {
            public TestAlert(ResourceIdentifier resourceIdentifier) : base("Test title", resourceIdentifier)
            {
            }

            [AlertPresentationProperty(AlertPresentationSection.Property, "Summary title", InfoBalloon = "Summary info")]
            public string Summary { get; } = "Summary value";
        }
    }
}