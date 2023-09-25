
''' <summary>Executes a Git Merge operation, combining changes from different branches into the specified GitProject [Object Command].</summary>
''' <remarks>A Git Merge integrates changes from one branch into another, resolving differences and creating a new commit if necessary. More details can be found <a href="https://git-scm.com/docs/git-merge">here</a>.</remarks>
<DataContract, IconResource("Merge"), DisplayName("Merge"), Category("Version Control")>
Public Class Merge : Inherits ObjectCommand(Of GitProject)

    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GitProject)

    End Sub
    Protected Overrides Function CanExecute(Item As GitProject) As Boolean
        Return True
    End Function

End Class
