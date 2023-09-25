''' <summary>Executes a Git Pull operation, updating the specified GitProject with changes from a remote repository [Object Command].</summary>
''' <remarks>A Git Pull fetches changes from a remote branch and then merges them into the current local branch, ensuring the project is up-to-date. More details can be found <a href="https://git-scm.com/docs/git-pull">here</a>.</remarks>
<DataContract, IconResource("Pull"), DisplayName("Pull"), Category("Version Control")>
Public Class Pull : Inherits ObjectCommand(Of GitProject)

    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As GitProject)

    End Sub

End Class
