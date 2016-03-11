using System.Collections.Generic;
using CommandLine;

namespace FileFind
{
    [Verb("listfiles", HelpText = "List Files")]
    public class ListFilesOptions : ListFilesAndListFoldersBase
    {
    }

    [Verb("listfolders", HelpText = "List Folders")]
    public class ListFoldersOptions : ListFilesAndListFoldersBase
    {
    }

    [Verb("copyfiles", HelpText = "Copy files to output folder")]
    public class CopyFilesOptions : CopyFilesAndZipFilesOptions
    {
        [Option('o', "outfolder", Required = true, HelpText = "Output folder")]
        public string OutFolder { get; set; }
    }

    [Verb("zipfiles", HelpText = "Add files to zip. Create zip if it doesn't exist")]
    public class ZipFilesOptions : CopyFilesAndZipFilesOptions
    {
        [Option('z', "zipfilename", Required = true, HelpText = "Name of the output zip file")]
        public string ZipFileName { get; set; }

        [Option('p', "zippath", HelpText = "Base path in zip file to write all files to")]
        public string ZipBaseFolder { get; set; }
    }

    [Verb("searchpath", HelpText = "Search the paths in the PATH environment variable")]
    public class SearchPathOptions : CommonOptions
    {
    }

    public class CopyFilesAndZipFilesOptions : CommonDerivedOptions
    {

    }

    public class ListFilesAndListFoldersBase : CommonDerivedOptions
    {
        [Option('r', "rootedpaths", HelpText = "Return rooted (fully qualified) paths")]
        public bool ReturnRootedPaths { get; set; }
    }

    public class CommonDerivedOptions : CommonOptions
    {
        [Option('b', "basefolder", HelpText = @"Specifies an input base path  ex: -b ..\..\folder")]
        public string BaseFolder { get; set; }
    }

    public class CommonOptions
    {
        [Option('i', "includepaths", Required = true, HelpText = @"Inclusion path expressions  ex: -i **\*.txt **\a?.doc")]
        public IEnumerable<string> IncludePathExpressions { get; set; }

        [Option('e', "excludepaths", HelpText = @"Exclusion path expressions  ex: -e **\a1.dat **\a*\*.cs")]
        public IEnumerable<string> ExcludePathExpressions { get; set; }

        [Option('v', "verbose", HelpText = "Show verbose diagnostic errors")]
        public bool ShowDiagnosticsOnError { get; set; }

        [Option('w', "wait", HelpText = "Wait for a key-press before closing")]
        public bool WaitBeforeClosing { get; set; }

        [Option('x', "abortaccess", HelpText = "Abort on Access Denied and other permission errors")]
        public bool AbortOnAccessErrors { get; set; }
    }
}
