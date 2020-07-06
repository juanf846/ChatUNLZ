Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Runtime.Serialization
Imports System.Security
Imports ProyectoFinal_Diz_Juan_Francisco.Logica

Namespace UDP
    Public Module Debug
        Public nextIdEscuchador As Integer = 0
        Public Sub log(id As Integer, msg As String)
            Console.WriteLine("Escuchador " & id & " : " & msg)
        End Sub
    End Module
    Public Class EscuchadorUDP
        Public Structure MensajeParaEnviar
            Public Sub New(bytes As Byte(), endPoint As IPEndPoint)
                Me.bytes = bytes
                Me.endPoint = endPoint
            End Sub
            Public bytes() As Byte
            Public endPoint As IPEndPoint
        End Structure


        Private EscuchadorID As Integer

        Private Const VERSION As Integer = 5
        Private Formatter As New Formatters.Binary.BinaryFormatter()
        Private CLIENT_RECEIVE_TIMEOUT As Integer = 100               'Valor en milisegundos  (0.1 segundos) (1 seg = 1000 milis)
        Private MENSAJE_RECEIVE_TIMEOUT As Integer = 20000000         'Valor en ticks         (2 segundos)   (1 seg = 10000000 tick)
        Private MENSAJE_REINTENTOS_MAXIMOS As Integer = 3

        Private Puerto As Integer
        Private ThreadMain As Thread
        Private Client As UdpClient
        Public OnNewMessage As Action(Of Logica.MensajeData, Long, IPEndPoint)
        Private Cifrador As CifradorAES

        Private Ejecutandose As Boolean = True
        Private UsaCifrado As Boolean = False

        Private nextIdMensaje As Integer = 0
        Private MensajesSinRespuesta As New List(Of MensajeUDP)
        Private MensajesParaEnviar As New List(Of MensajeParaEnviar)


        Public Sub Iniciar(puerto As Integer, claveCifrado As String)
            EscuchadorID = Debug.nextIdEscuchador
            Debug.nextIdEscuchador += 1
            Me.Puerto = puerto
            If puerto = 0 Then
                Client = New UdpClient(0)
            Else
                Client = New UdpClient(puerto)
            End If
            Client.Client.ReceiveTimeout = CLIENT_RECEIVE_TIMEOUT
            Debug.log(EscuchadorID, "Iniciado en puerto " & puerto)

            SetClaveCifrado(claveCifrado)

            ThreadMain = New Thread(AddressOf Escuchador)
            ThreadMain.Start()

        End Sub

        Private Sub Escuchador()
            While Ejecutandose
                'espera mensaje
                Try
                    Dim receiveEndPoint As New IPEndPoint(IPAddress.Any, 0)
                    Dim Input() As Byte = Client.Receive(receiveEndPoint)
                    Debug.log(EscuchadorID, "Recibido")
                    Dim stream As New IO.MemoryStream
                    stream.Write(Input, 0, Input.Length)
                    stream.Position = 0

                    'decodifica
                    Dim mensajeUDP As MensajeUDP = Formatter.Deserialize(stream)

                    If mensajeUDP.Cifrado And Not UsaCifrado Then
                        Throw New SecurityException("Se recibió un mensaje cifrado en un escuchador que no usa cifrado")
                    End If

                    If mensajeUDP.Cifrado Then
                        Try
                            Dim streamCifrado = mensajeUDP.Contenido
                            Dim streamNoCifrado = New IO.MemoryStream(Cifrador.Descifrar(streamCifrado))
                            streamNoCifrado.Position = 0

                            mensajeUDP.Contenido = Formatter.Deserialize(streamNoCifrado)
                        Catch e As Exception
                            Dim mensajeDataLocal = New MensajeData(MensajeData.Tipos.ESTADO_ERROR, {MensajeData.TiposError.BADPASS})
                            EnviarMensaje(receiveEndPoint, mensajeDataLocal, Nothing, False, mensajeUDP.IdMensaje, True)
                            Throw New SecurityException("No se pudo deserializar el mensaje porque hubo un problema en el cifrado")
                        End Try
                    End If

                    If UsaCifrado AndAlso Not mensajeUDP.Cifrado AndAlso mensajeUDP.Contenido.Tipo <> MensajeData.Tipos.INFO AndAlso
                        mensajeUDP.Contenido.Tipo <> MensajeData.Tipos.ESTADO_ERROR Then

                        Throw New SecurityException("Se recibió un mensaje no cifrado en un escuchador que usa cifrado")
                    End If


                    'Si es una respuesta, busca el primer mensaje
                    SyncLock MensajesSinRespuesta
                        Debug.log(EscuchadorID, "Actualmente hay " & MensajesSinRespuesta.Count & " mensajes sin respuesta")
                        For Each msg In MensajesSinRespuesta
                            If msg Is Nothing Then
                                Continue For
                            End If
                            If mensajeUDP.IdMensaje = msg.IdMensaje Then
                                Debug.log(EscuchadorID, "Nuevo mensaje, listener = mensaje")
                                If msg.OnResponse IsNot Nothing Then
                                    msg.OnResponse.Invoke(mensajeUDP.Contenido, mensajeUDP.IdMensaje, mensajeUDP.EndPoint)
                                End If
                                MensajesSinRespuesta.Remove(msg)
                                Continue While
                            End If
                        Next
                    End SyncLock

                    'Si no es una respuesta, se lo envia al creador del escuchador
                    If mensajeUDP.Contenido.Tipo = MensajeData.Tipos.INFO Then
                        Debug.log(EscuchadorID, "Nuevo mensaje, listener = INFO")
                        OnNewINFO(mensajeUDP.Contenido, mensajeUDP.IdMensaje, receiveEndPoint)
                    ElseIf Not IsNothing(OnNewMessage) Then
                        Debug.log(EscuchadorID, "Nuevo mensaje, listener = general")
                        OnNewMessage.Invoke(mensajeUDP.Contenido, mensajeUDP.IdMensaje, receiveEndPoint)
                    Else
                        Debug.log(EscuchadorID, "Nuevo mensaje, listener = null")
                    End If

                Catch e As SerializationException
                    Debug.log(EscuchadorID, "Nuevo mensaje, error al deserializar")
                Catch e As SocketException
                    If e.ErrorCode <> 10060 Then
                        Debug.log(EscuchadorID, "Nuevo mensaje, error: " & e.Message & " | error code: " & e.ErrorCode)
                    End If
                Catch e As SecurityException
                    Debug.log(EscuchadorID, e.Message)
                End Try

                'envia todos los mensajes que estan esperando
                SyncLock MensajesParaEnviar
                    For Each msg In MensajesParaEnviar
                        Try
                            Client.Send(msg.bytes, msg.bytes.Length, msg.endPoint)
                        Catch e As Exception
                            Debug.log(EscuchadorID, "Error al enviar mensaje: " & e.Message)
                        End Try
                    Next
                    MensajesParaEnviar.Clear()
                End SyncLock
                SyncLock MensajesSinRespuesta
                    Dim mensajesAEliminar As New List(Of MensajeUDP)
                    For i = 0 To MensajesSinRespuesta.Count - 1
                        Dim msg = MensajesSinRespuesta(i)
                        Dim ticksActuales = DateTime.Now.Ticks
                        If msg.TicksParaReenviar < ticksActuales Then
                            If msg.ReintentosRestantes = 0 Then
                                Debug.log(EscuchadorID, "No se pudo enviar el mensaje " & msg.IdMensaje & " despues de " &
                                          MENSAJE_REINTENTOS_MAXIMOS & " intentos")
                                mensajesAEliminar.Add(msg)

                                Dim newMensajeData As New Logica.MensajeData(Logica.MensajeData.Tipos.ESTADO_ERROR)
                                newMensajeData.Parametros = {Logica.MensajeData.TiposError.LOSTCONECTION}

                                msg.OnResponse.Invoke(newMensajeData, msg.IdMensaje, msg.EndPoint)
                            Else
                                Dim stream As New IO.MemoryStream
                                Formatter.Serialize(stream, msg)
                                stream.Position = 0

                                msg.ReintentosRestantes -= 1
                                msg.TicksParaReenviar = ticksActuales + MENSAJE_RECEIVE_TIMEOUT
                                Debug.log(EscuchadorID, "Reenviando el mensaje " & msg.IdMensaje & " intento " & msg.ReintentosRestantes)

                                Dim bytes() As Byte = stream.ToArray()
                                Try
                                    Client.Send(bytes, bytes.Length, msg.EndPoint)
                                Catch e As Exception
                                    Debug.log(EscuchadorID, "Error al reenviar mensaje: " & e.Message)
                                End Try
                            End If
                        End If
                    Next
                    For Each msg In mensajesAEliminar
                        MensajesSinRespuesta.Remove(msg)
                    Next
                End SyncLock
            End While

            'Liberar recursos
            Client.Close()

        End Sub

        Public Sub EnviarMensaje(ByRef remoteEndPoint As IPEndPoint, ByRef mensaje As MensajeData,
                                 ByRef onResponse As Action(Of MensajeData, Long, IPEndPoint),
                                 requireResponse As Boolean)
            Dim ts As TimeSpan = DateTime.Now - New DateTime(1970, 1, 1)
            Dim i As Long = Convert.ToInt64(ts.TotalMilliseconds)
            i += Convert.ToInt64(nextIdMensaje) << 32

            nextIdMensaje += 1
            EnviarMensaje(remoteEndPoint, mensaje, onResponse, requireResponse, i)
        End Sub

        Public Sub EnviarMensaje(ByRef remoteEndPoint As IPEndPoint, ByRef mensaje As MensajeData,
                                  ByRef onResponse As Action(Of MensajeData, Long, IPEndPoint),
                                  requireResponse As Boolean, idMensaje As Long)
            EnviarMensaje(remoteEndPoint, mensaje, onResponse, requireResponse, idMensaje, False)
        End Sub

        Public Sub EnviarMensaje(ByRef remoteEndPoint As IPEndPoint, ByRef mensaje As MensajeData,
                                  ByRef onResponse As Action(Of MensajeData, Long, IPEndPoint),
                                  requireResponse As Boolean, idMensaje As Long, forzarNoCifrado As Boolean)
            Dim mensajeUDP As New MensajeUDP
            mensajeUDP.ServerVersion = VERSION
            mensajeUDP.IdMensaje = idMensaje
            mensajeUDP.Contenido = mensaje

            Debug.log(EscuchadorID, "Enviando mensaje a " & remoteEndPoint.ToString & " | ID de mensaje: " & mensajeUDP.IdMensaje &
                              " | Tipo de mensaje: " & mensajeUDP.Contenido.Tipo.ToString())

            If Not mensaje.Tipo = MensajeData.Tipos.INFO And UsaCifrado And Not forzarNoCifrado Then
                Dim streamNoCifrado As New IO.MemoryStream
                Formatter.Serialize(streamNoCifrado, mensajeUDP.Contenido)
                streamNoCifrado.Position = 0

                mensajeUDP.Contenido = Cifrador.Cifrar(streamNoCifrado.ToArray())
                mensajeUDP.Cifrado = True
            End If

            'Agregar al array
            If requireResponse Then
                mensajeUDP.ReintentosRestantes = MENSAJE_REINTENTOS_MAXIMOS
                mensajeUDP.TicksParaReenviar = DateTime.Now.Ticks + MENSAJE_RECEIVE_TIMEOUT
                mensajeUDP.OnResponse = onResponse
                mensajeUDP.EndPoint = remoteEndPoint
                SyncLock MensajesSinRespuesta
                    MensajesSinRespuesta.Add(mensajeUDP)
                End SyncLock
                Debug.log(EscuchadorID, "Mensaje agregado a no respondidos: " & idMensaje)
            End If

            Dim stream As New IO.MemoryStream
            Formatter.Serialize(stream, mensajeUDP)
            stream.Position = 0

            Dim bytes() As Byte = stream.ToArray()


            MensajesParaEnviar.Add(New MensajeParaEnviar(bytes, remoteEndPoint))
        End Sub

        Private Sub OnNewINFO(mensajeData As Logica.MensajeData, idMensajeRespuesta As Long, remoteEndPoint As IPEndPoint)
            Dim newMensajeData As New Logica.MensajeData(Logica.MensajeData.Tipos.ESTADO_OK)
            newMensajeData.Parametros = {VERSION, UsaCifrado}

            EnviarMensaje(remoteEndPoint, newMensajeData, Nothing, False, idMensajeRespuesta, True)
        End Sub

        Private Sub OnBADPASS(idMensajeRespuesta As Long, remoteEndPoint As IPEndPoint)
            Dim newMensajeData As New Logica.MensajeData(Logica.MensajeData.Tipos.ESTADO_ERROR)
            newMensajeData.Parametros = {Logica.MensajeData.TiposError.BADPASS}

            EnviarMensaje(remoteEndPoint, newMensajeData, Nothing, False, idMensajeRespuesta, True)
        End Sub

        Public Function GetReceiverPort()
            Return CType(Client.Client.LocalEndPoint, IPEndPoint).Port
        End Function

        Public Sub SetClaveCifrado(claveCifrado As String)
            If Not IsNothing(claveCifrado) Then
                UsaCifrado = True
                Cifrador = New CifradorAES(claveCifrado)
            Else
                UsaCifrado = False
            End If
        End Sub

        Public Sub Terminate()
            Ejecutandose = False
        End Sub
    End Class
End Namespace
