
''' <summary>Executes a Git Fetch operation, retrieving changes from a remote repository to the specified GitProject without merging [Object Command].</summary>
''' <remarks>A Git Fetch updates the local project with changes from a remote repository, allowing for a subsequent merge or review of differences. More details can be found <a href="https://git-scm.com/docs/git-fetch">here</a>.</remarks>
<DataContract, IconResource("Fetch"), DisplayName("Fetch"), Category("Version Control")>
Public Class Fetch : Inherits ObjectCommand(Of GitProject)

    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GitProject)

    End Sub
    Protected Overrides Function CanExecute(Item As GitProject) As Boolean
        Return True
    End Function
End Class
