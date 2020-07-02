Imports System.Net

Public Class FrmConectarServer
    Private Cancelado As Boolean = True
    Private EndPoint As IPEndPoint

    Public Shared Function Mostrar(ByRef endPoint As IPEndPoint) As Boolean
        Dim frm As New FrmConectarServer
        frm.ShowDialog()
        If Not frm.Cancelado Then
            endPoint = frm.EndPoint
        End If
        Return Not frm.Cancelado
    End Function

    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnAceptar.Click
        Dim puerto As Integer
        If Not Integer.TryParse(TxtPuerto.Text, puerto) Then
            MsgBox("El puerto no es valido")
            Return
        End If
        If puerto < 1 Or puerto > 65536 Then
            MsgBox("El puerto no es valido")
            Return
        End If

        Dim ip As IPAddress
        If Not IPAddress.TryParse(TxtIp.Text, ip) Then
            MsgBox("La IP no es valida")
            Return
        End If

        EndPoint = New IPEndPoint(ip, puerto)

        Cancelado = False
        Me.Close()
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Me.Close()
    End Sub
End Class