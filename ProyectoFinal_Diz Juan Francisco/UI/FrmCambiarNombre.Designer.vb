<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmCambiarNombre
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TxtNombre = New System.Windows.Forms.TextBox()
        Me.BtnColor = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.BtnCancelar = New System.Windows.Forms.Button()
        Me.BtnAceptar = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(47, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Nombre:"
        '
        'TxtNombre
        '
        Me.TxtNombre.Location = New System.Drawing.Point(65, 13)
        Me.TxtNombre.Name = "TxtNombre"
        Me.TxtNombre.Size = New System.Drawing.Size(191, 20)
        Me.TxtNombre.TabIndex = 1
        '
        'BtnColor
        '
        Me.BtnColor.Location = New System.Drawing.Point(65, 40)
        Me.BtnColor.Name = "BtnColor"
        Me.BtnColor.Size = New System.Drawing.Size(25, 25)
        Me.BtnColor.TabIndex = 2
        Me.BtnColor.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(25, 46)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(34, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Color:"
        '
        'BtnCancelar
        '
        Me.BtnCancelar.Location = New System.Drawing.Point(181, 79)
        Me.BtnCancelar.Name = "BtnCancelar"
        Me.BtnCancelar.Size = New System.Drawing.Size(75, 23)
        Me.BtnCancelar.TabIndex = 4
        Me.BtnCancelar.Text = "Cancelar"
        Me.BtnCancelar.UseVisualStyleBackColor = True
        '
        'BtnAceptar
        '
        Me.BtnAceptar.Location = New System.Drawing.Point(100, 79)
        Me.BtnAceptar.Name = "BtnAceptar"
        Me.BtnAceptar.Size = New System.Drawing.Size(75, 23)
        Me.BtnAceptar.TabIndex = 5
        Me.BtnAceptar.Text = "Aceptar"
        Me.BtnAceptar.UseVisualStyleBackColor = True
        '
        'FrmCambiarNombre
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(268, 114)
        Me.Controls.Add(Me.BtnAceptar)
        Me.Controls.Add(Me.BtnCancelar)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.BtnColor)
        Me.Controls.Add(Me.TxtNombre)
        Me.Controls.Add(Me.Label1)
        Me.Name = "FrmCambiarNombre"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Cambiar nombre"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TxtNombre As TextBox
    Friend WithEvents BtnColor As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents BtnCancelar As Button
    Friend WithEvents BtnAceptar As Button
End Class
