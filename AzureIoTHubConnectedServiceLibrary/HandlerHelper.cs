// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

// This file will be completely removed once Connected Services fully supports C++ project type

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.ConnectedServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.VisualStudio.TextManager.Interop;

namespace AzureIoTHubConnectedService
{
    using EnvDTE;

    internal static class IVsTextLinesExtensions
    {
        /// <summary>
        /// We set an arbitrary limit on the line length that we're willing to handle. This is to avoid
        /// loading an unreasonably long string into memory. The buffer is compressed under the covers, so it
        /// can handle much longer strings.
        /// </summary>
        private const int MaxLineLengthToConsider = 4096;

        /// <summary>
        /// Enumerate over the lines [startIndex, endIndex) for the IVsTextLines buffer.
        /// </summary>
        /// <param name="buffer"> The buffer to enumerate. </param>
        /// <param name="startIndex"> The starting index, inclusive. </param>
        /// <param name="endIndex"> The end index, exclusive. </param>
        /// <returns>
        /// Returns an enumrator that will enumerate over the lines [startIndex, endIndex) of the buffer.
        /// </returns>
        public static IEnumerable<Tuple<int, string>> GetTextLinesEnumerator(this IVsTextLines buffer, int startIndex, int endIndex)
        {
            return GetTextLinesEnumeratorInternal(buffer, startIndex, endIndex);
        }

        /// <summary>
        /// Enumerate over the lines [startIndex, endIndex) for the IVsTextLines buffer.
        /// </summary>
        /// <param name="buffer"> The buffer to enumerate. </param>
        /// <param name="startIndex"> The starting index, inclusive. </param>
        /// <param name="endIndex"> The end index, exclusive. </param>
        /// <returns>
        /// Returns an enumrator that will enumerate over the lines [startIndex, endIndex) of the buffer.
        /// </returns>
        private static IEnumerable<Tuple<int, string>> GetTextLinesEnumeratorInternal(IVsTextLines buffer, int startIndex, int endIndex)
        {
            LINEDATA[] lineData = new LINEDATA[1];
            for (int i = startIndex; i < endIndex; ++i)
            {
                int hr = buffer.GetLineData(i, lineData, null);

                // Skip the line if there is an error.
                if (ErrorHandler.Failed(hr))
                {
                    continue;
                }

                string line;
                try
                {
                    // We set an arbitrary limit on the line length that we're willing to handle. This is to avoid
                    // loading an unreasonably long string into memory. The buffer is compressed under the covers, so it
                    // can handle much longer strings.
                    // If we choose to handle the case where the string is longer, we would complicate the code significantly.
                    // Since this is an unlikely scenario to have the script tag on a line this long, we will optimize
                    // for the standard case and error;
                    if (lineData[0].iLength >= MaxLineLengthToConsider)
                    {
                        continue;
                    }

                    line = Marshal.PtrToStringUni(lineData[0].pszText);
                }
                finally
                {
                    buffer.ReleaseLineData(lineData);
                }

                yield return new Tuple<int, string>(i, line);
            }
        }
    }

    internal static class BufferUtilities
    {
        /// <summary>
        /// Get the IVsTextLines buffer for the ProjectItem.
        /// </summary>
        /// <param name="projectItem">
        /// The ProjectItem to open.
        /// </param>
        /// <param name="action">
        /// The Action that will be executed that uses the IVsTextLines object.
        /// </param>
        private static void UsingProjectItemBuffer(ProjectItem projectItem, Action<IVsTextLines> action)
        {
            UsingProjectItemBuffer(projectItem, false, action);
        }

        /// <summary>
        /// Get the IVsTextLines buffer for the ProjectItem.
        /// </summary>
        /// <param name="projectItem">
        /// The ProjectItem to open.
        /// </param>
        /// <param name="ensureWritable">
        /// Ensure that the buffer's docdata is read/write.
        /// </param>
        /// <param name="action">
        /// The Action that will be executed that uses the IVsTextLines object.
        /// </param>
        private static void UsingProjectItemBuffer(ProjectItem projectItem, bool ensureWritable, Action<IVsTextLines> action)
        {
            var invisibleEditorManager = (IVsInvisibleEditorManager)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsInvisibleEditorManager));

