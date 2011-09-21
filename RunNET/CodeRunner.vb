' CodeRunner.vb
' 
' Brought to you by MOBZystems, Home of Tools (http://www.mobzystems.com)
' 
' Use at will - no restrictions!

Option Strict On
Option Explicit On
Option Infer Off

Imports System.CodeDom.Compiler
Imports System.Reflection

''' <summary>
''' Class CodeRunnner. Allows snippets of C# and VB.NET code to be compiled
''' and run with arguments.
''' </summary>
Public Class CodeRunner

  ''' <summary>
  ''' This exception is thrown by RunCode when compilation fails.
  ''' The results of the compilation are available in the Results property
  ''' </summary>
  Public Class CompileException
    Inherits ApplicationException

    Public Property Results As CompilerResults

    Public Sub New(message As String, results As CompilerResults)
      MyBase.new(message)
      Me.Results = results
    End Sub
  End Class

  ''' <summary>
  ''' This exception is thrown by RunCode when the code compiles but does not execute
  ''' The run time stack trace indicates the source of the error in the compiled code snippet
  ''' </summary>
  Public Class ExecutionException
    Inherits ApplicationException

    Public Property RunTimeStackTrace As StackTrace

    Public Sub New(message As String, innerException As Exception)
      MyBase.New(message, innerException)
      Me.RunTimeStackTrace = New StackTrace(innerException, True)
    End Sub
  End Class

  ' Default warning level
  Public Const WarningLevel As Integer = 3

  ''' <summary>
  ''' Compile and run a code snippet
  ''' </summary>
  ''' <param name="code">The source code.
  ''' 
  ''' Must be a public class called Program with a static/Shared method Main that:
  ''' - returns int/Integer OR void (Sub)
  ''' - tabkes no parameters OR a string array containing arguments
  ''' </param>
  ''' <param name="codeLanguage">Language to use for compilation</param>
  ''' <param name="arguments">Arguments to pass to Program.Main()</param>
  ''' <returns>The result of calling Program.Main() as a nullable integer</returns>
  ''' <remarks>Program.Main must either return void or an int (i.e. be a Sub or a Function returning Integer)</remarks>
  Public Shared Function RunCode(
    code As String,
    codeLanguage As String,
    arguments() As String,
    references() As String
  ) As Integer?

    '--- Create the correct CodeComProvider for the supplied language
    Dim domProvider As CodeDomProvider = CodeDomProvider.CreateProvider(codeLanguage)
    If domProvider Is Nothing Then
      Throw New ApplicationException(String.Format("Unknown language: '{0}'", codeLanguage.ToString()))
    End If

    '--- Set up parameters for the compiler
    Dim parameters As CompilerParameters = CodeDomProvider.GetCompilerInfo(codeLanguage).CreateDefaultCompilerParameters()
    parameters.TreatWarningsAsErrors = True
    parameters.IncludeDebugInformation = True
    parameters.GenerateExecutable = True
    parameters.GenerateInMemory = True
    parameters.WarningLevel = WarningLevel
    ' We always start the Main method of the Program class
    parameters.MainClass = "Program"

    '--- Add a default reference to System.dll
    parameters.ReferencedAssemblies.Add("System.dll")

    ' Add the references supplied by the caller
    If references IsNot Nothing Then
      parameters.ReferencedAssemblies.AddRange(references)
    End If

    '--- Perform the actual compilation
    Dim compilerResults As CompilerResults = domProvider.CompileAssemblyFromSource(
      parameters,
      code
    )

    ' Do we have any errors and/or warnings?
    If compilerResults.Errors.Count > 0 Then
      Throw New CompileException("Code was not compiled", compilerResults)
    End If

    '--- Now call the in-memory assembly we just generated

    ' Get the method we are looking for: Program.Main()
    ' First get the Program class
    Dim classType As Type = compilerResults.CompiledAssembly.GetType(
      "Program",
      False,
      True
    )

    If classType Is Nothing Then
      Throw New ApplicationException("Cannot locate class 'Program'")
    End If

    ' Then get the Main method
    Dim method As MethodInfo = classType.GetMethod(
      "Main",
      BindingFlags.Public Or BindingFlags.Static Or BindingFlags.IgnoreCase
    )

    If method Is Nothing Then
      Throw New ApplicationException("Cannot locate method Program.Main()")
    End If

    ' Call the method, passing the arguments passed to ourselves
    Try
      ' Set up arguments for Program.Main (if it has any)
      Dim mainArguments() As Object = Nothing
      If method.GetParameters().Length = 1 Then
        mainArguments = New Object() {arguments}
      End If
      Dim callResult As Integer? = CInt(method.Invoke(Nothing, mainArguments))
      ' Return the result of the method call
      Return callResult
    Catch ex As TargetInvocationException
      ' On exception: dump the stack of the INNER exception (which points to the compiled assembly)
      Throw New ExecutionException(
        String.Format("Error calling Program.Main(): {0}", ex.InnerException.Message),
        ex.InnerException
      )
    Catch ex As Exception
      ' Other exceptions: there is some problem calling the method
      Throw New ApplicationException(String.Format("Error calling Program.Main(): {0}", ex.Message), ex)
    End Try
  End Function
End Class
