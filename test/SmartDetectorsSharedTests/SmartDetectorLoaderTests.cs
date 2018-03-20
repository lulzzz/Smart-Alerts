//-----------------------------------------------------------------------
// <copyright file="SmartDetectorLoaderTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
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
    /// The Smart Detector loader tests rely on detectors that are defined in TestSmartDetectorLibrary and TestSmartDetectorDependentLibrary.
    /// These DLLs are not directly referenced, but only copied to the output directory - so we can test how the loader dynamically
    /// loads them and runs their Smart Detectors.
    /// </summary>
    [TestClass]
    public class SmartDetectorLoaderTests
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
            this.InitializeDll("TestSmartDetectorLibrary");
            this.InitializeDll("TestSmartDetectorDependentLibrary");
            this.InitializeDll("Newtonsoft.Json");

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            this.manifests = new Dictionary<string, SmartDetectorManifest>()
            {
                ["1"] = new SmartDetectorManifest("1", "Test Smart Detector", "Test Smart Detector description", Version.Parse("1.0"), "TestSmartDetectorLibrary", "TestSmartDetectorLibrary.TestSmartDetector", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 }),
                ["2"] = new SmartDetectorManifest("2", "Test Smart Detector with dependency", "Test Smart Detector with dependency description", Version.Parse("1.0"), "TestSmartDetectorLibrary", "TestSmartDetectorLibrary.TestSmartDetectorWithDependency", new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 })
            };

            this.assemblies = new Dictionary<string, Dictionary<string, byte[]>>
            {
                ["1"] = new Dictionary<string, byte[]>()
                {
                    ["TestSmartDetectorLibrary"] = this.dllInfos["TestSmartDetectorLibrary"].Bytes
                },
                ["2"] = new Dictionary<string, byte[]>()
                {
                    ["TestSmartDetectorLibrary"] = this.dllInfos["TestSmartDetectorLibrary"].Bytes,
                    ["TestSmartDetectorDependentLibrary"] = this.dllInfos["TestSmartDetectorDependentLibrary"].Bytes,
                    ["Newtonsoft.Json"] = this.dllInfos["Newtonsoft.Json"].Bytes
                },
                ["3"] = new Dictionary<string, byte[]>()
                {
                    [currentAssembly.GetName().Name] = File.ReadAllBytes(currentAssembly.Location)
                }
            };
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorFromDllThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSmartDetectorFromDll("1", "test title");
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorFromMultipleDllsThenItIsLoadedSuccessfully()
        {
            await this.TestLoadSmartDetectorFromDll("2", "test title - with dependency - [1,2,3]");
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorTheInstanceIsCreatedSuccessfully()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetector));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithWrongTypeThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorNoInterface));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithoutDefaultConstructorThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorNoDefaultConstructor));
        }

        [TestMethod]
        [ExpectedException(typeof(SmartDetectorLoadException))]
        public async Task WhenLoadingSmartDetectorWithGenericDefinitionThenTheCorrectExceptionIsThrown()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorGeneric<>));
        }

        [TestMethod]
        public async Task WhenLoadingSmartDetectorWithConcreteGenericDefinitionThenItWorks()
        {
            await this.TestLoadSmartDetectorSimple(typeof(TestSmartDetectorGeneric<string>), typeof(string).Name);
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

        private async Task TestLoadSmartDetectorSimple(Type smartDetectorType, string expectedTitle = "test test test")
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tracerMock.Object);
            SmartDetectorManifest manifest = new SmartDetectorManifest("3", "simple", "description", Version.Parse("1.0"), smartDetectorType.Assembly.GetName().Name, smartDetectorType.FullName, new List<ResourceType>() { ResourceType.Subscription }, new List<int> { 60 });
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
            Assert.AreEqual(1, alerts.Count, "Incorrect number of alerts returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Alert title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Alert resource identifier is wrong");
        }

        private async Task TestLoadSmartDetectorFromDll(string smartDetectorlId, string expectedTitle)
        {
            ISmartDetectorLoader loader = new SmartDetectorLoader(this.tracerMock.Object);
            SmartDetectorPackage package = new SmartDetectorPackage(this.manifests[smartDetectorlId], this.assemblies[smartDetectorlId]);
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
            Assert.AreEqual(1, alerts.Count, "Incorrect number of alerts returned");
            Assert.AreEqual(expectedTitle, alerts.Single().Title, "Alert title is wrong");
            Assert.AreEqual(resource, alerts.Single().ResourceIdentifier, "Alert resource identifier is wrong");
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

        private class TestAlert : Alert
        {
            public TestAlert(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
            {
            }
        }

        private class TestSmartDetector : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert("test test test", analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }
       
        private class TestSmartDetectorNoInterface
        {
        }

        private class TestSmartDetectorNoDefaultConstructor : ISmartDetector
        {
            private readonly string message;

            public TestSmartDetectorNoDefaultConstructor(string message)
            {
                this.message = message;
            }

            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert(this.message, analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }

        private class TestSmartDetectorGeneric<T> : ISmartDetector
        {
            public Task<List<Alert>> AnalyzeResourcesAsync(AnalysisRequest analysisRequest, ITracer tracer, CancellationToken cancellationToken)
            {
                List<Alert> alerts = new List<Alert>();
                alerts.Add(new TestAlert(typeof(T).Name, analysisRequest.TargetResources.Single()));
                return Task.FromResult(alerts);
            }
        }
    }
}