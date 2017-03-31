// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// VS project utilities.
    /// </summary>
    internal static class ProjectUtilities
    {
        public static Project GetDteProject(IVsHierarchy projectHierarchy)
        {
            object pvar;
            int hr = projectHierarchy.GetProperty((uint)VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out pvar);
            ErrorHandler.ThrowOnFailure(hr);

            return (Project)pvar;
        }

        /// <summary>
        /// Find the first project item with the given filename.
        /// </summary>
        /// <param name="projectItems">
        /// The list of project items to inspect recursively.
        /// </param>
        /// <param name="filename">
        /// The name of the project item to find.
        /// </param>
        /// <param name="recurse">
        /// Whether to recurse into project items.  Optional, true by default.
        /// </param>
        /// <returns>
        /// Returns the first project item with the given filename.
        /// </returns>
        public static ProjectItem FindProjectItem(ProjectItems projectItems, string filename, bool recurse = true)
        {
            if (projectItems == null)
            {
                return null;
            }

            foreach (ProjectItem item in projectItems)
            {
                if (string.Equals(item.Name, filename, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                else if (recurse && item.ProjectItems != null)
                {
                    var subItem = ProjectUtilities.FindProjectItem(item.ProjectItems, filename);
                    if (subItem != null)
                    {
                        return subItem;
                    }
                }
            }

            return null;
        }
    }
}
