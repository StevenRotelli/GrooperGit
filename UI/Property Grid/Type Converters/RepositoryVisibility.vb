
Public Class RepositoryVisibility : Inherits PgListConverter(Of Publish, String)
    Protected Overrides Function GetListItems(Instance As Publish, pd As PropertyDescriptor) As IEnumerable(Of String)
        Return New List(Of String)({"Public", "Private"})
    End Function
End Class
