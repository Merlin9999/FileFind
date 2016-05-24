# FileFind
FileFind is a console utility written in c# that is both useful and a working example of using the PCLFileSet and PCLFileSet.Desktop libraries. These libraries are available [here](http://github.com/Merlin9999/PCLFileSet).
## Available command line verbs:
```
  listfiles      List Files
  listfolders    List Folders
  copyfiles      Copy files to output folder
  zipfiles       Add files to zip. Create zip if it doesn't exist
  searchpath     Search the paths in the PATH environment variable
  help           Display more information on a specific command.
  version        Display version information.
```
## Parameters for the verb listfiles
```
  -r, --rootedpaths     Return rooted (fully qualified) paths
  -b, --basefolder      Specifies an input base path  ex: -b ..\..\folder
  -i, --includepaths    Required. Inclusion path expressions  ex: -i **\*.txt
                        **\a?.doc
  -e, --excludepaths    Exclusion path expressions  ex: -e **\a1.dat **\a*\*.cs
  -v, --verbose         Show verbose diagnostic errors
  -w, --wait            Wait for a key-press before closing
  -x, --abortaccess     Abort on Access Denied and other permission errors
  --help                Display this help screen.
  --version             Display version information.
```
## Example
`C:> filefind listfiles -b .\\..\\.. -i bin\\Debug\\*.exe bin\\Debug\\*.dll bin\\Debug\\*.config -e **\\*.vshost.*`

This example sets upa base folder, then add 3 include paths and 1 exclude path. The output of this command is:
```
bin\Debug\CommandLine.dll
bin\Debug\FileFind.exe
bin\Debug\FileFind.exe.config
bin\Debug\FSharp.Core.dll
bin\Debug\Ionic.Zip.dll
bin\Debug\PCLFileSet.Desktop.dll
bin\Debug\PCLFileSet.dll
bin\Debug\PCLStorage.Abstractions.dll
bin\Debug\PCLStorage.dll
bin\Debug\System.Reactive.Core.dll
bin\Debug\System.Reactive.Interfaces.dll
bin\Debug\System.Reactive.Linq.dll
bin\Debug\System.Reactive.PlatformServices.dll
```

