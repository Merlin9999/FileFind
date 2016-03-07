using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using PCLFileSet;
using PCLStorage;

namespace FileFind
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserResult<object> optionsResult = Parser.Default.ParseArguments<ListFilesOptions, ListFoldersOptions>(args);
            var parsedOptions = optionsResult as Parsed<object>;
            CommonOptions optionsTemp = parsedOptions != null
                ? (CommonOptions)parsedOptions.Value
                : new ListFilesOptions();

            try
            {
                int exitCode = optionsResult
                    .MapResult(
                        (ListFilesOptions options) =>
                        {
                            ListFiles(options);
                            return 0;
                            //if (options.Verbose) Console.WriteLine("Filenames: {0}", string.Join(",", options.InputFiles.ToArray()));
                            //return 0;
                        },
                        (ListFoldersOptions options) =>
                        {
                            ListFiles(options);
                            return 0;
                            //if (options.Verbose) Console.WriteLine("Filenames: {0}", string.Join(",", options.InputFiles.ToArray()));
                            //return 0;
                        },
                        errors =>
                        {
                            //List<Error> errorList = errors.ToList();
                            //foreach (Error error in errorList)
                            //    Console.WriteLine(error);
                            //if (errorList.Any())
                            //    ConditionallyWaitBeforeClosing(optionsTemp);
                            return 1;
                        });

                //if (args.HelpSyntax)
                //{
                //    var msg = new StringBuilder();
                //    msg.Append(optMgr.OptionSets.First().BuildHelpText(false)).AppendLine()
                //        .AppendLine("Examples:")
                //        .AppendFormat("   {0} **\\*.exe", Process.GetCurrentProcess().ProcessName).AppendLine()
                //        .AppendFormat("   {0} /p+ *.exe", Process.GetCurrentProcess().ProcessName).AppendLine()
                //        .AppendFormat("   {0} /e:**\\a*.exe /e:**\\a*.txt **\\*.exe **\\*.txt", Process.GetCurrentProcess().ProcessName).AppendLine();
                //    Console.Write(msg.ToString());
                //    ConditionallyWaitBeforeClosing(args);
                //    return;
                //}

            }
            catch (SecurityException se)
            {
                HandleException(se, false, optionsTemp.ShowDiagnosticsOnError);
            }
            catch (UnauthorizedAccessException uae)
            {
                HandleException(uae, false, optionsTemp.ShowDiagnosticsOnError);
            }
            catch (FileFindException ffe)
            {
                HandleException(ffe, false, optionsTemp.ShowDiagnosticsOnError);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                HandleException(dnfe, false, optionsTemp.ShowDiagnosticsOnError);
            }
            catch (IOException ioe)
            {
                HandleException(ioe, false, optionsTemp.ShowDiagnosticsOnError);
            }
            catch (Exception exc)
            {
                HandleException(exc, true, optionsTemp.ShowDiagnosticsOnError);
            }

            ConditionallyWaitBeforeClosing(optionsTemp);
        }

        private static void ListFiles(CommonOptions options)
        {
            if (!options.IncludePathExpressionList.Any())
                throw new FileFindException("At least one include path is required.");

            if (options.ZipFileName != null && options.FindFolders)
                throw new FileFindException(
                    "The arguments \"/ZipFileName=<ZipFile>\" and \"/Directory+\" are incompatible.");
            
            var fileSet = new FileSet(new DesktopFileSystem(), options.BaseFolder ?? @".\");

            //EDirectoryEntrySetOptions desOptions =
            //    (options.FindFiles ? EDirectoryEntrySetOptions.FindFiles : EDirectoryEntrySetOptions.None) |
            //    (options.FindFolders ? EDirectoryEntrySetOptions.FindFolders : EDirectoryEntrySetOptions.None);
            //var fes = new FolderEntrySet(optionsResult.BaseFolder, desOptions);

            foreach (string includePathExpression in options.IncludePathExpressionList)
                fileSet.Include(includePathExpression);

            foreach (string excludePathExpression in options.ExcludePathExpressionList)
                fileSet.Exclude(excludePathExpression);

            if (options.UseEnvironmentPath)
            {
                if (!string.IsNullOrWhiteSpace(options.BaseFolder))
                    throw new FileFindException("The arguments \"/BasePath+\" and \"/Path+\" are incompatible.");

                if (options.ZipFileName != null)
                    throw new FileFindException(
                        "The arguments \"/ZipFileName=<ZipFile>\" and \"/Path+\" are incompatible.");

                var pathList = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator);
                foreach (string envPath in pathList)
                {
                    foreach (string includePathExpression in options.IncludePathExpressionList)
                    {
                        if (!Path.IsPathRooted(includePathExpression))
                            fileSet.Include(Path.Combine(envPath, includePathExpression));
                    }
                }
            }

            //bool createdZip = false;
            //bool copiedFiles = false;

            //if (options.ZipFileName != null)
            //{
            //    fes.ZipFiles(options.ZipFileName);
            //    createdZip = true;
            //}

            //if (options.CopyToFolder != null)
            //{
            //    fes.CopyFiles(options.CopyToFolder);
            //    copiedFiles = true;
            //}

            //if (!createdZip && !copiedFiles)
            //{
            //    List<string> matchingFiles = fes.MatchingFiles.ToList();
            //    List<string> matchingFolders = fes.MatchingFolders.ToList();

            //    if (options.ShowPermissionErrors)
            //    {
            //        Console.WriteLine("Permission Error Messages:");
            //        Console.WriteLine();
            //        var priorErrorMessages = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            //        foreach (Exception exc in fes.PermissionExceptionList)
            //        {
            //            if (priorErrorMessages.Contains(exc.Message))
            //                continue;

            //            Console.WriteLine(exc.Message);
            //            priorErrorMessages.Add(exc.Message);
            //        }
            //        Console.WriteLine();
            //    }

            Task<IEnumerable<string>> matchingFilesTask = fileSet.GetFilesAsync();
            matchingFilesTask.Wait();
            IEnumerable<string> matchingFiles = matchingFilesTask.Result;


            if (options.ReturnRootedPaths)
            {
                matchingFiles =
                    matchingFiles.Select(
                        path =>
                            Path.GetFullPath(string.IsNullOrWhiteSpace(options.BaseFolder)
                                ? path
                                : Path.Combine(options.BaseFolder, path)));
            }

            if (options.FindFiles && options.FindFolders)
            {
                Console.WriteLine();

                if (!matchingFiles.Any())
                {
                    Console.WriteLine("No matching files.");
                }
                else
                {
                    Console.WriteLine("Matching Files:");
                    Console.WriteLine();
                    foreach (string fileName in matchingFiles)
                        Console.WriteLine(fileName);
                }

                Console.WriteLine();

                //if (matchingFolders.Count == 0)
                //{
                //    Console.WriteLine("No matching folders.");
                //}
                //else
                //{
                //    Console.WriteLine("Matching Folders:");
                //    Console.WriteLine();
                //    foreach (string folderName in matchingFolders)
                //        Console.WriteLine(folderName);
                //}
            }
            // FolderEntrySet defaults to find files when neither find files or find folders are selected.
            else if (options.FindFiles || (!options.FindFiles && !options.FindFolders))
            {
                foreach (string fileName in matchingFiles)
                    Console.WriteLine(fileName);
            }
            else //if (args.FindFolders)
            {
                //foreach (string folderName in matchingFolders)
                //    Console.WriteLine(folderName);
            }
        }

        private static void HandleException(Exception exc, bool unrecognizedError, bool showDiagnostic)
        {
            if (showDiagnostic)
                Console.WriteLine(exc.ToString());
            else
                Console.WriteLine(unrecognizedError
                    ? string.Format("Unexpected Error: \"{0}\"", exc.Message)
                    : exc.Message);
        }

        private static void ConditionallyWaitBeforeClosing(CommonOptions args)
        {
            if (args.WaitBeforeClosing)
            {
                Console.WriteLine();
                Console.WriteLine("Press any character or function key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
