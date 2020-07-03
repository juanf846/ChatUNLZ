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
        For i = 0 To lista.Count
            If id = lista(i).ServerId Then
                Return lista(i)
            End If
        Next
        Return Nothing
    End Function
End Class
