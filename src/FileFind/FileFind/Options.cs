using System.Collections.Generic;
using CommandLine;

namespace FileFind
{
    [Verb("listfiles", HelpText = "List Files")]
    public class ListFilesOptions : CommonOptions
    {
    }

    [Verb("listfolders", HelpText = "List Folders")]
    public class ListFoldersOptions : CommonOptions
    {
    }

    public class CommonOptions : OldOptions
    {
        [Option('b', "basefolder", HelpText = @"Specifies a base path  ex: -b ..\..\folder")]
        public string BaseFolder { get; set; }

        [Option('i', "includepaths", HelpText = @"Inclusion path expressions  ex: -i **\*.txt **\a?.doc")]
        public IEnumerable<string> IncludePathExpressions { get; set; }

        [Option('e', "excludepaths", HelpText = @"Exclusion path expressions  ex: -e **\a1.dat **\a*\*.cs")]
        public IEnumerable<string> ExcludePathExpressions { get; set; }


        // This does not work since the new glob algorithm does NOT iterate through the file system 
        // for each include path provided. That is what the old system did. How might this be altered to work?
        // Perhaps a separate verb ??? Could call it searchpath or pathsearch.
        //
        //[Option('p', "envpath", HelpText = "Include paths in the system environment PATH")]
        //public bool UseEnvironmentPath { get; set; }

 
        [Option('r', "rootedpaths", HelpText = "Return rooted (fully qualified) paths")]
        public bool ReturnRootedPaths { get; set; }

        [Option('v', "verbose", HelpText = "Show verbose diagnostic errors")]
        public bool ShowDiagnosticsOnError { get; set; }

        [Option('w', "wait", HelpText = "Wait for a key-press before closing")]
        public bool WaitBeforeClosing { get; set; }

        [Option('a', "accesserrors", HelpText = "Show Access Denied and other permission errors")]
        public bool ShowPermissionErrors { get; set; }
    }

    public class OldOptions
    {
        public OldOptions()
        {
            this.ZipFileName = null;
            this.CopyToFolder = null;
        }

        public string ZipFileName { get; set; }
        public string CopyToFolder { get; set; }
    }



    //var args = new Options();

    //        try
    //        {
    //            var optMgr = new OptionSetManager(
    //                new OptionSet(
    //                    new CommandOption<Options>("Normal", args, "Select normal command set.", paramObj => { }),
    //                    true,
    //                    true,

    //                    // Note: Options are added in Switch and Command option pairs to support both syntaxes. 
    //                    //       Only one of the two parameter formats are on the help page. Doesn't apply to NameValue 
    //                    //       and Sequenced option types.
    //                    new NameValueOption<Options>("ZipFileName", args, "Zip the resulting files to the given file name.", (paramObj, value) => paramObj.ZipFileName = value),
    //                    new NameValueOption<Options>("CopyToFolder", args, "Copy the resulting files to the specified folder.", (paramObj, value) => paramObj.CopyToFolder = value),
    //                    )
    //            );

}
