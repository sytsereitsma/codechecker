using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.IO;

namespace CodeChecker
{
    using VCPE = Microsoft.VisualStudio.VCProjectEngine;

    class ProjectSettings
    {
        private string[] _IncludeDirs;
        private string[] _PreprocessorFlags;
        private string _ProjectPath;

        public ProjectSettings (Document doc)
        {
            _ProjectPath = Path.GetDirectoryName(doc.ProjectItem.ContainingProject.FullName);

            VCPE.VCCLCompilerTool compilerTool = GetCompilerTool(doc);
            if (!compilerTool.Equals(null))
            {
                char[] delimiterChars = { ';' };
                _IncludeDirs = MakeUniqueList(compilerTool.AdditionalIncludeDirectories, delimiterChars);
                _PreprocessorFlags = MakeUniqueList(compilerTool.PreprocessorDefinitions, delimiterChars);
            }
        }

        internal string[] IncludeDirs
        {
            get { return _IncludeDirs; }
        }

        internal string[] PreprocessorFlags
        {
            get { return _PreprocessorFlags; }
        }

        internal string ProjectPath
        {
            get { return _ProjectPath; }
        }
        
        private string[] MakeUniqueList(string inList, char[] inDelimiterChars)
        {
            string[] parts = inList.Split(inDelimiterChars);

            HashSet<string> uniqueList = new HashSet<string>();
            foreach (var part in parts)
            {
                uniqueList.Add(part);
            }

            return uniqueList.ToArray();
        }

        private VCPE.VCProject GetProject(Document doc)
        {
            Project project = doc.ProjectItem.ContainingProject;
            return (VCPE.VCProject)project.Object;
        }

        private VCPE.VCCLCompilerTool GetCompilerTool(Document doc)
        {
            VCPE.VCProject project = GetProject(doc);
            VCPE.IVCCollection configurationsCollection = (VCPE.IVCCollection)project.Configurations;

            foreach (VCPE.VCConfiguration configuration in configurationsCollection)
            {
                VCPE.IVCCollection toolsCollection = (VCPE.IVCCollection)configuration.Tools;
                foreach (Object toolObject in toolsCollection)
                {
                    if (toolObject is VCPE.VCCLCompilerTool)
                    {
                        return (VCPE.VCCLCompilerTool)toolObject;
                    }
                }
            }

            return null;
        }
    }
}
