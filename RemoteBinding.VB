Imports Grooper.CMIS
Imports EX = Microsoft.Exchange.WebServices.Data
Imports PortCMIS.Data
Imports PortCMIS
Imports PortCMIS.Enums
Imports System.Net

Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

''' <summary>Defines a connection to the <a target="_blank" href="https://git-scm.com/">Git</a> remote repository platform.</summary>
''' <remarks>Exposes commit histories, branches, tags, and other repository information. Connectivity is via Git remote protocols, which supports 
''' various platforms like GitHub, GitLab, and Bitbucket.
''' <h2>Content Types</h2>
''' This binding implements the following 4 document types.  
''' <table>
'''   <tr><th>Type Name</th><th>File Type</th><th>Description</th></tr>
'''   <tr><td>Commit</td><td>text/plain (.txt)</td><td>Represents an individual commit in the repository.</td></tr>
'''   <tr><td>Branch</td><td>text/plain (.txt)</td><td>Represents a branch in the repository.</td></tr>
'''   <tr><td>Tag</td><td>text/plain (.txt)</td><td>Represents a specific tagged version in the repository.</td></tr>
'''   <tr><td>File</td><td>Various</td><td>Represents a file within the repository.</td></tr>
''' </table>
''' <h2>Search Functions</h2>
''' All document types are full-text searchable, and each type defines queryable properties specific to its nature. For example,
''' the Commit type defines queryable properties relevant to individual commits, such as "Author", "Commit Message", and etc. Branches, 
''' Tags, and Files have their own unique set of queryable properties.
''' <p>The base type "RepositoryItem", from which all 4 document types inherit, is also full-text searchable and defines a common set of queryable 
''' properties which are common to all 4 types.</p>
''' <p>Full text searches find exact substrings in the body of the document. There is no support for operators such as AND / OR inside a CONTAINS() predicate. To implement
''' complex full-text searches, use multiple CONTAINS() predicates.</p>
''' </remarks>
<DataContract, DisplayName("Remote Repository")>
Public Class RemoteGitBinding : Inherits CustomBinding

    Public Sub New(Owner As CmisConnection)
        MyBase.New(Owner)
    End Sub

    Public Overrides ReadOnly Property AuthMethod As HttpAuthenticationMethod
        Get
            Return AuthenticationMethod
        End Get
    End Property

#Region "Viewable Properties"

    ''' <summary>The host name or IP address of the Git remote repository server. For example: 'github.com' is the hostname for GitHub.</summary>
    ''' <remarks>If no value is specified, the default remote, typically named 'origin', will be used to reference the repository.</remarks>
    <DataMember, Viewable>
    Public Property RemoteURL As String

    <DataMember(Name:="AuthenticationMethod")>
    Private _AuthenticationMethod As HttpAuthenticationMethod

    ''' <summary>Specifies the method used to authenticate user credentials.</summary>
    ''' <remarks>When using [Basic Authentication], enter the user name.</remarks>
    <Viewable, TypeConverter(GetType(AuthMethodConverter)), Required>
    Public Property AuthenticationMethod As HttpAuthenticationMethod
        Get
            If (_AuthenticationMethod IsNot Nothing) Then _AuthenticationMethod.Parent = Me
            Return _AuthenticationMethod
        End Get
        Set(value As HttpAuthenticationMethod)
            _AuthenticationMethod = value
        End Set
    End Property

    ''' <summary>Specifies a list of mailboxes to be mounted as repositories.</summary>
    ''' <remarks>This list should contain one email address per line.  The login account used must have access to each mailbox listed.
    ''' Refer to Microsoft documentation on <a target='_blank' href='https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/delegate-access-and-ews-in-exchange'>
    ''' configuring delegate access</a> when connecting to mailboxes other than the login user's primary email account.
    ''' </remarks>
    <DataMember, Viewable, TypeConverter(GetType(PgStringCollectionConverter)), UI(GetType(StringListEditor))>
    Public Property BranchList As List(Of String)

    ''' <summary>Enables the use of Git "Commit History" to enhance query functionality.</summary>
    ''' <remarks>When enabled, the commit history provides two primary advantages: 
    ''' <ol>
    '''   <li>Searches execute fast, leveraging the commit logs.</li>
    '''   <li>A total count of matching commits will be returned.</li>
    ''' </ol>
    ''' <p>Grooper will automatically track the Commit History, indexing all changes below the root of the repository. The history will be updated automatically the first time a query is executed, or can be updated manually using the 
    ''' [Git - Rebuild Commit History] command, available on the right-click menu from the Property Grid.</p>
    ''' <p>When a search is performed, the Commit History will be used only when the following conditions are true:
    ''' <ul>
    '''   <li>The content type being queried is "Commit".</li>
    '''   <li>The query applies to the entire repository (i.e. no specific branch or tag has been selected).</li>
    '''   <li>The WHERE clause does not include a [CONTAINS Predicate].</li>
    ''' </ul>
    ''' </p>
    ''' </remarks>
    <DataMember, Viewable>
    Public Property UseCommitHistory As Boolean

    ''' <summary>The connection timeout, in seconds.</summary>
    <DataMember, Viewable, DV(100), ValueRange(1, 1000)>
    Public Property Timeout As Integer

#End Region

#Region "Repository Service"

    Private _RootFolders As Dictionary(Of String, EX.Folder)

    Private Function GetRootFolder(RepId As String) As EX.Folder
        If (_RootFolders Is Nothing) Then _RootFolders = New Dictionary(Of String, Microsoft.Exchange.WebServices.Data.Folder)
        If (_RootFolders.ContainsKey(RepId)) Then Return _RootFolders(RepId)
        Dim Id As New EX.FolderId(EX.WellKnownFolderName.MsgFolderRoot, New EX.Mailbox(RepId))
        Dim RetVal As EX.Folder = EX.Folder.Bind(Service, Id, GetFolderPropertySet())
        _RootFolders.Add(RepId, RetVal)
        Return RetVal
    End Function

    Public Overrides Function GetRepositoryInfo(RepId As String, extension As Extensions.IExtensionsData) As IRepositoryInfo
        Dim RootFolder As EX.Folder = Nothing
        Dim RetVal As RepositoryInfo = Nothing
        RootFolder = GetRootFolder(RepId)
        RetVal = New RepositoryInfo With {.Id = RepId, .Name = RepId, .ProductName = "Microsoft Exchange",
              .ProductVersion = Service.ServerInfo.VersionString, .CmisVersionSupported = "1.1", .VendorName = "Microsoft", .RootFolderId = "/",
              .Description = "Mailbox on " & Service.Url.Host}

        RetVal.ThinClientUri = Service.Url.Scheme & "://" & Service.Url.Host
        RetVal.Capabilities = New RepositoryCapabilities With {.OrderByCapability = CapabilityOrderBy.Custom, .QueryCapability = CapabilityQuery.BothCombined}

        Return RetVal
    End Function

    Public Overrides Function GetRepositoryInfos(extension As Extensions.IExtensionsData) As IList(Of IRepositoryInfo)
        Dim RetVal As New List(Of IRepositoryInfo)

        '    RetVal.Add(GetRepositoryInfo(UserName, Nothing))

        For Each DelegateAccount As String In BranchList.Distinct()
            RetVal.Add(GetRepositoryInfo(DelegateAccount, Nothing))
        Next

        Return RetVal
    End Function

    Public Overrides Function GetTypeDefinition(repositoryId As String, typeId As String, extension As Extensions.IExtensionsData) As ITypeDefinition
        Return GetTypeDef(typeId)
    End Function

    Public Overrides Function GetTypeChildren(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean?, maxItems As Numerics.BigInteger?, skipCount As Numerics.BigInteger?, extension As Extensions.IExtensionsData) As ITypeDefinitionList
        If String.IsNullOrEmpty(typeId) Then Return MyBase.GetTypeChildren(repositoryId, typeId, includePropertyDefinitions, maxItems, skipCount, extension)
        Dim Items As New List(Of ITypeDefinition)(From Item In TypeDefs Where Item.ParentTypeId = typeId)
        Return New TypeDefinitionList With {.List = Items, .HasMoreItems = False, .NumItems = Items.Count}
    End Function

    Public Overrides Function GetTypeDescendants(repositoryId As String, typeId As String, depth As Numerics.BigInteger?, includePropertyDefinitions As Boolean?, extension As Data.Extensions.IExtensionsData) As IList(Of ITypeDefinitionContainer)
        Dim RetVal As New List(Of ITypeDefinitionContainer)
        Dim Types As ITypeDefinitionList = GetTypeChildren(repositoryId, typeId, includePropertyDefinitions, Nothing, Nothing, extension)

        For Each Item In Types.List
            RetVal.Add(New TypeDefinitionContainer With {.TypeDefinition = Item, .Children = GetTypeDescendants(repositoryId, Item.Id, depth, includePropertyDefinitions, extension)})
        Next

        Return RetVal
    End Function

    Private _TypeDefs As ITypeDefinition()

    Private ReadOnly Property TypeDefs As ITypeDefinition()
        Get
            If (_TypeDefs Is Nothing) Then _TypeDefs = CreateTypeDefs().ToArray()
            Return _TypeDefs
        End Get
    End Property

    Private Function GetTypeDef(TypeId As String) As ITypeDefinition
        Return (From Item In TypeDefs Where Item.Id = TypeId).FirstOrDefault
    End Function

    Private Iterator Function CreateTypeDefs() As IEnumerable(Of ITypeDefinition)
        Yield CreateDocumentTypeDefinition("cmis:document", "Item", "", Nothing)
        Yield CreateDocumentTypeDefinition(AppointmentItemId, AppointmentItemId, AppointmentItemClass, "cmis:document")
        Yield CreateDocumentTypeDefinition(ContactItemId, ContactItemId, ContactItemClass, "cmis:document")
        Yield CreateDocumentTypeDefinition(MessageItemId, "Encrypted" & MessageItemId, MessageItemClass, "cmis:document")
        Yield CreateDocumentTypeDefinition(MessageItemId, "Signed" & MessageItemId, MessageSecureItemClass, "cmis:document")
        Yield CreateDocumentTypeDefinition(MessageItemId, MessageItemId, MessageSignedItemClass, "cmis:document")
        Yield CreateDocumentTypeDefinition(TaskItemId, TaskItemId, TaskItemClass, "cmis:document")

        Yield CreateFolderTypeDefinition("cmis:folder", "Folder", "", Nothing)
        Yield CreateFolderTypeDefinition(AppointmentFolderId, AppointmentFolderId, AppointmentFolderClass, "cmis:folder")
        Yield CreateFolderTypeDefinition(ContactFolderId, ContactFolderId, ContactFolderClass, "cmis:folder")
        Yield CreateFolderTypeDefinition(MessageFolderId, MessageFolderId, MessageFolderClass, "cmis:folder")
        Yield CreateFolderTypeDefinition(TaskFolderId, TaskFolderId, TaskFolderClass, "cmis:folder")
    End Function

    Protected Overridable Function CreateFolderTypeDefinition(TypeId As String, Name As String, LocalName As String, ParentTypeId As String) As FolderTypeDefinition
        Dim RetVal As New FolderTypeDefinition With {.Id = TypeId, .QueryName = Name, .LocalName = LocalName, .DisplayName = GetSpacedDisplayName(Name),
              .BaseTypeId = BaseTypeId.CmisFolder, .IsFileable = True, .IsCreatable = (TypeId <> "cmis:folder"), .ParentTypeId = ParentTypeId}

        For Each PropDef As PropertyDefinition In FolderPropDefs
            ConfigurePropDef(PropDef, GetType(EX.Folder))
            RetVal.AddPropertyDefinition(PropDef)
        Next

        Dim mi As XmlDocumentation.MemberInfo = XmlDocumentation.GetClassInfo(GetType(EX.Folder))
        RetVal.Description = If(mi.Summary Is Nothing, "", mi.Summary.Trim())

        Return RetVal
    End Function

    Protected Overridable Function CreateDocumentTypeDefinition(TypeId As String, Name As String, LocalName As String, ParentTypeId As String) As DocumentTypeDefinition
        Dim RetVal As New DocumentTypeDefinition With {.Id = TypeId, .QueryName = Name, .LocalName = LocalName, .DisplayName = GetSpacedDisplayName(Name),
              .ContentStreamAllowed = ContentStreamAllowed.Allowed, .BaseTypeId = BaseTypeId.CmisDocument, .ParentTypeId = ParentTypeId,
              .IsQueryable = True, .IsFulltextIndexed = True, .IsCreatable = (TypeId <> "cmis:document"), .IsFileable = True}

        Dim PropDefs As IEnumerable(Of IPropertyDefinition) = Nothing
        Select Case TypeId
            Case AppointmentItemId : PropDefs = AppointmentPropDefs
            Case ContactItemId : PropDefs = ContactPropDefs
            Case MessageItemId : PropDefs = MessagePropDefs
            Case TaskItemId : PropDefs = TaskPropDefs
            Case Else : PropDefs = ItemPropDefs
        End Select

        Dim EwsType As Type = GetEwsType(RetVal)

        For Each PropDef As PropertyDefinition In PropDefs
            ConfigurePropDef(PropDef, EwsType)
            RetVal.AddPropertyDefinition(PropDef)
        Next

        Dim mi As XmlDocumentation.MemberInfo = XmlDocumentation.GetClassInfo(EwsType)
        RetVal.Description = If(mi.Summary Is Nothing, "", mi.Summary.Trim())

        Return RetVal
    End Function

#End Region

#Region "Repository Service - Property Definitions"

    Private ReadOnly Iterator Property FolderPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedCmisPD(EX.FolderSchema.DisplayName, Nothing, PropertyIds.Name)
            Yield CreateLinkedPD(EX.FolderSchema.TotalCount, Nothing)
            Yield CreatePD(PropertyIds.Description, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.CreatedBy, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.CreationDate, PropertyType.DateTime, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.LastModifiedBy, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.LastModificationDate, PropertyType.DateTime, Updatability.ReadOnly, PdOptions.None)

            Yield CreatePD(PropertyIds.ParentId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.Path, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.AllowedChildObjectTypeIds, PropertyType.Id, Updatability.ReadOnly, PdOptions.Multi)

            Yield CreatePD(PropertyIds.ObjectId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.BaseTypeId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.ObjectTypeId, PropertyType.Id, Updatability.OnCreate, PdOptions.None)

        End Get
    End Property

    Private ReadOnly Iterator Property AppointmentPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedCmisPD(EX.ItemSchema.Subject, "subject", PropertyIds.Name)
            Yield CreateLinkedPD(EX.AppointmentSchema.Organizer, "organizer")
            Yield CreateLinkedPD(EX.AppointmentSchema.Location, "location", PdOptions.None)
            Yield CreateLinkedPD(EX.AppointmentSchema.Start, Nothing)
            Yield CreateLinkedPD(EX.AppointmentSchema.End, Nothing)
            Yield CreateLinkedPD(EX.AppointmentSchema.AppointmentType, Nothing, PdOptions.None)
            Yield CreateLinkedPD(EX.AppointmentSchema.IsAllDayEvent, Nothing)
            Yield CreateLinkedPD(EX.AppointmentSchema.IsCancelled, Nothing, PdOptions.None)
            Yield CreateLinkedPD(EX.AppointmentSchema.IsOnlineMeeting, Nothing, PdOptions.None)
            Yield CreateLinkedPD(EX.AppointmentSchema.IsMeeting, Nothing, PdOptions.None)
            Yield CreateLinkedPD(EX.AppointmentSchema.IsRecurring, "isrecurring", PdOptions.None)

            For Each pd As IPropertyDefinition In CommonItemPropDefs
                Yield pd
            Next
        End Get
    End Property

    Private ReadOnly Iterator Property ContactPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedPD(EX.ContactSchema.DisplayName, "subject")
            Yield CreateLinkedPD(EX.ContactSchema.CompanyName, "company", PdOptions.None)
            Yield CreateLinkedPD(EX.ContactSchema.Department, Nothing)
            Yield CreateLinkedPD(EX.ContactSchema.JobTitle, "jobtitle", PdOptions.None)
            Yield CreateIndexedPD(EX.ContactSchema.EmailAddresses, EX.ContactSchema.EmailAddress1, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.EmailAddresses, EX.ContactSchema.EmailAddress2, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.EmailAddresses, EX.ContactSchema.EmailAddress3, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.PhoneNumbers, EX.ContactSchema.MobilePhone, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.PhoneNumbers, EX.ContactSchema.BusinessPhone, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.PhoneNumbers, EX.ContactSchema.BusinessFax, Nothing)
            Yield CreateIndexedPD(EX.ContactSchema.PhoneNumbers, EX.ContactSchema.HomePhone, Nothing)
            Yield CreateLinkedCmisPD(EX.ItemSchema.Subject, "subject", PropertyIds.Name)

            For Each pd As IPropertyDefinition In CommonItemPropDefs
                Yield pd
            Next
        End Get
    End Property

    Private ReadOnly Iterator Property MessagePropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedCmisPD(EX.ItemSchema.Subject, "subject", PropertyIds.Name)
            Yield CreateLinkedPD(EX.EmailMessageSchema.Sender, "from")
            Yield CreateLinkedPD(EX.EmailMessageSchema.ToRecipients, "to", PdOptions.Queryable)
            Yield CreateLinkedPD(EX.EmailMessageSchema.CcRecipients, "cc", PdOptions.Queryable)
            Yield CreateLinkedPD(EX.EmailMessageSchema.BccRecipients, "bcc", PdOptions.Queryable)
            Yield CreateLinkedPD(EX.EmailMessageSchema.DateTimeReceived, "received")
            Yield CreateLinkedPD(EX.EmailMessageSchema.DateTimeSent, "sent")
            Yield CreateLinkedPD(EX.EmailMessageSchema.IsRead, "isread", PdOptions.Queryable)
            Yield CreateLinkedPD(EX.EmailMessageSchema.IsDraft, Nothing, PdOptions.None)

            For Each pd As IPropertyDefinition In CommonItemPropDefs
                Yield pd
            Next
        End Get
    End Property

    Private ReadOnly Iterator Property TaskPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedCmisPD(EX.ItemSchema.Subject, "subject", PropertyIds.Name)
            Yield CreateLinkedPD(EX.TaskSchema.Owner, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.AssignedTime, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.StartDate, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.DueDate, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.CompleteDate, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.Status, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.Delegator, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.IsComplete, Nothing)
            Yield CreateLinkedPD(EX.TaskSchema.Mileage, Nothing)

            For Each pd As IPropertyDefinition In CommonItemPropDefs
                Yield pd
            Next
        End Get
    End Property

    Private ReadOnly Iterator Property ItemPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreateLinkedCmisPD(EX.ItemSchema.Subject, "subject", PropertyIds.Name)
            For Each pd As IPropertyDefinition In CommonItemPropDefs
                Yield pd
            Next
        End Get
    End Property

    Private Const MailboxPropId As String = "Mailbox"

    ''' <summary>Generates property definitions common to all item types.</summary>
    Private ReadOnly Iterator Property CommonItemPropDefs As IEnumerable(Of PropertyDefinition)
        Get
            Yield CreatePD(PropertyIds.CreatedBy, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreateLinkedCmisPD(EX.ItemSchema.DateTimeCreated, Nothing, PropertyIds.CreationDate)
            Yield CreateLinkedCmisPD(EX.ItemSchema.LastModifiedName, Nothing, PropertyIds.LastModifiedBy)
            Yield CreateLinkedCmisPD(EX.ItemSchema.LastModifiedTime, Nothing, PropertyIds.LastModificationDate)
            Yield CreateLinkedCmisPD(EX.ItemSchema.Size, "size", PropertyIds.ContentStreamLength)
            Yield CreateLinkedPD(EX.ItemSchema.HasAttachments, "hasattachments")

            Yield CreatePropDef(MailboxPropId, MailboxPropId, "", PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreateLinkedPD(EX.ItemSchema.Culture, Nothing, PdOptions.None)
            Yield CreateLinkedPD(EX.ItemSchema.Sensitivity, Nothing)
            Yield CreateLinkedPD(EX.ItemSchema.Importance, "importance", PdOptions.None)
            Yield CreatePD(PropertyIds.ContentStreamMimeType, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.ContentStreamFileName, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.ContentStreamId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.Description, PropertyType.String, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.ObjectId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.BaseTypeId, PropertyType.Id, Updatability.ReadOnly, PdOptions.None)
            Yield CreatePD(PropertyIds.ObjectTypeId, PropertyType.Id, Updatability.OnCreate, PdOptions.None)
        End Get
    End Property

    Protected Function CreateLinkedPD(ExPd As EX.PropertyDefinition, AqsName As String) As PropertyDefinition
        Dim Options As PdOptions = PdOptions.Orderable Or If(AqsName Is Nothing, PdOptions.None, PdOptions.Queryable)
        Dim RetVal As PropertyDefinition = CreatePropDef(ExPd.Name, GetSpacedDisplayName(ExPd.Name), ExPd.Name, GetCmisType(ExPd.Type), Updatability.ReadOnly, Options)
        RetVal.LocalNamespace = AqsName
        Return RetVal
    End Function

    Protected Function CreateLinkedPD(ExPd As EX.PropertyDefinition, AqsName As String, Options As PdOptions) As PropertyDefinition
        Dim RetVal As PropertyDefinition = CreatePropDef(ExPd.Name, GetSpacedDisplayName(ExPd.Name), ExPd.Name, GetCmisType(ExPd.Type), Updatability.ReadOnly, Options)
        RetVal.LocalNamespace = AqsName
        Return RetVal
    End Function

    Protected Function CreateLinkedCmisPD(ExPd As EX.PropertyDefinition, AqsName As String, CmisId As String) As PropertyDefinition
        Dim Options As PdOptions = PdOptions.Orderable Or If(AqsName Is Nothing, PdOptions.None, PdOptions.Queryable)
        Dim RetVal As PropertyDefinition = CreatePropDef(CmisId, GetStandardDisplayName(ExPd.Name), ExPd.Name, ExPd.Name, GetCmisType(ExPd.Type), Updatability.ReadOnly, Options)
        RetVal.LocalNamespace = AqsName
        Return RetVal
    End Function

    Protected Function CreateIndexedPD(ExPd As EX.PropertyDefinition, IxPd As EX.IndexedPropertyDefinition, AqsName As String) As PropertyDefinition
        Dim Options As PdOptions = If(AqsName Is Nothing, PdOptions.None, PdOptions.Queryable)
        Dim RetVal As PropertyDefinition = CreatePropDef(IxPd.Index, GetSpacedDisplayName(IxPd.Index), ExPd.Name & "." & IxPd.Index, GetCmisType(ExPd.Type), Updatability.ReadOnly, Options)
        RetVal.LocalNamespace = AqsName
        Return RetVal
    End Function

    Protected Function CreatePD(Id As String, Type As Enums.PropertyType, Updatability As Enums.Updatability, Options As PdOptions) As PropertyDefinition
        Return CreatePropDef(Id, GetStandardDisplayName(Id), "", Type, Updatability, Options)
    End Function

    Private Sub ConfigurePropDef(pd As PropertyDefinition, EwsType As Type)
        If (String.IsNullOrWhiteSpace(pd.LocalName)) Then Return
        If (pd.LocalName.Contains(".")) Then pd.Updatability = Updatability.ReadOnly : Return

        Dim pi As Reflection.PropertyInfo = EwsType.GetProperty(pd.LocalName)

        If PropIsWritable(EwsType, pd, pi) Then pd.Updatability = Updatability.ReadWrite

        If (pi.PropertyType.IsEnum) And (pd.LocalName <> "EffectiveRights") Then
            Dim spd As PropertyStringDefinition = pd
            Dim Choices As New List(Of IChoice(Of String))

            For Each Value As Object In System.Enum.GetValues(pi.PropertyType)
                Dim Choice As New Choice(Of String) With {.DisplayName = Value.ToString(), .Value = New List(Of String)({Value.ToString()})}
                Choices.Add(Choice)
            Next
            spd.Choices = Choices
        End If

        Dim mi As XmlDocumentation.MemberInfo = XmlDocumentation.GetPropertyInfo(pi)
        pd.Description = If(mi.Summary Is Nothing, "", mi.Summary.Trim())
    End Sub

    Private Function PropIsWritable(EwsType As Type, pd As PropertyDefinition, pi As Reflection.PropertyInfo) As Boolean
        If (pi.PropertyType Is GetType(EX.EmailAddress)) Then Return False
        If (pd.Id = "Subject") Then Return False
        If (pi.DeclaringType IsNot GetType(EX.Item)) And (pi.DeclaringType IsNot GetType(EX.EmailMessage)) Then Return False
        Return pi.CanWrite
    End Function

#End Region

#Region "Object Service"

    Public Overrides Function CreateDocument(RepId As String, Props As IProperties, FolderId As String, ContentStream As IContentStream, versioningState As VersioningState?, policies As IList(Of String), addAces As IAcl, removeAces As IAcl, extension As Extensions.IExtensionsData) As String
        Dim ParentFolder As EX.Folder = LoadFolderObject(RepId, FolderId, EX.PropertySet.IdOnly)

        Dim ObjectTypeId As String = Props.GetPropertyValue(PropertyIds.ObjectTypeId)
        Dim TypeDef As ITypeDefinition = (From td In TypeDefs Where td.Id = ObjectTypeId).First

        Dim EwsType As Type = GetEwsType(TypeDef)

        Dim Item As EX.Item = EwsType.CreateInstance(Service)

        Dim cs As CachedStream = ContentStream.ReadLocal()
        Dim ba() As Byte = cs.ToArray()

        ContentStream.Stream.Dispose() : cs.Dispose()

        Item.MimeContent = New EX.MimeContent("utf-8", ba)
        Item.ItemClass = TypeDef.LocalName

        SetDocumentProperties(Item, Props)

        Item.Save(ParentFolder.Id)

        Return Item.Id.UniqueId
    End Function

    Public Overrides Function CreateFolder(RepId As String, Props As IProperties, FolderId As String, policies As IList(Of String), addAces As IAcl, removeAces As IAcl, extension As Extensions.IExtensionsData) As String
        Dim ParentFolder As EX.Folder = LoadFolderObject(RepId, FolderId, GetFolderPropertySet())

        Dim ObjectTypeId As String = Props.GetPropertyValue(PropertyIds.ObjectTypeId)
        Dim TypeDef As ITypeDefinition = (From td In TypeDefs Where td.Id = ObjectTypeId).First

        Dim EwsType As Type = GetEwsType(TypeDef)
        Dim Folder As EX.Folder = EwsType.CreateInstance(Service)
        Folder.FolderClass = TypeDef.LocalName
        Folder.DisplayName = Props.GetPropertyValue(PropertyIds.Name)

        If (String.IsNullOrWhiteSpace(Folder.DisplayName)) Then Throw New Exception("A name is required for a folder object.")

        Folder.Save(ParentFolder.Id)

        Folder.Load(GetFolderPropertySet())

        Return GetFolderPath(RepId, Folder)
    End Function

    Public Overrides Function GetContentStream(repositoryId As String, objectId As String, streamId As String, offset As Numerics.BigInteger?, length As Numerics.BigInteger?, extension As Extensions.IExtensionsData) As IContentStream
        Dim Item As EX.Item = EX.Item.Bind(Service, objectId, ContentStreamPropertySet)
        Dim cs As New CachedStream(Item.MimeContent.Content) With {.Filename = GetName(Item) & GetExtension(Item), .MimeType = GetMimeType(Item)}
        Return New Data.ContentStream With {.Stream = cs, .FileName = cs.Filename, .MimeType = cs.MimeType, .Length = cs.Length}
    End Function

    Public Overrides Function GetObject(repositoryId As String, objectId As String, filter As String, includeAllowableActions As Boolean?, includeRelationships As IncludeRelationships?, renditionFilter As String, includePolicyIds As Boolean?, includeAcl As Boolean?, extension As Extensions.IExtensionsData) As IObjectData
        If objectId.StartsWith("/") Then
            Return GetFolderObject(repositoryId, objectId, filter, includeAllowableActions.GetValueOrDefault())
        Else
            Return GetDocumentObject(repositoryId, objectId, filter, includeAllowableActions.GetValueOrDefault())
        End If
    End Function

    Private Function GetFolderObject(RepId As String, ObjectId As String, FilterString As String, IncludeActions As Boolean) As IObjectData
        Dim Filter As New PropertyFilter(FilterString, DefaultFolderPropertyIds)
        Dim Folder As EX.Folder = LoadFolderObject(RepId, ObjectId, GetFolderPropertySet())
        Dim RetVal As New ObjectData With {.Properties = New Properties(GetFolderProperties(RepId, Folder, Filter))}
        If (IncludeActions) Then RetVal.AllowableActions = GetActions(Folder)
        Return RetVal
    End Function

    Private Function GetDocumentObject(RepId As String, ObjectId As String, FilterString As String, IncludeActions As Boolean) As IObjectData
        Dim Filter As New PropertyFilter(FilterString, DefaultDocumentPropertyIds)

        Dim PropSet As EX.PropertySet = GetItemPropertySet(GetTypeDef("cmis:document"), Filter, IncludeActions)
        Dim Item As EX.Item = EX.Item.Bind(Service, ObjectId, PropSet)

        Dim RetVal As New ObjectData With {.Properties = New Properties(GetDocumentProperties(RepId, Item, Filter))}
        If (IncludeActions) Then RetVal.AllowableActions = GetActions(Item)
        Return RetVal
    End Function

    Public Overrides Sub DeleteObject(RepId As String, ObjectId As String, AllVersions As Boolean?, extension As Extensions.IExtensionsData)
        If ObjectId.StartsWith("/") Then
            Dim Folder As EX.Folder = LoadFolderObject(RepId, ObjectId, EX.PropertySet.IdOnly)
            Folder.Delete(EX.DeleteMode.HardDelete)
        Else
            Dim Item As EX.Item = EX.Item.Bind(Service, ObjectId, EX.PropertySet.IdOnly)
            Item.Delete(EX.DeleteMode.HardDelete)
        End If
    End Sub

    Public Overrides Function DeleteTree(RepId As String, FolderId As String, allVersions As Boolean?, unfileObjects As UnfileObject?, continueOnFailure As Boolean?, extension As Extensions.IExtensionsData) As IFailedToDeleteData
        Dim Folder As EX.Folder = LoadFolderObject(RepId, FolderId, EX.PropertySet.IdOnly)
        Folder.Delete(EX.DeleteMode.HardDelete)
        Return Nothing
    End Function

    Public Overrides Sub MoveObject(RepId As String, ByRef ObjectId As String, TargetFolderId As String, SourceFolderId As String, extension As Extensions.IExtensionsData)
        Dim PropSet As New EX.PropertySet({EX.ItemSchema.ParentFolderId, EX.ItemSchema.Id})
        Dim TargetFolder As EX.Folder = LoadFolderObject(RepId, TargetFolderId, EX.PropertySet.IdOnly)

        If ObjectId.StartsWith("/") Then
            Dim Folder As EX.Folder = LoadFolderObject(RepId, ObjectId, PropSet)
            If Folder.ParentFolderId.Equals(TargetFolder.Id) Then Throw New Exception("Cannot move item to the current parent.")
            Folder.Move(TargetFolder.Id)
        Else
            Dim Item As EX.Item = EX.Item.Bind(Service, ObjectId, PropSet)
            If Item.ParentFolderId.Equals(TargetFolder.Id) Then Throw New Exception("Cannot move item to the current parent.")
            Dim NewItem As EX.Item = Item.Move(TargetFolder.Id)
            ObjectId = NewItem.Id.ToString()
        End If
    End Sub

#End Region

#Region "Object Service - GET Properties"

    Public Overrides Function GetProperties(RepId As String, ObjectId As String, filter As String, extension As Extensions.IExtensionsData) As IProperties
        If ObjectId.StartsWith("/") Then
            Return New Properties(GetFolderProperties(RepId, ObjectId, filter))
        Else
            Return New Properties(GetDocumentProperties(RepId, ObjectId, filter))
        End If
    End Function

    Private Function GetFolderProperties(RepId As String, ObjectId As String, FilterString As String) As IEnumerable(Of IPropertyData)
        Dim Filter As New PropertyFilter(FilterString, DefaultFolderPropertyIds)

        Dim Folder As EX.Folder = LoadFolderObject(RepId, ObjectId, GetFolderPropertySet())

        Return GetFolderProperties(RepId, Folder, Filter)
    End Function

    Private Iterator Function GetFolderProperties(RepId As String, Folder As EX.Folder, Filter As PropertyFilter) As IEnumerable(Of IPropertyData)
        Dim ItemType As ITypeDefinition = GetFolderType(Folder)
        Dim Path As String = GetFolderPath(RepId, Folder)

        '    If Filter.Includes(PropertyIds.Name) Then Yield CreateProperty(PropertyIds.Name, PropertyType.String, Folder.DisplayName)

        For Each pd As IPropertyData In GetCommonProperties(RepId, Path, "cmis:folder", ItemType.Id, "", Filter, True)
            Yield pd
        Next

        For Each pd As IPropertyDefinition In ItemType.PropertyDefinitions
            If (String.IsNullOrWhiteSpace(pd.LocalName)) Or (Filter.Includes(pd.Id) = False) Then Continue For
            Dim Ref As New PropRef(pd.LocalName)

            Dim ExPd As EX.PropertyDefinition = (From ExPropDef As EX.PropertyDefinition In Folder.Schema Where ExPropDef.Name = Ref.EwsId).FirstOrDefault
            Dim Value As Object = Folder(ExPd)
            Yield CreateProperty(pd.Id, pd.PropertyType, ConvertValue(Value, pd, Ref.Index))
        Next

        If Filter.Includes(PropertyIds.AllowedChildObjectTypeIds) Then Yield CreateProperty(PropertyIds.AllowedChildObjectTypeIds, PropertyType.Id, GetAllowedChildTypes(ItemType.Id))
        If Filter.Includes(PropertyIds.ParentId) Then Yield CreateProperty(PropertyIds.ParentId, PropertyType.Id, GetParentId(RepId, Folder))
        If Filter.Includes(PropertyIds.Path) Then Yield CreateProperty(PropertyIds.Path, PropertyType.String, Path)

    End Function

    Private Function GetDocumentProperties(RepId As String, ObjectId As String, FilterString As String) As IEnumerable(Of IPropertyData)
        Dim Filter As New PropertyFilter(FilterString, DefaultDocumentPropertyIds)
        Dim PropSet As EX.PropertySet = GetItemPropertySet(GetTypeDef("cmis:document"), Filter, False)

        Dim Item As EX.Item = EX.Item.Bind(Service, ObjectId, PropSet)
        Return GetDocumentProperties(RepId, Item, Filter)
    End Function

    Private Iterator Function GetDocumentProperties(RepId As String, Item As EX.Item, Filter As PropertyFilter) As IEnumerable(Of IPropertyData)
        Dim TypeId As String = ExToLocal(Item.ItemClass)
        Dim ItemType As ITypeDefinition = (From td In TypeDefs Where td.Id = TypeId).FirstOrDefault

        For Each pd As IPropertyData In GetCommonProperties(RepId, Item.Id.UniqueId, "cmis:document", ItemType.Id, "", Filter, False)
            Yield pd
        Next

        If Filter.Includes(PropertyIds.CreatedBy) Then Yield CreateProperty(PropertyIds.CreatedBy, PropertyType.String, RepId)
        If Filter.Includes(MailboxPropId) Then Yield CreateProperty(MailboxPropId, PropertyType.String, RepId)

        Dim PropDefs As New List(Of Object)(Item.Schema)

        For Each pd As IPropertyDefinition In ItemType.PropertyDefinitions
            If (String.IsNullOrWhiteSpace(pd.LocalName)) Or (Filter.Includes(pd.Id) = False) Then Continue For

            Dim Ref As New PropRef(pd.LocalName)

            Dim ExPd As EX.PropertyDefinition = (From ExPropDef In Item.Schema Where ExPropDef.Name = Ref.EwsId).FirstOrDefault

            Dim Value As Object = Nothing
            Item.TryGetProperty(ExPd, Value)

            Yield CreateProperty(pd.Id, pd.PropertyType, ConvertValue(Value, pd, Ref.Index))
        Next

        If Filter.Includes(PropertyIds.ContentStreamMimeType) Then Yield CreateProperty(PropertyIds.ContentStreamMimeType, PropertyType.String, GetMimeType(Item))
        If Filter.Includes(PropertyIds.ContentStreamFileName) Then Yield CreateProperty(PropertyIds.ContentStreamFileName, PropertyType.String, GetName(Item) & GetExtension(Item))

    End Function

    Private Iterator Function GetCommonProperties(RepId As String, ObjectId As String, BaseTypeId As String, ObjectTypeId As String, Description As String, Filter As PropertyFilter, IncludeBlanks As Boolean) As IEnumerable(Of IPropertyData)
        If Filter.Includes(PropertyIds.Description) Then Yield CreateProperty(PropertyIds.Description, PropertyType.String, Description)
        If Filter.Includes(PropertyIds.ObjectId) Then Yield CreateProperty(PropertyIds.ObjectId, PropertyType.Id, ObjectId)
        If Filter.Includes(PropertyIds.BaseTypeId) Then Yield CreateProperty(PropertyIds.BaseTypeId, PropertyType.Id, BaseTypeId)
        If Filter.Includes(PropertyIds.ObjectTypeId) Then Yield CreateProperty(PropertyIds.ObjectTypeId, PropertyType.Id, ObjectTypeId)
        If (IncludeBlanks = False) Then Return
        If Filter.Includes(PropertyIds.CreatedBy) Then Yield CreateProperty(PropertyIds.CreatedBy, PropertyType.String, RepId)
        If Filter.Includes(PropertyIds.CreationDate) Then Yield CreateProperty(PropertyIds.CreationDate, PropertyType.DateTime, Nothing)
        If Filter.Includes(PropertyIds.LastModifiedBy) Then Yield CreateProperty(PropertyIds.LastModifiedBy, PropertyType.String, RepId)
        If Filter.Includes(PropertyIds.LastModificationDate) Then Yield CreateProperty(PropertyIds.LastModificationDate, PropertyType.DateTime, Nothing)
    End Function

    Private Function CleanName(Name As String) As String
        Return If(String.IsNullOrWhiteSpace(Name), "(No Name)", StringManager.CleanFilename(Name.Replace("/", "-")))
    End Function


    Private Function ConvertValue(Value As Object, PropDef As IPropertyDefinition, Index As String) As Object
        If (PropDef.Id = PropertyIds.Name) Then Return CleanName(Value)

        If (Value Is Nothing) Then Return Nothing

        If (PropDef.PropertyType = PropertyType.Integer) Then Return New Numerics.BigInteger(CLng(Value))

        Dim ValType As Type = Value.GetType()

        If (ValType.IsEnum) Then Return Value.ToString()


        Select Case ValType
            Case GetType(Integer) : Return New Numerics.BigInteger(CInt(Value))
            Case GetType(EX.EmailAddress) : Return EmailAddressToString(Value)
            Case GetType(EX.EmailAddressCollection) : Return CombineAddresses(Value)
            Case GetType(EX.ConversationId) : Return DirectCast(Value, EX.ConversationId).UniqueId
            Case GetType(EX.EmailAddressDictionary)
                Dim Dic As EX.EmailAddressDictionary = Value
                Dim Key As EX.EmailAddressKey = [Enum].Parse(GetType(EX.EmailAddressKey), Index)
                Return If(Dic.Contains(Key), Dic(Key).ToString, "")
            Case GetType(EX.PhoneNumberDictionary)
                Dim Dic As EX.PhoneNumberDictionary = Value
                Dim Key As EX.PhoneNumberKey = [Enum].Parse(GetType(EX.PhoneNumberKey), Index)
                Return If(Dic.Contains(Key), Dic(Key).ToString, "")
            Case GetType(EX.ImAddressDictionary)
                Dim Dic As EX.ImAddressDictionary = Value
                Dim Key As EX.ImAddressKey = [Enum].Parse(GetType(EX.ImAddressKey), Index)
                Return If(Dic.Contains(Key), Dic(Key).ToString, "")
            Case GetType(EX.PhysicalAddressDictionary)
                Dim Dic As EX.PhysicalAddressDictionary = Value
                Dim Key As EX.PhysicalAddressKey = [Enum].Parse(GetType(EX.PhysicalAddressKey), Index)
                Return If(Dic.Contains(Key), Dic(Key).ToString, "")
            Case GetType(Nullable(Of DateTime))
                Dim Val As Nullable(Of DateTime) = Value
                Return If(Val.HasValue, Val.Value, DateTime.MinValue)
            Case Else : Return Value
        End Select
    End Function

    Private Function EmailAddressToString(Item As EX.EmailAddress) As String
        If (String.IsNullOrWhiteSpace(Item.Name)) Then Return Item.Address
        Return String.Format("{0} <{1}>", Item.Name, Item.Address)
    End Function

    Private Function CombineAddresses(Items As EX.EmailAddressCollection) As String
        Return String.Join(", ", From Item In Items Select EmailAddressToString(Item))
    End Function

    Private Function GetAllowedChildTypes(TypeId As String) As String()
        Select Case TypeId
            Case AppointmentFolderId : Return {AppointmentFolderId, AppointmentItemId}
            Case ContactFolderId : Return {ContactFolderId, ContactItemId}
            Case MessageFolderId : Return {MessageFolderId, MessageItemId}
            Case TaskFolderId : Return {TaskFolderId, TaskItemId}
            Case "cmis:folder" : Return {AppointmentFolderId, ContactFolderId, MessageFolderId, TaskFolderId}
            Case Else : Return {}
        End Select
    End Function

    Private BOM As String = System.Text.Encoding.Unicode.GetString(New Byte() {&HFE, &HFF})

    Private Function GetFolderPath(RepId As String, Folder As EX.Folder) As String
        Dim RootFolder As EX.Folder = GetRootFolder(RepId)
        If (Folder.Id.UniqueId = RootFolder.Id.UniqueId) Then Return "/"

        Dim RawPath As String = (From Item In Folder.ExtendedProperties Where Item.PropertyDefinition = PR_FOLDER_PATH).First.Value

        Dim RetVal As String = ""
        For Each ch As Char In RawPath
            Select Case ch
                Case BOM : RetVal &= "/"
                Case "\" : RetVal &= "/"
                Case Else : RetVal &= ch
            End Select
        Next
        If (RetVal.StartsWith("/") = False) Then RetVal = "/" & RetVal
        Return RetVal
    End Function

    Private Function GetFolderType(Folder As EX.Folder) As ITypeDefinition
        If (Folder.FolderClass Is Nothing) Then Return (From TypeDef In TypeDefs Where TypeDef.Id = "cmis:folder").First
        Return (From TypeDef In TypeDefs Where TypeDef.LocalName = Folder.FolderClass).FirstOrDefault
    End Function

    Private Function GetParentId(RepId As String, Folder As EX.Folder) As String
        Dim Path As String = GetFolderPath(RepId, Folder)
        Return GetParentId(Path)
    End Function

    Private Function GetParentId(Path As String) As String
        If (Path = "/") Then Return ""
        Dim Pos As Integer = Path.LastIndexOf("/"c)
        If (Pos = 0) Then Return "/"
        Return Path.Substring(0, Pos)
    End Function

    Private Function GetFolderName(Path As String) As String
        Dim Pos As Integer = Path.LastIndexOf("/"c)
        Return Path.Substring(Pos + 1)
    End Function

#End Region

#Region "Object Service - SET Properties"

    Public Overrides Sub UpdateProperties(RepId As String, ByRef ObjectId As String, ByRef ChangeToken As String, Props As IProperties, extension As Extensions.IExtensionsData)
        If (ObjectId.StartsWith("/")) Then UpdateFolderProperties(RepId, ObjectId, Props) Else UpdateDocumentProperties(RepId, ObjectId, Props)
    End Sub

    Private Sub UpdateFolderProperties(RepId As String, ByRef ObjectId As String, Props As IProperties)
        Dim Folder As EX.Folder = LoadFolderObject(RepId, ObjectId, EX.PropertySet.IdOnly)

        Dim NewName As String = Props.PropertyList.First.FirstValue
        Folder.DisplayName = NewName
        Folder.Update()

        Dim Pos As Integer = ObjectId.LastIndexOf("/")

        Dim NewId As String = ObjectId.Substring(0, Pos + 1) & NewName
        ObjectId = NewId
    End Sub


    Private Sub UpdateDocumentProperties(RepId As String, ByRef ObjectId As String, Props As IProperties)
        Dim PropSet As New EX.PropertySet(EX.BasePropertySet.IdOnly, EX.ItemSchema.ItemClass)
        Dim Item As EX.Item = EX.Item.Bind(Service, ObjectId, PropSet)

        SetDocumentProperties(Item, Props)

        Item.Update(EX.ConflictResolutionMode.AutoResolve)

    End Sub

    Private Sub SetDocumentProperties(Item As EX.Item, Props As IProperties)
        Dim TypeId As String = ExToLocal(Item.ItemClass)
        Dim ItemType As ITypeDefinition = (From td In TypeDefs Where td.Id = TypeId).FirstOrDefault

        For Each Prop As IPropertyData In Props.PropertyList
            Dim PropDef As IPropertyDefinition = (From pd In ItemType.PropertyDefinitions Where pd.Id = Prop.Id).First
            If (PropDef Is Nothing) Then Throw New Exception($"Invalid type name '{ItemType.DisplayName}'")
            If (String.IsNullOrWhiteSpace(PropDef.LocalName)) Then Continue For
            If (PropDef.Updatability = Updatability.ReadOnly) Then Throw New Exception($"Property '{PropDef.DisplayName}' of type '{ItemType.DisplayName}' is read only.")
            Dim pi As Reflection.PropertyInfo = Item.GetType().GetProperty(PropDef.LocalName)
            SetEwsPropertyValue(Item, pi, Prop.FirstValue)
        Next
    End Sub



    Private Sub SetEwsPropertyValue(Item As EX.Item, pi As Reflection.PropertyInfo, Value As Object)
        If (pi.PropertyType.IsEnum) Then
            Dim EnumValue As Object = System.Enum.Parse(pi.PropertyType, Value)
            pi.SetValue(Item, EnumValue)
            Return
        End If

        pi.SetValue(Item, Value)
    End Sub


#End Region

#Region "Object Service - Allowable Actions"

    Public Overrides Function GetAllowableActions(repositoryId As String, objectId As String, extension As Extensions.IExtensionsData) As IAllowableActions
        If (objectId = "/") Then Return New AllowableActions With {.Actions = New HashSet(Of Action)}

        If objectId.StartsWith("/") Then
            Dim PropSet As New EX.PropertySet(EX.BasePropertySet.IdOnly, EX.FolderSchema.EffectiveRights)
            Dim Folder As EX.Folder = LoadFolderObject(repositoryId, objectId, PropSet)
            Return GetActions(Folder)
        Else
            Dim PropSet As New EX.PropertySet(EX.BasePropertySet.IdOnly, EX.ItemSchema.EffectiveRights)
            Dim Item As EX.Item = EX.Item.Bind(Service, objectId, PropSet)
            Return GetActions(Item)
        End If

        Return New AllowableActions With {.Actions = New HashSet(Of Action)}
    End Function

    Private Function GetActions(Folder As EX.Folder) As IAllowableActions
        Dim Actions As New HashSet(Of Action)

        If (Folder.EffectiveRights.HasFlag(EX.EffectiveRights.Delete)) Then Actions.Add(Action.CanDeleteObject) : Actions.Add(Action.CanDeleteTree)
        If (Folder.EffectiveRights.HasFlag(EX.EffectiveRights.Modify)) Then Actions.Add(Action.CanUpdateProperties) : Actions.Add(Action.CanMoveObject)
        If (Folder.EffectiveRights.HasFlag(EX.EffectiveRights.CreateContents)) Then Actions.Add(Action.CanCreateDocument)
        If (Folder.EffectiveRights.HasFlag(EX.EffectiveRights.CreateHierarchy)) Then Actions.Add(Action.CanCreateFolder)

        Return New AllowableActions With {.Actions = Actions}
    End Function

    Private Function GetActions(Item As EX.Item) As IAllowableActions
        Dim Actions As New HashSet(Of Action)

        If (Item.EffectiveRights.HasFlag(EX.EffectiveRights.Delete)) Then Actions.Add(Action.CanDeleteObject)
        If (Item.EffectiveRights.HasFlag(EX.EffectiveRights.Modify)) Then Actions.Add(Action.CanUpdateProperties) : Actions.Add(Action.CanMoveObject)

        Return New AllowableActions With {.Actions = Actions}
    End Function


#End Region

#Region "Navigation Service"

    Public Overrides Function GetChildren(repositoryId As String, folderId As String, filter As String, orderBy As String, includeAllowableActions As Boolean?, includeRelationships As IncludeRelationships?, renditionFilter As String, includePathSegment As Boolean?, maxItems As Numerics.BigInteger?, skipCount As Numerics.BigInteger?, extension As Extensions.IExtensionsData) As IObjectInFolderList
        Dim MaxResults As Long = maxItems.GetValueOrDefault(), ItemsToSkip As Long = skipCount.GetValueOrDefault()
        Dim Folder As EX.Folder = LoadFolderObject(repositoryId, folderId, GetFolderPropertySet())
        Return GetFolderChildren(repositoryId, Folder, filter, includeAllowableActions, includePathSegment, MaxResults, ItemsToSkip)
    End Function

    Private Function GetFolderChildren(RepId As String, Folder As EX.Folder, filter As String, includeAllowableActions As Boolean?, includePathSegment As Boolean?, MaxItems As Long, SkipCount As Long) As IObjectInFolderList
        Dim Objects As New List(Of IObjectInFolderData)

        Dim FolderFilter As New PropertyFilter(filter, DefaultFolderPropertyIds)
        Dim DocumentFilter As New PropertyFilter(filter, DefaultDocumentPropertyIds)

        Dim RetVal As New ObjectInFolderList With {.Objects = Objects}

        Dim FolderView As New EX.FolderView(1024) With {.PropertySet = GetFolderPropertySet()}

        Dim FolderSF As New EX.SearchFilter.SearchFilterCollection(EX.LogicalOperator.Or)
        For Each TypeId As String In ExFolderTypes
            FolderSF.Add(New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, TypeId))
        Next

        Dim FolderResults As EX.FindFoldersResults = Folder.FindFolders(FolderSF, FolderView)

        For Each ChildFolder As EX.Folder In FolderResults
            If (SkipCount > 0) Then SkipCount -= 1 : Continue For
            If (Objects.Count = MaxItems) Then RetVal.HasMoreItems = True : Exit For
            Dim Item As New ObjectInFolderData With {.Object = GetFolderObject(RepId, ChildFolder, FolderFilter, includeAllowableActions.GetValueOrDefault())}
            Objects.Add(Item)
        Next

        MaxItems -= Objects.Count

        If (Folder.FolderClass Is Nothing) Then RetVal.NumItems = FolderResults.TotalCount : Return RetVal

        Dim ItemView As New EX.ItemView(MaxItems, SkipCount) With {.PropertySet = EX.PropertySet.IdOnly}

        Dim ItemSF As New EX.SearchFilter.SearchFilterCollection(EX.LogicalOperator.Or)
        For Each TypeId As String In ExDocumentTypes
            ItemSF.Add(New EX.SearchFilter.IsEqualTo(EX.ItemSchema.ItemClass, TypeId))
        Next

        Dim ItemResults As EX.FindItemsResults(Of EX.Item) = Folder.FindItems(ItemSF, ItemView)

        Dim PropSet As EX.PropertySet = GetItemPropertySetForFolder(Folder.FolderClass, FolderFilter, includeAllowableActions.GetValueOrDefault())
        If (ItemResults.Any) Then Service.LoadPropertiesForItems(ItemResults.Items, PropSet)

        For Each ChildItem As EX.Item In ItemResults
            Dim Item As New ObjectInFolderData With {.Object = GetDocumentObject(RepId, ChildItem, FolderFilter, includeAllowableActions.GetValueOrDefault())}
            Objects.Add(Item)
        Next

        RetVal.NumItems = FolderResults.TotalCount + ItemResults.TotalCount

        Return RetVal
    End Function

    Private Function GetFolderObject(RepId As String, Folder As EX.Folder, Filter As PropertyFilter, includeAllowableActions As Boolean) As IObjectData
        Dim Props As New Properties(GetFolderProperties(RepId, Folder, Filter))
        Dim RetVal As New ObjectData With {.Properties = Props}
        If (includeAllowableActions) Then RetVal.AllowableActions = GetActions(Folder)
        Return RetVal
    End Function

    Private Function GetDocumentObject(RepId As String, Item As EX.Item, Filter As PropertyFilter, includeAllowableActions As Boolean) As IObjectData
        Dim Props As New Properties(GetDocumentProperties(RepId, Item, Filter))

        Dim RetVal As New ObjectData With {.Properties = Props}
        If (includeAllowableActions) Then RetVal.AllowableActions = GetActions(Item)
        Return RetVal
    End Function

    Public Overrides Function GetObjectByPath(RepId As String, Path As String, FilterString As String, includeAllowableActions As Boolean?, includeRelationships As IncludeRelationships?, renditionFilter As String, includePolicyIds As Boolean?, includeAcl As Boolean?, extension As Extensions.IExtensionsData) As IObjectData
        Dim PropFilter As New PropertyFilter(FilterString, DefaultFolderPropertyIds)
        Dim Folder As EX.Folder = LoadFolderObject(RepId, Path, GetFolderPropertySet())
        Return GetFolderObject(RepId, Folder, PropFilter, includeAllowableActions.GetValueOrDefault())
    End Function

    Protected Overrides Function GetParent(RepId As String, ObjectId As String, FilterString As String, IncludeAllowableActions As Boolean) As IObjectData
        If (ObjectId = "/") Then Return Nothing

        Dim Filter As New PropertyFilter(FilterString, DefaultFolderPropertyIds)

        If (ObjectId.StartsWith("/")) Then
            Dim ParentId As String = GetParentId(ObjectId)
            Return GetFolderObject(RepId, ParentId, FilterString, IncludeAllowableActions)
        Else
            Dim PropSet As New EX.PropertySet(EX.BasePropertySet.IdOnly, {EX.ItemSchema.ParentFolderId, EX.ItemSchema.Subject})
            Dim Item As EX.Item = EX.Item.Bind(Service, New EX.ItemId(ObjectId), PropSet)

            Dim Folder As EX.Folder = EX.Folder.Bind(Service, Item.ParentFolderId, GetFolderPropertySet())

            Return GetFolderObject(RepId, Folder, Filter, IncludeAllowableActions)
        End If

    End Function

    Private Function LoadFolderObject(RepId As String, Path As String, PropSet As EX.PropertySet) As EX.Folder
        Dim RootFolder As EX.Folder = GetRootFolder(RepId)
        If (Path = "/") Then Return RootFolder

        Dim SearchFilter As EX.SearchFilter = Nothing
        Dim PathSegments = Path.Split("/")
        Dim FolderView As New EX.FolderView(1) With {.PropertySet = PropSet, .Traversal = EX.FolderTraversal.Shallow}
        Dim Results As EX.FindFoldersResults = Nothing
        Dim RetryCount As Integer = 3

        For Each Segment In PathSegments
            If String.IsNullOrEmpty(Segment) Then Continue For
            Dim RetryAttempt As Integer = 0
            Do
                Try
                    SearchFilter = New EX.SearchFilter.IsEqualTo(EX.FolderSchema.DisplayName, Segment)
                    Results = Service.FindFolders(RootFolder.Id, SearchFilter, FolderView)
                    RootFolder = Results.FirstOrDefault
                    Exit Do
                Catch ex As Exception
                    RetryAttempt += 1
                    If RetryAttempt >= RetryCount Then Throw ex
                End Try
            Loop
        Next
        Return RootFolder
    End Function


#End Region

#Region "Discovery Service"

    Public Overrides Function Query(RepId As String, statement As String, searchAllVersions As Boolean?, includeAllowableActions As Boolean?, includeRelationships As IncludeRelationships?, renditionFilter As String, maxItems As Numerics.BigInteger?, skipCount As Numerics.BigInteger?, extension As Extensions.IExtensionsData) As IObjectList
        Dim NumToSkip As Long = CLng(skipCount.GetValueOrDefault()), MaxResults As Long = CLng(maxItems.GetValueOrDefault())

        Dim Types As New TypeDefinitionCollection(From Item In TypeDefs Where Item.IsQueryable.GetValueOrDefault() = True)
        Dim cqs As New CmisQuery(Me, statement, Types)

        Dim ItemType As ITypeDefinition = (From Item In TypeDefs Where Item.QueryName.ToLower() = cqs.PrimaryContentType.ToLower()).First

        Dim FilterString As String = String.Join(",", From Element In cqs.RuntimeSelectElements Select QueryNameToId(ItemType, Element.PropertyName))
        Dim Filter As New PropertyFilter(FilterString, {})

        Dim Scope As ScopePredicate = GetScopeElement(cqs)
        Dim PropSet As EX.PropertySet = GetItemPropertySet(ItemType, Filter, includeAllowableActions.GetValueOrDefault())
        Dim IncludeActions As Boolean = includeAllowableActions.GetValueOrDefault()

        Dim Folders As List(Of EX.Folder) = GetFoldersToSearch(RepId, ItemType, Scope, cqs)
        Dim IsSearchFolder As Boolean = (Folders.Count = 1) AndAlso (Folders.First.GetType() Is GetType(EX.SearchFolder))
        Dim QueryFilter As String = GetAqsFilter(ItemType, cqs)

        Dim RetVal As ObjectList
        If (Folders.Count = 1) Then
            Dim View As New EX.ItemView(MaxResults, NumToSkip) With {.PropertySet = EX.PropertySet.IdOnly}
            FillOrderBy(cqs, View, ItemType)
            RetVal = QuerySingle(RepId, QueryFilter, IncludeActions, PropSet, Filter, View, Folders.First)
        Else
            If (cqs.OrderByElements.Count > 0) Then Throw New Exception("ORDER BY can only be used in the following cases: (a) when searching the Message type with " &
                  "Use Search Folder enabled, or (b) when an IN_FOLDER() predicate is used, restricting the search to a single folder.  The IN_TREE() predicate cannot " &
                  "be used in conjunction with ORDER BY.")

            RetVal = QueryMulti(RepId, QueryFilter, IncludeActions, PropSet, Filter, MaxResults, NumToSkip, Folders)
        End If

        For Each Obj As ObjectData In RetVal.Objects
            For Each Prop As PropertyData In Obj.Properties.PropertyList
                Prop.QueryName = cqs.GetAliasName(Prop.QueryName)
            Next
        Next

        Return RetVal
    End Function

    ''' <summary>Performs a non-recursive search on the contents of a single folder.</summary>
    Private Function QuerySingle(RepId As String, AqsQuery As String, IncludeActions As Boolean, PropSet As EX.PropertySet, PropFilter As PropertyFilter, View As EX.ItemView, Folder As EX.Folder) As IObjectList
        Dim RetVal As New ObjectList With {.Objects = New List(Of IObjectData)}

        Dim Results As EX.FindItemsResults(Of EX.Item) = Folder.FindItems(AqsQuery, View)
        If (Results.Any = False) Then Return RetVal

        Service.LoadPropertiesForItems(Results.Items, PropSet)

        For Each Item As EX.Item In Results
            RetVal.Objects.Add(GetDocumentObject(RepId, Item, PropFilter, IncludeActions))
        Next

        RetVal.NumItems = Results.TotalCount
        Return RetVal
    End Function

    ''' <summary>Performs a recursive search on the contants and all descendants of a folder.</summary>
    Private Function QueryMulti(RepId As String, AqsQuery As String, IncludeActions As Boolean, PropSet As EX.PropertySet, PropFilter As PropertyFilter, MaxItems As Long, SkipCount As Long, Folders As List(Of EX.Folder)) As IObjectList
        Dim RetVal As New ObjectList With {.Objects = New List(Of IObjectData)}

        Dim Results As List(Of EX.Item) = QueryFolders(Folders, AqsQuery, MaxItems, SkipCount)
        If (Results.Any = False) Then Return RetVal

        Service.LoadPropertiesForItems(Results, PropSet)

        For Each Item As EX.Item In Results
            If (RetVal.Objects.Count = MaxItems) Then RetVal.HasMoreItems = True : Exit For
            RetVal.Objects.Add(GetDocumentObject(RepId, Item, PropFilter, IncludeActions))
        Next

        Return RetVal
    End Function

    Private Function QueryFolders(Folders As List(Of EX.Folder), AqsQuery As String, MaxResults As Long, SkipCount As Long) As List(Of EX.Item)
        Dim RetVal As New List(Of EX.Item)

        For Each Folder As EX.Folder In Folders
            If (Folder.TotalCount = 0) Then Continue For
            Dim View As New EX.ItemView(MaxResults + SkipCount + 1, 0) With {.PropertySet = EX.PropertySet.IdOnly}
            Dim ItemResults As EX.FindItemsResults(Of EX.Item) = Folder.FindItems(AqsQuery, View)
            For Each Item As EX.Item In ItemResults
                If (SkipCount > 0) Then SkipCount -= 1 : Continue For
                RetVal.Add(Item)
                If (RetVal.Count > MaxResults) Then Return RetVal
            Next
        Next
        Return RetVal
    End Function

    ''' <summary>Returns a sequence of EX.Folder objects which need to be searched.</summary>
    Private Function GetFoldersToSearch(RepId As String, ItemType As ITypeDefinition, Scope As ScopePredicate, cqs As CmisQuery) As List(Of EX.Folder)
        Select Case True
            Case CanUseSearchFolder(ItemType, Scope, cqs)
                Dim SearchFolder As EX.SearchFolder = GetSearchFolder()
                If (SearchFolder Is Nothing) Then SearchFolder = CreateSearchFolder()
                Return New List(Of EX.Folder)({SearchFolder})
            Case (Scope IsNot Nothing) AndAlso (Scope.IncludeSubfolders = False)
                Return New List(Of EX.Folder)({GetBaseFolder(RepId, Scope)})

            Case Else
                Dim BaseFolder As EX.Folder = GetBaseFolder(RepId, Scope)
                Dim Folders As New List(Of EX.Folder)({BaseFolder})
                Dim ChildFolders As EX.FindFoldersResults = GetDescendantFolders(BaseFolder.Id, ItemType)
                'Dim ChildFolders As EX.FindFoldersResults = GetDescendantFolders(BaseFolder.Id, ItemType)
                Folders.AddRange(ChildFolders)

                Return Folders
        End Select
    End Function

    ''' <summary>Returns true if the current query can be executed against the search folder.</summary>
    Private Function CanUseSearchFolder(ItemType As ITypeDefinition, Scope As ScopePredicate, cqs As CmisQuery) As Boolean
        'If (UseSearchFolder = False) Or (ItemType.Id <> MessageItemId) Or (Scope IsNot Nothing) Then Return False
        'If (From Item In cqs.AllLeafElements Where Item.GetType() Is GetType(ContainsPredicate)).Any Then Return False

        'Dim ExPds() As EX.PropertyDefinition = {EX.EmailMessageSchema.Sender, EX.EmailMessageSchema.ToRecipients, EX.EmailMessageSchema.CcRecipients, EX.EmailMessageSchema.BccRecipients}
        'Dim ExPropIds() As String = (From ExPd In ExPds Select ExPd.Name.ToLower()).ToArray()

        'For Each Element As WherePredicate In cqs.AllLeafElements
        '    Select Case Element.GetType()
        '        Case GetType(ComparisonPredicate)
        '            If (ExPropIds.Contains(DirectCast(Element, ComparisonPredicate).PropertyName.ToLower())) Then Return False
        '        Case GetType(InPredicate)
        '            If (ExPropIds.Contains(DirectCast(Element, InPredicate).PropertyName.ToLower())) Then Return False
        '    End Select
        'Next

        Return True
    End Function

    ''' <summary>Returns all descendant folders of a base folder.</summary>
    Private Function GetDescendantFolders(BaseFolderId As EX.FolderId, ItemType As ITypeDefinition) As EX.FindFoldersResults
        Dim FolderView As New EX.FolderView(1024) With {.Traversal = EX.FolderTraversal.Deep, .PropertySet = GetFolderPropertySet()}
        Dim sf As EX.SearchFilter = GetFolderSearchFilter(ItemType.Id)
        Return Service.FindFolders(BaseFolderId, sf, FolderView)
    End Function

    Private Sub FillOrderBy(Query As CmisQuery, View As EX.ItemView, ItemType As ITypeDefinition)
        If (Query.OrderByElements Is Nothing) Then Return
        Dim SchemaType As Type = GetSchemaType(ItemType)
        For Each Element As OrderByElement In Query.OrderByElements
            Dim EwsPropId As String = GetEwsPropId(Element.PropertyName.ToLower, ItemType)
            Dim ExPd As EX.PropertyDefinition = GetExPropDef(SchemaType, EwsPropId)
            Dim Direction As EX.SortDirection = If(Element.SortDirection = OrderByElement.OrderByDirection.DESC, EX.SortDirection.Descending, EX.SortDirection.Ascending)
            View.OrderBy.Add(ExPd, Direction)
        Next
    End Sub

    Private Function GetScopeElement(cqs As CmisQuery) As ScopePredicate
        Const ScopeError As String = "Only a single IN_FOLDER or IN_TREE predicate is supported.  The predicate must be at the root of the WHERE clause in" &
             "a group using the AND operator."

        Dim ScopeCount As Integer = (From Item In cqs.AllLeafElements Where Item.GetType() Is GetType(ScopePredicate)).Count
        If (ScopeCount = 0) Then Return Nothing
        If (ScopeCount > 1) Then Throw New Exception(ScopeError)

        Dim RetVal As ScopePredicate = (From Item In cqs.RootWhereElements Where Item.GetType() Is GetType(ScopePredicate)).FirstOrDefault
        If (RetVal Is Nothing) Then Throw New Exception(ScopeError)

        If RetVal.Inverted Then Throw New Exception("NOT cannot be used with the IN_FOLDER or IN_TREE predicates.")

        If (cqs.IsRootAND = False) Then Throw New Exception(ScopeError)
        Return RetVal
    End Function

    Private Function GetBaseFolder(RepId As String, Scope As ScopePredicate) As EX.Folder
        If (Scope Is Nothing) Then Return GetRootFolder(RepId)
        Return LoadFolderObject(RepId, Scope.FolderId, GetFolderPropertySet())
    End Function

#Region "AQS Query Generation"

    Private Function GetAqsFilter(ItemType As ITypeDefinition, cqs As CmisQuery) As String
        Dim Criteria As New List(Of String)({GetAqsTypeFilter(ItemType)})

        If (cqs.WhereClause IsNot Nothing) Then Criteria.Add(GenerateAQS(cqs.WhereClause, ItemType))

        Return String.Join(" ", Criteria)
    End Function

    Private Function GetAqsTypeFilter(ItemType As ITypeDefinition) As String
        Select Case ItemType.Id
            Case AppointmentItemId : Return "kind:=appointment"
            Case ContactItemId : Return "kind:=contact"
            Case TaskItemId : Return "kind:=task"
            Case MessageItemId : Return "kind:=(email OR appointment)"
            Case Else : Return "kind:=(email OR appointment OR contact OR task)"
        End Select
    End Function

    Private Function GenerateAQS(Element As WherePredicate, ItemType As ITypeDefinition) As String
        Select Case Element.GetType()
            Case GetType(ComparisonPredicate) : Return Element.Prefix & GetComparisonAQS(Element, ItemType)
            Case GetType(ContainsPredicate) : Return Element.Prefix & GetContainsAQS(Element, ItemType)
            Case GetType(InPredicate) : Return Element.Prefix & GetInAQS(Element, ItemType)
            Case GetType(PredicateCollection) : Return Element.Prefix & GetCollectionAQS(Element, ItemType)
            Case Else : Return Nothing
        End Select
    End Function

    Private Function GetComparisonAQS(Element As ComparisonPredicate, ItemType As ITypeDefinition) As String
        Dim pd As IPropertyDefinition = (From Item In ItemType.PropertyDefinitions Where Item.QueryName.ToLower() = Element.PropertyName.ToLower()).First

        If (pd.PropertyType = PropertyType.String) Then Return GetStringAQS(Element, pd)

        Return String.Format("{0}:{1}{2}", pd.LocalNamespace, Element.CompOp.ToUpper(), Element.Value)
    End Function

    Private Function GetStringAQS(Element As ComparisonPredicate, PropDef As IPropertyDefinition) As String
        Select Case Element.CompOp.ToUpper()
            Case "LIKE" : Return GetLikeAQS(Element, PropDef)
            Case "="
                ValidateEqualCondition(Element, PropDef)
                Return String.Format("{0}:=""{1}""", PropDef.LocalNamespace, Element.Value)
            Case Else : Throw New Exception($"Exchange does not support the {Element.CompOp.ToUpper()} operator on string properties.")
        End Select
    End Function

    Private Function GetLikeAQS(Element As ComparisonPredicate, PropDef As IPropertyDefinition) As String
        If Element.IsEquality Then ValidateEqualCondition(Element, PropDef) : Return String.Format("{0}:=""{1}""", PropDef.LocalNamespace, Element.Value)
        If (Element.IsContains = False) Then Throw New Exception("EWS can only perform substring searches.  Therefore, all LIKE expressions must begin and end with a wildcard: PropName LIKE '%value%'.")
        Return String.Format("{0}:~=""{1}""", PropDef.LocalNamespace, Element.Value.Substring(1, Element.Value.Length - 2))
    End Function

    Private Sub ValidateEqualCondition(Element As ComparisonPredicate, PropDef As IPropertyDefinition)
        If (PropDef.Id = "Subject") Then Return
        Throw New Exception($"The = operator cannot be used with '{PropDef.QueryName}', because EWS only supports substring searches on this property. Use {PropDef.QueryName} LIKE '%{Element.Value}%' instead.")
    End Sub

    Private Function GetContainsAQS(Element As ContainsPredicate, ItemType As ITypeDefinition) As String
        If (Element.AllLeafParts.Count > 1) Then Return "(" & Element.SearchString & ")" Else Return Element.SearchString
    End Function

    Private Function GetInAQS(Element As InPredicate, ItemType As ITypeDefinition) As String
        Dim pd As IPropertyDefinition = (From Item In ItemType.PropertyDefinitions Where Item.QueryName.ToLower() = Element.PropertyName.ToLower()).First
        If (pd.LocalName <> "Subject") Then Throw New Exception("The IN Predicate can only be used with the Subject property.")
        Dim QuotedValues As IEnumerable(Of String) = (From Value In Element.Values Select """" & Value & """")

        Return String.Format("{0}:=({1})", pd.LocalNamespace, String.Join(" OR ", QuotedValues))
    End Function

    Private Function GetCollectionAQS(Elements As PredicateCollection, ItemType As ITypeDefinition) As String
        Dim Separator As String = If(Elements.LogicalOperator.ToString().ToUpper() = "AND", " ", " OR ")
        Dim InnerContent As IEnumerable(Of String) = (From Element In Elements Select Val = GenerateAQS(Element, ItemType) Where Not String.IsNullOrEmpty(Val))
        Return $"({String.Join(Separator, InnerContent)})"
    End Function

#End Region

    Private Function QueryNameToId(td As ITypeDefinition, QueryName As String) As String
        If (QueryName = "*") Then Return QueryName
        Dim pd As IPropertyDefinition = (From Item In td.PropertyDefinitions Where Item.QueryName.ToLower() = QueryName.ToLower()).FirstOrDefault
        If (pd Is Nothing) Then Throw New Exception($"'{QueryName}' does not match the queryname of a any property on content type '{td.QueryName}'.")

        Return pd.Id
    End Function

    Private Function GetFolderSearchFilter(ItemTypeId As String) As EX.SearchFilter
        Select Case ItemTypeId
            Case AppointmentItemId : Return New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, AppointmentFolderClass)
            Case ContactItemId : Return New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, ContactFolderClass)
            Case MessageItemId : Return New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, MessageFolderClass)
            Case TaskItemId : Return New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, TaskFolderClass)
        End Select

        Dim sfc As New EX.SearchFilter.SearchFilterCollection(EX.LogicalOperator.Or)
        sfc.Add(New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, AppointmentFolderClass))
        sfc.Add(New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, ContactFolderClass))
        sfc.Add(New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, MessageFolderClass))
        sfc.Add(New EX.SearchFilter.IsEqualTo(EX.FolderSchema.FolderClass, TaskFolderClass))
        Return sfc
    End Function


#End Region

#Region "Misc Overrides"

    Public Overrides Sub ClearAllCaches()
        _RootFolders = Nothing : _Service = Nothing : _TypeDefs = Nothing
    End Sub

#End Region

#Region "Misc Private Members"

    Private Function GetCmisType(Type As Type) As PropertyType
        If (Type.IsGenericType) Then Type = Nullable.GetUnderlyingType(Type)
        Select Case Type
            Case GetType(Decimal) : Return PropertyType.Decimal
            Case GetType(Boolean) : Return PropertyType.Boolean
            Case GetType(DateTime) : Return PropertyType.DateTime
            Case GetType(Boolean) : Return PropertyType.Boolean
            Case GetType(Integer) : Return PropertyType.Integer
            Case GetType(Long) : Return PropertyType.Integer
            Case Else : Return PropertyType.String
        End Select
    End Function

#End Region

#Region "Search Folder Management"

    Private Const SearchFolderName As String = "Grooper Search"

    Private Function GetSearchFolder() As EX.SearchFolder
        Dim SearchFolderRoot As EX.Folder = EX.Folder.Bind(Service, New EX.FolderId(EX.WellKnownFolderName.SearchFolders))

        Dim sf As EX.SearchFilter = New EX.SearchFilter.IsEqualTo(EX.FolderSchema.DisplayName, SearchFolderName)
        Dim Results As EX.FindFoldersResults = SearchFolderRoot.FindFolders(sf, New EX.FolderView(1))
        Return Results.FirstOrDefault
    End Function

    Private Function CreateSearchFolder() As EX.SearchFolder
        Dim RetVal As New EX.SearchFolder(Service) With {.DisplayName = SearchFolderName}
        RetVal.SearchParameters.RootFolderIds.Add(New EX.FolderId(EX.WellKnownFolderName.MsgFolderRoot))
        RetVal.SearchParameters.Traversal = EX.SearchFolderTraversal.Deep
        RetVal.Save(EX.WellKnownFolderName.SearchFolders)
        Threading.Thread.Sleep(5000)
        Return RetVal
    End Function

#End Region

#Region "PropertySet Generation"

    Private ReadOnly Property PR_FOLDER_PATH As EX.ExtendedPropertyDefinition
        Get
            Return New EX.ExtendedPropertyDefinition(26293, EX.MapiPropertyType.String)
        End Get
    End Property

    Private Function GetFolderPropertySet() As EX.PropertySet
        Dim RetVal As New EX.PropertySet(EX.BasePropertySet.FirstClassProperties)
        RetVal.Add(PR_FOLDER_PATH)
        RetVal.Add(EX.FolderSchema.TotalCount)
        Return RetVal
    End Function

    ''' <summary>Returns the property set to be used for folders of the indicated folder class.</summary>
    Private Function GetItemPropertySetForFolder(FolderClass As String, Filter As PropertyFilter, IncludeActions As Boolean) As EX.PropertySet
        Dim ItemClassName As String = FolderClass.Replace("IPF.", "IPM.")
        Dim ItemType As ITypeDefinition = (From Item In TypeDefs Where Item.LocalName = ItemClassName).First
        Return GetItemPropertySet(ItemType, Filter, IncludeActions)
    End Function

    ''' <summary>Returns the property set to be used for items of the indicated type.</summary>
    ''' <param name="TypeDef">The content type being queried.</param>
    Private Function GetItemPropertySet(TypeDef As ITypeDefinition, Filter As PropertyFilter, IncludeActions As Boolean) As EX.PropertySet
        If (TypeDef.Id = "cmis:document") Then Return GetBasePropertySet(TypeDef, Filter, IncludeActions)

        Dim UsedNames As New HashSet(Of String)
        Dim RetVal As New EX.PropertySet(RequiredPropDefs)

        Dim SchemaType As Type = GetSchemaType(TypeDef)

        For Each CmisPd As IPropertyDefinition In TypeDef.PropertyDefinitions
            If String.IsNullOrWhiteSpace(CmisPd.LocalName) Or Filter.Excludes(CmisPd.Id) Then Continue For
            Dim EwsPropId As String = GetEwsPropId(CmisPd)
            If (UsedNames.Contains(EwsPropId)) Then Continue For
            Dim ExPd As EX.PropertyDefinition = GetExPropDef(SchemaType, EwsPropId)
            RetVal.Add(ExPd)
        Next

        If (IncludeActions) And (UsedNames.Contains(EX.ItemSchema.EffectiveRights.Name) = False) Then RetVal.Add(EX.ItemSchema.EffectiveRights)
        Return RetVal
    End Function

    Private Function GetBasePropertySet(BaseTypeDef As ITypeDefinition, Filter As PropertyFilter, IncludeActions As Boolean) As EX.PropertySet
        Dim DerivedTypes As IEnumerable(Of ITypeDefinition) = (From Item In TypeDefs Where Item.ParentTypeId = BaseTypeDef.Id)

        Dim RetVal As New EX.PropertySet(RequiredPropDefs)
        Dim UsedNames As New HashSet(Of String)

        For Each TypeDef As ITypeDefinition In DerivedTypes
            Dim SchemaType As Type = GetSchemaType(TypeDef)

            For Each CmisPd As IPropertyDefinition In TypeDef.PropertyDefinitions
                If String.IsNullOrWhiteSpace(CmisPd.LocalName) Or UsedNames.Contains(CmisPd.LocalName) Or Filter.Excludes(CmisPd.Id) Then Continue For
                Dim EwsPropId As String = GetEwsPropId(CmisPd)
                Dim ExPd As EX.PropertyDefinition = GetExPropDef(SchemaType, EwsPropId)
                RetVal.Add(ExPd)
            Next
        Next

        If (IncludeActions) And (UsedNames.Contains(EX.ItemSchema.EffectiveRights.Name) = False) Then RetVal.Add(EX.ItemSchema.EffectiveRights)

        Return RetVal
    End Function

    Private ReadOnly Property RequiredPropDefs As EX.PropertyDefinition()
        Get
            Return {EX.ItemSchema.ItemClass, EX.ItemSchema.Subject, EX.ContactSchema.DisplayName}
        End Get
    End Property

    Private Function GetSchemaType(TypeDef As ITypeDefinition) As Type
        Select Case TypeDef.Id
            Case "cmis:document" : Return GetType(EX.ItemSchema)
            Case AppointmentItemId : Return GetType(EX.AppointmentSchema)
            Case ContactItemId : Return GetType(EX.ContactSchema)
            Case MessageItemId : Return GetType(EX.EmailMessageSchema)
            Case TaskItemId : Return GetType(EX.TaskSchema)
            Case Else : Return GetType(EX.FolderSchema)
        End Select
    End Function

    Private Function GetEwsType(TypeDef As ITypeDefinition) As Type
        Select Case TypeDef.Id
            Case "cmis:document" : Return GetType(EX.Item)
            Case AppointmentItemId : Return GetType(EX.Appointment)
            Case ContactItemId : Return GetType(EX.Contact)
            Case MessageItemId : Return GetType(EX.EmailMessage)
            Case TaskItemId : Return GetType(EX.Task)
            Case AppointmentFolderId : Return GetType(EX.CalendarFolder)
            Case ContactFolderId : Return GetType(EX.ContactsFolder)
            Case TaskFolderId : Return GetType(EX.TasksFolder)
            Case Else : Return GetType(EX.Folder)
        End Select
    End Function


    Private Function GetExPropDef(SchemaType As Type, Name As String) As EX.PropertyDefinition
        Dim fi As Reflection.FieldInfo = SchemaType.GetField(Name, Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.FlattenHierarchy)
        If (fi Is Nothing) Then Throw New Exception($"'{Name}' is not a valid property name.")
        Return fi.GetValue(Nothing)
    End Function

    Private Function GetEwsPropId(QueryName As String, ItemType As ITypeDefinition) As String
        Dim pd As IPropertyDefinition = (From Item In ItemType.PropertyDefinitions Where Item.QueryName.ToLower() = QueryName.ToLower()).First
        Return GetEwsPropId(pd)
    End Function

    Private Function GetEwsPropId(pd As IPropertyDefinition) As String
        If String.IsNullOrWhiteSpace(pd.LocalName) Then Return Nothing
        Dim Segments() As String = pd.LocalName.Split("."c)
        Return Segments(0)
    End Function

#End Region

#Region "Content Type Data"

    'CMIS folder classes
    Private Const AppointmentFolderId As String = "CalendarFolder"
    Private Const ContactFolderId As String = "ContactsFolder"
    Private Const MessageFolderId As String = "MailFolder"
    Private Const TaskFolderId As String = "TasksFolder"

    'CMIS item classes
    Private Const AppointmentItemId As String = "Appointment"
    Private Const ContactItemId As String = "Contact"
    Private Const MessageItemId As String = "Message"
    Private Const TaskItemId As String = "Task"

    'Exchange folder classes
    Private Const AppointmentFolderClass As String = "IPF.Appointment"
    Private Const ContactFolderClass As String = "IPF.Contact"
    Private Const MessageFolderClass As String = "IPF.Note"
    Private Const TaskFolderClass As String = "IPF.Task"

    'Exchange item classes
    Private Const AppointmentItemClass As String = "IPM.Appointment"
    Private Const ContactItemClass As String = "IPM.Contact"
    Private Const MessageItemClass As String = "IPM.Note"
    Private Const MessageSecureItemClass As String = "IPM.Note.SMIME"
    Private Const MessageSignedItemClass As String = "IPM.Note.SMIME.MultipartSigned"
    Private Const TaskItemClass As String = "IPM.Task"


    Private ReadOnly Property ExFolderTypes As IEnumerable(Of String)
        Get
            Return From Item In TypeDefs Where (Item.ParentTypeId = "cmis:folder") And (String.IsNullOrWhiteSpace(Item.LocalName) = False) Select Item.LocalName
        End Get
    End Property

    Private ReadOnly Property ExDocumentTypes As IEnumerable(Of String)
        Get
            Return From Item In TypeDefs Where (Item.ParentTypeId = "cmis:document") Select Item.LocalName
        End Get
    End Property

    Private Function ExToLocal(TypeId As String) As String
        Dim MatchingType As ITypeDefinition = (From Item In TypeDefs Where Item.LocalName = TypeId).FirstOrDefault
        Return If(MatchingType Is Nothing, MessageItemId, MatchingType.Id)
    End Function

    Private Function GetName(Item As EX.Item) As String
        Dim Name As String = Nothing

        Select Case ExToLocal(Item.ItemClass)
            Case ContactItemId
                Name = DirectCast(Item, EX.Contact).DisplayName
            Case Else
                Name = If(Item.Subject Is Nothing, "", Item.Subject)
                If (Name.Length > 64) Then Name = Name.Substring(0, 64)
        End Select

        Return CleanName(Name)
    End Function

    Private Function GetMimeType(Item As EX.Item) As String
        Select Case ExToLocal(Item.ItemClass)
            Case MessageItemId : Return "message/rfc822"
            Case ContactItemId : Return "text/x-vcard"
            Case AppointmentItemId : Return "text/calendar"
            Case Else : Return "text/plain"
        End Select
    End Function

    Private Function GetExtension(Item As EX.Item) As String
        Select Case ExToLocal(Item.ItemClass)
            Case MessageItemId : Return ".eml"
            Case ContactItemId : Return ".vcf"
            Case AppointmentItemId : Return ".ics"
            Case Else : Return ".txt"
        End Select
    End Function


    Private ReadOnly Property ContentStreamPropertySet As EX.PropertySet
        Get
            Return New EX.PropertySet({EX.ItemSchema.MimeContent, EX.ItemSchema.ItemClass, EX.ItemSchema.Subject, EX.ContactSchema.DisplayName})
        End Get
    End Property

#End Region

#Region "EWS Connection"

    Private _Service As EX.ExchangeService

    Private ReadOnly Property Service As EX.ExchangeService
        Get
            If (_Service IsNot Nothing AndAlso _Service.Credentials IsNot Nothing AndAlso Not _Service.HttpResponseHeaders.ContainsKey("x-ms-diagnostics")) Then Return _Service
            ServicePointManager.ServerCertificateValidationCallback = AddressOf CertValidationCallback
            _Service = New EX.ExchangeService With {
                  .Timeout = Timeout * 1000,
                  .UserAgent = "Grooper",
                  .Url = New Uri(ApiUrl)
                }
            Authenticate(_Service)

            Return _Service
        End Get
    End Property

    Private Function RedirectCallback(URL As String) As Boolean
        Dim Uri As New Uri(URL)
        Return (Uri.Scheme = "https")
    End Function

    Private Function CertValidationCallback(Sender As Object, Certificate As X509Certificate, Chain As X509Chain, SslPolicyErrors As SslPolicyErrors) As Boolean
        If (SslPolicyErrors = Security.SslPolicyErrors.None) Then Return True

        If (SslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors)) Then
            If (Chain Is Nothing) Then Return True
            If (Chain.ChainStatus Is Nothing) Then Return True
            For Each Status In Chain.ChainStatus
                If ((Certificate.Subject = Certificate.Issuer) And (Status.Status = X509ChainStatusFlags.UntrustedRoot)) Then Continue For
                If (Status.Status <> X509ChainStatusFlags.NoError) Then Return False
            Next
        End If

        Return True
    End Function

    Private ReadOnly Property ApiUrl As String
        Get
            Return String.Format("https://{0}/ews/Exchange.asmx", RemoteURL)
        End Get
    End Property


