''' <summary>Executes a Git Compare/Diff operation, showing differences between files or branches in the specified GitProject [Object Command].</summary>
''' <remarks>A Git Compare/Diff highlights the changes between two sets of code, aiding in understanding modifications and aiding in code review. More details can be found <a href="https://git-scm.com/docs/git-diff">here</a>.</remarks>
<DataContract, IconResource("Diff"), DisplayName("MarkAsChanged")>
Public Class MarkAsChanged : Inherits ObjectCommand(Of GrooperNode)
    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GrooperNode)
        Item.SetValue("GitStatus", "Modified")
    End Sub
    Protected Overrides Function CanExecute(Item As GrooperNode) As Boolean
        Dim myTypes As List(Of Type) = New List(Of Type) From {
        GetType(Project), GetType(GrooperRoot), GetType(MachinesFolder), GetType(ThreadPool),
        GetType(Machine), GetType(BatchObject), GetType(BatchesFolder), GetType(Grooper.ScriptObject), GetType(GrooperRoot)
    }
        If Not myTypes.Contains(Item.GetType()) AndAlso Not myTypes.Contains(Item.GetType().BaseType) Then

            If TypeOf Item.ParentProject Is GitProject Then
                Return True
            End If
        End If

        Return False ' Add a return statement in case none of the conditions are met.
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
