Imports System.Net

Public Class FrmChat
    Private UsuarioInfo As Logica.Usuario
    Private ClientMode As Boolean
    Private Cliente As Logica.Cliente
    Private Server As Logica.Server

    Private FrmMain As FrmMain

    Public Class ListBoxItem
        Public Texto As String
        Public Color As Color
    End Class

    Public Shared Sub Open(clientMode As Boolean, ipAddress As IPEndPoint, usuarioInfo As Logica.Usuario, contraseña As String, ByRef mainFrm As FrmMain)
        Dim frmChat As New FrmChat
        frmChat.ClientMode = clientMode
        frmChat.UsuarioInfo = usuarioInfo
        frmChat.LblNombre.Text = usuarioInfo.Nombre
        frmChat.LblNombre.ForeColor = usuarioInfo.Color
        frmChat.FrmMain = mainFrm
        If Not clientMode Then
            frmChat.Server = New Logica.Server(ipAddress.Port, contraseña)
            If Not frmChat.Server.Funcionando Then
                Return
            End If
            ipAddress = New IPEndPoint(Net.IPAddress.Loopback, ipAddress.Port)
        End If
        frmChat.Show()
        frmChat.Cliente = New Logica.Cliente(ipAddress, usuarioInfo, frmChat, contraseña, Not clientMode)

    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub BtnEnviar_Click(sender As Object, e As EventArgs) Handles BtnEnviar.Click
        If TxtEntrada.Text.Trim().Length = 0 Then Return
        Cliente.EnviarMSG(TxtEntrada.Text)
        TxtEntrada.Text = ""
    End Sub

    Private Sub TxtEntrada_TextChanged(sender As Object, e As EventArgs) Handles TxtEntrada.TextChanged
        BtnEnviar.Enabled = TxtEntrada.Text.Trim().Length > 0
    End Sub

    Private Sub LtbChat_DrawItem(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MeasureItemEventArgs) Handles LtbChat.MeasureItem
        Dim textSize = TextRenderer.MeasureText(LtbChat.Items(e.Index).Texto, LtbChat.Font).Width + 20
        If textSize > LtbChat.HorizontalExtent Then
            LtbChat.HorizontalExtent = textSize
        End If
    End Sub

    Private Sub LtbChat_DrawItem(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DrawItemEventArgs) Handles LtbChat.DrawItem
        e.DrawBackground()

        If e.Index = -1 Then Return
        Dim customColor = New SolidBrush(LtbChat.Items(e.Index).Color)
        e.Graphics.DrawString(LtbChat.Items(e.Index).Texto, e.Font, customColor, e.Bounds.X, e.Bounds.Y)

        e.DrawFocusRectangle()
    End Sub

    Private Sub LtbUsuarios_DrawItem(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DrawItemEventArgs) Handles LtbUsuarios.DrawItem
        e.DrawBackground()

        If e.Index = -1 Then Return
        Dim customColor = New SolidBrush(LtbUsuarios.Items(e.Index).Color)
        e.Graphics.DrawString(LtbUsuarios.Items(e.Index).Texto, e.Font, customColor, e.Bounds.X, e.Bounds.Y)

        e.DrawFocusRectangle()
    End Sub

    Private Sub CheckedListBox1_ItemCheck(ByVal sender As Object, ByVal e As EventArgs) Handles LtbChat.SelectedIndexChanged
        LtbChat.SelectedIndex = -1
    End Sub

    Public Sub AgregarUsuario(u As Logica.Usuario)
        If u.Conectado Then
            Dim item As New ListBoxItem
            item.Texto = u.Nombre
            item.Color = u.Color
            LtbUsuarios.Items.Add(item)
            LtbUsuarios.TopIndex = LtbChat.Items.Count - 1
        End If
    End Sub

    Public Sub AgregarUsuarios(usuarios As List(Of Logica.Usuario))
        LtbUsuarios.Items.Clear()
        For Each u In usuarios
            AgregarUsuario(u)
        Next
    End Sub

    Public Sub AgregarMensajes(usuarioNombre As String(), mensaje As String(), hora As Date(), c As Color())
        Dim items(usuarioNombre.Length - 1) As ListBoxItem
        For i = 0 To usuarioNombre.Length - 1
            items(i) = NewListBoxItem(usuarioNombre(i), mensaje(i), hora(i), c(i))
        Next
        LtbChat.Items.AddRange(items)
        LtbChat.TopIndex = LtbChat.Items.Count - 1
    End Sub
    Public Sub AgregarMensaje(usuarioNombre As String, mensaje As String, hora As Date, c As Color)
        Dim item As ListBoxItem = NewListBoxItem(usuarioNombre, mensaje, hora, c)
        LtbChat.Items.Add(item)
        LtbChat.TopIndex = LtbChat.Items.Count - 1
    End Sub

    Private Function NewListBoxItem(usuarioNombre As String, mensaje As String, hora As Date, c As Color) As ListBoxItem
        Dim salida = usuarioNombre & " (" & FormatDateTime(hora, DateFormat.ShortTime) & "): " & mensaje
        Dim item As New ListBoxItem
        item.Texto = salida
        item.Color = c
        Return item
    End Function

    Public Sub LimpiarMensajes()
        LtbChat.Items.Clear()
    End Sub

    Private Sub BtnCambiar_Click(sender As Object, e As EventArgs) Handles BtnCambiar.Click
        Dim frm As New FrmCambiarNombre(UsuarioInfo.Nombre, UsuarioInfo.Color)
        frm.ShowDialog()
        If frm.Cancelado Then
            Return
        End If

        LblNombre.Text = frm.NewNombre
        LblNombre.ForeColor = frm.NewColor

        UsuarioInfo.Nombre = frm.NewNombre
        UsuarioInfo.Color = frm.NewColor
        Cliente.EnviarCHGNAME(UsuarioInfo.Nombre, UsuarioInfo.Color)
    End Sub

    Private Sub BtnDesconectar_Click(sender As Object, e As EventArgs) Handles BtnDesconectar.Click
        Me.Close()
    End Sub

    Private Sub FrmChat_Close(sender As Object, e As EventArgs) Handles Me.Closed
        Cliente.Terminate()
        If Not IsNothing(Server) Then
            Server.Terminate()
        End If
        FrmMain.Show()
    End Sub

End Class