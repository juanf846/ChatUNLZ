Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Public Class CifradorAES
    Private Const tamañoClave As Integer = 256
    Private Const salt As String = "ChatUNLZ creado por JFD"

    Private Algoritmo As RijndaelManaged

    Public Sub New(clave As String)
        Dim keyBuilder As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(salt, Encoding.Unicode.GetBytes(clave))
        Algoritmo = New RijndaelManaged()
        Algoritmo.KeySize = tamañoClave
        Algoritmo.IV = keyBuilder.GetBytes(CType(Algoritmo.BlockSize / 8, Integer))
        Algoritmo.Key = keyBuilder.GetBytes(CType(Algoritmo.KeySize / 8, Integer))
        Algoritmo.Padding = PaddingMode.PKCS7
    End Sub

    Public Function Cifrar(input As Byte()) As Byte()
        Dim outStream As New MemoryStream
        Dim cryptoStream As New CryptoStream(outStream, Algoritmo.CreateEncryptor(), CryptoStreamMode.Write)
        cryptoStream.Write(input, 0, input.Count)
        cryptoStream.FlushFinalBlock()
        Return outStream.ToArray()
    End Function
    Public Function Descifrar(input As Byte()) As Byte()
        Try
            Dim inStream As New MemoryStream(input)
            Dim cryptoStream As New CryptoStream(inStream, Algoritmo.CreateDecryptor(), CryptoStreamMode.Read)
            Dim output(inStream.Length - 1) As Byte
            cryptoStream.Read(output, 0, output.Count)
            Return output
        Catch e As Exception
            Throw e
        End Try
    End Function

End Class
