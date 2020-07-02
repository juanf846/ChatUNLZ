﻿Public Class FrmMain
    Public Sub New()

        ' Esta llamada es exigida por el diseñador.
        InitializeComponent()

        ' Agregue cualquier inicialización después de la llamada a InitializeComponent().

        Control.CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub BtnCrear_Click(sender As Object, e As EventArgs) Handles BtnCrear.Click
        Dim nombre As String = ""
        If Not VerificarNombre(nombre) Then
            Return
        End If

        Dim puerto As Integer
        Dim contraseña As String

        If Not FrmCrearServer.Mostrar(puerto, contraseña) Then
            Return
        End If

        Dim usuario As New Usuario
        usuario.Nombre = nombre
        usuario.Color = BtnColor.BackColor

        FrmChat.Open(False, New Net.IPEndPoint(Net.IPAddress.Any, puerto), usuario, contraseña)


    End Sub

    Private Sub BtnUnirse_Click(sender As Object, e As EventArgs) Handles BtnUnirse.Click
        Dim nombre As String = ""
        If Not VerificarNombre(nombre) Then
            Return
        End If

        Dim IPServer As Net.IPEndPoint = Nothing
        If Not FrmConectarServer.Mostrar(IPServer) Then
            Return
        End If

        Dim usuario As New Usuario
        usuario.Nombre = nombre
        usuario.Color = BtnColor.BackColor

        Console.WriteLine("Conectando a " & IPServer.ToString)

        FrmChat.Open(True, IPServer, usuario, Nothing)
    End Sub

    Private Sub FrmMain_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Application.Exit()
    End Sub

    Public Function VerificarNombre(ByRef nombre As String) As Boolean
        nombre = TxtNombre.Text.Trim(" ")
        If nombre.Length = 0 Or nombre.Length > 32 Then
            MsgBox("Ingrese un nombre entre 1 y 32 caracteres")
            Return False
        End If
        Return True
    End Function

    Private Sub BtnColor_Click(sender As Object, e As EventArgs) Handles BtnColor.Click
        Dim colorPicker As New ColorDialog
        colorPicker.Color = BtnColor.BackColor
        colorPicker.ShowDialog()
        BtnColor.BackColor = colorPicker.Color
    End Sub

End Class
