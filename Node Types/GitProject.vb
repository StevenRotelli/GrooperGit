''' <summary>A Git Project is a library of resources that can be version controlled and managed by other people, and serves as the primary container in which design components are created and organized.</summary>
''' <remarks>Project 'resources' are configuration objects created by a designer, which define the connections, data structures, and logic 
''' used when processing documents through Grooper. </remarks>
<Category("Miscellaneous"), IconResource("Git"), HomeFolder("Projects"), SettingsType(GetType(GitProject.GitProject_Properties)), ChildTypes(GetType(ProjectResourceAttribute), GetType(Folder))>
Public Class GitProject : Inherits Project
    Private ReadOnly Property Props As GitProject_Properties
        Get
            Return NodeProps
        End Get
    End Property
#Region "NodeProps"
    <DataContract> Public Class GitProject_Properties : Inherits Project_Properties
        <DataMember> Public gitBranch As String
        <DataMember> Public localRepository As String
        <DataMember> Public gitStatus As String
        <DataMember> Public gitChanges As String
        <DataMember> Public gitIgnore As String
        <DataMember> Public README As String
        <DataMember> Public ChangedIds As List(Of Guid)
        '<DataMember> Public Visibility As String
#End Region
        Public Sub New(Owner As GrooperNode)
            MyBase.New(Owner)
        End Sub
    End Class
#Region "Constructors and Overrides"

    Public Sub New(gdb As GrooperDb)
        MyBase.New(gdb)
        Attributes = NodeAttributes.Sorted
    End Sub

    Public Sub New(gdb As GrooperDb, Data As NodeData)
        MyBase.New(gdb, Data)
    End Sub

    Protected Overrides Function IsPropertyVisible(PropertyName As String) As Boolean?
        'Select Case PropertyName
        '    Case NameOf(ContextElement) : Return Not String.IsNullOrEmpty(OutputElement)
        '    Case NameOf(ValuePattern) : Return Not IsWinForms
        '    Case NameOf(Pattern) : Return IsWinForms
        '    Case Else : Return MyBase.IsPropertyVisible(PropertyName)
        'End Select
        Return MyBase.IsPropertyVisible(PropertyName)
    End Function

#End Region

#Region "Viewable Properties"
    ''' <summary>Represents the URL of the Git repository associated with the GitProject.</summary>
    ''' <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
    '<DataMember, Viewable, DisplayName("Git URL"), Required, Category("Git")>
    'Public ReadOnly Property gitRepository As String
    '    Get
    '        Return Props.gitUrl
    '    End Get
    'End Property
    ''' <summary>Represents the URL of the Git repository associated with the GitProject.</summary>
    ''' <remarks>The repository URL points to the location where the Git project is hosted. Developers can use this URL to clone, fetch, or push changes to the repository. It's an essential link for collaboration and code management.</remarks>
    <DataMember, Viewable, DisplayName("Git Repositiory"), Required, Category("Git")>
    Public ReadOnly Property localRepository As String
        Get
            Return Props.localRepository
        End Get
    End Property

    ''' <summary>Represents the visibility settings of the GitProject.</summary>
    ''' <remarks>Visibility settings determine who can access and view the GitProject. This can be set to public, private, or other custom settings depending on the project's requirements.</remarks>
    '<DataMember, Viewable, DisplayName("Visibility"), Required, Category("Git"), [ReadOnly](True)>
    'Public ReadOnly Property Visibility As String
    '    Get
    '        Return Props.Visibility
    '    End Get
    'End Property
    ''' <summary>Manages Git branches within the specified GitProject.</summary>
    ''' <remarks>Git branches allow developers to work on features or fixes in isolation, without affecting the main or other development lines. This ensures code stability and streamlined collaboration. More details can be found <a href="https://git-scm.com/docs/git-branch">here</a>.</remarks>
    <Viewable, DisplayName("Branch"), Category("Git"), TypeConverter(GetType(BranchListConverter)), Required>
    Public Property gitBranch As String
        Get
            gitBranch = Props.gitBranch
            Return Props.gitBranch
        End Get
        Set(value As String)
            Props.gitBranch = value : PropsDirty = True
        End Set
    End Property

    ''' <summary>Configures the .gitignore settings for the specified GitProject [Object Command].</summary>
    ''' <remarks>The .gitignore file specifies intentionally untracked files that Git should ignore. This helps in keeping the repository clean by excluding logs, binaries, and other non-source files. More details can be found <a href="https://git-scm.com/docs/gitignore">here</a>.</remarks>
    <Viewable, DisplayName("Git Ignore"), Category("Git"), UI(GetType(PgTextEditor))>
    Public Property gitIgnore As String
        Get
            Return Props.gitIgnore
        End Get
        Set(value As String)
            Props.gitIgnore = value : PropsDirty = True
        End Set
    End Property
    ''' <summary>A list of internal changes from the current head of the repository to be included.</summary>
    ''' <remarks>Staged changes will not be committed untill manually committed.</remarks>
    <DisplayName("Staged Changes"), RequiresUI, Viewable, UI(GetType(GitNodeListEditor)), TypeConverter(GetType(PgCollectionConverter)), Category("Git")>
    Public Property ChangedNodes As List(Of GrooperNode)
        Get
            Return Database.GetNodes(Of GrooperNode)(Props.ChangedIds)
        End Get
        Set(value As List(Of GrooperNode))
            Props.ChangedIds = New List(Of Guid)(From Item In value Where (Item.Id <> Id) Select Item.Id)
            PropsDirty = True
        End Set
    End Property
    ''' <summary>Manages the README documentation within the specified GitProject.</summary>
    ''' <remarks>
    ''' The README file provides information about the project, its purpose, setup instructions, and other essential details. 
    ''' It serves as an introductory guide for new users or contributors. 
    ''' More details about README best practices can be found <a href="https://docs.github.com/en/github/creating-cloning-and-archiving-repositories/about-readmes">here</a>.
    ''' This section provides an overview of typical Markdown syntax used in README files:
    ''' <ul>
    ''' <li> `#` for headers</li>
    ''' <li> `-` or `*` for bullet lists</li>
    ''' <li> `[Link text](https://example.com)` for hyperlinks</li>
    ''' <li> `**bold**` for bold text</li>
    ''' <li> `*italic*` for italicized text</li>
    ''' <li> ````code block```` for code sections</li>
    ''' </ul>
    ''' </remarks>
    <DataMember, Viewable, DisplayName("README"), Category("Git"), UI(GetType(MarkDownEditor))>
    Public Property README As String
        Get
            Return Props.README
        End Get
        Set(value As String)
            Props.README = value : PropsDirty = True

        End Set
    End Property
    Private Function IsValidGitURL(ByVal url As String) As Boolean
        Dim result As Uri
        If Uri.TryCreate(url, UriKind.Absolute, result) Then
            Return url.EndsWith(".git") OrElse url.Contains("github.com/")
        End If
        Return False
    End Function
    ''' <summary>
    ''' Ensures [localRepository] always ends in a / character
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Private Function SanitizePath(path As String) As String
        If Not path.EndsWith("/") Then Return path + "/"
    End Function
#End Region
    Public Overrides Function ValidateProperties() As ValidationErrorList

        Dim RetVal As ValidationErrorList = MyBase.ValidateProperties()
        'If Props.gitUrl IsNot Nothing Then
        '    If IsValidGitURL(gitRepository) = False Then RetVal.Add("GitRepository", "Invalid git URL")
        'End If
        Return RetVal
    End Function

End Class


