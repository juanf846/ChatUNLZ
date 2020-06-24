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

        Dim Input As String
        Input = InputBox("Ingrese el puerto para el servidor (por defecto = 10846)")
        If Input = "" Then
            Return
        End If

        Dim Puerto As Integer
        If Not Integer.TryParse(Input, Puerto) Then
            MsgBox("El puerto no es valido")
            Return
        End If
        If Puerto < 1024 Or Puerto > 65536 Then
            MsgBox("El puerto no es valido")
            Return
        End If

        Dim usuario As New Usuario
        usuario.Nombre = nombre

        Dim chat As New FrmChat(False, New Net.IPEndPoint(Net.IPAddress.Any, Puerto), usuario)
        chat.Show()

    End Sub

    Private Sub BtnUnirse_Click(sender As Object, e As EventArgs) Handles BtnUnirse.Click
        Dim nombre As String = ""
        If Not VerificarNombre(nombre) Then
            Return
        End If

        Dim Ip As String
        Ip = InputBox("Ingrese la IP del servidor")
        If Ip = "" Then
            Return
        End If


        Dim usuario As New Usuario
        usuario.Nombre = nombre

        Dim IPServer As New Net.IPEndPoint(Net.IPAddress.Parse(Ip), 10846)
        Console.WriteLine("Conectando a " & IPServer.ToString & " | " & Ip)

        Dim chat As New FrmChat(True, IPServer, usuario)
        chat.Show()
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

End Class
