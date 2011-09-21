// This is the minimal skeleton of a program that
// can be run using RunNET: a Program class
// with a Main method

using System;

public class Program
{
  // This method should be declared as: 
  //
  // Main(string[] arguments)
  //
  // or
  //
  // Main()
  //
  // The return type can be void or int
  public static int Main(string[] arguments)
  {
    Console.WriteLine("Hello world from C#!");
    for (int index = 0; index < arguments.Length; index++)
    {
      Console.WriteLine("Argument({0}) = '{1}'", index, arguments[index]);
    }
    return arguments.Length;
  }
}