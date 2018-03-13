//-----------------------------------------------------------------------
// <copyright file="SmartSignalLoaderTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.SmartDetectorLoader;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// The smart signal loader tests rely on signals that are defined in TestSignalLibrary and TestSignalDependentLibrary.
    /// These DLLs are not directly referenced, but only copied to the output directory - so we can test how the loader dynamically
    /// loads them and runs their signals.
    /// </summary>
    [TestClass]
    public class SmartSignalLoaderTests
    {
        private Dictionary<string, DllInfo> dllInfos;
        private Mock<ITracer> tracerMock;
        private Dictionary<string, SmartDetectorManifest> manifests;
        private Dictionary<string, Dictionary<string, byte[]>> assemblies;

        [TestInitialize]
        public void TestInitialize()
        {
            this.tracerMock = new Mock<ITracer>();

            // Handle DLLs
            this.dllInfos = new Dictionary<string, DllInfo>();
            this.InitializeDll("TestSignalLibrary");
            this.InitializeDll("TestSignalDependentLibrary");
            this.InitializeDll("Newtonsoft.Json");

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            this.manifests = new Dictionary<string, SmartDetectorManifest>()
            {
                ["1"] = new SmartDetectorManifest("1", "Test signal", "Test signal description", Version.Parse("1.0"), "TestSignalLibrary", "TestSignalLibrary.TestSignal", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 }),
                ["2"] = new SmartDetectorManifest("2", "Test signal with dependency", "Test signal with dependency description", Version.Parse("1.0"), "TestSignalLibrary", "TestSignalLibrary.TestSignalWithDependency", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 })
            };

            this.assemblies = new Dictionary<string, Dictionary<string, byte[]>>
            {
                ["1"] = new Dictionary<string, byte[]>()
                {
                    ["TestSignalLibrary"] = this.dllInfos["TestSignalLibrary"].Bytes
                },
                ["2"] = new Dictionary<string, byte[]>()
                {
                    ["TestSignalLibrary"] = this.dllInfos["TestSignalLibrary"].Bytes,
                    ["TestSignalDependentLibrary"] = this.dllInfos["TestSignalDependentLibrary"].Bytes,
                    ["Newtonsoft.Json"] = this.dllInfos["Newtonsoft.Json"].Bytes
                },
                ["3"] = new Dictionary<string, byte[]>()
                {
                    [currentAssembly.GetName().Name] = File.ReadAllBytes(currentAssembly.Location)
                }
            };
        }

        [TestMethod]
        public async Task WhenLoadingSignalFromDllThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSignalFromDll("1", "test title");
        }

        [TestMethod]
        public async Task WhenLoadingSignalFromMultipleDllsThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSignalFromDll("2", "test title - with dependency - [1,2,3]");
        }

        [TestMethod]
        public async Task WhenLoadingSignalTheInstanceIsCreatedSuccessfully()
        {
            await this.TestLoadSignalSimple(typeof(TestSignal));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSignalWithWrongTypeThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalNoInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSignalWithoutDefaultConstructorThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalNoDefaultConstructor));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSignalWithGenericDefinitionThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalGeneric<>));
        }

        [TestMethod]
        public async Task WhenLoadingSignalWithConcreteGenericDefinitionThenItWorks()
        {
            await this.TestLoadSignalSimple(typeof(TestSignalGeneric<string>), typeof(string).Name);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Rename DLLs back
            foreach (var info in this.dllInfos.Values)
            {
                File.Delete(info.OriginalPath);
                File.Move(info.NewPath, info.OriginalPath);
            }
        }

        private async Task TestLoadSignalSimple(Type signalType, string expectedTitle = "test test test")
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tracerMock.Object);
            SmartDetectorManifest manifest = new SmartDetectorManifest("3", "simple", "description", Version.Parse("1.0"), signalType.Assembly.GetName().Name, signalType.FullName, new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 });
            SmartDetectorPackage package = new SmartDetectorPackage(manifest, this.assemblies["3"]);
            ISmartDetector detector = loader.LoadSmartDetector(package);
            Assert.IsNotNull(detector, "Smart Detector is NULL");

            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new List<ResourceIdentifier> { resource },
                DateTime.UtcNow.AddMinutes(-20),
                TimeSpan.FromDays(1),
                null,
                new Mock<IAnalysisServicesFactory>().Object);
            List<Alert> alerts = await detector.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, alerts.Count, "Incorrect number of result items returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Result item title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Result item resource identifier is wrong");
        }

        private async Task TestLoadSignalFromDll(string signalId, string expectedTitle)
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tracerMock.Object);
            SmartDetectorPackage package = new SmartDetectorPackage(this.manifests[signalId], this.assemblies[signalId]);
            ISmartDetector detector = loader.LoadSmartDetector(package);
            Assert.IsNotNull(detector, "Smart Detector is NULL");

            var resource = new ResourceIdentifier(ResourceType.VirtualMachine, "someSubscription", "someGroup", "someVM");
            var analysisRequest = new AnalysisRequest(
                new List<ResourceIdentifier> { resource },
                DateTime.UtcNow.AddMinutes(-20),
                TimeSpan.FromDays(1),
                null,
                new Mock<IAnalysisServicesFactory>().Object);
            List<Alert> alerts = await detector.AnalyzeResourcesAsync(analysisRequest, this.tracerMock.Object, default(CancellationToken));
            Assert.AreEqual(1, alerts.Count, "Incorrect number of result items returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Result item title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Result item resource identifier is wrong");
        }

        private void InitializeDll(string name)
        {
            // Determine DLL path
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (folder == null)
            {
                throw new NullReferenceException();
            }

            string path = Path.Combine(folder, name + ".dll");

            // Read DLL bytes
            byte[] bytes = File.ReadAllBytes(path);

            // Rename the DLL
            string newPath = Path.ChangeExtension(path, ".x.dll");
            File.Delete(newPath);
            File.Move(path, newPath);

            // Save information
            this.dllInfos[name] = new DllInfo()
            {
                OriginalPath = path,
                NewPath = newPath,
                Bytes = bytes
            };
        }

        private class DllInfo
        {
            public string OriginalPath { get; set; }

            public string NewPath { get; set; }

            public byte[] Bytes { get; set; }
        }

        private class TestResultItem : Alert
        {
            public TestResultItem(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
            {
            }
        }

        private class TestSignal : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestResultItem("test test test", analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }

        private class TestSignalNoInterface
        {
        }

        private class TestSignalNoDefaultConstructor : ISmartDetector
        {
            private readonly string message;

            public TestSignalNoDefaultConstructor(string message)
            {
                this.message = message;
            }

            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestResultItem(this.message, analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }

        private class TestSignalGeneric<T> : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestResultItem(typeof(T).Name, analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }
    }
}