            // Register a invisible editor, then the specific document will be loaded into the RDT.
            IVsInvisibleEditor invisibleEditor;
            int hr = invisibleEditorManager.RegisterInvisibleEditor(projectItem.FileNames[1], null, (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING, null, out invisibleEditor);

            if (ErrorHandler.Failed(hr) ||
                invisibleEditor == null)
            {
                return;
            }

            IntPtr docData;
            Guid textLinesGuid = typeof(IVsTextLines).GUID;
            hr = invisibleEditor.GetDocData(fEnsureWritable: Convert.ToInt32(ensureWritable), riid: ref textLinesGuid, ppDocData: out docData);

            if (ErrorHandler.Failed(hr) ||
                docData == IntPtr.Zero)
            {
                return;
            }

            try
            {
                IVsTextLines buffer = Marshal.GetObjectForIUnknown(docData) as IVsTextLines;

                if (buffer == null)
                {
                    return;
                }

                action(buffer);
            }
            finally
            {
                Marshal.Release(docData);
                docData = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Update a ProjectItem's content to match the supplied file.
        /// </summary>
        /// <param name="projectItem">
        /// The ProjectItem for the file to update.
        /// </param>
        /// <param name="sourceFile">
        /// The file from where to copy the content.
        /// </param>
        public static void UpdateProjectItemFromFile(ProjectItem projectItem, string sourceFile)
        {
            UpdateProjectItem(projectItem, (startPoint, endPoint) =>
            {
                startPoint.Delete(endPoint);
                startPoint.InsertFromFile(sourceFile);
            });
        }

        /// <summary>
        /// Update a ProjectItem's content using the supplied function.
        /// </summary>
        /// <param name="projectItem">
        /// The ProjectItem for the file to update.
        /// </param>
        /// <param name="processItemContent">
        /// The function to use to update the content.  This will be called with two EditPoints 
        /// representing the start and end of the file.
        /// </param>
        public static void UpdateProjectItem(ProjectItem projectItem, Action<EditPoint, EditPoint> processItemContent)
        {
            UsingProjectItemBuffer(
                projectItem,
                true,
                buffer =>
                {
                    int lastLine;
                    int lastLineIndex;

                    // Find the last point of the document for creating the EndPoint EditPoint.
                    int hr = buffer.GetLastLineIndex(out lastLine, out lastLineIndex);

                    object editPointObject;
                    object endPointObject;

                    // Get the first and last point of the document to overwrite the content.
                    buffer.CreateEditPoint(0, 0, out editPointObject);
                    buffer.CreateEditPoint(lastLine, lastLineIndex, out endPointObject);

                    EditPoint editPoint = editPointObject as EditPoint;
                    EditPoint endPoint = endPointObject as EditPoint;

                    // Something went wrong in getting the end points. Don't overwrite the document.
                    if (editPoint == null ||
                        endPoint == null)
                    {
                        throw new ArgumentException("projectItem", "invalid buffer");
                    }

                    processItemContent(editPoint, endPoint);

                    // Ensure the file is saved.
                    var persistDocData = buffer as IVsPersistDocData;
                    if (persistDocData != null)
                    {
                        string newDocumentMoniker;
                        int saveCanceled;
                        persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out newDocumentMoniker, out saveCanceled);
                    }
                });
        }

        /// <summary>
        /// Determine if the projectItem is equal to the file.
        /// </summary>
        /// <param name="projectItem">
        /// The Project Item to compare.
        /// </param>
        /// <param name="fileName">
        /// The file to compare the project item against.
        /// </param>
        /// <param name="tokenReplacement">
        /// Token replacement to user for the file on disk before comparing.
        /// </param>
        /// <returns>
        /// Returns true if the two are equal; false otherwise.
        /// </returns>
        public static bool AreFilesEqual(ProjectItem projectItem, string fileName, TokenReplacementBuilder tokenReplacement)
        {
            bool areEqual = true;

            UsingProjectItemBuffer(
                projectItem,
                buffer =>
                {
                    int lineCount;
                    buffer.GetLineCount(out lineCount);

                    using (var reader = new StreamReader(fileName, detectEncodingFromByteOrderMarks: true))
                    {
                        foreach (var item in buffer.GetTextLinesEnumerator(0, lineCount))
                        {
                            // We reached the end of file name before the end of the projectItem.
                            if (reader.EndOfStream)
                            {
                                // Ignore whitespace at the end of the file. VB/C# seem to
                                // return blank lines as part of the text buffer.
                                if (string.IsNullOrWhiteSpace(item.Item2))
                                {
                                    continue;
                                }

                                areEqual = false;
                                return;
                            }

                            string lhs = reader.ReadLine();
                            if (tokenReplacement != null)
                            {
                                lhs = tokenReplacement.Build(lhs);
                            }

                            string rhs = item.Item2.TrimEnd(Environment.NewLine.ToCharArray());

                            areEqual &= string.Equals(lhs, rhs, StringComparison.CurrentCulture);

                            if (!areEqual)
                            {
                                return;
                            }
                        }

                        // We didn't reach the end of file name even though we reached the end of the projectItem.
                        if (!reader.EndOfStream)
                        {
                            areEqual = false;
                            return;
                        }
                    }
                });

            return areEqual;
        }
    }

    /// <summary>
    /// An attribute that can be applied to a method to signal that it can be used
    /// as a Token Replacement method.
    /// </summary>
    /// <remarks>
    /// The method is required to only have one parameter of type string and
    /// the return value must be of type string.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class TokenReplacementMethodAttribute : Attribute
    {
    }

