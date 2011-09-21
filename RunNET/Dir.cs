using System;
using System.IO;

public class Program
{
  public static void Main()
  {
    DirectoryInfo dirInfo = new DirectoryInfo(".");

    Console.WriteLine("Directory of " + dirInfo.FullName);
    foreach (FileInfo f in dirInfo.GetFiles())
    {
      Console.WriteLine(string.Format("{0,-30} {1,10}", f.Name, f.Length));
    }
    Console.WriteLine("END");
  }
}

