Imports System.IO
Imports System.IO.Compression
''' <summary>A Class to maintain a file abstract of the Grooper Tree Under the git project in order to leverage GIT commands against the Grooper Nodes.
''' </summary><remarks>This is because Grooper Nodes aren't files and need to be able to detect differences as files and compare differences as files</remarks>
Public Class FileManager

    ''' <summary>
    ''' Syncs the active node settings of all chidlren in the project and marks there appropriate GitStatus
    ''' </summary>
    ''' <param name="grooperNode"></param>
    Public Shared Sub SyncProject(grooperNode As GrooperNode)
        If grooperNode.TypeDisplayName = "GitProject" Then

            Dim gitProject = DirectCast(grooperNode, GitProject)
            If gitProject.localRepository Is Nothing Or "" Then Exit Sub
            Dim gitConsole As New GitObject(gitProject.localRepository)
            For Each node As GrooperNode In gitProject.AllChildren
                If Not IO.File.Exists("") Then
                    node.SetValue("GitStatus", "New")
                    Continue For
                End If
                FileManager.NodeToFile(node)
            Next
        Else
        End If
        'TODO:  Write execution code that will be performed on the configured interval
    End Sub
    Public Shared Sub NodeToFile(Obj As Object)
        If TypeOf (Obj) Is GitProject Then
            Dim node As GitProject = Obj
            File.WriteAllLines(node.localRepository & "\" & node.Id.ToString & ".json", node.PropertiesJson.Split(vbLf))
        Else
            Dim node As GrooperNode = DirectCast(Obj, GrooperNode)
            Dim parentNode As GitProject = node.ParentProject
            File.WriteAllLines(parentNode.localRepository & "\" & node.Id.ToString & ".json", node.PropertiesJson.Split(vbLf))
        End If
    End Sub
End Class