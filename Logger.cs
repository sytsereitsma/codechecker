// Copyright 2013, Shining Dragon Software Limited
// This file is part of CompraeToBranch Extension for Visual Studio
// CompraeToBranch Extension is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

using EnvDTE;
using EnvDTE80;

namespace CodeChecker
{
    /// <summary>
    /// Helper class to do all the logging
    /// Assumes an event source of name 'eventSource' exists
    /// Run the following powershell snippet to set it up if debugging 'New-EventLog -LogName Application -Source MyAddIn'
    /// 'Remove-EventLog -Source MyAddIn'
    /// On real installations the event source is created by the installer
    /// </summary>
    internal class Logger
    {
        private string eventSource = string.Empty;
        private OutputWindowPane outputWindowPane = null;
        private bool logtoOutputWindow = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_applicationObject"></param>
        /// <param name="_eventSource"></param>
        /// <param name="_logtoOutputWindow"></param>
        internal Logger(DTE2 _applicationObject, string _eventSource, bool _logtoOutputWindow)
        {
            try
            {
                logtoOutputWindow = _logtoOutputWindow;
                eventSource = _eventSource;
                if (logtoOutputWindow)
                {
                    // Create an output pane for this addin
                    Window window = _applicationObject.Windows.Item(Constants.vsWindowKindOutput);
                    OutputWindow outputWindow = (OutputWindow)window.Object;
                    outputWindowPane = null;

                    for (int i = 1; i <= outputWindow.OutputWindowPanes.Count; ++i)
                    {
                        if (outputWindow.OutputWindowPanes.Item(i).Name.Equals(eventSource,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            outputWindowPane = outputWindow.OutputWindowPanes.Item(i);
                            break;
                        }
                    }

                    if (outputWindowPane == null)
                        outputWindowPane = outputWindow.OutputWindowPanes.Add(eventSource);
                }
            }
            catch
            {
                // Swallow it, never let errors in logging stop the add in
            }
        }

        /// <summary>
        /// Log trace
        /// </summary>
        /// <param name="message"></param>
        internal void LogMessage(string message)
        {
            try
            {
                if (logtoOutputWindow)
                {
                    outputWindowPane.OutputString(string.Format("{0}\n", message));
                }
                else
                {
                    EventLog.WriteEntry(eventSource, message);
                }
            }
            catch
            {
                // Swallow, never let errors in logging stop the add in
            }
        }

        /// <summary>
        /// Log an error
        /// </summary>
        /// <param name="message"></param>
        internal void LogError(string message)
        {
            try
            {
                if (logtoOutputWindow)
                {
                    outputWindowPane.OutputString(string.Format("Error: {0}\n", message));
                }
                else
                {
                    EventLog.WriteEntry(eventSource, message, EventLogEntryType.Error);
                }
            }
            catch
            {
                // Swallow, never let errors in logging stop the add in
            }
        }
    }
}