''' <summary>Executes a Git Commit operation on the specified GitProject [Object Command].</summary>
''' <remarks>A Git Commit captures changes to files in the project, allowing for tracking of modifications and collaboration with other contributors. More details can be found <a href="https://git-scm.com/docs/git-commit">here</a>.</remarks>
<DataContract, IconResource("Commit"), DisplayName("Commit"), Category("Version Control")>
Public Class Commit : Inherits ObjectCommand(Of GitProject)
    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GitProject)
        'TODO Git Commit
    End Sub
    Protected Overrides Function CanExecute(Item As GitProject) As Boolean
        If Item.ChangedNodes.Count > 0 Then Return True
    End Function

End Class
