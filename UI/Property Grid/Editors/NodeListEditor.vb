Imports System.Drawing.Design
Imports System.Windows.Forms.Design

Public Class NodeListEditor : Inherits ReferenceEditorBase

    Public Overrides Function GetSelectableTypes(PropertyType As Type) As IEnumerable(Of Type)
        Dim PropType As Type = If(TypeData.GetCollectionElementType(PropertyType), PropertyType)
        Return MyBase.GetSelectableTypes(PropType)
    End Function

    Public Overrides Function GetStyle(context As System.ComponentModel.ITypeDescriptorContext) As UITypeEditorEditStyle
        If (context.PropertyDescriptor.IsReadOnly) Then Return UITypeEditorEditStyle.None
        Return UITypeEditorEditStyle.Modal
    End Function

End Class

''' <summary>Used for editing a list of GrooperNodes in a PropertyGrid.</summary>
Public Class NodeListEditor(Of NodeType As GrooperNode) : Inherits NodeListEditor

    Public Overrides Function EditValue(context As ITypeDescriptorContext, provider As IServiceProvider, value As Object) As Object
        Dim wfes As IWindowsFormsEditorService = provider.GetService(GetType(IWindowsFormsEditorService))

        Dim Item As ConnectedObject = context.Instance

        Dim BaseNodes As IEnumerable(Of GrooperNode) = GetBaseNodes(GetType(NodeType), Item)
        Dim SelectableTypes() As Type = GetSelectableTypes(GetType(NodeType)).ToArray()

        Dim List As IList = value

        Dim SelectedNodes As New List(Of GrooperNode)(List.Cast(Of GrooperNode))

        Using nle As New frmNodeListEditor(BaseNodes, SelectableTypes, SelectedNodes) With {.Text = context.PropertyDescriptor.DisplayName}
            If (wfes.ShowDialog(nle) = DialogResult.Cancel) Then Return value
            Return New List(Of NodeType)(nle.Value.Cast(Of NodeType))
        End Using

    End Function

End Class