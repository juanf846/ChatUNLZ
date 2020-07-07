Public Class FrmCambiarNombre
    Public Cancelado As Boolean = True
    Public NewNombre As String
    Public NewColor As Color

    Public Sub New(nombreActual As String, colorActual As Color)
        InitializeComponent()

        TxtNombre.Text = nombreActual
        BtnColor.BackColor = colorActual
    End Sub

    Private Sub BtnColor_Click(sender As Object, e As EventArgs) Handles BtnColor.Click
        Dim frm As New ColorDialog
        frm.Color = BtnColor.BackColor
        frm.ShowDialog()
        BtnColor.BackColor = frm.Color
    End Sub
    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnAceptar.Click
        NewNombre = TxtNombre.Text.Trim(" ")
        If NewNombre.Length = 0 Or NewNombre.Length > 32 Then
            MsgBox("Ingrese un nombre entre 1 y 32 caracteres")
            Return
        End If
        NewColor = BtnColor.BackColor
        Cancelado = False
        Me.Close()
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        Me.Close()
    End Sub
End Class