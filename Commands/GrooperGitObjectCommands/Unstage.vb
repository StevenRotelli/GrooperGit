''' <summary>Executes a Git Stage operation, preparing changes for commit in the specified GitProject [Object Command].</summary>
''' <remarks>Git Staging allows you to prepare changes for a commit, helping you organize modifications and facilitating code review. Learn more about Git Staging <a href="https://git-scm.com/docs/git-stage">here</a>.</remarks>
<DataContract, IconResource("Unstage"), DisplayName("Unstage")>
Public Class Unstage : Inherits ObjectCommand(Of GrooperNode)
    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GrooperNode)
        Dim Project As GitProject.GitProject_Properties = Item.ParentProject.NodeProps
        Project.ChangedIds.Remove(Item.Id)
    End Sub
    Protected Overrides Function CanExecute(Item As GrooperNode) As Boolean
        Dim myTypes As List(Of Type) = New List(Of Type) From {
        GetType(Project), GetType(GrooperRoot), GetType(MachinesFolder), GetType(ThreadPool),
        GetType(Machine), GetType(BatchObject), GetType(BatchesFolder), GetType(Grooper.ScriptObject), GetType(GrooperRoot)
    }

        If Not myTypes.Contains(Item.GetType()) AndAlso Not myTypes.Contains(Item.GetType().BaseType) Then
            If TypeOf Item.ParentProject Is GitProject Then
                Dim Parent As GitProject = Item.ParentProject
                If Parent.ChangedNodes.Contains(Item) And Item.HasValue("GitStatus") Then
                    Return True
                End If
            End If
        End If

        Return False
    End Function

    Protected Overrides Function IsPropertyVisible(PropertyName As String) As Boolean?
        Return MyBase.IsPropertyVisible(PropertyName)
    End Function
    Protected Overridable Function Types(ByVal t As Type()) As List(Of String)
        Dim innerList As New List(Of String)
        For Each i In t
            innerList.Add(i.Name)
        Next
        Return innerList
    End Function
End Class