    internal class TokenReplacementBuilder
    {
        private IDictionary<string, MethodInfo> methods;
        private IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Add a dictionary of replacement tokens to consider when performing token replacement.
        /// </summary>
        /// <param name="tokens">
        /// The set of key/value pairs that can be used as token values.
        /// </param>
        public void AddDictionary(IEnumerable<KeyValuePair<string, string>> tokens)
        {
            foreach (var item in tokens)
            {
                this.dictionary[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Build a new string based on replacement parameters.
        /// </summary>
        /// <param name="input">
        /// The input to use as a template.
        /// </param>
        /// <returns>
        /// Returns a new string with the tokens replacement.
        /// </returns>
        /// <remarks>
        /// If a token or method is not recognized, the token is left unaltered.
        /// </remarks>
        public string Build(string input)
        {
            int currentPosition = 0;
            int startToken = input.IndexOf('$', currentPosition);
            int endToken;

            StringBuilder sb = new StringBuilder(input.Length);
            while (startToken >= 0)
            {
                endToken = input.IndexOf('$', startToken + 1);

                // No matching close $. We just ignore
                // the token replacement in that case.
                if (endToken < 0)
                {
                    break;
                }

                // If these are adjacent '$', we treat that as an escaped character and collapse it
                // into a single '$'.
                if (startToken + 1 == endToken)
                {
                    sb.Append(input.Substring(currentPosition, startToken - currentPosition + 1));
                }
                else
                {
                    // We get the whole token (e.g., $ServiceInstance.Name$, $METHOD(ServiceInstance.Name)$).
                    string token = input.Substring(startToken, endToken - startToken + 1);

                    string tokenValue;
                    if (this.TryEvaluateToken(token, out tokenValue))
                    {
                        sb.Append(input.Substring(currentPosition, startToken - currentPosition));
                        sb.Append(tokenValue);
                    }
                    else
                    {
                        // Unrecognized token.
                        sb.Append(input.Substring(currentPosition, endToken - currentPosition + 1));
                    }
                }

                currentPosition = endToken + 1;
                startToken = input.IndexOf('$', currentPosition);
            }

            // Add the rest of the string at the end.
            if (currentPosition < input.Length)
            {
                sb.Append(input.Substring(currentPosition));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get a list of methods supported for token replacement methods.
        /// </summary>
        /// <returns>
        /// Returns a dictionary of methods where the key is the case-insensitive method name.
        /// </returns>
        private IDictionary<string, MethodInfo> GetMethods()
        {
            if (this.methods == null)
            {
                try
                {
                    // For now, get all the instance methods on this type that are candidates for token replacement values.
                    // We require that the return type is of type string and takes one parameter of type string.
                    this.methods = (from item in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                    where
                                        item.GetCustomAttribute<TokenReplacementMethodAttribute>() != null &&
                                        item.ReturnType == typeof(string)
                                    let parameters = item.GetParameters()
                                    where
                                        parameters.Count() == 1 &&
                                        parameters.First().ParameterType == typeof(string)
                                    select item).ToDictionary(mi => mi.Name, StringComparer.OrdinalIgnoreCase);
                }
                catch (Exception)
                {
                    // Create empty list of methods.
                    Debug.Fail("Failed to find methods for token replacement.");
                    this.methods = new Dictionary<string, MethodInfo>();
                }
            }

            return this.methods;
        }

        /// <summary>
        /// Try to get the value for a token (e.g., $TokenName$ or $Method(TokenName)$).
        /// </summary>
        /// <param name="token">
        /// The token to evaluate.
        /// </param>
        /// <param name="tokenValue">
        /// The token value result.
        /// </param>
        /// <returns>
        /// Returns true if the token is recognized and evaluated; otherwise false.
        /// </returns>
        private bool TryEvaluateToken(string token, out string tokenValue)
        {
            tokenValue = null;
            int methodOpenParenthesis = token.IndexOf('(');

            if (methodOpenParenthesis < 0)
            {
                // No method call is detected, return token value.
                return this.dictionary.TryGetValue(token.Trim(new char[] { '$', ' ' }), out tokenValue);
            }
            else
            {
                // Determine the method to call.
                string method = token.Substring(0, methodOpenParenthesis).Trim(new char[] { '$', ' ' });
                string tokenName = token.Substring(methodOpenParenthesis + 1).Trim(new char[] { '$', ' ', ')' });

                if (!this.dictionary.TryGetValue(tokenName, out tokenValue))
                {
                    return false;
                }

                return this.TryEvaluateMethod(method, tokenValue, out tokenValue);
            }
        }

        /// <summary>
        /// Call a method with the token value as a parameter.
        /// </summary>
        /// <param name="methodName">
        /// The case-insensitive name of the method to call.
        /// </param>
        /// <param name="tokenValue">
        /// The parameter to the method.
        /// </param>
        /// <param name="result">
        /// If the method succeeds, result is the value returned from the method or null if the
        /// method failed or was not recognized.
        /// </param>
        /// <returns>
        /// Returns true if the method was executed successfully; otherwise false.
        /// </returns>
        private bool TryEvaluateMethod(string methodName, string tokenValue, out string result)
        {
            result = null;

            // If the method is not recognized, return.
            var methods = this.GetMethods();
            if (!methods.ContainsKey(methodName))
            {
                return false;
            }

            try
            {
                result = (string)methods[methodName].Invoke(this, new object[] { tokenValue });
            }
            catch (Exception)
            {
                Debug.Fail("Failed to evaluate method for token replacement.");
                return false;
            }

            return true;
        }

        public static string MakeSafeIdentifier(string tokenValue)
        {
            return Regex.Replace(tokenValue, "[^a-zA-Z0-9_]", "_");
        }
    }

    internal static class ConnectedServicesUtilities
    {
        public static Project GetDteProject(this IVsHierarchy projectHierarchy)
        {
            object pvar;
            int hr = projectHierarchy.GetProperty((uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out pvar);
            ErrorHandler.ThrowOnFailure(hr);

            return (Project)pvar;
        }

        /// <summary>
        /// Get an absolute path of the directory in which the project is contained.
        /// </summary>
        /// <param name="projectHierarchy"> The parent project hierarchy object. </param>
        /// <returns> Returns an absolute path of the project's directory. </returns>
        public static string GetProjectDirectoryPath(IVsHierarchy projectHierarchy)
        {
            return Path.GetDirectoryName(GetDteProject(projectHierarchy).FullName);
        }

        /// <summary>
        /// Get an absolute path for a file that is in the project structure.
        /// </summary>
        /// <param name="projectHierarchy"> The parent project hierarchy object. </param>
        /// <param name="fileName"> The file name to convert to an absolute path. </param>
        /// <returns> Returns an absolute path for a file that is in the project structure. </returns>
        public static string GetProjectFullPath(IVsHierarchy projectHierarchy, string fileName)
        {
            return Path.Combine(GetProjectDirectoryPath(projectHierarchy), fileName);
        }

        public static string GetDefaultNamespace(this IVsHierarchy projectHierarchy)
        {
            object defaultNamespace;
            projectHierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_DefaultNamespace, out defaultNamespace);
            return defaultNamespace?.ToString();
        }

        public static string GetProjectProperty(IVsHierarchy hierarchy, __VSHPROPID propertyId)
        {
            return GetProjectProperty(hierarchy, (int)propertyId);
        }

        public static string GetProjectProperty(IVsHierarchy hierarchy, __VSHPROPID4 propertyId)
        {
            return GetProjectProperty(hierarchy, (int)propertyId);
        }

        public static string GetProjectProperty(IVsHierarchy hierarchy, __VSHPROPID5 propertyId)
        {
            return GetProjectProperty(hierarchy, (int)propertyId);
        }

        public static string GetProjectProperty(IVsHierarchy hierarchy, int propertyId)
        {
            object obj;

            var hr = hierarchy.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                propertyId,
                out obj);

            // We *allow* failure, because the property might not exist...
            // we just treat this as a null return value.
            if (ErrorHandler.Succeeded(hr))
            {
                return obj as string;
            }

            return null;
        }

        public static TInterface GetService<TInterface, TService>(this IServiceProvider serviceProvider)
            where TInterface : class
        {
            return serviceProvider.GetService(typeof(TService)) as TInterface;
        }

        public static string FormatCurrentCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string FormatInvariantCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static string TrimPrefix(this string value, string prefix)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(prefix.Length);
            }

            return value;
        }

        internal static string GetCapabilities(this IVsHierarchy project)
        {
            string capabilities = null;

            object capabilitiesObj;
            if (ErrorHandler.Succeeded(project.GetProperty(
                    (uint)VSConstants.VSITEMID.Root,
                    (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities,
                    out capabilitiesObj)))
            {
                capabilities = capabilitiesObj as string;
            }

            return capabilities;
        }

        internal static string GetTargetPlatformIdentifier(this IVsHierarchy project)
        {
            string targetPlatformIdentifier = ConnectedServicesUtilities.GetProjectProperty(project, __VSHPROPID5.VSHPROPID_TargetPlatformIdentifier);
            if (!string.IsNullOrEmpty(targetPlatformIdentifier))
            {
                targetPlatformIdentifier = targetPlatformIdentifier.CleanForCapabilitiesComponent();
            }
            return targetPlatformIdentifier;
        }

        private static string CleanForCapabilitiesComponent(this string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value));

            // We only allow letters, digits, periods, and underscores.
            // All other characters (especially whitespace and commas!)
            // are removed.
            return new string(value.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_').ToArray());
        }

        internal static string GetProjectTypesString(this IVsHierarchy project)
        {
            string projectTypeGuids = string.Empty;

            IVsAggregatableProject aggregatableProject = project as IVsAggregatableProject;
            if (aggregatableProject != null)
            {
                ErrorHandler.ThrowOnFailure(aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids));
            }

            return string.Join(" ",
                projectTypeGuids
                    .Split(';')
                    .Select(s => !String.IsNullOrEmpty(s) ? new Guid(s) : Guid.Empty)
                    .Select(g => g.ToString("N")));
        }

        internal static Version GetTargetPlatformVersion(this IVsHierarchy project)
        {
            Version platformVersion = null;

            string targetPlatformVersion = ConnectedServicesUtilities.GetProjectProperty(project, __VSHPROPID5.VSHPROPID_TargetPlatformVersion);
            if (!string.IsNullOrEmpty(targetPlatformVersion))
            {
                Version.TryParse(targetPlatformVersion, out platformVersion);
            }

            return platformVersion;
        }
    }

