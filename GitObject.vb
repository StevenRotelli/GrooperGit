Public Class GitObject
    Private _cwd As String
    <DisplayName("Current Working Directory")>
    Public Property cwd As String
        Get
            Return _cwd
        End Get
        Set(value As String)
            Dim di As New IO.DirectoryInfo(value)
            If Not di.Exists Then di.Create()
            _cwd = value
        End Set
    End Property
    ''' <summary>
    ''' Initializer for the GitObject
    ''' </summary>
    ''' <param name="cwd">The current working directory that will be used</param>
    Public Sub New(cwd As String)
        Me._cwd = cwd
    End Sub
#Region "Visible Methods"
    ''' <summary>
    ''' Initializes a new Git repository for the given repository name.
    ''' This method invokes the 'git init' command which initializes a new Git repository and begins tracking an existing directory.
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-init
    ''' </summary>
    Public Sub Init()
        BaseCommand(cwd, "init")
    End Sub
    ''' <summary>
    ''' Stages the specified files or directories in preparation for a commit.
    ''' The 'git add' command updates the index using the current content found in the working tree.
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-add
    ''' </summary>
    ''' <param name="pathSpec">The file or directory paths to stage. Can use patterns like '*.txt'.</param>
    Public Sub Add(pathSpec As String)
        BaseCommand(cwd, $"add {pathSpec}")
    End Sub

    ''' <summary>
    ''' Unstages or removes the specified files or directories from the index and the working tree.
    ''' The 'git rm' command removes files from the working tree and from the index.
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-rm
    ''' </summary>
    ''' <param name="pathSpec">The file or directory paths to remove. Can use patterns like '*.txt'.</param>
    Public Sub Remove(pathSpec As String)
        BaseCommand(cwd, $"rm {pathSpec}")
    End Sub
    ''' <summary>
    ''' Shows changes between the working directory and the index.
    ''' The 'git diff' command shows changes between the working directory and the index (by default).
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-diff
    ''' </summary>
    ''' <param name="File">The file to be comparred with the active branch</param>
    Public Function Diff(file As String) As IEnumerable(Of String)
        BaseCommand(cwd, "diff").Split(vbLf)
    End Function
    ''' <summary>
    ''' Records changes made to the repository.
    ''' The 'git commit' command creates a new commit containing the current contents of the index with a log message.
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-commit
    ''' </summary>
    ''' <param name="message">The commit message describing the changes.</param>
    Public Sub Commit(message As String)
        BaseCommand(cwd, $"commit -m '{message}'")
    End Sub
    Public Function ListBranches() As IEnumerable(Of String)
        Return BaseCommand(cwd, "branch").Split(vbLf)
    End Function
    ''' <summary>
    ''' Interacts with the branch command in Git.
    ''' The 'git branch' command lists, creates, or deletes branches.
    ''' For more information, refer to the official Git documentation: https://git-scm.com/docs/git-branch
    ''' </summary>
    ''' <param name="operation">The operation to perform: "list", "create", or "delete".</param>
    ''' <param name="branchName">The name of the branch, if applicable.</param>
    Public Sub Branch(operation As String, Optional branchName As String = "")
        Select Case operation.ToLower()
            Case "list"
                BaseCommand(cwd, "branch")
            Case "create"
                If Not String.IsNullOrEmpty(branchName) Then
                    BaseCommand(cwd, $"branch {branchName}")
                Else
                    Throw New ArgumentException("Branch name is required for create operation.")
                End If
            Case "delete"
                If Not String.IsNullOrEmpty(branchName) Then
                    BaseCommand(cwd, $"branch -d {branchName}")
                Else
                    Throw New ArgumentException("Branch name is required for delete operation.")
                End If
            Case Else
                Throw New ArgumentException($"Invalid operation: {operation}. Supported operations are 'list', 'create', and 'delete'.")
        End Select
    End Sub

#End Region
    ''' <summary>
    ''' Executes a git command passed to the CLI. Requires git to be installed.
    ''' </summary>
    ''' <param name="gitArguments">Arguments are the same as the git cli command, i.e init, connect, push, clone</param>
    Private Shared Function BaseCommand(path As String, Optional ByRef gitArguments As String = "help") As String
        Dim di As New IO.DirectoryInfo(path)
        If di.Exists = False Then di.Create()
        Dim command As String = $"cd '{path}'; git {gitArguments}"

        Dim process As Process = New Process()
        Dim pinfo As New ProcessStartInfo() With {
            .FileName = "powershell.exe",
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .Arguments = "-NoProfile -ExecutionPolicy Bypass -Command " & command
        }
        process.StartInfo = pinfo
        process.Start()

        Dim output As String = process.StandardOutput.ReadToEnd()
        Dim errorOutput As String = process.StandardError.ReadToEnd()

        process.WaitForExit()
        If Not String.IsNullOrEmpty(errorOutput) Then
            Dim err As New GrooperException($"GitOutput: {errorOutput}")
            Return errorOutput
        End If
        Return output
    End Function

End Class
