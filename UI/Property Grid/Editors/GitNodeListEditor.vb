Imports System.Drawing.Design
Imports System.Windows.Forms.Design
''' <summary>Used for editing a list of GrooperNodes in a PropertyGrid.</summary>
Public Class GitNodeListEditor : Inherits NodeListEditor(Of GrooperNode)
    Public Overrides Function GetBaseNodes(PropertyType As Type, ConnectedItem As ConnectedObject) As IEnumerable(Of GrooperNode)
        Return GetModified(ConnectedItem)
    End Function
    Protected Shared Function GetModified(ConnectedItem As ConnectedObject) As IEnumerable(Of GrooperNode)
        Dim RetVal As New List(Of GrooperNode)
        For Each node As GrooperNode In DirectCast(ConnectedItem, GrooperNode)
            If node.HasValue("GitStatus") Then
                RetVal.Add(node)
            End If
        Next
        Return RetVal
    End Function
End Class



