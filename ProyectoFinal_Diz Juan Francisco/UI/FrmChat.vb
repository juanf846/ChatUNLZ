Imports System.Net

Public Class FrmChat
    Private UsuarioInfo As Usuario
    Private ClientMode As Boolean
    Private Cliente As Logica.Cliente
    Private Server As Logica.Server

    Public Shared Sub Open(clientMode As Boolean, ipAddress As IPEndPoint, usuarioInfo As Usuario, contraseña As String)
        Dim frmChat As New FrmChat
        frmChat.ClientMode = clientMode
        frmChat.UsuarioInfo = usuarioInfo
        frmChat.LblNombre.Text = usuarioInfo.Nombre
        frmChat.LblNombre.ForeColor = usuarioInfo.Color

        If Not clientMode Then
            frmChat.Server = New Logica.Server(ipAddress.Port, contraseña)
            If Not frmChat.Server.Funcionando Then
                Return
            End If
            ipAddress = New IPEndPoint(Net.IPAddress.Loopback, ipAddress.Port)
        End If

        frmChat.Cliente = New Logica.Cliente(ipAddress, usuarioInfo, frmChat, contraseña, Not clientMode)
        frmChat.Show()

    End Sub

    Public Sub New()
        InitializeComponent()
        LtbChat.DrawMode = DrawMode.OwnerDrawFixed
    End Sub

    Private Sub BtnEnviar_Click(sender As Object, e As EventArgs) Handles BtnEnviar.Click
        Cliente.EnviarMSG(TxtEntrada.Text)
        TxtEntrada.Text = ""
    End Sub

    Private Sub FrmChat_Close(sender As Object, e As EventArgs) Handles Me.Closed
        Cliente.Terminate()
        If Not IsNothing(Server) Then
            Server.Terminate()
        End If
    End Sub

    Private Sub LtbChat_DrawItem(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DrawItemEventArgs) Handles LtbChat.DrawItem
        e.DrawBackground()

        Dim customColor = New SolidBrush(LtbChat.Items(e.Index).Color)
        e.Graphics.DrawString(LtbChat.Items(e.Index).Texto, e.Font, customColor, e.Bounds.X, e.Bounds.Y)

        e.DrawFocusRectangle()
    End Sub

    Public Sub AgregarUsuario(u As Usuario)
        If u.Conectado Then
            LtbUsuarios.Items.Add(u.Nombre)
            LtbUsuarios.TopIndex = LtbUsuarios.Items.Count - 1
        End If
    End Sub

    Public Sub AgregarUsuarios(usuarios As List(Of Usuario))
        LtbUsuarios.Items.Clear()
        For Each u In usuarios
            AgregarUsuario(u)
        Next
    End Sub

    Public Class ListBoxItem
        Public Texto As String
        Public Color As Color
    End Class

    Public Sub AgregarMensaje(usuarioNombre As String, mensaje As String, hora As Date, c As Color)
        Dim salida = usuarioNombre & " (" & FormatDateTime(hora, DateFormat.ShortTime) & "): " & mensaje
        Dim item As New ListBoxItem
        item.Texto = salida
        item.Color = c
        LtbChat.Items.Add(item)
        LtbChat.TopIndex = LtbChat.Items.Count - 1
    End Sub

    Public Sub LimpiarMensajes()
        LtbChat.Items.Clear()
    End Sub

    Private Sub BtnCambiar_Click(sender As Object, e As EventArgs) Handles BtnCambiar.Click
        UsuarioInfo.Nombre = "test"
        UsuarioInfo.Color = Color.Red
        Cliente.EnviarCHGNAME("test", Color.Red)
    End Sub

    Private Sub BtnDesconectar_Click(sender As Object, e As EventArgs) Handles BtnDesconectar.Click
        Me.Close()
    End Sub
End Class