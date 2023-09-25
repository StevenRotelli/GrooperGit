''' <summary>Grooper SciptingSession object. 2019 by BIS (http://www.bisok.com).</summary>
''' <remarks>
''' Welcome to Grooper Scripting!  Here is some quick information for beginners:
''' 
''' 1) Click on the Task List at the bottom, to see (and navigate to) suggested tasks.  If the Task List is not visible,
'''    it can be shown by going to the View menu and selecting Task List (or Ctrl+\,T).
''' 2) Initialize() is called when the object is loaded into memory, and Uninitialize() is called when the object is
'''    unloaded or when the application exits.
''' 3) Member variables of this class will stay intact between Initialize() and Uninitialize().
''' 4) Please note that compiling from this script will not add/update the ObjectLibrary DLL in Grooper.  To add/update 
'''    this DLL in Grooper and make the functionality in this project visible to Grooper, the Compile button must be 
'''    used from the Script tab in Grooper Design Studio.
''' 5) Object Library scripts can be used to customize the way that Grooper works.  This can be done by extending a 
'''    Grooper class through inheritance or by creating methods that can be called from within Grooper.
''' 6) Data Model and Batch Process Scripts operate primarily by responding to events on the object to which they are
'''    attached.  Viewing a list of events your script can handle is easy if you are working in Visual Studio: simply
'''    select ObjectLibrary from the objects dropdown above, and then view the right-most dropdown for a list of events.
''' 7) Custom templates.  Right-clicking the project from Solution Explorer and selecting Add..New Item will show the
'''    list of custom Grooper templates that can be used.
''' 8) SDK Help can be found at http://grooper.bisok.com/Documentation/2.80/SDK/HTML5/index.htm
'''
'''  Refer to Grooper Documentation for more information: https://xchange.grooper.com/discussion/793/visual-studio-integration-vsi.
''' </remarks>
Public Class ScriptingSession : Inherits ScriptObject
  Private WithEvents ObjectLibrary As Grooper.ObjectLibrary
  Private Root As GrooperRoot

  Public Overrides Function Initialize(Item As GrooperNode) As Boolean

        Me.ObjectLibrary = Item
        Me.Root = Item.Root

        'Dim projects As ProjectsFolder = Item.Root.Projects
        'Dim ico As Drawing.Image = projects.IconImage
        'Dim currentAllowedTypes As List(Of Type) = projects.AllowedChildTypes.ToList()

        'If currentAllowedTypes.Any(Function(t) t.FullName = GetType(GitProject).FullName) Then
        '    Return True
        'End If

        'currentAllowedTypes.Add(GetType(GitProject))
        'projects.SetAllowedTypes(currentAllowedTypes)

        Return True

    End Function

  Public Overrides Function Uninitialize() As Boolean
    Return True
  End Function

End Class
