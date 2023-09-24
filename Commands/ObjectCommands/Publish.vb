Imports System.Reflection
''' <summary>Executes a publish to git, and creates a git repository on a Project [Object Command].</summary>
''' <remarks>Transforms the node type from Project and to GitProject, and begins a repository connection<a href="https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository">here</a>.</remarks>
<DataContract, IconResource("Git"), DisplayName("Publish to Git Repository"), Category("Share")>
Public Class Publish : Inherits ObjectCommand(Of Project)
    '<DataMember> Public Visibility As String
    '<DataMember> Public gitUrl As String
    '<DataMember> Public gitUser As String
    '<DataMember> Public PAT As String
    <DataMember, DV("C:\GitRepositories\")> Public localRepository As String
    ''' <summary>Represents the URL of the Git repository associated with the GitProject.</summary>
    ''' <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
    '<DisplayName("Git RemoteURL"), Required, Viewable, DV("Private")>
    'Public Property _gitURL As String
    '    Get
    '        Return gitUrl
    '    End Get
    '    Set(value As String)
    '        gitUrl = value
    '    End Set
    'End Property
    ''' <summary>Represents the visibility settings of the GitProject.</summary>
    ''' <remarks>Visibility settings determine who can access and view the GitProject. This can be set to public, private, or other custom settings depending on the project's requirements.</remarks>
    '<DisplayName("Visibility"), Required, Viewable, TypeConverter(GetType(RepositoryVisibility)), DV("Private")>
    'Public Property _Visibility As String
    '    Get
    '        Return Visibility
    '    End Get
    '    Set(value As String)
    '        Visibility = value
    '    End Set
    'End Property
    ''' <summary>Represents a Git user account.</summary>
    ''' <remarks>The username is the logon name or email address of the GitUser's profile.</remarks>
    '<DisplayName("Git Username"), Required, Viewable>
    'Public Property _gitUser As String
    '    Get
    '        Return gitUser
    '    End Get
    '    Set(value As String)
    '        gitUser = value
    '    End Set
    'End Property
    ''' <summary>Represents a Personal Access Token (PAT) for Git authentication.</summary>
    ''' <remarks>A Personal Access Token is a secure way to authenticate and access Git repositories.</remarks>
    '<DisplayName("Personal Access Token"), Required, Viewable>
    'Public Property _personalAccessToken As String
    '    Get
    '        Return PAT
    '    End Get
    '    Set(value As String)
    '        PAT = value
    '    End Set
    'End Property
    <DisplayName("Local Repository"), Required, Viewable>
    Public Property _LocalRepository As String
        Get
            Return localRepository
        End Get
        Set(value As String)
            localRepository = value
        End Set
    End Property

    ''' <summary>Code that will be executed when the [Object Command] is executed.</summary>
    ''' <param name="Item">The Grooper object that the command will execute against.</param>
    Protected Overrides Sub Execute(Item As Project)
        Dim AssemblyName As String = Assembly.GetExecutingAssembly.FullName
        Dim TypeName As String = AssemblyName.Split(",")(0) + ".GitProject"

        Database.ExecuteScalar($"UPDATE TreeNode SET TypeName=N'{TypeName}' OUTPUT Inserted.RowVersion WHERE Id='{Item.Id}'")
        Item.Database.PurgeCache()
        Item.Database.ResetCache()

        Dim ProjectNode As GitProject = Me.Database.GetNode(Item.Id)
        Dim ProjectNodeProperties As GitProject.GitProject_Properties = ProjectNode.NodeProps

        ProjectNodeProperties.localRepository = localRepository
        ProjectNodeProperties.gitBranch = "* master"
        ProjectNodeProperties.README = $"# {Item.Name}"
        ProjectNodeProperties.gitIgnore = $"*.jpg{vbCrLf}"

        Dim json As String
        json = CleanSql(ObjectSerializer.ToJson(ProjectNodeProperties))
        Database.ExecuteScalar($"UPDATE TreeNode SET Properties=N'{json}' OUTPUT Inserted.RowVersion WHERE Id='{Item.Id}'")

        Dim gitConsole As New GitObject(localRepository)
        gitConsole.Init()

        FileManager.NodeToFile(ProjectNode)
        For Each ChildNode In ProjectNode.AllChildren
            FileManager.NodeToFile(ChildNode)
        Next

        gitConsole.Add(".")
        gitConsole.Commit("Repository Initialized")

    End Sub
    Protected Overrides Function CanExecute(Item As Project) As Boolean
        If Item.TypeName.Contains("GitProject") Then Return False
        Return True
    End Function
End Class
