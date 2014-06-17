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
        private string[] _ForceIncludes;
        private string _ProjectPath;

        public ProjectSettings (Document doc)
        {
            _ProjectPath = Path.GetDirectoryName(doc.ProjectItem.ContainingProject.FullName);
            VCPE.VCCLCompilerTool compilerTool = GetCompilerTool(doc);            
            if (!compilerTool.Equals(null))
            {
                LoadIncludeDirectories(compilerTool, Path.GetDirectoryName(doc.FullName));

                char[] delimiterChars = { ';' };
                _PreprocessorFlags = MakeUnique(StringToList(compilerTool.PreprocessorDefinitions, delimiterChars));
                _ForceIncludes = MakeUnique(StringToList(compilerTool.ForcedIncludeFiles + ";limits.h", delimiterChars));
            }
        }

        internal string PredefinedMacros
        {
            get
            {
                string common = "-D_MSC_VER=1600 -D_MT";
                if (_PreprocessorFlags.Contains("WIN64"))
                {
                    return common + " -D_M_AI64 -D_WIN64";
                }
                else
                {
                    return common + " -D_M_IX86 -D_WIN32";
                }
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

        internal string[] ForceIncludes
        {
            get { return _ForceIncludes; }
        }

        private void LoadIncludeDirectories(VCPE.VCCLCompilerTool compilerTool, string inDocumentPath)
        {
            char[] delimiterChars = { ';' };
            string[] includeDirs = StringToList(compilerTool.FullIncludePath, delimiterChars);
            List<string> finalDirs = new List<string>();
            for (UInt32 t = 0; t < includeDirs.Length; ++t)
            {
                string abspath = GetAbsoluteIncludePath(includeDirs[t], inDocumentPath);
                if (abspath != null)
                {
                    finalDirs.Add(abspath);
                }
            }

            _IncludeDirs = MakeUnique(finalDirs.ToArray());
        }

        private void LoadForcedIncludes(VCPE.VCCLCompilerTool compilerTool, string inDocumentPath)
        {
            char[] delimiterChars = { ';' };
            string[] includes = StringToList(compilerTool.ForcedIncludeFiles, delimiterChars);
            List<string> finalIncludes = new List<string>();
            for (UInt32 t = 0; t < includes.Length; ++t)
            {
                string abspath = GetAbsoluteIncludeFile(includes[t], inDocumentPath);
                if (abspath != null)
                {
                    finalIncludes.Add(abspath);
                }
            }

            _ForceIncludes = MakeUnique(finalIncludes.ToArray());
        }

        private string GetAbsoluteIncludePath(string inCandidate, string inDocumentPath)
        {
            string abspath = Path.GetFullPath(inCandidate);
            if (!Directory.Exists(abspath))
            {
                abspath = Path.GetFullPath(Path.Combine(inDocumentPath, inCandidate));
            }

            if (!Directory.Exists(abspath))
            {
                abspath = Path.GetFullPath(Path.Combine(_ProjectPath, inCandidate));
            }

            if (Directory.Exists(abspath))
            {
                return abspath;
            }

            return null;
        }

        private string GetAbsoluteIncludeFile(string inCandidate, string inDocumentPath)
        {
            string abspath = Path.GetFullPath(inCandidate);
            if (!File.Exists(abspath))
            {
                abspath = Path.GetFullPath(Path.Combine(inDocumentPath, inCandidate));
            }

            if (!File.Exists(abspath))
            {
                abspath = Path.GetFullPath(Path.Combine(_ProjectPath, inCandidate));
            }

            if (File.Exists(abspath))
            {
                return abspath;
            }

            return null;
        }
        
        private string[] StringToList(string inString, char[] inDelimiterChars) {
            string[] parts = inString.Split(inDelimiterChars);

            List<string> list = new List<string>();
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.Length != 0)
                {
                    list.Add(part);
                }
            }

            return list.ToArray();
        }

        private string[] MakeUnique(string [] inList)
        {
            HashSet<string> uniqueList = new HashSet<string>();
            foreach (string element in inList)
            {
                uniqueList.Add(element);
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
