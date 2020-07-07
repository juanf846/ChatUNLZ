Imports System.IO
Imports System.Drawing.Color
Namespace Configuracion
    Module Configuracion
        Private Const RUTA As String = "config.cfg"
        Private NombreValue As String
        Private ColorValue As System.Drawing.Color
        Private IPConexionValue As String
        Private PuertoConexionValue As Integer

        Public Property Nombre() As String
            Get
                Return NombreValue
            End Get
            Set(ByVal value As String)
                NombreValue = value
                GuardarEnArchivo()
            End Set
        End Property
        Public Property Color() As Color
            Get
                Return ColorValue
            End Get
            Set(ByVal value As Color)
                ColorValue = value
                GuardarEnArchivo()
            End Set
        End Property
        Public Property IPConexion() As String
            Get
                Return IPConexionValue
            End Get
            Set(ByVal value As String)
                IPConexionValue = value
                GuardarEnArchivo()
            End Set
        End Property
        Public Property PuertoConexion() As Integer
            Get
                Return PuertoConexionValue
            End Get
            Set(ByVal value As Integer)
                PuertoConexionValue = value
                GuardarEnArchivo()
            End Set
        End Property


        Public Sub Inicializar()
            Dim stream As FileStream = Nothing
            Try
                If Not File.Exists(RUTA) Then
                    Throw New IOException("El archivo no existe")
                End If
                stream = File.OpenRead(RUTA)
                Dim reader As New StreamReader(stream)
                If reader.EndOfStream Then Throw New IOException("El archivo no es valido")
                NombreValue = reader.ReadLine()
                If reader.EndOfStream Then Throw New IOException("El archivo no es valido")
                ColorValue = System.Drawing.Color.FromArgb(Integer.Parse(reader.ReadLine()))
                If reader.EndOfStream Then Throw New IOException("El archivo no es valido")
                IPConexionValue = reader.ReadLine()
                If reader.EndOfStream Then Throw New IOException("El archivo no es valido")
                PuertoConexionValue = Integer.Parse(reader.ReadLine())
                reader.Close()
                stream.Close()
            Catch e As Exception
                Console.Error.WriteLine("Error al leer el archivo de configuracion: " & e.Message)
                If Not IsNothing(stream) Then stream.Close()
                InicializarDefault()
                GuardarEnArchivo()
            End Try
        End Sub

        Private Sub InicializarDefault()
            NombreValue = ""
            Dim ran As New Random
            ColorValue = System.Drawing.Color.FromArgb((ran.Next() And &HFFFFFF) Or &HFF000000)
            IPConexionValue = ""
            PuertoConexionValue = 10846
        End Sub

        Private Sub GuardarEnArchivo()
            Dim stream As FileStream = Nothing
            Try
                stream = File.Create(RUTA)
                Dim writer As New StreamWriter(stream)
                writer.WriteLine(NombreValue)
                writer.WriteLine(ColorValue.ToArgb)
                writer.WriteLine(IPConexionValue)
                writer.WriteLine(PuertoConexion)
                writer.Close()
                stream.Close()
            Catch e As Exception
                Console.Error.WriteLine("Error al escribir el archivo de configuracion: " & e.Message)
                If Not IsNothing(stream) Then stream.Close()
            End Try
        End Sub
    End Module
End Namespace