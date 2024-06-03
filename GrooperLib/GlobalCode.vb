Imports System.Xml, System.Text
Imports System.Text.RegularExpressions

Public Module GlobalCode
  Public ReadOnly TypeMgr As TypeManager
  Public DebugNodeId As Guid
  Public DebugPath As String

  Sub New()
    TypeMgr = New TypeManager
    InitLicensing()
    InitEnvironment()
  End Sub

#Region "Assembly Initialization"

  Private Sub InitLicensing()

    ' Atalasoft 11.2 Key
    Atalasoft.Licensing.AtalaLicense.SetAssemblyLicense("<?xml version='1.0' encoding='utf-8'?>" +
     "<License>" +
       "<Assembly>Atalasoft.dotImage.Barcoding.Reading</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Barcoding.Writing</Assembly>" +
       "<Assembly>Atalasoft.PdfDoc</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Dicom</Assembly>" +
       "<Assembly>Atalasoft.dotImage</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Isis</Assembly>" +
       "<Assembly>Atalasoft.DotTwain</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Dwg</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Jbig2</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Jpeg2000</Assembly>" +
       "<Assembly>Atalasoft.dotImage.Pdfium</Assembly>" +
       "<Version>11.2</Version>" +
       "<Name>Atalasoft_ATL_Grooper_v11.2_2021-01-11</Name>" +
       "<ServerCPUs>3200</ServerCPUs>" +
       "<TiedAssembly>Grooper</TiedAssembly>" +
       "<Flag name='AdvancedDocClean' />" +
       "<Flag name='AdvancedPhotoEffects'/>" +
       "<Flag name='All1Dand2D' />" +
       "<Flag name='Professional'/>" +
       "<Flag name ='Document' />" +
       "<Flag name='v2Signature'/>" +
       "<Signature>aP8SzdiTWqa+dAVwFR1O4HmCg7/2W016U/0qPCb6hsrEW4ZlqYuS5LJyRzKN7vgHk2ugtEiAHdUVF/s5doaa29bnA4kcgnp+ByZDvOLNY+7HBHaU15s6gVG6ZsnPUVpMISZSGkZaDrRmkIpUP0GgjjjGD/HwfmSXrRRl3LCPhLM=</Signature>" +
      "</License>")

    'The following forces licensing to initialize at startup on the main thread.  If it happens in multithreading code, we get licensing errors:
    Dim Decoder As New Atalasoft.Imaging.Codec.Pdf.PdfDecoder
    Decoder.Dispose()
  End Sub

  Private Sub InitEnvironment()
    Dim Path As String = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process)
    Environment.SetEnvironmentVariable("Path", $"{Path};{TypeManager.CodeBase};{TypeManager.CodeBase}\IPP", EnvironmentVariableTarget.Process)
  End Sub

#End Region

