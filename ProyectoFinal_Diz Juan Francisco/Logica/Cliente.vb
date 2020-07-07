Imports System.Net
Imports ProyectoFinal_Diz_Juan_Francisco.Logica

Namespace Logica
    Public Class Cliente

        Private Escuchador As UDP.EscuchadorUDP
        Private BEANSenderActivo As BEANSender
        Private Mensajes As New List(Of Mensaje)
        Private Usuarios As New List(Of Usuario)
        Private UsuarioLocal As Usuario

        Private IPServidor As IPEndPoint
        Private Conectado As Boolean = False
        Private Terminando As Boolean = False

        Private FrmChatActivo As FrmChat
        Private FrmCargandoActivo As FrmCargando


        Public Sub New(ip As IPEndPoint, usuarioLocal As Usuario, frmChat As FrmChat, contraseña As String, conexionLocal As Boolean)
            Me.UsuarioLocal = usuarioLocal
            Me.FrmChatActivo = frmChat
            Me.IPServidor = ip

            Escuchador = New UDP.EscuchadorUDP()
            Escuchador.Iniciar(0, Nothing)
            Escuchador.OnNewMessage = AddressOf OnNewMessage

            FrmCargandoActivo = New FrmCargando()
            FrmChatActivo.Enabled = False
            FrmCargandoActivo.Show(FrmChatActivo)


            If Not conexionLocal Then
                Dim newMensajeDataINFO As New MensajeData(MensajeData.Tipos.INFO)
                Dim m As Action(Of Logica.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensajeRecibido As Logica.MensajeData, idRespuesta As Long, IPRespuesta As IPEndPoint)
                    If Not VerificarSeguridad("EnviarINFO", IPRespuesta) Then
                        Return
                    End If
                    If mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        EnviarCONNECT(mensajeRecibido.Parametros(1), ip)
                    ElseIf mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                            mensajeRecibido.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                        FrmCargandoActivo.ForzarCierre = True
                        MsgBox("No se pudo conectar al servidor")
                        FrmCargandoActivo.Close()
                        Terminate()
                    Else
                        FrmCargandoActivo.ForzarCierre = True
                        MsgBox("No se pudo conectar al servidor")
                        FrmCargandoActivo.Close()
                        Terminate()

                        Dim causa = ""
                        Try
                            causa = CType(mensajeRecibido.Parametros(0), MensajeData.TiposError).ToString
                        Catch ex As Exception
                            causa = "No se pudo castear el tipo de error (" & mensajeRecibido.Parametros(0) & ")"
                        End Try
                        Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                    End If

                End Sub

                Escuchador.EnviarMensaje(ip, newMensajeDataINFO, m, True)
            Else
                If Not IsNothing(contraseña) Then
                    Escuchador.SetClaveCifrado(contraseña)
                End If
                EnviarCONNECT(False, ip)
            End If

        End Sub

        Private Sub EnviarCONNECT(requiereContraseña As Boolean, ip As IPEndPoint)
            If requiereContraseña Then
                Dim contraseña As String
                Do
                    contraseña = InputBox("Ingrese la contraseña del servidor")
                    If contraseña = "" Then
                        Terminate()
                        Return
                    End If
                    If contraseña.Length < 8 Then
                        MsgBox("Ingrese una contraseña de 8 caracteres o mas")
                    End If
                Loop While contraseña.Length < 8
                Escuchador.SetClaveCifrado(contraseña)
            End If

            Dim newCONNECT As New MensajeData(MensajeData.Tipos.CONNECT)
            newCONNECT.Parametros = {UsuarioLocal, Escuchador.GetReceiverPort()}


            Escuchador.EnviarMensaje(ip, newCONNECT,
                Sub(ByVal mensajeRecibido As Logica.MensajeData, idRespuesta As Long, IPRespuesta As IPEndPoint)
                    If Not VerificarSeguridad("EnviarCONNECT", IPRespuesta) Then
                        Return
                    End If
                    If mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        UsuarioLocal.ServerId = CInt(mensajeRecibido.Parametros(0))
                        Conectado = True
                        FrmCargandoActivo.ForzarCierre = True
                        FrmCargandoActivo.Close()
                        FrmChatActivo.Enabled = True
                        FrmChatActivo.WindowState = FormWindowState.Normal
                        FrmChatActivo.TopMost = True
                        FrmChatActivo.TopMost = False
                        BEANSenderActivo = New BEANSender(Escuchador, AddressOf OnLostConection, IPServidor, UsuarioLocal.ServerId)
                        BEANSenderActivo.Iniciar()
                        EnviarALLUSR()
                        EnviarALLMSG()
                        Console.WriteLine("Cliente conectado correctamente con el id: " & UsuarioLocal.ServerId)
                    ElseIf mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                            mensajeRecibido.Parametros(0) = MensajeData.TiposError.BADPASS Then
                        MsgBox("La contraseña ingresada no es valida")
                        EnviarCONNECT(True, ip)
                    ElseIf mensajeRecibido.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                            mensajeRecibido.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                        FrmCargandoActivo.ForzarCierre = True
                        MsgBox("No se pudo conectar al servidor")
                        FrmCargandoActivo.Close()
                        Terminate()
                    Else
                        FrmCargandoActivo.ForzarCierre = True
                        MsgBox("No se pudo conectar al servidor")
                        FrmCargandoActivo.Close()
                        Terminate()
                        Dim causa = ""
                        Try
                            causa = CType(mensajeRecibido.Parametros(0), MensajeData.TiposError).ToString
                        Catch ex As Exception
                            causa = "No se pudo castear el tipo de error (" & mensajeRecibido.Parametros(0) & ")"
                        End Try
                        Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                    End If
                End Sub, True)
        End Sub

        Public Sub EnviarALLMSG()
            Escuchador.EnviarMensaje(IPServidor, New MensajeData(MensajeData.Tipos.ALLMSG, {UsuarioLocal.ServerId}),
                    Sub(ByVal mensaje As Logica.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                        If Not VerificarSeguridad("EnviarALLMSG", IPResponse) Then
                            Return
                        End If
                        If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                            FrmChatActivo.LimpiarMensajes()
                            Mensajes.Clear()
                            AgregarMensajesAlChat(mensaje.Parametros(0), True)
                        ElseIf mensaje.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                               mensaje.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                            OnLostConection()
                        Else
                            OnLostConection()
                            Dim causa = ""
                            Try
                                causa = CType(mensaje.Parametros(0), MensajeData.TiposError).ToString
                            Catch ex As Exception
                                causa = "No se pudo castear el tipo de error (" & mensaje.Parametros(0) & ")"
                            End Try
                            Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                        End If
                    End Sub, True)
        End Sub
        Public Sub EnviarALLUSR()
            Escuchador.EnviarMensaje(IPServidor, New MensajeData(MensajeData.Tipos.ALLUSR, {UsuarioLocal.ServerId}),
                    Sub(ByVal mensaje As Logica.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                        If Not VerificarSeguridad("EnviarALLUSR", IPResponse) Then
                            Return
                        End If
                        If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                            Usuarios = mensaje.Parametros(0)
                            FrmChatActivo.AgregarUsuarios(Usuarios)
                        ElseIf mensaje.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                               mensaje.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                            OnLostConection()
                        Else
                            OnLostConection()
                            Dim causa = ""
                            Try
                                causa = CType(mensaje.Parametros(0), MensajeData.TiposError).ToString
                            Catch ex As Exception
                                causa = "No se pudo castear el tipo de error (" & mensaje.Parametros(0) & ")"
                            End Try
                            Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                        End If
                    End Sub, True)
        End Sub


        Public Sub EnviarMSG(texto As String)
            Dim msg As New Logica.MensajeData
            msg.Tipo = Logica.MensajeData.Tipos.MSG
            Dim msgContenido As New Mensaje
            msgContenido.Hora = DateTime.Now
            msgContenido.UsuarioId = UsuarioLocal.ServerId
            msgContenido.Contenido = texto
            msg.Parametros = New Object() {msgContenido}

            Dim method As Action(Of Logica.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensaje As Logica.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                    If Not VerificarSeguridad("EnviarMSG", IPResponse) Then
                        Return
                    End If
                    If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        Console.WriteLine("(Cliente) El mensaje se envió correctamente")
                    ElseIf mensaje.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                           mensaje.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                        OnLostConection()
                    Else
                        OnLostConection()
                        Dim causa = ""
                        Try
                            causa = CType(mensaje.Parametros(0), MensajeData.TiposError).ToString
                        Catch ex As Exception
                            causa = "No se pudo castear el tipo de error (" & mensaje.Parametros(0) & ")"
                        End Try
                        Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                    End If
                End Sub
            Console.WriteLine(IPServidor.ToString)
            Escuchador.EnviarMensaje(IPServidor, msg, method, True)
        End Sub

        Public Sub EnviarCHGNAME(nombre As String, color As Color)
            Dim method As Action(Of Logica.MensajeData, Long, IPEndPoint) =
                Sub(ByVal mensaje As Logica.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                    If Not VerificarSeguridad("EnviarCHGNAME", IPResponse) Then
                        Return
                    End If
                    If mensaje.Tipo = MensajeData.Tipos.ESTADO_OK Then
                        UsuarioLocal.Nombre = nombre
                        UsuarioLocal.Color = color
                        Console.WriteLine("(Cliente) El nombre cambio correctamente")
                    ElseIf mensaje.Tipo = MensajeData.Tipos.ESTADO_ERROR AndAlso
                           mensaje.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                        OnLostConection()
                    Else
                        OnLostConection()
                        Dim causa = ""
                        Try
                            causa = CType(mensaje.Parametros(0), MensajeData.TiposError).ToString
                        Catch ex As Exception
                            causa = "No se pudo castear el tipo de error (" & mensaje.Parametros(0) & ")"
                        End Try
                        Console.Error.WriteLine("Cliente: INFO - error desconocido: " + causa)
                    End If
                End Sub

            Dim mensajeCHGNAME = New MensajeData(
                MensajeData.Tipos.CHGNAME, {UsuarioLocal.ServerId, UsuarioLocal.Nombre, UsuarioLocal.Color})

            Escuchador.EnviarMensaje(IPServidor, mensajeCHGNAME, method, True)
        End Sub

        Public Sub OnNewMessage(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            If Terminando Then
                Return
            End If
            Select Case mensajeData.Tipo
                Case MensajeData.Tipos.NEWMSG
                    OnReceiveNEWMSG(mensajeData, idMensajeResponse, ip)
                Case MensajeData.Tipos.NEWUSR
                    OnReceiveNEWUSR(mensajeData, idMensajeResponse, ip)
                Case MensajeData.Tipos.CLOSED
                    OnReceiveCLOSED(mensajeData, idMensajeResponse, ip)
                Case Else
                    Console.WriteLine("Cliente: se recibio un mensaje de tipo no implementado: " & mensajeData.Tipo.ToString)
            End Select
        End Sub

        Public Sub OnReceiveNEWMSG(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            If Not VerificarSeguridad("NEWMSG", ip) Then
                Return
            End If
            Dim mensaje As Mensaje
            Try
                mensaje = mensajeData.Parametros(0)
            Catch e As InvalidCastException
                Console.WriteLine("Cliente: NEWMSG - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeResponse)
                Return
            End Try

            AgregarMensajeAlChat(mensaje, True)
            Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False, idMensajeResponse)
        End Sub

        Public Sub OnReceiveNEWUSR(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            If Not VerificarSeguridad("NEWUSR", ip) Then
                Return
            End If
            Dim tipo As String
            Dim newUsuario As Usuario
            Try
                tipo = mensajeData.Parametros(0)
                newUsuario = mensajeData.Parametros(1)
            Catch e As InvalidCastException
                Console.WriteLine("Cliente: NEWUSR - error al castear: " & e.Message)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeResponse)
                Return
            End Try
            If tipo = "ADD" Then
                Usuarios.Add(newUsuario)
                FrmChatActivo.AgregarUsuario(newUsuario)

                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False, idMensajeResponse)
            ElseIf tipo = "CHANGE" Then
                Dim index As Integer = -1
                For i = 0 To Usuarios.Count - 1
                    If newUsuario.ServerId = Usuarios(i).ServerId Then
                        index = i
                    End If
                Next
                If index = -1 Then
                    Console.Error.WriteLine("Cliente: usuario no encontrado " & newUsuario.ServerId)
                    EnviarALLUSR()
                    Return
                End If
                Usuarios(index) = newUsuario

                FrmChatActivo.AgregarUsuarios(Usuarios)
                SyncLock Mensajes
                    FrmChatActivo.LimpiarMensajes()
                    AgregarMensajesAlChat(Mensajes, False)
                End SyncLock

                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_OK), Nothing, False, idMensajeResponse)
            Else
                Console.Error.WriteLine("Cliente: tipo de NEWUSR no valido " & tipo)
                Escuchador.EnviarMensaje(ip, New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPROTOCOL}), Nothing, False, idMensajeResponse)
            End If
        End Sub

        Public Sub OnReceiveCLOSED(mensajeData As MensajeData, idMensajeResponse As Long, ip As IPEndPoint)
            If Not VerificarSeguridad("CLOSED", ip) Then
                Return
            End If
            MsgBox("El servidor se cerró")
            Terminate()
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

        Private Sub AgregarMensajesAlChat(mensajes As List(Of Mensaje), agregarAMensajes As Boolean)
            Dim nombreUsuario(mensajes.Count - 1) As String
            Dim contenido(mensajes.Count - 1) As String
            Dim hora(mensajes.Count - 1) As Date
            Dim colorUsuario(mensajes.Count - 1) As Color

            For i = 0 To mensajes.Count - 1
                Dim mensaje As Mensaje = mensajes(i)
                For Each u In Usuarios
                    If u.ServerId = mensaje.UsuarioId Then
                        nombreUsuario(i) = u.Nombre
                        colorUsuario(i) = u.Color
                    End If
                Next

                If mensaje.UsuarioId = 0 Then
                    nombreUsuario(i) = "Server"
                    colorUsuario(i) = Color.Black
                End If
                contenido(i) = mensaje.Contenido
                hora(i) = mensaje.Hora
            Next

            FrmChatActivo.AgregarMensajes(nombreUsuario, contenido, hora, colorUsuario)
            If agregarAMensajes Then
                Me.Mensajes.AddRange(mensajes)
            End If
        End Sub

        Private Function VerificarSeguridad(ByRef debugTag As String, ByRef ip As IPEndPoint) As Boolean
            If IsNothing(ip) Then Return True
            If Not IPServidor.Address.Equals(ip.Address) OrElse Not IPServidor.Port = ip.Port Then
                Console.Error.WriteLine("Cliente: {0} del servidor desde un EndPoint diferente", debugTag)
                Return False
            End If
            Return True
        End Function

        Private Sub OnLostConection()
            MsgBox("Se perdió la conexión con el servidor")
            Terminate()
        End Sub

        Public Sub Terminate()
            If Not Terminando Then
                Terminando = True
                If Not IsNothing(BEANSenderActivo) Then BEANSenderActivo.Terminate()
                FrmChatActivo.Close()
                If Conectado Then
                    Dim newMensajeDataDISCONNECT As New MensajeData(MensajeData.Tipos.DISCONNECT, {UsuarioLocal.ServerId})
                    Escuchador.EnviarMensaje(IPServidor, newMensajeDataDISCONNECT, Nothing, False)
                    Threading.Thread.Sleep(500)
                End If
                Escuchador.Terminate()
            End If
        End Sub

        Public Class BEANSender
            Private Thread As Threading.Thread
            Private Continuar = True
            Private Escuchador As UDP.EscuchadorUDP
            Private LostConnection As Action
            Private IPServer As IPEndPoint
            Private IdUsuario As Integer


            Public Sub New(ByRef escuchador As UDP.EscuchadorUDP, ByRef onLostConnection As Action, ipServer As IPEndPoint, idUsuario As Integer)
                Me.Escuchador = escuchador
                Me.LostConnection = onLostConnection
                Me.IPServer = ipServer
                Me.IdUsuario = idUsuario
            End Sub

            Public Sub Iniciar()
                Thread = New Threading.Thread(AddressOf Sender)
                Thread.Start()
            End Sub

            Private Sub Sender()
                While Continuar
                    Escuchador.EnviarMensaje(IPServer, New MensajeData(MensajeData.Tipos.BEAN, {IdUsuario}),
                         Sub(ByVal mensaje As Logica.MensajeData, idMensajeResponse As Long, IPResponse As IPEndPoint)
                             If mensaje.Tipo = MensajeData.Tipos.ESTADO_ERROR Then
                                 If mensaje.Parametros(0) = MensajeData.TiposError.LOSTCONECTION Then
                                     LostConnection.Invoke()
                                     Terminate()
                                 End If
                             End If
                         End Sub, True)
                    If Continuar Then Threading.Thread.Sleep(5000)
                End While
            End Sub

            Public Sub Terminate()
                Continuar = False
            End Sub
        End Class

    End Class
End Namespace
