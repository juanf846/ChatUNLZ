Imports System.Net

Public Class FrmChat
    Private UsuarioInfo As Usuario
    Private ClientMode As Boolean
    Private Cliente As Logica.Cliente
    Private Server As Logica.Server


    Public Sub New(clientMode As Boolean, ipAddress As IPEndPoint, usuarioInfo As Usuario)
        InitializeComponent()
        Me.ClientMode = clientMode
        Me.UsuarioInfo = usuarioInfo
        LblNombre.Text = usuarioInfo.Nombre

        If Not clientMode Then
            Server = New Logica.Server(ipAddress.Port)
            ipAddress = New IPEndPoint(Net.IPAddress.Loopback, ipAddress.Port)
        End If


        Cliente = New Logica.Cliente(ipAddress, usuarioInfo, Me)


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

    Public Sub AgregarUsuario(u As Usuario)
        If u.Conectado Then
            LtbUsuarios.Items.Add(u.Nombre)
            LtbUsuarios.TopIndex = LtbChat.Items.Count - 1
        End If
    End Sub

    Public Sub AgregarUsuarios(usuarios As List(Of Usuario))
        LtbUsuarios.Items.Clear()
        For Each u In usuarios
            AgregarUsuario(u)
        Next
    End Sub

    Public Sub AgregarMensaje(usuarioNombre As String, mensaje As String, hora As Date, c As Color)
        Dim salida = usuarioNombre & " (" & FormatDateTime(hora, DateFormat.ShortTime) & "): " & mensaje

        LtbChat.Items.Add(salida)
        LtbChat.TopIndex = LtbChat.Items.Count - 1
    End Sub

    Public Sub LimpiarMensajes()
        LtbChat.Items.Clear()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UsuarioInfo.Nombre = "test"
        UsuarioInfo.Color = Color.Red
        Cliente.EnviarCHGNAME("test", Color.Red)
    End Sub
End Class