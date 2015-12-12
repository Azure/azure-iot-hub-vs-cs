using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NuGet.VisualStudio;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AzureIoTHubConnectedService
{
    [ConnectedServiceHandlerExport("Microsoft.AzureIoTHubService",
    AppliesTo = "CSharp")]
    internal class Handler : ConnectedServiceHandler
    {

        [Import]
        private IVsPackageInstaller PackageInstaller { get; set; }

        [Import]
        private IVsPackageInstallerServices PackageInstallerServices { get; set; }

        public override async Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken ct)
        {
            HandlerManifest configuration = await this.BuildHandlerManifest(context);

            await this.AddSdkReferenceAsync(context, configuration, ct);

            var tokenDict = new Dictionary<string, string>();
            tokenDict.Add("iotHubUri", context.ServiceInstance.Metadata["iotHubUri"] as string);

            foreach (var fileToAdd in configuration.Files)
            {
                var file = this.CopyResourceToTemporaryPath(fileToAdd.Path, context.HandlerHelper, tokenDict);
                string targetPath = Path.GetFileName(fileToAdd.Path); // Use the same name
                string addedFile = await context.HandlerHelper.AddFileAsync(file, targetPath);
            }

            AddServiceInstanceResult result = new AddServiceInstanceResult(
                "Sample",
                new Uri("https://github.com/Microsoft/ConnectedServicesSdkSamples"));

            return result;
        }

        protected string UriPrefix
        {
            get {
                return "pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString() + ";component/Resources/";
            }
        }

        string CopyResourceToTemporaryPath(string resource, ConnectedServiceHandlerHelper helper, Dictionary<string, string> tokenDict)
        {
#if false
            int k = 0;
            try {
                var uriPrefix = "pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString();
                var uri = new Uri(uriPrefix + ";component/Resources/CSharp/SendDataToAzureIoTHub.cs");
                new StreamReader(Application.GetResourceStream(uri).Stream);
            }
            catch(Exception ex)
            {
                ex = ex;
            }

            try
            {
                var uriPrefix = "pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString();
                var uri = new Uri(uriPrefix + ";component/SendDataToAzureIoTHub.cs");
                var reader = new StreamReader(Application.GetResourceStream(uri).Stream);
                var path = Path.GetTempFileName();
                File.WriteAllText(path, reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                ex = ex;
            }

            try
            {
                var uriPrefix = "pack://application:,,/" + Assembly.GetAssembly(this.GetType()).ToString();
                var uri = new Uri(uriPrefix + ";component/CSharp/SendDataToAzureIoTHub.cs");
                var reader = new StreamReader(Application.GetResourceStream(uri).Stream);
                var path = Path.GetTempFileName();
                File.WriteAllText(path, reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                ex = ex;
            }

            try
            {
                var uriPrefix = "";
                var uri = new Uri("/Resources/CSharp/SendDataToAzureIoTHub.cs", UriKind.Relative);
                var reader = new StreamReader(Application.GetResourceStream(uri).Stream);
                var path = Path.GetTempFileName();
                File.WriteAllText(path, reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                ex = ex;
            }
#endif
            using (var reader = new StreamReader(Application.GetResourceStream(new Uri(this.UriPrefix + resource)).Stream))
            {
                var path = Path.GetTempFileName();
                var text = reader.ReadToEnd();

                var replaced = helper.PerformTokenReplacement(text, tokenDict);

                File.WriteAllText(path, replaced);

                return path;
            }
        }

        protected virtual Task<HandlerManifest> BuildHandlerManifest(ConnectedServiceHandlerContext context)
        {
            HandlerManifest manifest = new HandlerManifest();

            manifest.PackageReferences.Add(new NuGetReference("Newtonsoft.Json", "6.0.8"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Amqp", "1.0.0-preview-003"));
            manifest.PackageReferences.Add(new NuGetReference("Microsoft.Azure.Devices.Client", "1.0.0-preview-007"));

            manifest.Files.Add(new FileToAdd("CSharp/SendDataToAzureIoTHub.cs", @"path\path"));

            return Task.FromResult(manifest);

        }

        private async Task AddSdkReferenceAsync(ConnectedServiceHandlerContext context, HandlerManifest manifest, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Dictionary<string, string> packages = new Dictionary<string, string>();
            manifest.PackageReferences.ForEach(nuget => packages.Add(nuget.Id, nuget.Version));

            await NuGetUtilities.InstallPackagesAsync(
                packages,
                "AzureIoTHubConnectedService.MSFT.5750a17f-a0c1-455d-ab31-82004e56e0b0",
                context.Logger,
                ProjectUtilities.GetDteProject(context.ProjectHierarchy),
                this.PackageInstallerServices,
                this.PackageInstaller);
        }
    }

    internal class NuGetReference
    {
        public NuGetReference(string packageId, string packageVersion)
        {
            this.Id = packageId;
            this.Version = packageVersion;
        }

        public string Id { get; set; }
        public string Version { get; set; }
    }

    /// <summary>
    /// The root of the handler manifest file.
    /// </summary>
    public class HandlerManifest
    {
        private List<SnippetToInsert> snippets;
        private List<FileToAdd> files;
        private List<NuGetReference> packageReferences;

        /// <summary>
        /// Gets or sets a list of snippet groups that will be inserted into the project.
        /// </summary>
        internal List<SnippetToInsert> Snippets { get { return snippets; } }

        /// <summary>
        /// Gets or sets a list of file groups that will be added to the project.
        /// </summary>
        internal List<FileToAdd> Files { get { return files; } }

        /// <summary>
        /// Gets or sets a list of NuGet references that will be added to the project.
        /// </summary>
        internal List<NuGetReference> PackageReferences { get { return packageReferences; } }

        internal HandlerManifest()
        {
            this.files = new List<FileToAdd>();
            this.snippets = new List<SnippetToInsert>();
            this.packageReferences = new List<NuGetReference>();
        }
    }

    /// <summary>
    /// The common attributes for a particular item that will be added or inserted into the project.
    /// </summary>
    internal abstract class ContentItem
    {
        /// <summary>
        /// Gets or sets the path to the file in question.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// Add a specific file to the project.
    /// </summary>
    internal class FileToAdd : ContentItem
    {
        public FileToAdd(string resourcePath, string targetFilename)
        {
            this.Path = resourcePath;
            this.TargetFilename = targetFilename;
        }

        /// <summary>
        /// Gets or sets the filename to use for the newly added file.
        /// </summary>
        /// <remarks>
        /// This is the filename only and cannot be a path.
        /// </remarks>
        public string TargetFilename { get; set; }
    }

    internal class SnippetToInsert : ContentItem
    {
        public SnippetToInsert(string snippetPath, InjectionContext target)
        {
            this.Path = snippetPath;
            this.Target = target;
        }

        /// <summary>
        /// Gets or sets the target for the snippet insertion.
        /// </summary>
        public InjectionContext Target { get; set; }
    }

    /// <summary>
    /// The context for where to add a specific snippet within a project.
    /// </summary>
    internal enum InjectionContext
    {
        /// <summary>
        /// For WinJS applications, this is the start page as specified in the manifest file.
        /// </summary>
        StartPage,

        /// <summary>
        /// For Jupiter projects, this indicates that a field or member declaration should be added
        /// to the Application 'App' class.
        /// </summary>
        AppField,

        /// <summary>
        /// For Jupiter projects, this is the OnLaunched event on the Application 'App' class.
        /// </summary>
        AppStart,

        /// <summary>
        /// For C++ projects, this is an include directive in the Application class's cpp file.
        /// </summary>
        AppInclude
    }
}

