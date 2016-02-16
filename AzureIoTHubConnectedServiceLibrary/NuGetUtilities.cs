// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.Win32;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// NuGet package utilities.
    /// </summary>
    internal static class NuGetUtilities
    {
        private const string RepositoryRegistryKeyPath = @"SOFTWARE\NuGet\Repository";

        /// <summary>
        /// Get local repository folder path from registory: HKLM\Software\NuGet\Repository
        /// </summary>
        public static string GetPackageRepositoryPath(string repositoryName)
        {
            Debug.Assert(!string.IsNullOrEmpty(repositoryName));

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NuGetUtilities.RepositoryRegistryKeyPath))
            {
                return key?.GetValue(repositoryName) as string;
            }
        }

        /// <summary>
        /// Ensures the appropriate version of the specified packages are installed.  If an existing version of the package
        /// already exists the following will happen:
        /// If a semantically compatible version already exists, no change is made to the package.
        /// If an older major version exists, a warning is logged and the package is upgraded.
        /// If an older minor/build version exists, an information message is logged and the package is upgraded.
        /// If a newer major version exists, a warning is logged and no change is made to the package.
        /// </summary>
        public static async Task InstallPackagesAsync(
            Dictionary<string, string> packages,
            string extensionId,
            ConnectedServiceLogger logger,
            Project project,
            IVsPackageInstallerServices packageInstallerServices,
            IVsPackageInstaller packageInstaller)
        {
            Dictionary<string, string> packagesToInstall = new Dictionary<string, string>();

            await NuGetUtilities.InstallPackagesAsync(
                project,
                packages,
                (packageId, packageVersion) =>
                {
                    packagesToInstall.Add(packageId, packageVersion);
                    return Task.FromResult<object>(null);
                },
                logger,
                packageInstallerServices);

            if (packagesToInstall.Any())
            {
                packageInstaller.InstallPackagesFromVSExtensionRepository(extensionId, false, false, project, packagesToInstall);
            }
        }

        /// <summary>
        /// Uninstall the packages that exist in the project.
        /// </summary>
        public static async Task UninstallPackagesAsync(
            Project targetProject,
            ISet<string> packages,
            Func<string, Task> uninstallPackage,
            ConnectedServiceLogger logger,
            IVsPackageInstallerServices packageInstallerServices)
        {
            IEnumerable<IVsPackageMetadata> installedPackages = packageInstallerServices.GetInstalledPackages(targetProject);

            foreach (string packageId in packages)
            {
                IVsPackageMetadata installedPackage = installedPackages.FirstOrDefault(p => p.Id == packageId);
                if (installedPackage != null)
                {
                    await logger.WriteMessageAsync(
                        LoggerMessageCategory.Information,
                        Resource.LogMessage_RemovingNuGetPackage,
                        packageId,
                        installedPackage.VersionString);

                    await uninstallPackage(packageId);
                }
            }
        }

        /// <summary>
        /// Ensures the appropriate version of the specified packages are installed.  If an existing version of the package
        /// already exists the following will happen:
        /// If a semantically compatible version already exists, no change is made to the package.
        /// If an older major version exists, a warning is logged and the package is upgraded.
        /// If an older minor/build version exists, an information message is logged and the package is upgraded.
        /// If a newer major version exists, a warning is logged and no change is made to the package.
        /// </summary>
        public static async Task InstallPackagesAsync(
            Project targetProject,
            Dictionary<string, string> packages,
            Func<string, string, Task> installPackage,
            ConnectedServiceLogger logger,
            IVsPackageInstallerServices packageInstallerServices)
        {
            IEnumerable<IVsPackageMetadata> installedPackages;
            try
            {
                installedPackages = packageInstallerServices.GetInstalledPackages(targetProject);
            }
            catch (ArgumentException)
            {
                // This happens for C++ projects
                installedPackages = new List<IVsPackageMetadata>();
            }

            foreach (KeyValuePair<string, string> requiredPackage in packages)
            {
                IVsPackageMetadata installedPackage = installedPackages.FirstOrDefault(p => p.Id == requiredPackage.Key);
                if (installedPackage == null)
                {
                    // The package does not exist - notify and install the package.
                    await logger.WriteMessageAsync(
                        LoggerMessageCategory.Information,
                        Resource.LogMessage_AddingNuGetPackage,
                        requiredPackage.Key,
                        requiredPackage.Value);
                }
                else
                {
                    Version installedVersion = NuGetUtilities.GetVersion(installedPackage.VersionString);
                    Version requiredVersion = NuGetUtilities.GetVersion(requiredPackage.Value);
                    if (installedVersion == null || requiredVersion == null)
                    {
                        // Unable to parse the version - continue.
                        continue;
                    }
                    else if (installedVersion.Major < requiredVersion.Major)
                    {
                        // An older potentially non-compatible version of the package already exists - warn and upgrade the package.
                        await logger.WriteMessageAsync(
                            LoggerMessageCategory.Warning,
                            Resource.LogMessage_OlderMajorVersionNuGetPackageExists,
                            requiredPackage.Key,
                            installedPackage.VersionString,
                            requiredPackage.Value);
                    }
                    else if (installedVersion.Major > requiredVersion.Major)
                    {
                        // A newer potentially non-compatible version of the package already exists - warn and continue.
                        await logger.WriteMessageAsync(
                            LoggerMessageCategory.Warning,
                            Resource.LogMessage_NewerMajorVersionNuGetPackageExists,
                            requiredPackage.Key,
                            requiredPackage.Value,
                            installedPackage.VersionString);

                        continue;
                    }
                    else if (installedVersion >= requiredVersion)
                    {
                        // A semantically compatible version of the package already exists - continue.
                        continue;
                    }
                    else
                    {
                        // An older semantically compatible version of the package exists - notify and upgrade the package.
                        await logger.WriteMessageAsync(
                            LoggerMessageCategory.Information,
                            Resource.LogMessage_UpgradingNuGetPackage,
                            requiredPackage.Key,
                            installedPackage.VersionString,
                            requiredPackage.Value);
                    }
                }

                await installPackage(requiredPackage.Key, requiredPackage.Value);
            }
        }

        private static Version GetVersion(string versionString)
        {
            Version version;
            int index = versionString.IndexOfAny(new char[] { '-', '+' });
            if (index != -1)
            {
                // Ignore any pre-release/build versions - they are not taken into account when comparing versions.
                versionString = versionString.Substring(0, index);
            }

            if (!Version.TryParse(versionString, out version))
            {
                Debug.Fail("Unable to parse the NuGet package version " + versionString);
            }

            return version;
        }
    }
}
