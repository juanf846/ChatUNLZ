Imports System.Net

<Serializable()>
Public Class Usuario
    Public ServerId As Integer
    Public Nombre As String
    Public Color As Color
    Public Conectado As Boolean = True
    <NonSerialized()>
    Public EndPoint As IPEndPoint

    Public Shared Function GetUserById(id As Integer, lista As List(Of Usuario)) As Usuario
        For Each u In lista
            If id = u.ServerId Then
                Return u
            End If
        Next
        Return Nothing
    End Function
End Class
