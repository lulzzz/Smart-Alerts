//-----------------------------------------------------------------------
// <copyright file="SmartSignalRunnerTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Presentation;
    using Microsoft.Azure.Monitoring.SmartDetectors.SmartDetectorLoader;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SmartSignalRunnerTests
    {
        private SmartDetectorPackage smartDetectorPackage;
        private List<string> resourceIds;
        private SmartDetectorRequest request;
        private TestSignal signal;
        private Mock<ITracer> tracerMock;
        private Mock<ISmartSignalRepository> smartSignalsRepositoryMock;
        private Mock<ISmartDetectorLoader> smartDetectorLoaderMock;
        private Mock<IAnalysisServicesFactory> analysisServicesFactoryMock;
        private Mock<IAzureResourceManagerClient> azureResourceManagerClientMock;
        private Mock<IQueryRunInfoProvider> queryRunInfoProviderMock;

        [TestInitialize]
        public void TestInitialize()
        {
            this.TestInitialize(ResourceType.VirtualMachine, ResourceType.VirtualMachine);
        }

        [TestMethod]
        public async Task WhenRunningSignalThenTheCorrectResultItemIsReturned()
        {
            // Run the signal and validate results
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartDetectorLoaderMock.Object, this.analysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.queryRunInfoProviderMock.Object, this.tracerMock.Object);
            List<AlertPresentation> alertPresentations = await runner.RunAsync(this.request, default(CancellationToken));
            Assert.IsNotNull(alertPresentations, "Presentation list is null");
            Assert.AreEqual(1, alertPresentations.Count);
            Assert.AreEqual("Test title", alertPresentations.Single().Title);
        }

        [TestMethod]
        public void WhenRunningSignalThenCancellationIsHandledGracefully()
        {
            // Notify the signal that it should get stuck and wait for cancellation
            this.signal.ShouldStuck = true;

            // Run the signal asynchronously
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartDetectorLoaderMock.Object, this.analysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.queryRunInfoProviderMock.Object, this.tracerMock.Object);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task t = runner.RunAsync(this.request, cancellationTokenSource.Token);
            SpinWait.SpinUntil(() => this.signal.IsRunning);

            // Cancel and wait for expected result
            cancellationTokenSource.Cancel();
            try
            {
                t.Wait(TimeSpan.FromSeconds(10));
            }
            catch (AggregateException e) when ((e.InnerExceptions.Single() as SmartSignalCustomException).SignalExceptionType == typeof(TaskCanceledException).ToString())
            {
                Assert.IsTrue(this.signal.WasCanceled, "The signal was not canceled!");
            }
        }

        [TestMethod]
        public async Task WhenRunningSignalThenExceptionsAreHandledCorrectly()
        {
            // Notify the signal that it should throw an exception
            this.signal.ShouldThrow = true;

            // Run the signal
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartDetectorLoaderMock.Object, this.analysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.queryRunInfoProviderMock.Object, this.tracerMock.Object);
            try
            {
                await runner.RunAsync(this.request, default(CancellationToken));
            }
            catch (SmartSignalCustomException e) when (e.SignalExceptionType == typeof(DivideByZeroException).ToString())
            {
                // Expected exception
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SmartSignalCustomException))]
        public async Task WhenRunningSignalThenCustomExceptionsAreHandledCorrectly()
        {
            // Notify the signal that it should throw a custom exception
            this.signal.ShouldThrowCustom = true;

            // Run the signal
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartDetectorLoaderMock.Object, this.analysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.queryRunInfoProviderMock.Object, this.tracerMock.Object);
            await runner.RunAsync(this.request, default(CancellationToken));
        }

        [TestMethod]
        public async Task WhenRunningSignalWithSupportedTypeThenTheCorrectResultsAreReturned()
        {
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.Subscription, false);
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.ResourceGroup, false);
            await this.RunSignalWithResourceTypes(ResourceType.Subscription, ResourceType.VirtualMachine, false);
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.ResourceGroup, false);
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.VirtualMachine, false);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.VirtualMachine, false);
        }

        [TestMethod]
        public async Task WhenRunningSignalWithUnsupportedTypeThenAnExceptionIsThrown()
        {
            await this.RunSignalWithResourceTypes(ResourceType.ResourceGroup, ResourceType.Subscription, true);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.Subscription, true);
            await this.RunSignalWithResourceTypes(ResourceType.VirtualMachine, ResourceType.ResourceGroup, true);
        }

        private async Task RunSignalWithResourceTypes(ResourceType requestResourceType, ResourceType signalResourceType, bool shouldFail)
        {
            this.TestInitialize(requestResourceType, signalResourceType);
            ISmartSignalRunner runner = new SmartSignalRunner(this.smartSignalsRepositoryMock.Object, this.smartDetectorLoaderMock.Object, this.analysisServicesFactoryMock.Object, this.azureResourceManagerClientMock.Object, this.queryRunInfoProviderMock.Object, this.tracerMock.Object);
            try
            {
                List<AlertPresentation> alertPresentations = await runner.RunAsync(this.request, default(CancellationToken));
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

        private void TestInitialize(ResourceType requestResourceType, ResourceType signalResourceType)
        {
            this.tracerMock = new Mock<ITracer>();

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
            this.request = new SmartDetectorRequest(this.resourceIds, "1", DateTime.UtcNow.AddDays(-1), TimeSpan.FromDays(1), new SmartDetectorSettings());

            var smartDetectorManifest = new SmartDetectorManifest("1", "Test signal", "Test signal description", Version.Parse("1.0"), "assembly", "class", new List<ResourceType>() { signalResourceType }, new List<int> { 60 });
            this.smartDetectorPackage = new SmartDetectorPackage(smartDetectorManifest, new Dictionary<string, byte[]> { ["TestSignalLibrary"] = new byte[0] });

            this.smartSignalsRepositoryMock = new Mock<ISmartSignalRepository>();
            this.smartSignalsRepositoryMock
                .Setup(x => x.ReadSignalPackageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => this.smartDetectorPackage);

            this.analysisServicesFactoryMock = new Mock<IAnalysisServicesFactory>();

            this.signal = new TestSignal { ExpectedResourceType = signalResourceType };

            this.smartDetectorLoaderMock = new Mock<ISmartDetectorLoader>();
            this.smartDetectorLoaderMock
                .Setup(x => x.LoadSmartDetector(this.smartDetectorPackage))
                .Returns(this.signal);

            this.azureResourceManagerClientMock = new Mock<IAzureResourceManagerClient>();
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourceGroupsInSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.ResourceGroup, subscriptionId, "resourceGroupName", string.Empty) });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInSubscriptionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, "resourceGroupName", "resourceName") });
            this.azureResourceManagerClientMock
                .Setup(x => x.GetAllResourcesInResourceGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<ResourceType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string subscriptionId, string resourceGroupName, IEnumerable<ResourceType> resourceTypes, CancellationToken cancellationToken) => new List<ResourceIdentifier>() { new ResourceIdentifier(ResourceType.VirtualMachine, subscriptionId, resourceGroupName, "resourceName") });

            this.queryRunInfoProviderMock = new Mock<IQueryRunInfoProvider>();
        }

        private class TestSignal : ISmartDetector
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
                alerts.Add(new TestSignalResultItem(analysisRequest.TargetResources.First()));
                return await Task.FromResult(alerts);
            }

            private class CustomException : Exception
            {
            }
        }

        private class TestSignalResultItem : Alert
        {
            public TestSignalResultItem(ResourceIdentifier resourceIdentifier) : base("Test title", resourceIdentifier)
            {
            }

            [AlertPresentationProperty(AlertPresentationSection.Property, "Summary title", InfoBalloon = "Summary info")]
            public string Summary { get; } = "Summary value";
        }
    }
}