//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace AzureIoTHubConnectedService
{
    /// <summary>
    /// A DialogWindow class that derives from the MS.VS.Shell's DialogWindow class.
    /// </summary>
    /// <remarks>
    /// This is needed because we target both MS.VS.Shell.12.0 and MS.VS.Shell.14.0, and we can't
    /// have #if directives in .xaml files.
    /// </remarks>
    internal class DialogWindow : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
    }
}
