Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Text.RegularExpressions

Imports System.CodeDom

Module RunNET

  ''' <summary>
  ''' The exit codes of this application
  ''' </summary>
  ''' <remarks>Normally, the exit code is determined by the code snippet executed!</remarks>
  Public Enum ExitCode
    OK = 0
    [Error] = -1
    CompileError = -2
    ExecutionError = -3
  End Enum

  ''' <summary>
  ''' RunNET main method. Runs a C# or VB.NET code snippet as a console application
  ''' </summary>
  Public Function Main(arguments() As String) As Integer

    Try
      ' We need arguments, so complain if we don't have any
      If arguments.Length = 0 Then
        Usage()
      End If

      ' Options:
      Dim verboseOption As Boolean = False
      Dim showLanguagesOption As Boolean = False
      Dim language As String = Nothing

      ' File names:
      Dim filenames As New List(Of String)

      ' Parse arguments for options and file names
      For Each argument As String In arguments
        ' Is this an option?
        If argument.StartsWith("/") Or argument.StartsWith("-") Then
          Dim optionString As String = argument.Substring(1).ToLowerInvariant()
          Select Case optionString
            Case "l"
              showLanguagesOption = True
            Case "v"
              verboseOption = True
            Case Else
              If optionString.StartsWith("l") Then
                language = optionString.Substring(1)
              Else
                Throw New ApplicationException(String.Format("Unknown option: '{0}'", argument))
              End If
          End Select
        Else
          ' Not an option - must be a file name
          filenames.Add(argument)
        End If
      Next

      ' Show languages and exit is /l specified
      If showLanguagesOption Then
        ShowLanguages()
        Return 0
      End If

      ' We need a file name plus arguments
      If filenames.Count = 0 Then
        Usage()
      End If

      ' Remove the code file name from the list of filenames
      Dim codeFile As String = filenames(0)
      filenames.RemoveAt(0)

      ' No language specified on the command line? Then guess from extension:
      If language Is Nothing Then
        ' Get the language from the source code file extension
        Dim extension As String = Path.GetExtension(codeFile)
        language = Compiler.CodeDomProvider.GetLanguageFromExtension(extension)
        If language Is Nothing Then
          Throw New ApplicationException(String.Format("Cannot determine language from extension '{0}'", extension))
        End If
      End If

      If verboseOption Then
        Console.WriteLine(String.Format("Using language '{0}'", language))
      End If

      ' The rest are arguments for the Main()-method in the code
      Dim codeArguments() As String = filenames.ToArray()

      ' Do the actual compiling.

      ' Get the source from the code file
      Dim codeSource As String = File.ReadAllText(codeFile)

      ' Scan the source for additional references
      Dim referencesRegex As New Regex("\bref://(?<file>\S+)")

      ' Scan the source for more references. We use the form: ref://System.Net.dll
      Dim refMatches As MatchCollection = referencesRegex.Matches(codeSource)
      Dim references As New List(Of String)

      For Each refMatch As Match In refMatches
        Dim reference As String = refMatch.Groups("file").Value
        If verboseOption Then
          Console.WriteLine("Found reference: " + reference)
        End If
        references.Add(reference)
      Next

      ' Use CodeRunner to compile and run the code
      Dim callResult As Integer? = CodeRunner.RunCode(
          codeSource,
          language,
          codeArguments,
          references.ToArray()
        )


      ' Display the return value on the console in verbose mode
      If verboseOption Then
        If callResult.HasValue Then
          Console.WriteLine(
            String.Format("Program.Main() returned {0}", callResult.Value)
          )
        Else
          Console.WriteLine("Program.Main() did not return a value")
        End If
      End If

      ' Determine the result of this program
      Dim mainResult As Integer = ExitCode.OK

      ' Did we get a valid result from Program.Main()?
      If callResult.HasValue Then
        mainResult = callResult.Value
      End If

      ' Display exit code in verbose mode
      If verboseOption Then
        Console.WriteLine(String.Format("Exit code: {0}", mainResult))
      End If

      ' Pass the main result on as the result of this program
      Return mainResult

    Catch ex As CodeRunner.CompileException
      ' For compilation exceptions: show the error list
      Console.Error.WriteLine(My.Application.Info.Title + ": " + ex.Message)
      For Each compileError As Compiler.CompilerError In ex.Results.Errors
        Console.Error.WriteLine(String.Format("{0,4}: {1}", compileError.Line, compileError.ErrorText))
      Next
      Return ExitCode.CompileError

    Catch ex As CodeRunner.ExecutionException
      ' For execution exceptions: show the run time stack trace
      Console.Error.WriteLine(My.Application.Info.Title + ": " + ex.Message + " (" + ex.InnerException.GetType().Name + ")")
      Console.Error.WriteLine(ex.RunTimeStackTrace.ToString())
      Return ExitCode.ExecutionError

    Catch ex As Exception
      ' When an exception occurs, we return -1
      Console.Error.WriteLine(My.Application.Info.Title + ": " + ex.Message)
      Return ExitCode.Error
    End Try
  End Function

  ''' <summary>
  ''' Print out usage of this application
  ''' </summary>
  Private Sub Usage()
    Throw New ApplicationException(
      My.Application.Info.Title + " v" + My.Application.Info.Version.ToString(3) + " (" + CStr(IntPtr.Size * 8) + "-bit)" + Environment.NewLine +
      My.Application.Info.Copyright + Environment.NewLine +
      "http://www.mobzystems.com/tools/runnet" + Environment.NewLine +
      Environment.NewLine +
      "Run .NET code as a console application." + Environment.NewLine +
      Environment.NewLine +
      "Usage: RunNET [/v] source.ext [arg [arg ...]]" + Environment.NewLine +
      "Options:" + Environment.NewLine +
      "  /v  - verbose (show result of call to Program.Main and exit code)" + Environment.NewLine +
      "  /l  - show supported languages and extensions on this computer" + Environment.NewLine +
      "source.ext   - source code to run. Extensions must be a valid extension for the language used." + Environment.NewLine +
      "               The static/Shared method Main() in the public class Program is called," + Environment.NewLine +
      "               and its result is returned as exit code for " + My.Application.Info.Title + "." + Environment.NewLine +
      "               Main() must return an int/Integer, or return void/be a Sub." + Environment.NewLine +
      "               It must accept no arguments, or an array of string, specifying arguments" + Environment.NewLine +
      "arg          - argument(s) passed to Program.Main()"
    )
  End Sub

  ''' <summary>
  ''' Show the supported language providers, languages and extensions installed on this machine
  ''' </summary>
  ''' <remarks></remarks>
  Private Sub ShowLanguages()
    Dim compilerInfoList() As Compiler.CompilerInfo = Compiler.CodeDomProvider.GetAllCompilerInfo()
    For Each compilerInformation As Compiler.CompilerInfo In compilerInfoList
      Try
        Console.WriteLine(String.Format("Provider: {0}", compilerInformation.CodeDomProviderType.Name))
      Catch ex As Exception
        Console.WriteLine(ex.Message)
        Continue For
      End Try
      PrintQuotedList("Supported languages", compilerInformation.GetLanguages())
      PrintQuotedList("Supported extensions", compilerInformation.GetExtensions().Where(Function(e) e.StartsWith(".")))
    Next
  End Sub

  ''' <summary>
  ''' Print a label, then a quoted list of strings on a single line
  ''' </summary>
  Private Sub PrintQuotedList(label As String, list As IEnumerable(Of String))
    If list.Count > 0 Then
      Console.Write(label + ": ")
      Dim index As Integer = 0
      For Each item As String In list
        Console.Write("""" + item + """")
        index += 1
        If index < list.Count Then
          Console.Write(", ")
        End If
      Next
      Console.WriteLine()
    End If
  End Sub
End Module
