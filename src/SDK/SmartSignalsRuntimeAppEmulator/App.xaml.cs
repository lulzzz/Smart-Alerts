//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Emulator
{
    using System;
    using System.IO;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.SmartDetectorLoader;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.Azure.Monitoring.SmartSignals.Emulator.Models;
    using Microsoft.Win32;
    using Unity;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the unity container.
        /// </summary>
        public static IUnityContainer Container { get; private set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            ITracer stringTracer = new StringTracer(string.Empty);
            ITracer consoleTracer = new ConsoleTracer(string.Empty);
            var smartDetectorLoader = new SmartDetectorLoader(consoleTracer);

            // *Temporary*: if package file path wasn't accepted, raise file selection window to allow package file selection.
            // This option should be removed before launching version for customers (bug for tracking: 1177247)
            string signalPackagePath = e.Args.Length != 1 ? 
                this.GetSignalPackagePath() : 
                Diagnostics.EnsureStringNotNullOrWhiteSpace(() => e.Args[0]);
            
            SmartDetectorPackage smartDetectorPackage;
            using (var fileStream = new FileStream(signalPackagePath, FileMode.Open))
            {
                smartDetectorPackage = SmartDetectorPackage.CreateFromStream(fileStream, consoleTracer);
            }

            SmartDetectorManifest smartDetectorManifest = smartDetectorPackage.Manifest;
            ISmartDetector detector = smartDetectorLoader.LoadSmartDetector(smartDetectorPackage);

            // Authenticate the user to Active Directory
            var authenticationServices = new AuthenticationServices();
            authenticationServices.AuthenticateUser();
            ICredentialsFactory credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices);

            IAzureResourceManagerClient azureResourceManagerClient = new AzureResourceManagerClient(credentialsFactory, consoleTracer);

            // Create analysis service factory
            var queryRunInroProvider = new QueryRunInfoProvider(azureResourceManagerClient);
            var httpClientWrapper = new HttpClientWrapper();
            IAnalysisServicesFactory analysisServicesFactory = new AnalysisServicesFactory(consoleTracer, httpClientWrapper, credentialsFactory, azureResourceManagerClient, queryRunInroProvider);

            var signalRunner = new SmartSignalRunner(detector, analysisServicesFactory, queryRunInroProvider, smartDetectorManifest, stringTracer);

            // Create a Unity container with all the required models and view models registrations
            Container = new UnityContainer();
            Container
                .RegisterInstance(stringTracer)
                .RegisterInstance(new SignalsResultsRepository())
                .RegisterInstance(authenticationServices)
                .RegisterInstance(azureResourceManagerClient)
                .RegisterInstance(detector)
                .RegisterInstance(smartDetectorManifest)
                .RegisterInstance(analysisServicesFactory)
                .RegisterInstance(signalRunner);
        }

        /// <summary>
        /// Raises file selection dialog window to allow the user to select package file.
        /// </summary>
        /// <returns>The selected package file path or null if no file was selected</returns>
        private string GetSignalPackagePath()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }
    }
}
