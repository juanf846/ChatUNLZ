Imports System.Net

Namespace Logica
    Public Class Server
        Public USUARIO_SERVER As Usuario()

        Private nextServerId = 1

        Dim Escuchador As UDP.EscuchadorUDP
        Public Funcionando = False
        Dim Mensajes As New List(Of Mensaje)
        Dim Usuarios As New List(Of Usuario)

        Public Sub New(puerto As Integer, contraseña As String)
            Try
                Escuchador = New UDP.EscuchadorUDP()
                Escuchador.OnNewMessage = AddressOf OnNewMessage
                Escuchador.Iniciar(puerto, contraseña)
                Funcionando = True
            Catch e As Net.Sockets.SocketException
                Dim causa = ""
                If e.ErrorCode = 10048 Then
                    causa = "que el puerto ya está en uso"
                Else
                    causa = " un error desconocido"
                End If
                MsgBox("No se pudo crear el servidor por" & causa & " (error: " & e.ErrorCode & ")")
                Terminate()
            End Try
        End Sub

        Public Sub AgregarMensaje(newMensaje As Mensaje)
            Mensajes.Add(newMensaje)

            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWMSG, {newMensaje}))
        End Sub

        Public Sub EnviarATodos(newMensajeData As MensajeData)
            For Each u In Usuarios
                If Not u.Conectado Then
                    Continue For
                End If
                Dim m As Action(Of Logica.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensajeRecibido As Logica.MensajeData, idRespuesta As Long, IPRespuesta As IPEndPoint)
                    If mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_ERROR Then
                        Dim tipoError As MensajeData.TiposError
                        Try
                            tipoError = mensajeRecibido.Parametros(0)
                        Catch e As InvalidCastException
                            Console.WriteLine("Server: EnviarATodos - error al castear: " & e.Message)
                            Return
                        End Try
                        If tipoError = MensajeData.TiposError.LOSTCONECTION Then
                            EnviarClienteTimeout(u.ServerId)
                        Else
                            Console.WriteLine("Server: EnviarATodos - error desconocido: " & tipoError.ToString())
                        End If
                    End If
                End Sub

                Escuchador.EnviarMensaje(u.EndPoint, newMensajeData, m, True)
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
                    OnReceiveCHGNAME(mensaje, idMensajeResponse, ip)
                Case MensajeData.Tipos.BEAN
                    OnReceiveBEAN(mensaje, idMensajeResponse, ip)
                Case Else
                    Console.WriteLine("Server: se recibio un mensaje de tipo no implementado: " & mensaje.Tipo.ToString)
            End Select
        End Sub


        Public Sub OnReceiveCONNECT(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim newUsuario As Usuario
            Try
                newUsuario = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: CONNECT - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            Dim newID As Integer = ((DateTime.Now.Ticks / 10000000) +
                (nextServerId << 16)) Mod Integer.MaxValue

            newUsuario.ServerId = newID
            nextServerId += 1
            newUsuario.EndPoint = ip
            Usuarios.Add(newUsuario)

            ' Envia el OK al nuevo usuario
            Dim mensajeDataOK As New MensajeData(MensajeData.Tipos.ESTADO_OK)
            mensajeDataOK.Parametros = New Object() {newUsuario.ServerId}
            Escuchador.EnviarMensaje(newUsuario.EndPoint, mensajeDataOK, Nothing, False, idMensajeRespuesta)

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
            Dim idRecibido As Integer
            Try
                idRecibido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: DISCONNECT - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            Dim usuarioFound As Usuario = Nothing

            If Not VerificarSeguridad("DISCONNECT", idRecibido, ip, usuarioFound) Then
                Return
            End If

            usuarioFound.Conectado = False

            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "Se desconecto el usuario " & usuarioFound.Nombre
            AgregarMensaje(newMensaje)


            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWUSR, {"CHANGE", usuarioFound}))
        End Sub

        Public Sub OnReceiveALLMSG(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim idRecibido As Integer
            Try
                idRecibido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: ALLMSG - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try
            If Not VerificarSeguridad("ALLMSG", idRecibido, ip, Nothing) Then
                Return
            End If
            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK, {Mensajes}), Nothing, False, idMensajeRespuesta)
        End Sub

        Public Sub OnReceiveALLUSR(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim idRecibido As Integer
            Try
                idRecibido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: ALLUSR - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            If Not VerificarSeguridad("ALLUSR", idRecibido, ip, Nothing) Then
                Return
            End If
            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK, {Usuarios}), Nothing, False, idMensajeRespuesta)
        End Sub

        Public Sub OnReceiveMSG(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim msgContenido As Mensaje
            Try
                msgContenido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: MSG - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            Dim usuarioFound As Usuario = Nothing
            If Not VerificarSeguridad("MSG", msgContenido.UsuarioId, ip, usuarioFound) Then
                ' TODO Mejorar, enviar error especifico
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR),
                                         Nothing, False, idMensajeRespuesta)
                Return
            End If

            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK),
                                         Nothing, False, idMensajeRespuesta)
            AgregarMensaje(msgContenido)
        End Sub

        Public Sub OnReceiveCHGNAME(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)

            Dim idRecibido As Integer
            Try
                idRecibido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: CHGNAME - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            Dim usuarioFound As Usuario = Nothing

            If Not VerificarSeguridad("CHGNAME", idRecibido, ip, usuarioFound) Then
                Return
            End If

            Dim oldName = usuarioFound.Nombre
            Dim newName As String
            Dim newColor As Color
            Try
                newName = mensajeData.Parametros(1)
                newColor = mensajeData.Parametros(2)
            Catch e As InvalidCastException
                Console.WriteLine("Server: CHGNAME - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            usuarioFound.Nombre = newName
            usuarioFound.Color = newColor

            Escuchador.EnviarMensaje(usuarioFound.EndPoint, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False, idMensajeRespuesta)

            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "El usuario " & oldName & " cambió de nombre a " & usuarioFound.Nombre
            AgregarMensaje(newMensaje)

            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWUSR, {"CHANGE", usuarioFound}))
        End Sub

        Public Sub OnReceiveBEAN(mensajeData As MensajeData, idMensajeRespuesta As Long, ip As IPEndPoint)
            Dim idRecibido As Integer
            Try
                idRecibido = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Server: BEAN - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeRespuesta)
                Return
            End Try

            Dim usuarioFound As Usuario = Nothing
            If Not VerificarSeguridad("BEAN", idRecibido, ip, usuarioFound) Then
                Return
            End If

            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False, idMensajeRespuesta)

        End Sub

        Private Function VerificarSeguridad(ByRef debugTag As String, ByRef idRecibido As Integer, ByRef ip As IPEndPoint, ByRef usuarioFound As Usuario) As Boolean
            usuarioFound = Usuario.GetUserById(idRecibido, Usuarios)

            If IsNothing(usuarioFound) Then
                Console.Error.WriteLine("Server: {0} de un usuario no valido: ID = {1}", debugTag, idRecibido)
                Return False
            End If

            If usuarioFound.Conectado = False Then
                Console.Error.WriteLine("Server: {0} de un usuario ya desconectado: ID = {1}", debugTag, idRecibido)
                Return False
            End If

            If Not usuarioFound.EndPoint.Address.Equals(ip.Address) OrElse Not usuarioFound.EndPoint.Port = ip.Port Then
                Console.Error.WriteLine("Server: {0} de un usuario desde un EndPoint diferente: ID = {1}", debugTag, idRecibido)
                Return False
            End If

            Return True
        End Function

        Public Sub EnviarClienteTimeout(idUsuario As Integer)
            Dim usuarioFound As Usuario = Usuario.GetUserById(idUsuario, Usuarios)

            usuarioFound.Conectado = False

            Dim newMensaje As New Mensaje
            newMensaje.UsuarioId = 0
            newMensaje.Hora = DateTime.Now
            newMensaje.Contenido = "El usuario " & usuarioFound.Nombre & " perdió la conexión."
            AgregarMensaje(newMensaje)

            EnviarATodos(New MensajeData(MensajeData.Tipos.NEWUSR, {"CHANGE", usuarioFound}))
        End Sub

        Public Sub Terminate()
            Funcionando = False
            EnviarATodos(New MensajeData(MensajeData.Tipos.CLOSED, {}))
            Threading.Thread.Sleep(1000)
            Escuchador.Terminate()
        End Sub
    End Class

End Namespace