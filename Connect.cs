using System;
using System.IO;
using System.Collections.Generic;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using System.Diagnostics;

namespace CodeChecker
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            _logger = new Logger(_applicationObject, "CodeChecker", true);
            
            _events = _applicationObject.Events;
            //_buildEvents = _events.BuildEvents;
            //_buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            //_buildEvents.OnBuildDone += BuildEvents_OnBuildDone;

            _documentEvents = _events.DocumentEvents;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void DocumentEvents_DocumentSaved(Document doc)
        {
            string includeArguments = GetIncludePaths(doc);
            CallCppCheck(doc.FullName, includeArguments);
        }

        private string GetIncludePaths(Document doc)
        {
            Project project = doc.ProjectItem.ContainingProject;            

            Microsoft.VisualStudio.VCProjectEngine.VCProject proj;
            Microsoft.VisualStudio.VCProjectEngine.VCCLCompilerTool compilerTool;
            Microsoft.VisualStudio.VCProjectEngine.IVCCollection toolsCollection;
            Microsoft.VisualStudio.VCProjectEngine.IVCCollection configurationsCollection;

            proj = (Microsoft.VisualStudio.VCProjectEngine.VCProject)project.Object;

            configurationsCollection = (Microsoft.VisualStudio.VCProjectEngine.IVCCollection)proj.Configurations;

            foreach (Microsoft.VisualStudio.VCProjectEngine.VCConfiguration configuration in configurationsCollection)
            {
                toolsCollection = (Microsoft.VisualStudio.VCProjectEngine.IVCCollection)configuration.Tools;

                foreach (Object toolObject in toolsCollection)
                {
                    if (toolObject is Microsoft.VisualStudio.VCProjectEngine.VCCLCompilerTool)
                    {
                        compilerTool = (Microsoft.VisualStudio.VCProjectEngine.VCCLCompilerTool)toolObject;
                        string preprocessor = compilerTool.PreprocessorDefinitions;

                        return ConvertIncludeDirsToArgument(compilerTool.AdditionalIncludeDirectories, Path.GetDirectoryName(project.FullName));
                    }
                }
            }

            return "";
        }

        private string ConvertIncludeDirsToArgument (string inIncludeDirs, string baseDir)
        {
            char[] delimiterChars = { ';' };
            string[] dirs = inIncludeDirs.Split(delimiterChars);
            HashSet<string> uniqueList = new HashSet<string>();
            string argument = "";
            foreach (var dir in dirs)
            {
                if (!uniqueList.Contains(dir))
                {
                    argument += " -I" + Path.Combine(baseDir, dir);
                    uniqueList.Add(dir);
                }
            }

            return argument;
        }

        private void CallCppCheck(string filename, string inIncludeDirs)
        {
            // Start the child process.
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = "C:\\Program Files (x86)\\Cppcheck\\cppcheck.exe";
            p.StartInfo.Arguments = "--enable=all -D_DEBUG -D_MSC_VER -D_WIN32" + inIncludeDirs + " " + filename;
            p.Start();
            
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            output += p.StandardError.ReadToEnd();

            p.WaitForExit();
            _logger.LogMessage(output);
        }


        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Events _events;
        private DocumentEvents _documentEvents;
        private Logger _logger = null;
    }
}