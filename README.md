pathlib
=======

Path manipulation library for .Net

We're on [NuGet](https://www.nuget.org/packages/PathLib/)!

## Changelog

### Master

* Correct the XML Serializer, fixes #9

[Full Changelog](CHANGELOG.md)

## Why a library for paths?

Paths are commonly used in programming, from opening files to storage
directories. They're integral to any program, yet unlike their siblings URLs
and URIs very few programming languages (with strong typing) have a strongly
typed solution for storing and manipulating paths.

Instead, programmers are forced to store these paths as strings and use a host
of static methods to pinch and twist one path into another. In .Net, these are
found in the `System.IO.Path` and `System.IO.Directory` namespaces. Common
operations include combining paths (`Path.Combine(path1, path2)`) and
extracting the filename (`Path.GetFileName("C:\file.txt")`). Since these are
only valid for a particular subset of strings, I'm surprised that more
languages do not have objects corresponding to a path so that methods and
libraries can accept a "path object" and be confident that the input data
conforms to at least a rudimentary set of validation criteria (even if the
path itself doesn't exist on disk).

Despite being available in
[Ruby](http://www.ruby-doc.org/stdlib-1.9.3/libdoc/pathname/rdoc/Pathname.html),
[Python](https://docs.python.org/3/library/pathlib.html),
[C++](http://www.boost.org/doc/libs/1_33_1/libs/filesystem/doc/path.htm), and
[Java](http://docs.oracle.com/javase/tutorial/essential/io/pathClass.html),
I've never seen real-life code that uses any of these libraries, nor have I
read a single blog post extolling their virtues (and if my experience
dogfooding PathLib is any indication, there are many). The best thing about
having a class dedicated to paths, in my opinion, is setting expectations for
methods that use paths. Which of these more clearly explains what value to pass
into the method?

```csharp
public Database OpenDatabase(string dbLocation) {}
    
public Database OpenDatabase(Path dbLocation) {}
```
    
I can't speak for anyone else, but I'm certainly less tempted to pass
`"http://localhost:4000"` to the latter than the former.

To give you a small taste of the power of PathLib, compare implementations of
this (real) scenario: list all files within the user's "myapp" directory and
copy all alphanumeric characters into a new file with an extra ".clean"
extension (so `file.txt` becomes `file.clean.txt`). I've avoided using `var`
to show how many "[stringly typed](http://c2.com/cgi/wiki?StringlyTyped)"
objects are created in the non-PathLib version compared to using PathLib.

```csharp
IPath appDir = new WindowsPath("~/myapp").ExpandUser();
foreach(IPath file in appDir.ListDir())
{
  if(!file.IsFile()) continue;
  string text = file.ReadAsText();
  text = Regex.Replace(text, @"\W+", "");
  
  IPath newFile = file.WithFilename(
    file.Basename + ".clean" + file.Extension);
  using(var output = new StreamWriter(newFile.Open(FileMode.Create)))
  {
    output.Write(text);
  }
}
```

```csharp
string userDir = Environment.GetFolderPath(
  System.Environment.SpecialFolder.UserProfile);  // Only in .Net 4.0
string appDir = Path.Combine(userDir, "myapp");
foreach(string file in Directory.EnumerateFiles(appDir))
{
  string text = File.ReadAllText(file);
  text = Regex.Replace(text, @"\W+", "");

  string newFile = Path.Combine(appDir, 
    Path.GetFileNameWithoutExtension(file) + 
    ".clean" + 
    Path.GetExtension(file));
  using(var output = new StreamWriter(File.Open(newFile, FileMode.Create)))
  {
    output.Write(text);
  }
}
```

## What is PathLib?

The goal of PathLib is to extend the feature set of `System.IO.Path` and bundle
it all into a strongly typed path object. It borrows some terminology from the
similarly named Python library mentioned above.

There are four main classes and two main interfaces in the library:

* **IPurePath**: A platform-agnostic interface for "pure paths", or those that
do not touch the filesystem. All operations are guaranteed to be supported on
any platform so, for instance, your application can create and use
Windows-style paths on a Linux machine (perfect for remote applications or web
apps that manipulate client paths on a server).
* **IPath**: A platform-agnostic interface for concrete paths. These can
perform operations that touch the filesystem such as file/directory exists,
resolving symbolic links, and reading the contents of files. All IPaths inherit
the IPurePath interface which works as a form of multiple inheritance even
though the language itself doesn't support it.
* **PureWindowsPath**: A pure path using Windows validation and styling rules.
For example, absolute (non-UNC) paths begin with a drive letter and use
backslashes as separators and comparisons are case insensitive.
* **PurePosixPath**: A pure path using POSIX validation and styling rules.
POSIX-compliant systems include Linux, UNIX, and Mac OSX. These paths use
forward slashes as component separators and have case sensitive comparisons
among other differences.
* **WindowsPath**: A concrete path using Windows validation and styling rules.
Additionally, this class has methods that touch the filesystem. Due to this,
the class can only be used on Windows systems.
* **PosixPath**: A concrete path using POSIX validation and styling rules.
Additionally, this class has methods that touch the filesystem. Due to this,
the class can only be used on POSIX-compliant systems.

## Factories

Since application and library developers usually want to be as
cross-platform-compatible as possible, it doesn't make much sense to explicitly
create instances of a "windows path" or "posix path". To that end, the library
provides a couple of path factories that automatically detect the user's
operating system and create the appropriate path on command.

### PurePathFactory

This factory builds a pure path for the current operating system. You may also
provide a set of `PurePathFactoryOptions` to the builder:

* AutoNormalizeCase: Always normalize a path's case in the created object (this
has no effect on case sensitive platforms).

For convenience, a static, global PurePathFactory instance can be accessed from
the `PurePath` class (no generic arguments).

### PathFactory

This factory builds a concrete path for the current operating system. You may
also provide a set of `PathFactoryOptions` to the builder:

* AutoNormalizeCase: Always normalize a path's case in the created object (this
has no effect on case sensitive platforms).
* AutoExpandEnvironmentVariables: Always replace the value of environment
variables present in the created object.
* AutoExpandUserDirectory: If the path begins with a tilde (`~`), replace it
with the current user's directory. If the `UserDirectory` property on the
options class is non-null, use that path as the user directory.

For convenience, a static, global PurePathFactory instance can be accessed from
the `PurePath` class (no generic arguments).

## Serialization

One of the key behaviors of a new data type is the ability to serialize and
deserialize to/from the various data storage and transport formats out there
(namely, XML and JSON). Due to the
[magic of TypeConverters](http://www.hanselman.com/blog/TypeConvertersTheresNotEnoughTypeDescripterGetConverterInTheWorld.aspx),
that support is built in! Here's an example of deserializing a path in JSON.Net:

```csharp
class Data
{
  public IPath Path { get; set; }
}

public static void Main()
{
    var json = @"{ ""path"": ""C:/users/me/file.txt"" }";
    var data = JsonConvert.DeserializeObject<Data>(json);
    Console.WriteLine(data.Path.Directory);
    // C:\users\me
}
```

Note that I used an IPath interface rather than WindowsPath or PosixPath.
While each of those have their own TypeConverters, both `IPath` and
`IPurePath` have special converters that use a PathFactory to choose the
correct interface type. This makes it simple to support users on any platform.