#End Region

#Region "Authentication"

    Private Sub Authenticate(Svc As EX.ExchangeService)
        If (AuthenticationMethod Is Nothing) Then Throw New Exception("Authentication method not specified.")
        Select Case AuthenticationMethod.GetType()
            Case GetType(AutoAuthentication) : Svc.UseDefaultCredentials = True
            Case GetType(BasicAuthentication) : AutenticateBasic(Svc, AuthenticationMethod)
            Case GetType(NtlmAuthentication) : AutenticateNTLM(Svc, AuthenticationMethod)
            Case GetType(ExchangeOAuth) : AutenticateOAuth(Svc, AuthenticationMethod)
        End Select
    End Sub
    Private Sub AutenticateBasic(Svc As EX.ExchangeService, Auth As BasicAuthentication)
        Svc.Credentials = New EX.WebCredentials(Auth.UserName, Auth.Password)
    End Sub
    Private Sub AutenticateNTLM(Svc As EX.ExchangeService, Auth As NtlmAuthentication)
        Svc.Credentials = New EX.WebCredentials(Auth.UserName, Auth.Password, Auth.Domain)
    End Sub
    Private Sub AutenticateOAuth(Svc As EX.ExchangeService, Auth As ExchangeOAuth)
        Svc.Credentials = New EX.OAuthCredentials(Auth.AccessToken)
    End Sub

    ''' <summary>Implements <a target='_blank' href='https://en.wikipedia.org/wiki/OAuth'>OAuth 2.0</a> for connections to [Exchange].</summary>
    <DataContract, DisplayName("Exchange OAuth")>
    Public Class ExchangeOAuth : Inherits AzureOAuth

        Protected Overrides ReadOnly Property Resource As String
            Get
                Return "https://outlook.office365.com"
            End Get
        End Property

        Protected Overrides ReadOnly Property Scope As String
            Get
                Return "EWS.AccessAsUser.All offline_access"
            End Get
        End Property

    End Class

    Private Class AuthMethodConverter : Inherits ObjectTypeConverter
        Protected Overrides Function GetTypeList(context As ITypeDescriptorContext) As IEnumerable(Of Type)
            Return {GetType(AutoAuthentication), GetType(BasicAuthentication), GetType(NtlmAuthentication), GetType(ExchangeOAuth)}
        End Function
    End Class

#End Region

#Region "Nested Types"

    Private Class PropRef
        Public EwsId As String
        Public Index As String

        Public Sub New(CmisId As String)
            Dim sa As String() = CmisId.Split("."c)
            EwsId = sa(0)
            If (sa.Count = 2) Then Index = sa(1)
        End Sub
    End Class

    ''' <summary>Deletes and recreates the Exchange search folder created by Grooper.</summary>
    ''' <remarks>Grooper creates a search folder to perform searches against.  Rebuilding the search folder can improve performance by
    ''' clearing out old entries and rebuilding the search folder.</remarks>
    <IconResource(NameOf(My.Resources.Search)), SkipConfirmation>
    Public Class RebuildSearchFolder : Inherits ControlCommand(Of RemoteGitBinding, Grooper.PropertyGrid)
        Protected Overrides Sub Execute(Item As RemoteGitBinding)
            Dim Folder As EX.SearchFolder = Item.GetSearchFolder()
            If (Folder IsNot Nothing) Then Folder.Delete(EX.DeleteMode.HardDelete)
            Item.CreateSearchFolder()
        End Sub
    End Class

#End Region

End Class
