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
            ParserResult<object> optionsResult = Parser.Default
                .ParseArguments<ListFilesOptions, ListFoldersOptions, CopyFilesOptions, ZipFilesOptions, SearchPathOptions>(args);

            var parsedOptions = optionsResult as Parsed<object>;
            CommonOptions commonOptions = parsedOptions != null
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
                        },
                        (ListFoldersOptions options) =>
                        {
                            ListFolders(options);
                            return 0;
                        },
                        (CopyFilesOptions options) =>
                        {
                            CopyFiles(options);
                            return 0;
                        },
                        (ZipFilesOptions options) =>
                        {
                            ZipFiles(options);
                            return 0;
                        },
                        (SearchPathOptions options) =>
                        {
                            SearchEnvironmentPath(options);
                            return 0;
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
                HandleException(se, false, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (UnauthorizedAccessException uae)
            {
                HandleException(uae, false, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (FileFindException ffe)
            {
                HandleException(ffe, false, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                HandleException(dnfe, false, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (IOException ioe)
            {
                HandleException(ioe, false, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }
            catch (Exception exc)
            {
                HandleException(exc, true, commonOptions.ShowDiagnosticsOnError);
                exitCode = 1;
            }

            ConditionallyWaitBeforeClosing(commonOptions);
            return exitCode;
        }

        private static void ListFiles(ListFilesOptions options)
        {
            ValidateOptionsForConsistency(options);

            FileSet fileSet = CreateFileSet(options.BaseFolder, 
                options.IncludePathExpressions, options.ExcludePathExpressions,
                !options.AbortOnAccessErrors);

            //if (options.UseEnvironmentPath)
            //    IncludeEnvironmentPaths(options.IncludePathExpressions, fileSet);

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
                options.IncludePathExpressions, options.ExcludePathExpressions,
                !options.AbortOnAccessErrors);

            //if (options.UseEnvironmentPath)
            //    IncludeEnvironmentPaths(options.IncludePathExpressions, fileSet);

            IEnumerable<string> matchingFolderItems = GetMatchingFolderNames(fileSet);

            if (options.ReturnRootedPaths)
                matchingFolderItems = AlterFilePathsToFullyQualified(options.BaseFolder, matchingFolderItems);

            foreach (string fileName in matchingFolderItems)
                Console.WriteLine(fileName);
        }

        private static void CopyFiles(CopyFilesOptions options)
        {
            ValidateOptionsForConsistency(options);

            FileSet fileSet = CreateFileSet(options.BaseFolder,
                options.IncludePathExpressions, options.ExcludePathExpressions,
                !options.AbortOnAccessErrors);

            fileSet.CopyFiles(options.OutFolder);
        }

        private static void ZipFiles(ZipFilesOptions options)
        {
            ValidateOptionsForConsistency(options);

            FileSet fileSet = CreateFileSet(options.BaseFolder,
                options.IncludePathExpressions, options.ExcludePathExpressions,
                !options.AbortOnAccessErrors);

            fileSet.ZipFiles(options.ZipBaseFolder, options.ZipFileName);
        }

        private static void SearchEnvironmentPath(SearchPathOptions options)
        {
            ValidateOptionsForConsistency(options);

            var baseFolderAndFileSetList = new List<BaseFolderAndFileSet>();
            var pathList = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator);
            foreach (string envPath in pathList)
            {
                baseFolderAndFileSetList.Add(new BaseFolderAndFileSet()
                {
                    BaseFolder = envPath,
                    FileSet =
                        CreateFileSet(envPath, options.IncludePathExpressions, options.ExcludePathExpressions,
                            !options.AbortOnAccessErrors),
                });
            }

            foreach (BaseFolderAndFileSet baseFolderAndFileSet in baseFolderAndFileSetList)
            {
                IEnumerable<string> matchingFolderItems = GetMatchingFileNames(baseFolderAndFileSet.FileSet);

                matchingFolderItems = AlterFilePathsToFullyQualified(baseFolderAndFileSet.BaseFolder, matchingFolderItems);

                var alreadyListed = new HashSet<string>();
                foreach (string fileName in matchingFolderItems)
                {
                    if (alreadyListed.Add(fileName))
                        Console.WriteLine(fileName);
                }
            }
        }

        private class BaseFolderAndFileSet
        {
            public string BaseFolder { get; set; }
            public FileSet FileSet { get; set; }
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

        private static void ValidateOptionsForConsistency(CommonOptions options)
        {
            if (!options.IncludePathExpressions.Any())
                throw new FileFindException("At least one include path is required.");

            //if (options.UseEnvironmentPath)
            //{
            //    if (!string.IsNullOrWhiteSpace(options.BaseFolder))
            //        throw new FileFindException("The arguments \"/OutputBasePath+\" and \"/Path+\" are incompatible.");

            //    if (options.ZipFileName != null)
            //        throw new FileFindException(
            //            "The arguments \"/ZipFileName=<ZipFile>\" and \"/Path+\" are incompatible.");

            //    // is the copy file copy parameter compatible with UseEnvironmentPath?
            //}
        }

        private static FileSet CreateFileSet(string baseFolder, 
            IEnumerable<string> includedPathExpressions, IEnumerable<string> excludedPathExpressions,
            bool filterFileSystemAccessExceptions)
        {
            var fileSet = new FileSet(new DesktopFileSystem(), baseFolder ?? @".\");

            foreach (string includePathExpression in includedPathExpressions)
                fileSet.Include(includePathExpression);

            foreach (string excludePathExpression in excludedPathExpressions)
                fileSet.Exclude(excludePathExpression);

            if (filterFileSystemAccessExceptions)
            {
                fileSet.Catch<SecurityException>(ex => { })
                    .Catch<UnauthorizedAccessException>(ex => { })
                    .Catch<FileFindException>(ex => { })
                    .Catch<DirectoryNotFoundException>(ex => { })
                    .Catch<IOException>(ex => { });
            }

            return fileSet;
        }

        private static void EmptyMethod()
        {
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

        private static IEnumerable<string> GetMatchingFileNames(FileSet fileSet)
        {
            Task<IEnumerable<string>> matchingFilesTask = fileSet.GetFilesAsync();
            ExceptionHelper.WaitForTaskAndTranslateAggregateExceptions(matchingFilesTask);
            return matchingFilesTask.Result;
        }

        private static IEnumerable<string> GetMatchingFolderNames(FileSet fileSet)
        {
            Task<IEnumerable<string>> matchingFilesTask = fileSet.GetFoldersAsync();
            ExceptionHelper.WaitForTaskAndTranslateAggregateExceptions(matchingFilesTask);

            return matchingFilesTask.Result;
        }

        private static void HandleException(Exception exc, bool unrecognizedError, bool showDiagnostic)
        {
            if (showDiagnostic)
                Console.WriteLine(exc.ToString());
            else
                Console.WriteLine(unrecognizedError
                    ? $"Unexpected Error: \"{exc.Message}\""
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
