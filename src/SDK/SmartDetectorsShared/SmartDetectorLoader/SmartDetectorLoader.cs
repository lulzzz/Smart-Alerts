//-----------------------------------------------------------------------
// <copyright file="SmartDetectorLoader.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.SmartDetectorLoader
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Azure.Monitoring.SmartDetectors.Tools;

    /// <summary>
    /// Implementation of the <see cref="ISmartDetectorLoader"/> interface.
    /// </summary>
    public class SmartDetectorLoader : ISmartDetectorLoader
    {
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartDetectorLoader"/> class.
        /// </summary>
        /// <param name="tracer">The tracer</param>
        public SmartDetectorLoader(ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);
        }

        #region Implementation of ISmartDetectorLoader

        /// <summary>
        /// Loads a Smart Detector. 
        /// This method load the Smart Detector's assembly into the current application domain,
        /// and creates the Smart Detector object using reflection.
        /// </summary>
        /// <param name="smartDetectorPackage">The Smart Detector package.</param>
        /// <returns>The Smart Detector object.</returns>
        /// <exception cref="SmartDetectorLoadException">
        /// Thrown if an error occurred during the Smart Detector load (either due to assembly load
        /// error or failure to create the Smart Detector object).
        /// </exception>
        public ISmartDetector LoadSmartDetector(SmartDetectorPackage smartDetectorPackage)
        {
            SmartDetectorManifest smartDetectorManifest = smartDetectorPackage.Manifest;
            IReadOnlyDictionary<string, byte[]> smartDetectorAssemblies = smartDetectorPackage.Content;
            try
            {
                this.tracer.TraceInformation($"Read {smartDetectorAssemblies.Count} assemblies for Smart Detector ID {smartDetectorManifest.Id}");

                // Add assembly resolver, that uses the Smart Detector's assemblies
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    this.tracer.TraceInformation($"Resolving assembly {args.Name} for Smart Detector ID {smartDetectorManifest.Id}");

                    // Get the short name of the assembly (AssemblyName.Name)
                    AssemblyName assemblyName = new AssemblyName(args.Name);
                    string name = assemblyName.Name;

                    // Try to find the assembly bytes in the Smart Detector's assemblies
                    if (smartDetectorAssemblies.TryGetValue(name, out byte[] assemblyBytes))
                    {
                        // Load the assembly from its bytes
                        return Assembly.Load(assemblyBytes);
                    }

                    return null;
                };

                // Find the main Smart Detector assembly
                if (!smartDetectorAssemblies.TryGetValue(smartDetectorManifest.AssemblyName, out byte[] smartDetectorMainAssemblyBytes))
                {
                    throw new SmartDetectorLoadException($"Unable to find main Smart Detector assembly: {smartDetectorManifest.AssemblyName}");
                }

                Assembly mainSmartDetectorAssembly = Assembly.Load(smartDetectorMainAssemblyBytes);

                // Get the Smart Detector type from the assembly
                this.tracer.TraceInformation($"Creating Smart Detector for {smartDetectorManifest.Name}, version {smartDetectorManifest.Version}, using type {smartDetectorManifest.ClassName}");
                Type smartDetectorType = mainSmartDetectorAssembly.GetType(smartDetectorManifest.ClassName);
                if (smartDetectorType == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorManifest.ClassName} was not found in the main Smart Detector assembly {smartDetectorManifest.AssemblyName}");
                }

                // Check if the type inherits from ISmartDetector
                if (!typeof(ISmartDetector).IsAssignableFrom(smartDetectorType))
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} does not extend ISmartDetector");
                }

                // Check that type is not abstract
                if (smartDetectorType.IsAbstract)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} is abstract - a Smart Detector must be a concrete type");
                }

                // Check that type is not generic
                if (smartDetectorType.IsGenericTypeDefinition)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} is generic - a Smart Detector must be a closed constructed type");
                }

                // Check that type has a parameter-less constructor
                if (smartDetectorType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector type {smartDetectorType.Name} does not have a public, parameter-less constructor");
                }

                // Create the Smart Detector object
                ISmartDetector smartDetector = Activator.CreateInstance(smartDetectorType) as ISmartDetector;
                if (smartDetector == null)
                {
                    throw new SmartDetectorLoadException($"Smart Detector {smartDetectorType.Name} failed to be created - instance is null");
                }

                this.tracer.TraceInformation($"Successfully created Smart Detector of type {smartDetectorType.Name}");
                return smartDetector;
            }
            catch (Exception e)
            {
                this.tracer.TrackEvent(
                    "FailedToLoadSmartDetector",
                    properties: new Dictionary<string, string>
                    {
                        { "smartDetectorId", smartDetectorManifest.Id },
                        { "SmartDetectorName", smartDetectorManifest.Name },
                        { "ExceptionType", e.GetType().Name },
                        { "ExceptionMessage", e.Message },
                    });

                throw new SmartDetectorLoadException($"Failed to load Smart Detector {smartDetectorManifest.Name}", e);
            }
        }

        #endregion
    }
}