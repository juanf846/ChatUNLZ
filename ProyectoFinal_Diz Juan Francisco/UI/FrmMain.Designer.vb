<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.TxtNombre = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.BtnCrear = New System.Windows.Forms.Button()
        Me.BtnUnirse = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.BtnColor = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'TxtNombre
        '
        Me.TxtNombre.Location = New System.Drawing.Point(12, 25)
        Me.TxtNombre.Name = "TxtNombre"
        Me.TxtNombre.Size = New System.Drawing.Size(194, 20)
        Me.TxtNombre.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(61, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(96, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Nombre de usuario"
        '
        'BtnCrear
        '
        Me.BtnCrear.Location = New System.Drawing.Point(39, 95)
        Me.BtnCrear.Name = "BtnCrear"
        Me.BtnCrear.Size = New System.Drawing.Size(140, 23)
        Me.BtnCrear.TabIndex = 2
        Me.BtnCrear.Text = "Crear servidor"
        Me.BtnCrear.UseVisualStyleBackColor = True
        '
        'BtnUnirse
        '
        Me.BtnUnirse.Location = New System.Drawing.Point(39, 124)
        Me.BtnUnirse.Name = "BtnUnirse"
        Me.BtnUnirse.Size = New System.Drawing.Size(140, 23)
        Me.BtnUnirse.TabIndex = 3
        Me.BtnUnirse.Text = "Unirse a un servidor"
        Me.BtnUnirse.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(94, 50)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(31, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Color"
        '
        'BtnColor
        '
        Me.BtnColor.BackColor = System.Drawing.Color.Red
        Me.BtnColor.Location = New System.Drawing.Point(97, 66)
        Me.BtnColor.Name = "BtnColor"
        Me.BtnColor.Size = New System.Drawing.Size(25, 23)
        Me.BtnColor.TabIndex = 5
        Me.BtnColor.UseVisualStyleBackColor = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 150)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(39, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Label3"
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(218, 191)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.BtnColor)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.BtnUnirse)
        Me.Controls.Add(Me.BtnCrear)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TxtNombre)
        Me.Name = "FrmMain"
        Me.Text = "UNLZ chat"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TxtNombre As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents BtnCrear As Button
    Friend WithEvents BtnUnirse As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents BtnColor As Button
    Friend WithEvents Label3 As Label
End Class
