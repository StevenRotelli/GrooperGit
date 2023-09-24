
''' <summary>Executes a Git Push operation, sending local changes from the specified GitProject to a remote repository [Object Command].</summary>
''' <remarks>A Git Push updates the remote repository with commits made locally, sharing modifications with other contributors. More details can be found <a href="https://git-scm.com/docs/git-push">here</a>.</remarks>
<DataContract, IconResource("Push"), DisplayName("Push"), Category("Version Control")>
Public Class Push : Inherits ObjectCommand(Of GitProject)

    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GitProject)

    End Sub

End Class
