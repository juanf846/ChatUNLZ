Namespace Logica
    <Serializable()>
    Public Class MensajeData

        <Serializable()>
        Public Enum Tipos
            'estados
            ESTADO_OK
            ESTADO_ERROR

            'cliente a server
            CONNECT
            DISCONNECT
            MSG
            BEAN
            INFO
            ALLUSR
            ALLMSG
            CHGNAME

            'server a cliente
            NEWMSG
            NEWUSR
            CLOSED
        End Enum

        <Serializable()>
        Public Enum TiposError
            BADPASS
            LOSTCONECTION
            BADPROTOCOL
        End Enum

        Public Tipo As Tipos
        Public Parametros() As Object

        Public Sub New()

        End Sub

        Public Sub New(tipo As Tipos)
            Me.Tipo = tipo
        End Sub

        Public Sub New(tipo As Tipos, parametros As Object())
            Me.Tipo = tipo
            Me.Parametros = parametros
        End Sub

    End Class
End Namespace
