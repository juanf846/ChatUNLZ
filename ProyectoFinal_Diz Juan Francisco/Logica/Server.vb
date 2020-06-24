﻿Imports System.Net
Imports ProyectoFinal_Diz_Juan_Francisco.UDP

Namespace Logica
    Public Class Server
        Public USUARIO_SERVER As Usuario()

        Private nextServerId = 1

        Dim Escuchador As UDP.EscuchadorUDP
        Dim Mensajes As New List(Of Mensaje)
        Dim Usuarios As New List(Of Usuario)

        Public Sub New(puerto As Integer)
            Escuchador = New UDP.EscuchadorUDP()
            Escuchador.Iniciar(puerto, Nothing)
            Escuchador.OnNewMessage = AddressOf OnNewMessage
        End Sub

        Public Sub AgregarMensaje(newMensaje As Mensaje)
            Mensajes.Add(newMensaje)

            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWMSG, {newMensaje}))
        End Sub

        Public Sub EnviarATodos(newMensajeData As MensajeData)
            For Each u In Usuarios
                Escuchador.EnviarMensaje(u.IPRecibir, newMensajeData, Nothing, False)
            Next
        End Sub

        Public Sub OnNewMessage(mensaje As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            Select Case mensaje.Tipo
                Case MensajeData.Tipos.CONNECT
                    OnReceiveCONNECT(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.DISCONNECT
                    OnReceiveDISCONNECT(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.ALLMSG
                    OnReceiveALLMSG(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.ALLUSR
                    OnReceiveALLUSR(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.MSG
                    OnReceiveMSG(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.CHGNAME
                    onReceiveCHGNAME(mensaje, idMensajeResponse, ip)
                Case Else
                    Console.WriteLine("Server: se recibio un mensaje de tipo no implementado: " & mensaje.Tipo.ToString)
            End Select
        End Sub


        Public Sub OnReceiveCONNECT(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim newUsuario As Usuario = mensajeData.Parametros(0)
            newUsuario.ServerId = nextServerId
            nextServerId += 1
            newUsuario.IPEnviar = ip
            newUsuario.IPRecibir = New IPEndPoint(ip.Address, mensajeData.Parametros(1))
            Usuarios.Add(newUsuario)

            ' Envia el OK al nuevo usuario
            Dim mensajeDataOK As New MensajeData(MensajeData.Tipos.ESTADO_OK)
            mensajeDataOK.Parametros = New Object() {newUsuario.ServerId}
            Escuchador.EnviarMensaje(newUsuario.IPRecibir, mensajeDataOK, Nothing, False, idMensajeRespuesta)

            ' Envia la info del nuevo usuario a los demas
            Dim newMensajeDataNEWUSR As New MensajeData(MensajeData.Tipos.NEWUSR)
            newMensajeDataNEWUSR.Parametros = New Object() {"ADD", newUsuario}
            EnviarATodos(newMensajeDataNEWUSR)

            ' Envia el mensaje de nueva conexion
            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "Se conecto el usuario " & newUsuario.Nombre & " | debug: IP = " &
                                    ip.Address.ToString & " puerto = " & ip.Port
            AgregarMensaje(newMensaje)

        End Sub

        Public Sub OnReceiveDISCONNECT(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim idRecibido As Integer = mensajeData.Parametros(0)
            Dim usuarioFound As Usuario = Nothing
            Dim index As Integer = -1

            For i = 0 To Usuarios.Count - 1
                If Usuarios(i).ServerId = idRecibido Then
                    index = i
                End If
            Next
            If index = -1 Then
                Console.Error.WriteLine("Server: DISCONNECT de un usuario no valido: ID = " & idRecibido)
                Return
            End If

            Usuarios(index).Conectado = False
            usuarioFound = Usuarios(index)

            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "Se desconecto el usuario " & usuarioFound.Nombre
            AgregarMensaje(newMensaje)


            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWUSR, {"CHANGE", usuarioFound}))
        End Sub

        Public Sub OnReceiveALLMSG(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            'TODO implementar seguridad para que solo lo puedan pedir usuarios logueados
            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK, {Mensajes}), Nothing, False, idMensajeRespuesta)
        End Sub

        Public Sub OnReceiveALLUSR(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            'TODO implementar seguridad para que solo lo puedan pedir usuarios logueados
            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK, {Usuarios}), Nothing, False, idMensajeRespuesta)
        End Sub

        Public Sub OnReceiveMSG(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim msgContenido As Mensaje = mensajeData.Parametros(0)
            Dim encontrado As Boolean = False
            For Each u In Usuarios
                If msgContenido.UsuarioId = u.ServerId Then
                    encontrado = True
                End If
            Next
            If encontrado Then
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK),
                                         Nothing, False, idMensajeRespuesta)
                AgregarMensaje(msgContenido)
            Else
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR),
                                         Nothing, False, idMensajeRespuesta)
                Console.WriteLine("Se recibió un mensaje de un usuario invalido")
            End If
        End Sub

        Public Sub OnReceiveCHGNAME(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim idRecibido As Integer = mensajeData.Parametros(0)
            Dim usuarioFound As Usuario
            Dim index As Integer = -1

            For i = 0 To Usuarios.Count - 1
                If Usuarios(i).ServerId = idRecibido Then
                    index = i
                End If
            Next
            If index = -1 Then
                Console.Error.WriteLine("Server: CHGNAME de un usuario no valido: ID = " & idRecibido)
                ' TODO usuario no valido enviar mensaje
                Return
            End If
            Dim oldName = Usuarios(index).Nombre
            Usuarios(index).Nombre = mensajeData.Parametros(1)
            Usuarios(index).Color = mensajeData.Parametros(2)
            usuarioFound = Usuarios(index)

            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "El usuario " & oldName & " cambió de nombre a " & usuarioFound.Nombre
            AgregarMensaje(newMensaje)


            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWUSR, {"CHANGE", usuarioFound}))
        End Sub

        Public Sub Terminate()
            EnviarATodos(New MensajeData(MensajeData.Tipos.CLOSED, {}))
            Threading.Thread.Sleep(1000)
            Escuchador.Terminate()
        End Sub
    End Class

End Namespace