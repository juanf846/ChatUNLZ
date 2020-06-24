Imports System.Net
Imports ProyectoFinal_Diz_Juan_Francisco.UDP

Namespace Logica
    Public Class Cliente

        Private Escuchador As UDP.EscuchadorUDP
        Private Mensajes As New List(Of Mensaje)
        Private Usuarios As New List(Of Usuario)
        Private UsuarioLocal As Usuario

        Private IPServidor As IPEndPoint
        Private Conectado As Boolean = False

        Private FrmChatActivo As FrmChat


        Public Sub New(ip As IPEndPoint, usuarioLocal As Usuario, frmChat As FrmChat)
            Me.UsuarioLocal = usuarioLocal
            Me.FrmChatActivo = frmChat
            Me.IPServidor = ip

            Escuchador = New UDP.EscuchadorUDP()
            Escuchador.Iniciar(0, Nothing)
            Escuchador.OnNewMessage = AddressOf OnNewMessage

            Dim newMensajeDataINFO As New MensajeData(MensajeData.Tipos.INFO)
            Dim m As Action(Of UDP.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensajeRecibido As UDP.MensajeData, idRespuesta As Long, IPRespuesta As IPEndPoint)
                    EnviarCONNECT(mensajeRecibido.Parametros(1), ip)
                End Sub

            Escuchador.EnviarMensaje(ip, newMensajeDataINFO, m, True)

        End Sub

        Private Sub EnviarCONNECT(requiereContraseña As Boolean, ip As IPEndPoint)
            If requiereContraseña Then
                Dim contraseña = InputBox("Ingrese la contraseña del servidor")
                If contraseña = "" Then
                    FrmChatActivo.Close()
                    Return
                End If
                Escuchador.SetClaveCifrado(contraseña)
            End If

            Dim newCONNECT As New MensajeData(MensajeData.Tipos.CONNECT)
            newCONNECT.Parametros = {UsuarioLocal, Escuchador.GetReceiverPort()}


            Escuchador.EnviarMensaje(ip, newCONNECT,
                Sub(ByVal mensajeRecibido As UDP.MensajeData, idRespuesta As Long, IPRespuesta As IPEndPoint)
                    If mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        UsuarioLocal.ServerId = CInt(mensajeRecibido.Parametros(0))
                        Conectado = True
                        EnviarALLUSR()
                        EnviarALLMSG()
                        Console.WriteLine("Cliente conectado correctamente con el id: " & UsuarioLocal.ServerId)
                    ElseIf mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                            mensajeRecibido.Parametros(0) = MensajeData.TiposError.BADPASS Then
                        MsgBox("La contraseña ingresada no es valida")
                        EnviarCONNECT(True, ip)
                    End If
                End Sub, True)
        End Sub

        Public Sub EnviarALLMSG()
            Escuchador.EnviarMensaje(IPServidor, New MensajeData(MensajeData.Tipos.ALLMSG, {UsuarioLocal.ServerId}),
                    Sub(ByVal mensaje As UDP.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                        If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                            FrmChatActivo.LimpiarMensajes()
                            Mensajes.Clear()
                            For Each m In mensaje.Parametros(0)
                                AgregarMensajeAlChat(m, True)
                            Next
                        End If
                    End Sub, True)
        End Sub
        Public Sub EnviarALLUSR()
            Escuchador.EnviarMensaje(IPServidor, New MensajeData(MensajeData.Tipos.ALLUSR, {UsuarioLocal.ServerId}),
                    Sub(ByVal mensaje As UDP.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                        If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                            Usuarios = mensaje.Parametros(0)
                            FrmChatActivo.AgregarUsuarios(Usuarios)
                        End If
                    End Sub, True)
        End Sub


        Public Sub EnviarMSG(texto As String)
            Dim msg As New UDP.MensajeData
            msg.Tipo = UDP.MensajeData.Tipos.MSG
            Dim msgContenido As New Mensaje
            msgContenido.Hora = DateTime.Now
            msgContenido.UsuarioId = UsuarioLocal.ServerId
            msgContenido.Contenido = texto
            msg.Parametros = New Object() {msgContenido}

            Dim method As Action(Of UDP.MensajeData, Long, IPEndPoint) = Sub(ByVal mensaje As UDP.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                                                                             If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                                                                                 Console.WriteLine("(Cliente) El mensaje se envió correctamente")
                                                                             End If
                                                                         End Sub
            Console.WriteLine(IPServidor.ToString)
            Escuchador.EnviarMensaje(IPServidor, msg, method, True)
        End Sub

        Public Sub EnviarCHGNAME(nombre As String, color As Color)
            UsuarioLocal.Nombre = nombre
            UsuarioLocal.Color = color
            Dim method As Action(Of UDP.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensaje As UDP.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                    If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        Console.WriteLine("(Cliente) El nombre cambio correctamente")
                    End If
                End Sub

            Dim mensajeCHGNAME = New MensajeData(
                MensajeData.Tipos.CHGNAME, {UsuarioLocal.ServerId, UsuarioLocal.Nombre, UsuarioLocal.Color})

            Escuchador.EnviarMensaje(IPServidor, mensajeCHGNAME, method, True)
        End Sub

        Public Sub OnNewMessage(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            Select Case mensajeData.Tipo
                Case MensajeData.Tipos.NEWMSG
                    OnReceiveNEWMSG(mensajeData, idMensajeResponse, ip)
                Case MensajeData.Tipos.NEWUSR
                    OnReceiveNEWUSR(mensajeData, idMensajeResponse, ip)
                Case Else
                    Console.WriteLine("Cliente: se recibio un mensaje de tipo no implementado: " & mensajeData.Tipo.ToString)
            End Select
        End Sub

        Public Sub OnReceiveNEWMSG(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            Dim mensaje As Mensaje = mensajeData.Parametros(0)
            AgregarMensajeAlChat(mensaje, True)
            'Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False)
        End Sub

        Public Sub OnReceiveNEWUSR(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            Dim tipo As String = mensajeData.Parametros(0)
            Dim newUsuario As Usuario = mensajeData.Parametros(1)
            If tipo = "ADD" Then
                Usuarios.Add(newUsuario)
                FrmChatActivo.AgregarUsuario(newUsuario)

            ElseIf tipo = "CHANGE" Then
                Dim index As Integer = -1
                For i = 0 To Usuarios.Count - 1
                    If newUsuario.ServerId = Usuarios(i).ServerId Then
                        index = i
                    End If
                Next
                Usuarios(index) = newUsuario

                FrmChatActivo.AgregarUsuarios(Usuarios)
                SyncLock Mensajes
                    FrmChatActivo.LimpiarMensajes()
                    For Each m In Mensajes
                        AgregarMensajeAlChat(m, False)
                    Next
                End SyncLock

                If index = -1 Then
                    Console.Error.WriteLine("Cliente: usuario no encontrado " & newUsuario.ServerId)
                    EnviarALLUSR()
                End If
            Else
                Console.Error.WriteLine("Cliente: tipo de NEWUSR no valido " & tipo)
            End If

        End Sub

        Private Sub AgregarMensajeAlChat(mensaje As Mensaje, agregarAMensajes As Boolean)
            Dim nombreUsuario As String = ""
            Dim colorUsuario As Color

            For Each u In Usuarios
                If u.ServerId = mensaje.UsuarioId Then
                    nombreUsuario = u.Nombre
                    colorUsuario = u.Color
                End If
            Next

            If mensaje.UsuarioId = 0 Then
                nombreUsuario = "Server"
                colorUsuario = Color.Black
            End If

            FrmChatActivo.AgregarMensaje(nombreUsuario, mensaje.Contenido, mensaje.Hora, colorUsuario)
            If agregarAMensajes Then
                Mensajes.Add(mensaje)
            End If
        End Sub

        Public Sub Terminate()
            If Conectado Then
                Dim newMensajeDataDISCONNECT As New MensajeData(MensajeData.Tipos.DISCONNECT, {UsuarioLocal.ServerId})
                Escuchador.EnviarMensaje(IPServidor, newMensajeDataDISCONNECT, Nothing, False)
                Threading.Thread.Sleep(1000)
            End If
            Escuchador.Terminate()
        End Sub

    End Class
End Namespace
