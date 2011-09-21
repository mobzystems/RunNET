' This is the minimal skeleton of a program that
' can be run using RunNET: a Program class
' with a Main method

Imports System

Public Class Program
  ' This method should be declared as: 
  '
  ' Main(arguments() As String)
  '
  ' or
  '
  ' Main()
  '
  ' It can be either a Sub or a function returning Integer
  Public Shared Function Main(ByVal arguments() As String) As Integer
    Console.WriteLine("Hello world from VB.NET!")
    For index As Integer = 0 To arguments.Le - 1
      Console.WriteLine("Argument({0}) = '{1}'", index, arguments(index))
    Next
    Return arguments.Length
  End Function
End Class
