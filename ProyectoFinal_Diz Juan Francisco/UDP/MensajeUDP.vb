Imports System.Net
Imports System.Runtime.Serialization

Namespace UDP
    <Serializable()>
    Public Class MensajeUDP
        Public ServerVersion As Integer
        Public IdMensaje As Long
        ''' <summary>
        ''' Si la conexion es cifrada, se debe enviar un byte array
        ''' Si la conexion no es cifrada, se debe enviar un MensajeData
        ''' </summary>
        Public Contenido As MensajeData
        Public Cifrado As Boolean
        <NonSerialized>
        Public Enviado As Date
        ''' <summary>
        ''' * Mensaje es el recibido
        ''' * ID para enviar el mensaje de respuesta
        ''' * IP a la que enviar la respuesta
        ''' </summary>
        <NonSerialized>
        Public OnResponse As Action(Of MensajeData, Long, IPEndPoint)
        <NonSerialized>
        Public IPResponse As IPEndPoint

    End Class
End Namespace