    internal class AzureIoTHubConnectedServiceHandlerHelper : Microsoft.VisualStudio.ConnectedServices.ConnectedServiceHandlerHelper
    {
        internal const string DefaultFolder = "Service References";
        private const string RootNamespaceKey = "RootNamespace";

        private ConnectedServiceContext context;
        private IDictionary<string, string> tokenReplacementValues;

        protected AzureIoTHubConnectedServiceHandlerHelper()
        {
        }

        public AzureIoTHubConnectedServiceHandlerHelper(ConnectedServiceContext context)
        {
            this.context = context;
        }

        public override IDictionary<string, string> TokenReplacementValues
        {
            get
            {
                if (this.tokenReplacementValues == null)
                {
                    this.tokenReplacementValues = this.CreateTokenReplacementValues();
                }
                return this.tokenReplacementValues;
            }
        }

        private ConnectedServiceHandlerContext HandlerContext
        {
            get
            {
                ConnectedServiceHandlerContext handlerContext = this.context as ConnectedServiceHandlerContext;
                if (handlerContext == null)
                {
                    throw new InvalidOperationException("This CommonConnectedServiceHandlerHelper was not initialized with a ConnectedServiceHandlerContext");
                }
                return handlerContext;
            }
        }

        /// <summary>
        /// Adds a reference to the specified assembly to the project.
        /// </summary>
        /// <param name="assemblyPath">
        /// The assembly to which to add a reference.  This can be specified either as a simple .NET Framework object name, such 
        /// as "System.Web", or as a .NET Framework file name, such as "C:\path\program.dll".
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the assembly reference is not successfully added to the project.
        /// </exception>
        public override void AddAssemblyReference(string assemblyPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add the specified file to the project after doing token replacement.
        /// </summary>
        /// <param name="fileName">
        /// The file to copy and add to the project.
        /// </param>
        /// <param name="targetPath">
        /// The full or relative path where the file should be added.  If specifying a full path, the path must be in a sub-directory
        /// of the project's directory.
        /// </param>
        /// <param name="addFileOptions">
        /// The options to use while adding the file.
        /// </param>
        /// <returns>
        /// Returns the path to the file that was added.
        /// </returns>
        public override async Task<string> AddFileAsync(string fileName, string targetPath, AddFileOptions addFileOptions = null)
        {
            if (addFileOptions == null)
            {
                addFileOptions = new AddFileOptions();
            }

            await this.AddFileToProjectInFolder(this.context.ProjectHierarchy, fileName, targetPath, addFileOptions);

            return targetPath;
        }

        /// <summary>
        /// Copy fileToCopy, keeping fileToCopy unchanged.
        /// </summary>
        /// <param name="fileToCopy">
        /// The source file.
        /// </param>
        /// <param name="targetPath">
        /// The filename to use for the saved file.
        /// </param>
        /// <returns>
        /// Returns a task that can be awaited on and get the path to the newly created file.
        /// </returns>
        /// <remarks>
        /// The file will be created in the temp directory but will have the correct target name.
        /// This allows the caller to add the file to the project without having to worry about the renaming step.
        /// </remarks>
        private async Task<string> CopyFileAsync(string fileToCopy, string targetPath)
        {
            //// Note that we create the file in the temp directory.
            //// I would prefer the file to not be added at all.

            string tokenReplacedFileWithTargetName =
                Path.Combine(
                    Path.GetTempPath(), // The root path will be the temp directory
                    Guid.NewGuid().ToString("N"), // Create a unique directory name to prevent collisions
                    Path.GetFileName(targetPath)); // The target filename

            StreamReader reader;

            if (fileToCopy.StartsWith("pack://"))
            {
                reader = new StreamReader(Application.GetResourceStream(new Uri(fileToCopy)).Stream);
            }
            else
            {
                reader = new StreamReader(fileToCopy, detectEncodingFromByteOrderMarks: true);
            }

            // Copy the file, being sure to preserve encoding.
            using (reader)
            {
                // Ensure the directory exists.
                Directory.CreateDirectory(Path.GetDirectoryName(tokenReplacedFileWithTargetName));
                using (StreamWriter writer = new StreamWriter(tokenReplacedFileWithTargetName, append: false, encoding: reader.CurrentEncoding))
                {
                    string fileContent = await reader.ReadToEndAsync();
                    await writer.WriteAsync(fileContent);
                }
            }

            return tokenReplacedFileWithTargetName;
        }

        /// <summary>
        /// Gets the name of the root folder to place the service related artifacts in.  Typcially each provider
        /// instance should create its own subfolder under this root folder.
        /// </summary>
        /// <returns>
        /// The name of the root folder.
        /// </returns>
        public override string GetServiceArtifactsRootFolder()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Given an input, replace any tokens found in the specified dictionary with the specified values.
        /// </summary>
        /// <param name="input">
        /// The text to update.
        /// </param>
        /// <param name="additionalReplacementValues">
        /// A dictionary of key/value pairs that will be used to replace tokens in the input.
        /// These values are additional to the values in TokenReplacementValues.  In the case of conflicts, the
        /// values specified in additionalReplacementValues will override the TokenReplacementValues.
        /// </param>
        /// <returns>
        /// Returns a new string with the tokens replace with the values from the specified dictionary.
        /// </returns>
        public override string PerformTokenReplacement(string input, IDictionary<string, string> additionalReplacementValues = null)
        {
            TokenReplacementBuilder tokenReplacement = BuildTokenReplacement(additionalReplacementValues);
            return tokenReplacement.Build(input);
        }

        private void PerformTokenReplacement(ProjectItem item, IDictionary<string, string> additionalReplacementValues = null)
        {
            BufferUtilities.UpdateProjectItem(item, (startPoint, endPoint) =>
            {
                string oldContent = startPoint.GetText(endPoint);
                startPoint.Delete(endPoint);
                startPoint.Insert(this.PerformTokenReplacement(oldContent, additionalReplacementValues));
            });
        }

        private TokenReplacementBuilder BuildTokenReplacement(IDictionary<string, string> additionalReplacementValues)
        {
            TokenReplacementBuilder tokenReplacement = new TokenReplacementBuilder();
            tokenReplacement.AddDictionary(this.TokenReplacementValues);

            // add the additionalReplacementValues last, since any values specified by the caller will overwrite
            // the tokens from this.TokenReplacementValues.
            if (additionalReplacementValues != null)
            {
                tokenReplacement.AddDictionary(additionalReplacementValues);
            }

            return tokenReplacement;
        }

        /// <summary>
        /// Get the built-in set of tokens supported by the handler helper.
        /// </summary>
        /// <returns>
        /// Returns a new dictionary with built-in tokens and their values.
        /// </returns>
        private IDictionary<string, string> CreateTokenReplacementValues()
        {
            Dictionary<string, string> extended = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ServiceInstance.Name", this.HandlerContext.ServiceInstance.Name },
                { "ServiceInstance.InstanceId", this.HandlerContext.ServiceInstance.InstanceId },
                { "ProjectName", ConnectedServicesUtilities.GetProjectProperty(this.context.ProjectHierarchy, __VSHPROPID.VSHPROPID_Name) },
                { "vslcid", "0x{0:X}".FormatInvariantCulture(CultureInfo.CurrentUICulture.LCID) }
            };

            string defaultNamespace = this.context.ProjectHierarchy.GetDefaultNamespace();
            if (defaultNamespace != null)
            {
                extended.Add(AzureIoTHubConnectedServiceHandlerHelper.RootNamespaceKey, defaultNamespace);
                extended.Add("ProjectDefaultNamespace", defaultNamespace);
            }

            Project project = ConnectedServicesUtilities.GetDteProject(this.context.ProjectHierarchy);
            Property assemblyNameProperty = project.Properties.OfType<Property>().FirstOrDefault(p => string.Equals(p.Name, "AssemblyName", StringComparison.OrdinalIgnoreCase));
            if (assemblyNameProperty != null)
            {
                string assemblyName = assemblyNameProperty.Value as string;
                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    extended.Add("AssemblyName", assemblyName);
                }
            }

