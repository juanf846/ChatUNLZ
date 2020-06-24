<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmChat
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TxtEntrada = New System.Windows.Forms.TextBox()
        Me.BtnEnviar = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.LblNombre = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.BtnDesconectar = New System.Windows.Forms.Button()
        Me.LtbUsuarios = New System.Windows.Forms.ListBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.LtbChat = New System.Windows.Forms.ListBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'TxtEntrada
        '
        Me.TxtEntrada.Location = New System.Drawing.Point(204, 418)
        Me.TxtEntrada.Name = "TxtEntrada"
        Me.TxtEntrada.Size = New System.Drawing.Size(363, 20)
        Me.TxtEntrada.TabIndex = 1
        '
        'BtnEnviar
        '
        Me.BtnEnviar.Location = New System.Drawing.Point(573, 415)
        Me.BtnEnviar.Name = "BtnEnviar"
        Me.BtnEnviar.Size = New System.Drawing.Size(75, 23)
        Me.BtnEnviar.TabIndex = 2
        Me.BtnEnviar.Text = "Enviar"
        Me.BtnEnviar.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(79, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Nombre actual:"
        '
        'LblNombre
        '
        Me.LblNombre.AutoSize = True
        Me.LblNombre.Location = New System.Drawing.Point(13, 32)
        Me.LblNombre.Name = "LblNombre"
        Me.LblNombre.Size = New System.Drawing.Size(13, 13)
        Me.LblNombre.TabIndex = 4
        Me.LblNombre.Text = "?"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(14, 55)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(31, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Color"
        '
        'BtnDesconectar
        '
        Me.BtnDesconectar.Location = New System.Drawing.Point(33, 416)
        Me.BtnDesconectar.Name = "BtnDesconectar"
        Me.BtnDesconectar.Size = New System.Drawing.Size(127, 23)
        Me.BtnDesconectar.TabIndex = 6
        Me.BtnDesconectar.Text = "Desconectarse"
        Me.BtnDesconectar.UseVisualStyleBackColor = True
        '
        'LtbUsuarios
        '
        Me.LtbUsuarios.FormattingEnabled = True
        Me.LtbUsuarios.Location = New System.Drawing.Point(13, 223)
        Me.LtbUsuarios.Name = "LtbUsuarios"
        Me.LtbUsuarios.Size = New System.Drawing.Size(167, 95)
        Me.LtbUsuarios.TabIndex = 7
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(13, 204)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(110, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Usuarios conectados:"
        '
        'LtbChat
        '
        Me.LtbChat.FormattingEnabled = True
        Me.LtbChat.Location = New System.Drawing.Point(204, 15)
        Me.LtbChat.Name = "LtbChat"
        Me.LtbChat.SelectionMode = System.Windows.Forms.SelectionMode.None
        Me.LtbChat.Size = New System.Drawing.Size(444, 394)
        Me.LtbChat.TabIndex = 9
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(105, 8)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 10
        Me.Button1.Text = "Cambiar"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'FrmChat
        '
        Me.AcceptButton = Me.BtnEnviar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(660, 450)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.LtbChat)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.LtbUsuarios)
        Me.Controls.Add(Me.BtnDesconectar)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.LblNombre)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.BtnEnviar)
        Me.Controls.Add(Me.TxtEntrada)
        Me.Name = "FrmChat"
        Me.Text = "FrmChat"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TxtEntrada As TextBox
    Friend WithEvents BtnEnviar As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents LblNombre As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents BtnDesconectar As Button
    Friend WithEvents LtbUsuarios As ListBox
    Friend WithEvents Label4 As Label
    Friend WithEvents LtbChat As ListBox
    Friend WithEvents Button1 As Button
End Class