#Region "Miscellaneous Math Functions"

  Public Function RadToDeg(Angle As Double) As Double
    Return Angle * (180.0# / Math.PI)
  End Function

  Public Function DegToRad(Angle As Double) As Double
    Return Math.PI * Angle / 180.0#
  End Function

  Public Function GetPercentageDifference(Value1 As Double, Value2 As Double) As Double
    If (Value1 = 0.0#) And (Value2 = 0.0#) Then Return 0.0#
    If (Value1 = 0.0#) And (Value2 <> 0.0#) Then Return 1.0#
    If (Value1 <> 0.0#) And (Value2 = 0.0#) Then Return 1.0#
    If (Value1 = Value2) Then Return 0.0#

    Dim Difference As Decimal = Math.Abs(Value1 - Value2)
    Return Difference / Math.Max(Value1, Value2)
  End Function

#End Region

#Region "Encryption Functions"

  Private Const BaseKey As String = "bi$ok"
  Private Const SecretNumber As Integer = 13900  'Don't store the whole key as a string
  Private de As New Encoder(BaseKey & SecretNumber.ToString())

  Public Function EncryptString(Value As String) As String
    Return de.Encrypt(Value)
  End Function

  Public Function DecryptString(Value As String) As String
    Return de.Decrypt(Value)
  End Function

#End Region

#Region "Text Manipulation"

  Public Function ToStr(Item As Object) As String
    Return If(Item?.ToString(), String.Empty)
  End Function

  Public Function IsGuid(Value As String) As Boolean
    Return Guid.TryParse(Value, Nothing)
  End Function

  ''' <summary>Returns a display name with spaces inserted for each capitalization in a typename. ('GrooperNode' -> 'Grooper Node')</summary>
  ''' <remarks>The practice of using compound words in programming, indicating word boundaries with capitalization is called 
  ''' '<a href='https://en.wikipedia.org/wiki/Camel_case' target='_blank'>CamelCase</a>' or 'Pascal case'.
  ''' </remarks>
  Public Function GetSpacedDisplayName(TypeName As String) As String
    TypeName = TypeName.TrimStart({"_"c})
    If (TypeName.Length < 2) Then Return TypeName

    Dim Src() As Char = TypeName.ToCharArray(), Dst As New StringBuilder

    Dim PrevChar As Char = Src(0), NextChar As Char = Src(1)
    Dst.Append(PrevChar)

    For Idx As Integer = 1 To Src.Length - 2
      Dim CurChar As Char = NextChar : NextChar = Src(Idx + 1)

      Select Case True
        Case Char.IsLower(PrevChar) And Char.IsUpper(CurChar) : Dst.Append(" "c) : Dst.Append(CurChar)
        Case Char.IsUpper(PrevChar) And Char.IsUpper(CurChar) And Char.IsLower(NextChar) : Dst.Append(" "c) : Dst.Append(CurChar)
        Case CurChar = "_"c : Dst.Append(" "c)
        Case Else : Dst.Append(CurChar)
      End Select

      PrevChar = CurChar
    Next
    Dst.Append(NextChar)

    Return Dst.ToString().Trim()

  End Function

  Public Function LoadTextFile(FileName As String) As String
    Using sr As New IO.StreamReader(FileName)
      Return sr.ReadToEnd()
    End Using
  End Function

  Public Function SaveTextFile(FileName As String, TextContent As String) As Boolean
    Using sw As New IO.StreamWriter(FileName)
      sw.Write(TextContent)
    End Using
    Return True
  End Function

  Public Function CleanSql(Value As String) As String
    Return If(Value?.Replace("'", "''"), String.Empty)
  End Function

  ''' <summary>Creates a unique name for a file system file.</summary>
  ''' <param name="Path">Full path with filename and extension.</param>
  Public Function GetUniqueFileName(Path As String) As String
    Dim NewPath As String = Path, FileName = IO.Path.GetFileNameWithoutExtension(Path), Sequence As Integer = 0
    Dim BasePath As String = IO.Path.GetDirectoryName(Path), Ext = IO.Path.GetExtension(Path)

    While (IO.File.Exists(NewPath))
      Sequence += 1
      NewPath = String.Format("{0}\{1} ({2}){3}", BasePath, FileName, Sequence, Ext)
    End While
    Return NewPath
  End Function


#End Region

#Region "MessageBox Helper Functions"

  Public Sub InfoBox(Parent As IWin32Window, Message As String, Optional Title As String = "Information")
    MessageBox.Show(Parent, Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
  End Sub

  Public Function ConfirmBox(Parent As IWin32Window, Message As String, Optional Title As String = "Confirmation") As Boolean
    Return MessageBox.Show(Parent, Message, Title, MessageBoxButtons.YesNo, MessageBoxIcon.None) = DialogResult.Yes
  End Function

  Public Sub ErrBox(Parent As IWin32Window, Message As String, Optional Title As String = "Error Message")
    MessageBox.Show(Parent, Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
  End Sub

  Public Sub ErrBox(Parent As IWin32Window, ex As Exception)
    ErrBox(Parent, ex.GetDisplayMessage())
  End Sub

  Public Function InputBox(Parent As IWin32Window, Message As String, Title As String, DefaultValue As String) As String
    Using frm As New frmInputBox(Message, Title, DefaultValue)
      If frm.ShowDialog(Parent) = DialogResult.Cancel Then Return Nothing
      Return frm.Value
    End Using
  End Function

#End Region

#Region "Serialization Helper Functions"

  Public Function FromXml(Of TheType)(ByVal sXml As String, Optional Types() As System.Type = Nothing) As TheType
    Dim xs As New DataContractSerializer(GetType(TheType), Types)
    Using sr As New IO.StringReader(sXml), xr = XmlReader.Create(sr)
      Return xs.ReadObject(xr)
    End Using
  End Function

  Public Function ToXml(Of TheType)(ByVal oObject As TheType, Optional Types() As System.Type = Nothing) As String
    Dim xs As New DataContractSerializer(GetType(TheType), Types)
    Dim sbXml As New StringBuilder

    Using sw As New IO.StringWriter(sbXml), xw As New XmlTextWriter(sw)
      xs.WriteObject(xw, oObject)
    End Using

    Return sbXml.ToString()
  End Function

#End Region

#Region "Error Handling Helpers"

  Private Function GetStackTraceLines(MaxLines As Integer) As List(Of String)
    Dim Lines As New List(Of String)
    If (MaxLines = 0) Then Return Lines

    Dim Stacktrace As New StackTrace(True)
    Dim InvalidNames As String() = {NameOf(GetStackTraceText), NameOf(WriteToEventLog), NameOf(GrooperDb.WriteToLog), "GetStackTrace", "WriteToGrooperLog", NameOf(GetStackTraceLines)}

    Dim StackTraceText As String = Stacktrace.ToString()
    Dim RawLines() As String = StackTraceText.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

    For Each Line In RawLines
      If (From Name In InvalidNames Where Line.Contains(Name)).Any Then Continue For
      Lines.Add(Regex.Replace(Line, " in .+?:line ", ", Line "))
      If (Lines.Count = MaxLines) Then Exit For
    Next

    Return Lines
  End Function

  Friend Function GetStackTraceText(Optional MaxLines As Integer = Integer.MaxValue) As String
    Return String.Join(vbCrLf, GetStackTraceLines(MaxLines))
  End Function

  ''' <summary>Writes an error, warning, information, success audit, or failure audit entry with the given message text to the Windows Application event log.</summary>
  Public Sub WriteToEventLog(Type As EventLogEntryType, Message As String, ParamArray pa() As Object)
    Dim Msg As String = If(pa.Any, String.Format(Message, pa), Message)

    Dim Lines As New List(Of String)({$"{LogEvent.GetEventName(Type)} event logged from {TypeManager.EntryAssemblyName}:", String.Empty, Msg})

    If ((Type = GrooperDb.EventType.Err) Or (Type = GrooperDb.EventType.Wrn)) Then
      Lines.Add(String.Empty) : Lines.AddRange(GetStackTraceLines(Integer.MaxValue))
    End If

    EventLog.WriteEntry("Grooper", String.Join(vbCrLf, Lines), Type)
  End Sub

  Public Sub WriteToEventLog(ex As Exception)
    WriteToEventLog(ex, "")
  End Sub

  Public Sub WriteToEventLog(ex As Exception, Message As String)
    Dim Lines As New List(Of String)({
      $"Error in {TypeManager.EntryAssemblyName}: {Message}",
      "",
      ex.GetDisplayMessage(),
      "",
      ex.StackTrace})
    EventLog.WriteEntry("Grooper", String.Join(vbCrLf, Lines), EventLogEntryType.Error)
  End Sub

#End Region

End Module
