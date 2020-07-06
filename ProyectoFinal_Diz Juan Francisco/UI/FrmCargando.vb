Public Class FrmCargando
    Public ForzarCierre = False
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not ForzarCierre Then
            e.Cancel = True
        End If
    End Sub

End Class