# RunNET
In a nutshell: RunNET takes a .NET source code file (C# or VB.NET or whatever you have installed on your machine) and runs it as a console application. This is what RunNET itself shows when run without arguments:

```
RunNET: RunNET v1.0.0 (64-bit)
Copyright (C) MOBZystems, 2011
http://www.mobzystems.com/tools/runnet

Run .NET code as a console application.

Usage: RunNET [/v] source.ext [arg [arg ...]]
Options:
  /v  - verbose (show result of call to Program.Main and exit code)
  /l  - show supported languages and extensions on this computer
source.ext   - source code to run. Extensions must be a valid extension for the language used.
               The static/Shared method Main() in the public class Program is called,
               and its result is returned as exit code for RunNET.
               Main() must return an int/Integer, or return void/be a Sub.
               It must accept no arguments, or an array of string, specifying arguments
arg          - argument(s) passed to Program.Main()
```

The source code must contain a Program class with a Main method. e.g.:

```C#
// This is the minimal skeleton of a program that
// can be run using RunNET: a Program class
// with a Main method

using System;

public class Program
{
  // This method should be declared as: 
  //
  // Main(string arguments[])
  //
  // or
  //
  // Main()
  //
  // The return type can be void or int
  public static int Main()
  {
    Console.WriteLine("Hello world from C#!");
    // Use
    // RunNET /v test.cs
    // to see the exit code, in this case 1234
    return 1234;
  }
}
```

See the source code for more examples.

For more documentation and samples, please visit [MOBZystems, Home of Tools](http://www.mobzystems.com/Tools/RunNET)
