﻿//-----------------------------------------------------------------------
// <copyright file="PackageSmartDetector.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartDetectors.Build
{
    using System.IO;
    using System.Security;
    using Microsoft.Azure.Monitoring.SmartDetectors.Package;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Represents the build task of a Smart Detector
    /// </summary>
    public class PackageSmartDetector : Task
    {
        /// <summary>
        /// Gets or sets the path of the zipped package.
        /// </summary>
        public string PackagePath { private get; set; }

        /// <summary>
        /// Gets or sets the name of the zipped package.
        /// </summary>
        public string PackageName { private get; set; }

        /// <summary>
        /// Executes PackageSmartDetector task. 
        /// </summary>
        /// <returns>True if the task successfully executed; otherwise, False.</returns>
        public override bool Execute()
        {
            try
            {
                SmartDetectorPackage package = SmartDetectorPackage.CreateFromFolder(this.PackagePath);
                package.SaveToFile(Path.Combine(this.PackagePath, this.PackageName));
            }
            catch (InvalidSmartDetectorPackageException exception)
            {
                Log.LogError(exception.Message);
                return false;
            }
            catch (IOException ioe)
            {
                Log.LogError($"Failed to create Smart Detector Package - failed creating the package file: {ioe.Message}");
                return false;
            }
            catch (SecurityException securityException)
            {
                Log.LogError($"Failed to create Smart Detector Package - failed creating the package file: {securityException.Message}");
                return false;
            }

            return true;
        }
    }
}
