using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using PCLFileSet;

namespace FileFind
{
    /// <summary>
    /// A static class with extension methods for the interface <see cref="IFileSet"/>.
    /// </summary>
    public static class FileSetExtensions
    {
        /// <summary>
        /// Zips the files in the <see cref="FileSet"/>.
        /// </summary>
        /// <param name="fileSet">The file set.</param>
        /// <param name="zipFileName">Name of the zip file to create.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void ZipFiles(this FileSet fileSet, string zipFileName)
        {
            fileSet.ZipFiles(null, zipFileName);
        }

        /// <summary>
        /// Zips the files in the <see cref="IFileSet"/>.
        /// </summary>
        /// <param name="fileSet">The file set.</param>
        /// <param name="outputBasePath">Path to prepend to all files in the <see cref="fileSet"/> in the zip file</param>
        /// <param name="zipFileName">Name of the zip file to create.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void ZipFiles(this FileSet fileSet, string outputBasePath, string zipFileName)
        {
            ZipFiles(
                new[] {new FileSetAndBasePath() {OutputBasePath = outputBasePath, FileSet = fileSet}},
                zipFileName);
        }

        /// <summary>
        /// Zips the files in the <see cref="IEnumerable{FileSetAndBasePath}"/>.
        /// </summary>
        /// <param name="fileSets">The file sets.</param>
        /// <param name="zipFileName">Name of the zip file to create.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void ZipFiles(this IEnumerable<FileSet> fileSets, string zipFileName)
        {
            ZipFiles(
                fileSets.Select(fs => new FileSetAndBasePath() {OutputBasePath = null, FileSet = fs}),
                zipFileName);
        }

        /// <summary>
        /// Zips the files in the <see cref="IEnumerable{FileSetAndBasePath}"/>.
        /// </summary>
        /// <param name="fileSetsWithBasePaths">The file sets each paired with a base path.</param>
        /// <param name="zipFileName">Name of the zip file to create.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void ZipFiles(this IEnumerable<FileSetAndBasePath> fileSetsWithBasePaths,
            string zipFileName)
        {
            using (ZipFile zip = new ZipFile(zipFileName))
            {
                foreach (FileSetAndBasePath fileSetEntry in fileSetsWithBasePaths)
                {
                    Task<IEnumerable<string>> filesTask = fileSetEntry.FileSet.GetFilesAsync();

                    ExceptionHelper.TranslateAggregateExceptions(() =>
                    {
                        filesTask.Wait();

                        foreach (string fileToZip in filesTask.Result)
                        {
                            string fileToZipWithBasePath;
                            if (string.IsNullOrEmpty(fileSetEntry.FileSet.BasePath))
                                fileToZipWithBasePath = fileToZip;
                            else
                                fileToZipWithBasePath = Path.Combine(fileSetEntry.FileSet.BasePath, fileToZip);

                            string outputFileDirectory = Path.GetDirectoryName(fileToZip);
                            if (string.IsNullOrEmpty(outputFileDirectory))
                            {
                                if (string.IsNullOrEmpty(fileSetEntry.OutputBasePath))
                                    zip.AddFile(fileToZipWithBasePath);
                                else
                                    zip.AddFile(fileToZipWithBasePath, fileSetEntry.OutputBasePath);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(fileSetEntry.OutputBasePath))
                                    zip.AddFile(fileToZipWithBasePath, outputFileDirectory);
                                else
                                    zip.AddFile(fileToZipWithBasePath, Path.Combine(fileSetEntry.OutputBasePath, outputFileDirectory));
                            }
                        }
                    });
                }

                zip.Save();
            }
        }

        /// <summary>
        /// Copies the files in the <see cref="IFileSet"/>.
        /// </summary>
        /// <param name="fileSet">The file sets.</param>
        /// <param name="outputFolder">Path to copy the files to.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void CopyFiles(this FileSet fileSet, string outputFolder)
        {
            CopyFiles(
                new[] { new FileSetAndBasePath() { OutputBasePath = null, FileSet = fileSet } },
                outputFolder);
        }

        /// <summary>
        /// Copies the files in the <see cref="IEnumerable{IFileSet}"/>.
        /// </summary>
        /// <param name="fileSets">The file sets.</param>
        /// <param name="outputFolder">Base path to copy the files to.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void CopyFiles(this IEnumerable<FileSet> fileSets, string outputFolder)
        {
            CopyFiles(
                fileSets.Select(fs => new FileSetAndBasePath() { OutputBasePath = null, FileSet = fs }),
                outputFolder);
        }

        /// <summary>
        /// Copies the files in the <see cref="IEnumerable{FileSet}"/>.
        /// </summary>
        /// <param name="fileSetsWithBasePaths">The file sets each paired with a base path.</param>
        /// <param name="outputFolder">Base path to copy the files to.</param>
        /// <exception cref="System.ArgumentException">Unexpected path format.</exception>
        public static void CopyFiles(this IEnumerable<FileSetAndBasePath> fileSetsWithBasePaths,
            string outputFolder)
        {
            ExceptionHelper.TranslateAggregateExceptions(() =>
            {
                Parallel.ForEach(fileSetsWithBasePaths, async fileSetEntry =>
                {
                    foreach (string fileToCopy in await fileSetEntry.FileSet.GetFilesAsync())
                    {
                        string fileToCopyWithBasePath;
                        if (string.IsNullOrEmpty(fileSetEntry.FileSet.BasePath))
                            fileToCopyWithBasePath = fileToCopy;
                        else
                            fileToCopyWithBasePath = Path.Combine(fileSetEntry.FileSet.BasePath, fileToCopy);

                        string outputFileName = string.IsNullOrEmpty(fileSetEntry.OutputBasePath)
                            ? Path.Combine(outputFolder, fileToCopy)
                            : Path.Combine(outputFolder, fileSetEntry.OutputBasePath, fileToCopy);

                        string outputFilePath = Path.GetDirectoryName(outputFileName);
                        if (!string.IsNullOrEmpty(outputFilePath))
                            Directory.CreateDirectory(outputFilePath);

                        File.Copy(fileToCopyWithBasePath, outputFileName);
                    }
                });
            });
        }

        public class FileSetAndBasePath
        {
            public FileSet FileSet { get; set; }

            /// <summary>
            /// Gets or sets the path to prepend to all files in the <see cref="FileSet"/>
            /// </summary>
            public string OutputBasePath { get; set; }
        }
    }
}
