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