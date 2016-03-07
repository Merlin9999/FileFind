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
        static int Main(string[] args)
        {
            ParserResult<object> optionsResult = Parser.Default.ParseArguments<ListFilesOptions, ListFoldersOptions>(args);
            var parsedOptions = optionsResult as Parsed<object>;
            CommonOptions optionsTemp = parsedOptions != null
                ? (CommonOptions)parsedOptions.Value
                : new ListFilesOptions();

            int exitCode;

            try
            {
                exitCode = optionsResult
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
                            ListFolders(options);
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
            }
            catch (SecurityException se)
            {
                HandleException(se, false, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (UnauthorizedAccessException uae)
            {
                HandleException(uae, false, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (FileFindException ffe)
            {
                HandleException(ffe, false, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                HandleException(dnfe, false, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (IOException ioe)
            {
                HandleException(ioe, false, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (Exception exc)
            {
                HandleException(exc, true, optionsTemp.ShowDiagnosticsOnError);
                exitCode = 1;
            }

            ConditionallyWaitBeforeClosing(optionsTemp);
            return exitCode;
        }

        private static void ListFiles(ListFilesOptions options)
        {
            ValidateOptionsForConsistency(options);

            FileSet fileSet = CreateFileSet(options.BaseFolder, 
                options.IncludePathExpressions, options.ExcludePathExpressions);

            //if (options.UseEnvironmentPath)
            //    IncludeEnvironmentPaths(options.IncludePathExpressions, fileSet);

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

            IEnumerable<string> matchingFolderItems = GetMatchingFileNames(fileSet);

            if (options.ReturnRootedPaths)
                matchingFolderItems = AlterFilePathsToFullyQualified(options.BaseFolder, matchingFolderItems);

            foreach (string fileName in matchingFolderItems)
                Console.WriteLine(fileName);
        }

        private static void ListFolders(ListFoldersOptions options)
        {
            ValidateOptionsForConsistency(options);

            FileSet fileSet = CreateFileSet(options.BaseFolder,
                options.IncludePathExpressions, options.ExcludePathExpressions);

            //if (options.UseEnvironmentPath)
            //    IncludeEnvironmentPaths(options.IncludePathExpressions, fileSet);

            IEnumerable<string> matchingFolderItems = GetMatchingFolderNames(fileSet);

            if (options.ReturnRootedPaths)
                matchingFolderItems = AlterFilePathsToFullyQualified(options.BaseFolder, matchingFolderItems);

            foreach (string fileName in matchingFolderItems)
                Console.WriteLine(fileName);
        }

        private static void ValidateOptionsForConsistency(CommonOptions options)
        {
            if (!options.IncludePathExpressions.Any())
                throw new FileFindException("At least one include path is required.");

            //if (options.UseEnvironmentPath)
            //{
            //    if (!string.IsNullOrWhiteSpace(options.BaseFolder))
            //        throw new FileFindException("The arguments \"/BasePath+\" and \"/Path+\" are incompatible.");

            //    if (options.ZipFileName != null)
            //        throw new FileFindException(
            //            "The arguments \"/ZipFileName=<ZipFile>\" and \"/Path+\" are incompatible.");

            //    // is the copy file copy parameter compatible with UseEnvironmentPath?
            //}
        }

        private static FileSet CreateFileSet(string baseFolder, 
            IEnumerable<string> includedPathExpressions, IEnumerable<string> excludedPathExpressions)
        {
            var fileSet = new FileSet(new DesktopFileSystem(), baseFolder ?? @".\");

            foreach (string includePathExpression in includedPathExpressions)
                fileSet.Include(includePathExpression);

            foreach (string excludePathExpression in excludedPathExpressions)
                fileSet.Exclude(excludePathExpression);

            return fileSet;
        }

        private static IEnumerable<string> AlterFilePathsToFullyQualified(string baseFolder, 
            IEnumerable<string> matchingFolderItems)
        {
            matchingFolderItems =
                matchingFolderItems.Select(
                    path =>
                        Path.GetFullPath(string.IsNullOrWhiteSpace(baseFolder)
                            ? path
                            : Path.Combine(baseFolder, path)));
            return matchingFolderItems;
        }

        private static void IncludeEnvironmentPaths(
            IEnumerable<string> alreadyIncludedPathExpressions, FileSet fileSet)
        {
            var pathList = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator);
            foreach (string envPath in pathList)
            {
                foreach (string includePathExpression in alreadyIncludedPathExpressions)
                {
                    if (!Path.IsPathRooted(includePathExpression))
                        fileSet.Include(Path.Combine(envPath, includePathExpression));
                }
            }
        }

        private static IEnumerable<string> GetMatchingFileNames(FileSet fileSet)
        {
            Task<IEnumerable<string>> matchingFilesTask = fileSet.GetFilesAsync();
            matchingFilesTask.Wait();
            return matchingFilesTask.Result;
        }

        private static IEnumerable<string> GetMatchingFolderNames(FileSet fileSet)
        {
            Task<IEnumerable<string>> matchingFilesTask = fileSet.GetFoldersAsync();
            matchingFilesTask.Wait();
            return matchingFilesTask.Result;
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