            // Tokens for service metadata.
            if (this.HandlerContext.ServiceInstance.Metadata != null)
            {
                foreach (var item in this.HandlerContext.ServiceInstance.Metadata)
                {
                    if (item.Value is string)
                    {
                        // use the indexer instead of "Add" in case the Metadata dictionary conflicts with
                        // any 'extended' tokens above.  The Metadata dictionary will overwrite the built-in value.
                        extended["ServiceInstance.{0}".FormatInvariantCulture(item.Key)] = (string)item.Value;
                    }
                }
            }

            return extended;
        }

        /// <summary>
        /// Copy a file to a project relative path.
        /// </summary>
        /// <param name="projectHierarchy">
        /// The project where to add the file.
        /// </param>
        /// <param name="fileName">
        /// The path to the file to copy.
        /// </param>
        /// <param name="targetPath">
        /// The target path, including the filename.
        /// </param>
        /// <param name="addFileOptions">
        /// The options to use while coping the file.
        /// </param>
        private async Task AddFileToProjectInFolder(IVsHierarchy projectHierarchy, string fileName, string targetPath, AddFileOptions addFileOptions)
        {
            targetPath = AzureIoTHubConnectedServiceHandlerHelper.GetProjectRelativePath(projectHierarchy, targetPath);
            Project project = ConnectedServicesUtilities.GetDteProject(projectHierarchy);
            ProjectItems items = project.ProjectItems;

            fileName = await this.CopyFileAsync(fileName, targetPath);

            string fileToAdd = ConnectedServicesUtilities.GetProjectFullPath(projectHierarchy, targetPath);
            string targetFileName = Path.GetFileName(fileToAdd);

            // Build the directory structure if it doesn't already exist.
            Directory.CreateDirectory(Path.GetDirectoryName(fileToAdd));

            // clone the AdditionalReplacementValues dictionary so we aren't modifying the original
            Dictionary<string, string> replacementValues = new Dictionary<string, string>(addFileOptions.AdditionalReplacementValues);

            ProjectItem item = AzureIoTHubConnectedServiceHandlerHelper.GetNestedProjectItem(items, targetPath);
            bool existOnDisk = File.Exists(fileToAdd);

            if (item == null &&
                existOnDisk)
            {
                // The file is not in the project. We should add the file.
                // This is some arbitrary file, which we'll update in the same
                // path as existing project files.
                // This is 'fileToAdd' because we're not adding the final file here.
                item = items.AddFromFile(fileToAdd);
            }

            if (item != null)
            {
                // Add the folder-specific RootNamespace replacement value so $RootNamespace$ has the folder structure in it for C# projects
                this.AddRootNamespaceReplacementValue(replacementValues, item.Collection);

                bool filesEqual = this.AreFilesEqualWithReplacement(item, fileName, replacementValues);

                if (!filesEqual)
                {
                    if (!addFileOptions.SuppressOverwritePrompt && !this.PromptOverwrite(targetFileName))
                    {
                        // The user chose not to overwrite the file, so abort adding this file.
                        return;
                    }

                    // Get the document and overwrite with file content.
                    BufferUtilities.UpdateProjectItemFromFile(item, fileName);
                }
            }
            else
            {
                File.Copy(fileName, fileToAdd);
                item = items.AddFromFile(fileToAdd);

                // Add the folder-specific RootNamespace replacement value so $RootNamespace$ has the folder structure in it for C# projects
                this.AddRootNamespaceReplacementValue(replacementValues, item.Collection);
            }

            this.PerformTokenReplacement(item, replacementValues);

            if (addFileOptions.OpenOnComplete && !item.IsOpen)
            {
                try
                {
                    var window = item.Open();

                    // Ensure that the window is always shown regardless of "Preview"
                    // user settings.
                    if (window != null &&
                        !window.Visible)
                    {
                        window.Visible = true;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private bool AreFilesEqualWithReplacement(ProjectItem item, string fileName, IDictionary<string, string> additionalReplacementValues)
        {
            TokenReplacementBuilder tokenReplacement = this.BuildTokenReplacement(additionalReplacementValues);
            return BufferUtilities.AreFilesEqual(item, fileName, tokenReplacement);
        }

        /// <summary>
        /// Adds a RootNamespace replacement value to the dictionary for the folder represented by the specified ProjectItems.
        /// </summary>
        private void AddRootNamespaceReplacementValue(IDictionary<string, string> additionalReplacementValues, ProjectItems items)
        {
            // if additionalReplacementValues already contains a RootNamespace value, don't replace it
            if (!additionalReplacementValues.ContainsKey(AzureIoTHubConnectedServiceHandlerHelper.RootNamespaceKey))
            {
                // make sure to use the value for RootNamespace and not the current DefaultNamespace property.  This allows callers
                // to put their own RootNamespace value, which will get the folder path appended to it for C# projects.
                string rootNamespaceStartingValue;
                if (this.TokenReplacementValues.TryGetValue(AzureIoTHubConnectedServiceHandlerHelper.RootNamespaceKey, out rootNamespaceStartingValue))
                {
                    string folderDefaultNamespace = this.GetFolderDefaultNamespace(items);
                    if (!string.IsNullOrEmpty(folderDefaultNamespace))
                    {
                        if (!string.IsNullOrEmpty(rootNamespaceStartingValue))
                        {
                            rootNamespaceStartingValue += Type.Delimiter;
                        }

                        additionalReplacementValues.Add(AzureIoTHubConnectedServiceHandlerHelper.RootNamespaceKey, rootNamespaceStartingValue + folderDefaultNamespace);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the default namespace for the folder minus the project's DefaultNamespace and minus the 
        /// GetServiceArtifactsRootFolder() (i.e. "Service References").
        /// </summary>
        private string GetFolderDefaultNamespace(ProjectItems items)
        {
            string folderDefaultNamespace = null;

            // Try to get the 'default namespace' of the folder.
            ProjectItem parentItem = items.Parent as ProjectItem;
            if (parentItem != null)
            {
                Property folderDefaultNamespaceProperty = parentItem.Properties.OfType<Property>().FirstOrDefault(p => string.Equals(p.Name, "DefaultNamespace", StringComparison.OrdinalIgnoreCase));
                folderDefaultNamespace = folderDefaultNamespaceProperty?.Value as string;
                if (!string.IsNullOrEmpty(folderDefaultNamespace))
                {
                    // trim off the project's DefaultNamespace
                    string projectDefaultNamespace = this.context.ProjectHierarchy.GetDefaultNamespace();
                    folderDefaultNamespace = AzureIoTHubConnectedServiceHandlerHelper.TrimNamespacePrefix(folderDefaultNamespace, projectDefaultNamespace);

                    // trim off the service artifacts root folder name
                    string serviceRootArtifactNamespace = TokenReplacementBuilder.MakeSafeIdentifier(this.GetServiceArtifactsRootFolder());
                    folderDefaultNamespace = AzureIoTHubConnectedServiceHandlerHelper.TrimNamespacePrefix(folderDefaultNamespace, serviceRootArtifactNamespace);
                }
            }

            return folderDefaultNamespace;
        }

        private static string TrimNamespacePrefix(string value, string prefix)
        {
            value = value?.TrimPrefix(prefix);
            value = value?.TrimStart(Type.Delimiter);

            return value;
        }

        /// <summary>
        /// Returns the targetPath as a relative path from the project's directory.
        /// </summary>
        private static string GetProjectRelativePath(IVsHierarchy projectHierarchy, string targetPath)
        {
            if (Path.IsPathRooted(targetPath))
            {
                // if this is a rooted path, it must be a sub-directory of the project's directory
                string projectDirectory = ConnectedServicesUtilities.GetProjectDirectoryPath(projectHierarchy);
                Uri projectDirectoryUri = new Uri(projectDirectory, UriKind.Absolute);
                Uri targetPathUri = new Uri(targetPath, UriKind.Absolute);

                if (!projectDirectoryUri.IsBaseOf(targetPathUri))
                {
                    throw new ArgumentException(
                        "If targetPath is an absolute path, then it must be in a subdirectory under the project's directory.\nProject Directory: {0}\nTargetPath: {1}"
                            .FormatCurrentCulture(projectDirectory, targetPath),
                        "targetPath");
                }

                // turn the full path into a path relative to the project directory
                targetPath = targetPath.Substring(projectDirectory.Length);
                targetPath = targetPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return targetPath;
        }

        private static ProjectItem GetNestedProjectItem(ProjectItems items, string fileName)
        {
            ProjectItem currentItem = null;
            string[] pathParts = fileName.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in pathParts)
            {
                currentItem = items.OfType<ProjectItem>().FirstOrDefault(i => string.Equals(i.Name, part, StringComparison.OrdinalIgnoreCase));
                if (currentItem == null)
                {
                    return null;
                }
                items = currentItem.ProjectItems;
            }
            return currentItem;
        }

        private bool PromptOverwrite(string targetFileName)
        {
            // Prompt for overwriting.
            IVsUIShell uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            string prompt = string.Format(CultureInfo.InvariantCulture, Resource.FileAlreadyExists, targetFileName);
            bool overwrite = VsShellUtilities.PromptYesNo(prompt, null, OLEMSGICON.OLEMSGICON_QUERY, uiShell);
            return overwrite;
        }

        /// <summary>
        /// Add the file to the ProjectItems.
        /// </summary>
        /// <param name="items"> The item under which to add the file. </param>
        /// <param name="fileName"> The source file name. </param>
        /// <param name="fileToAdd"> The target file name. </param>
        /// <returns>
        /// Returns the newly added project item.
        /// </returns>
        protected virtual ProjectItem AddFromFileCopy(ProjectItems items, string fileName, string fileToAdd)
        {
            return items.AddFromFileCopy(fileName);
        }

        /// <summary>
        /// Add the folder to the specified ProjectItem.
        /// </summary>
        /// <param name="parent"> The parent to which to add the folder. </param>
        /// <param name="folder"> The name of the folder to add. </param>
        /// <returns> Returns the ProjectItems of the newly created folder. </returns>
        protected virtual ProjectItems AddFolder(ProjectItems parent, string folder)
        {
            return parent.AddFolder(folder).ProjectItems;
        }
    }
}
