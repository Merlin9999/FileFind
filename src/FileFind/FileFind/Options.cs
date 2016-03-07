using System.Collections.Generic;
using CommandLine;

namespace FileFind
{
    [Verb("listfiles", HelpText = "List Files (default).")]
    public class ListFilesOptions : CommonOptions
    {
    }

    [Verb("listfolders", HelpText = "List Folders.")]
    public class ListFoldersOptions : CommonOptions
    {
    }

    public class CommonOptions : OldOptions
    {
        [Value(0, HelpText = "Inclusion path expressions (space delimited).")]
        public IEnumerable<string> IncludePathExpressions { get; set; }

        [Option('e', "ExcludePaths", HelpText = "Specifies exclusion path expressions ex:-e;**/*.txt;**/*.doc", Separator = ';')]
        public IEnumerable<string> ExcludePathExpressions { get; set; }
    }

    public class OldOptions
    {
        public OldOptions()
        {
            //this.HelpSyntax = false;
            this.BaseFolder = null;
            this.FindFiles = false;
            this.FindFolders = false;
            this.UseEnvironmentPath = false;
            this.ShowPermissionErrors = false;
            this.ShowDiagnosticsOnError = true; //false;
            this.ZipFileName = null;
            this.CopyToFolder = null;
            this.ReturnRootedPaths = false;
            this.WaitBeforeClosing = false;
        }

        //public bool HelpSyntax { get; set; }
        public string BaseFolder { get; set; }

        public bool FindFiles { get; set; }
        public bool FindFolders { get; set; }
        public bool UseEnvironmentPath { get; set; }
        public bool ReturnRootedPaths { get; set; }
        public bool ShowDiagnosticsOnError { get; set; }
        public string ZipFileName { get; set; }
        public string CopyToFolder { get; set; }
        public bool ShowPermissionErrors { get; set; }
        public bool WaitBeforeClosing { get; set; }
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
    //                    new SwitchOption<Options>("File", args, "Find matching files (Default).", (paramObj, value) => paramObj.FindFiles = value),
    //                    new SwitchOption<Options>("Directory", args, "Find matching directories.", (paramObj, value) => paramObj.FindFolders = value),
    //                    new SwitchOption<Options>("RootedPath", args, "Return rooted (fully qualified) paths.", (paramObj, value) => paramObj.ReturnRootedPaths = value),
    //                    new SwitchOption<Options>("Path", args, "Use system environment PATH list as search paths.", (paramObj, value) => paramObj.UseEnvironmentPath = value),
    //                    new SwitchOption<Options>("Access", args, "Show Access Denied and other permission errors.", (paramObj, value) => paramObj.ShowPermissionErrors = value),
    //                    new SwitchOption<Options>("ShowDiagnostic", args, "Show diagnostic details on error.", (paramObj, value) => paramObj.ShowDiagnosticsOnError = value),
    //                    new SwitchOption<Options>("Wait", args, "Wait for a key-press before closing.", (paramObj, value) => paramObj.WaitBeforeClosing = value),
    //                    new NameValueOption<Options>("BasePath", args, "Specifies a base path.", (paramObj, value) => paramObj.BaseFolder = value),
    //                    new NameValueOption<Options>("ZipFileName", args, "Zip the resulting files to the given file name.", (paramObj, value) => paramObj.ZipFileName = value),
    //                    new NameValueOption<Options>("CopyToFolder", args, "Copy the resulting files to the specified folder.", (paramObj, value) => paramObj.CopyToFolder = value),
    //                    new NameValueOption<Options>("Exclude", args, "Specifies an exclusion path expression.", (paramObj, value) => paramObj.ExcludePathExpressions.Add(value)),
    //                    new SwitchOption<Options>("Help", args, null, (paramObj, value) => paramObj.HelpSyntax = value),
    //                    new CommandOption<Options>("Help", args, "Help Information.", paramObj => paramObj.HelpSyntax = true),
    //                    new SequencedOption<Options>("Include", false, true, args, "Specifies an inclusion path expression.", (paramObj, value) => paramObj.IncludePathExpressions.Add(value))
    //                    )
    //            );

}
