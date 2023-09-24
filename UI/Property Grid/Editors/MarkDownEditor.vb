Public Class MarkDownEditor : Inherits PgCodeEditor(Of GitProject)

    'Public Overrides ReadOnly Property Highlighter As CodeHighlighter = CodeHighlighter.CPP
    Public Overrides ReadOnly Property Highlighter As CodeHighlighter = New XshdHighlighter("MarkDown-Mode")
    Public Overrides ReadOnly Property ShowControlChars As Boolean? = False

    Protected Overrides Sub ConfigureEditor(Editor As CodeEditor, Instance As GitProject, pd As PropertyDescriptor)
        Editor.HighlightMode = CodeEditor.HighlightingMode.MarkDown
        'Editor.CompletionProvider = New CssCompletionProvider(Instance)
    End Sub

    Public Overrides Function GetCompletionProvider(Instance As Object, pd As PropertyDescriptor) As CompletionProvider
        ' Return New CssCompletionProvider(Instance)
    End Function
End Class