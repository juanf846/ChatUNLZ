Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Runtime.Serialization
Imports System.Security

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

        Private Const VERSION As Integer = 2
        Private Formatter As New Formatters.Binary.BinaryFormatter()

        Private Puerto As Integer
        Private ThreadMain As Thread
        Private Client As UdpClient
        Public OnNewMessage As Action(Of UDP.MensajeData, Long, IPEndPoint)
        Private Ejecutandose As Boolean = True
        Private Cifrado As Boolean = False
        Private ClaveCifrado As String

        Private nextIdMensaje As Integer = 0
        Private MensajesSinRespuesta As New List(Of MensajeUDP)
        Private MensajesParaEnviar As New List(Of MensajeParaEnviar)


        Public Sub Iniciar(puerto As Integer, claveCifrado As Object)
            EscuchadorID = Debug.nextIdEscuchador
            Debug.nextIdEscuchador += 1
            Me.ClaveCifrado = claveCifrado
            Me.Puerto = puerto
            If puerto = 0 Then
                Client = New UdpClient(0)
            Else
                Client = New UdpClient(puerto)
            End If
            Client.Client.ReceiveTimeout = 100
            Debug.log(EscuchadorID, "Iniciado en puerto " & puerto)

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
                    stream.Write(input, 0, input.Length)
                    stream.Position = 0

                    'decodifica
                    Dim mensajeUDP As MensajeUDP = Formatter.Deserialize(stream)
                    If mensajeUDP.Cifrado Then
                        'Descifrar
                        'TODO descifrar
                    End If
                    Debug.log(EscuchadorID, "Nuevo mensaje, tipo = " & mensajeUDP.Contenido.Tipo.ToString())

                    If Cifrado AndAlso Not mensajeUDP.Cifrado AndAlso mensajeUDP.Contenido.Tipo <> MensajeData.Tipos.INFO Then
                        Throw New SecurityException("Se recibió un mensaje no cifrado en un servidor que usa cifrado")
                    End If


                    Debug.log(EscuchadorID, "Actualmente hay " & MensajesSinRespuesta.Count & " mensajes sin respuesta")
                    'Si es una respuesta, busca el primer mensaje
                    For Each msg In MensajesSinRespuesta
                        If msg Is Nothing Then
                            Continue For
                        End If
                        If mensajeUDP.IdMensaje = msg.IdMensaje Then
                            Debug.log(EscuchadorID, "Nuevo mensaje, listener = mensaje")
                            If msg.OnResponse IsNot Nothing Then
                                msg.OnResponse.Invoke(mensajeUDP.Contenido, mensajeUDP.IdMensaje, mensajeUDP.IPResponse)
                            End If
                            MensajesSinRespuesta.Remove(msg)
                            Continue While
                        End If
                    Next

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
            Dim mensajeUDP As New MensajeUDP
            mensajeUDP.ServerVersion = VERSION
            mensajeUDP.IdMensaje = idMensaje
            mensajeUDP.Contenido = mensaje

            'Agregar al array
            If requireResponse Then
                mensajeUDP.OnResponse = onResponse
                mensajeUDP.IPResponse = remoteEndPoint
                MensajesSinRespuesta.Add(mensajeUDP)
                Debug.log(EscuchadorID, "Mensaje agregado a no respondidos: " & idMensaje)
            End If

            Dim stream As New IO.MemoryStream
            Formatter.Serialize(stream, mensajeUDP)
            stream.Position = 0

            Dim bytes() As Byte = stream.ToArray()
            Debug.log(EscuchadorID, "Enviando mensaje a " & remoteEndPoint.ToString & " | ID de mensaje: " & mensajeUDP.IdMensaje &
                              " | Tipo de mensaje: " & mensajeUDP.Contenido.Tipo.ToString())

            If Cifrado Then
                'Cifrar()
                'Todo Cifrado
            End If

            MensajesParaEnviar.Add(New MensajeParaEnviar(bytes, remoteEndPoint))
        End Sub

        Private Sub OnNewINFO(mensajeData As UDP.MensajeData, idMensajeRespuesta As Long, remoteEndPoint As IPEndPoint)
            Dim newMensajeData As New UDP.MensajeData(UDP.MensajeData.Tipos.ESTADO_OK)
            newMensajeData.Parametros = {VERSION, Cifrado}

            EnviarMensaje(remoteEndPoint, newMensajeData, Nothing, False, idMensajeRespuesta)
        End Sub

        Public Function GetReceiverPort()
            Return CType(Client.Client.LocalEndPoint, IPEndPoint).Port
        End Function

        Public Sub SetClaveCifrado(claveCifrado As String)
            Me.ClaveCifrado = claveCifrado

        End Sub

        Public Sub Terminate()
            Ejecutandose = False
        End Sub
    End Class
End Namespace
