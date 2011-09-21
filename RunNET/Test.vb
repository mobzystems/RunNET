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
  Public Shared Function Main() As Integer
    Console.WriteLine("Hello world from VB.NET!")
    ' Use
    ' RunNET /v test.cs
    ' to see the exit code, in this case 1234
    Return 1234
  End Function
End Class
