Public Class FrmCrearServer
    Private Cancelado As Boolean = True

    Public Shared Function Mostrar(ByRef puerto As Integer, ByRef contraseña As String) As Boolean
        Dim frm As New FrmCrearServer
        frm.ShowDialog()
        If Not frm.Cancelado Then
            puerto = CInt(frm.TxtPuerto.Text)
            If frm.ChkContraseña.Checked Then
                contraseña = frm.TxtContraseña.Text
            Else
                contraseña = Nothing
            End If
        End If
        Return Not frm.Cancelado
    End Function

    Private Sub ChkContraseña_CheckedChanged(sender As Object, e As EventArgs) Handles ChkContraseña.CheckedChanged
        TxtContraseña.Enabled = ChkContraseña.Checked
    End Sub

    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnAceptar.Click
        Dim Puerto As Integer
        If Not Integer.TryParse(TxtPuerto.Text, Puerto) Then
            MsgBox("El puerto no es valido")
            Return
        End If
        If Puerto < 1 Or Puerto > 65536 Then
            MsgBox("El puerto no es valido")
            Return
        End If

        If ChkContraseña.Checked Then
            If TxtContraseña.Text.Trim().Length = 0 Then
                MsgBox("Ingrese una contraseña")
                Return
            End If
        End If

        Cancelado = False
        Me.Close()
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Me.Close()
    End Sub
End Class