Public Class BranchListConverter : Inherits PgListConverter(Of GitProject, String)
    Protected Overrides Function GetListItems(Instance As GitProject, pd As PropertyDescriptor) As IEnumerable(Of String)
        If Instance IsNot Nothing Then
            Dim go As New GitObject(Instance.localRepository)
            Return go.ListBranches()
        Else
            Return {}
        End If
    End Function
End Class