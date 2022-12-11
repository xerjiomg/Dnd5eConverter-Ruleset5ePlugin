<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Dnd5eConverter
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Dnd5eConverter))
        Me.btn_load_monster_file = New System.Windows.Forms.Button()
        Me.btn_load_spell = New System.Windows.Forms.Button()
        Me.btn_load_spell_file = New System.Windows.Forms.Button()
        Me.panle_spell = New System.Windows.Forms.Panel()
        Me.label_spells = New System.Windows.Forms.Label()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.btn_paste_spell_file = New System.Windows.Forms.Button()
        Me.textbox_spell_load = New System.Windows.Forms.TextBox()
        Me.PictureBox5 = New System.Windows.Forms.PictureBox()
        Me.PictureBox4 = New System.Windows.Forms.PictureBox()
        Me.panel_bestiary = New System.Windows.Forms.Panel()
        Me.btn_paste_monster_file = New System.Windows.Forms.Button()
        Me.textbox_bestiary_load = New System.Windows.Forms.TextBox()
        Me.label_characters = New System.Windows.Forms.Label()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.PictureBox6 = New System.Windows.Forms.PictureBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.HelpProvider_bertiary_converter = New System.Windows.Forms.HelpProvider()
        Me.HelpProvider_spell_conveter = New System.Windows.Forms.HelpProvider()
        Me.HelpProvider_spell_load = New System.Windows.Forms.HelpProvider()
        Me.HelpProvider_bestiary_paste = New System.Windows.Forms.HelpProvider()
        Me.HelpProvider_spell_paste = New System.Windows.Forms.HelpProvider()
        Me.panle_spell.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panel_bestiary.SuspendLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btn_load_monster_file
        '
        Me.btn_load_monster_file.BackColor = System.Drawing.Color.SteelBlue
        Me.btn_load_monster_file.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_load_monster_file.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btn_load_monster_file.ForeColor = System.Drawing.Color.White
        Me.HelpProvider_bertiary_converter.SetHelpKeyword(Me.btn_load_monster_file, "Characters")
        Me.HelpProvider_bertiary_converter.SetHelpString(Me.btn_load_monster_file, "Button to convert a .Json structured file with a single or multiple monster on it" &
        " to a individual .Dnd5e structures files complatibles as Characters with RuleSet" &
        "5E ")
        Me.btn_load_monster_file.Location = New System.Drawing.Point(18, 93)
        Me.btn_load_monster_file.Name = "btn_load_monster_file"
        Me.HelpProvider_bertiary_converter.SetShowHelp(Me.btn_load_monster_file, True)
        Me.btn_load_monster_file.Size = New System.Drawing.Size(204, 42)
        Me.btn_load_monster_file.TabIndex = 0
        Me.btn_load_monster_file.Text = "CONVERT JSON TO DND5E FILES"
        Me.btn_load_monster_file.UseVisualStyleBackColor = False
        '
        'btn_load_spell
        '
        Me.btn_load_spell.BackColor = System.Drawing.Color.CadetBlue
        Me.btn_load_spell.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_load_spell.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btn_load_spell.ForeColor = System.Drawing.Color.White
        Me.HelpProvider_spell_load.SetHelpKeyword(Me.btn_load_spell, "")
        Me.HelpProvider_spell_load.SetHelpString(Me.btn_load_spell, "Button to load .Spell files to use as spell in the bestiary creation")
        Me.btn_load_spell.Location = New System.Drawing.Point(154, 171)
        Me.btn_load_spell.Name = "btn_load_spell"
        Me.HelpProvider_spell_load.SetShowHelp(Me.btn_load_spell, True)
        Me.btn_load_spell.Size = New System.Drawing.Size(144, 31)
        Me.btn_load_spell.TabIndex = 1
        Me.btn_load_spell.Text = "LOAD SPELL FILES"
        Me.btn_load_spell.UseVisualStyleBackColor = False
        '
        'btn_load_spell_file
        '
        Me.btn_load_spell_file.BackColor = System.Drawing.Color.CadetBlue
        Me.btn_load_spell_file.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_load_spell_file.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btn_load_spell_file.ForeColor = System.Drawing.Color.White
        Me.HelpProvider_spell_conveter.SetHelpString(Me.btn_load_spell_file, "Button to convert a .Json structured file with single or multiple spell into a .S" &
        "pell structurued files compatibles as Rolls in RuleSet5E and load them to use in" &
        " the bestiary creation")
        Me.btn_load_spell_file.Location = New System.Drawing.Point(154, 93)
        Me.btn_load_spell_file.Name = "btn_load_spell_file"
        Me.HelpProvider_spell_conveter.SetShowHelp(Me.btn_load_spell_file, True)
        Me.btn_load_spell_file.Size = New System.Drawing.Size(144, 58)
        Me.btn_load_spell_file.TabIndex = 2
        Me.btn_load_spell_file.Text = "CONVERT JSON TO SPELL FILES AND LOAD THEM"
        Me.btn_load_spell_file.UseVisualStyleBackColor = False
        '
        'panle_spell
        '
        Me.panle_spell.BackColor = System.Drawing.Color.GhostWhite
        Me.panle_spell.Controls.Add(Me.label_spells)
        Me.panle_spell.Controls.Add(Me.PictureBox3)
        Me.panle_spell.Controls.Add(Me.btn_paste_spell_file)
        Me.panle_spell.Controls.Add(Me.textbox_spell_load)
        Me.panle_spell.Controls.Add(Me.btn_load_spell)
        Me.panle_spell.Controls.Add(Me.btn_load_spell_file)
        Me.panle_spell.Controls.Add(Me.PictureBox5)
        Me.panle_spell.Controls.Add(Me.PictureBox4)
        Me.panle_spell.Location = New System.Drawing.Point(257, 12)
        Me.panle_spell.Name = "panle_spell"
        Me.panle_spell.Size = New System.Drawing.Size(310, 322)
        Me.panle_spell.TabIndex = 3
        '
        'label_spells
        '
        Me.label_spells.AutoSize = True
        Me.label_spells.BackColor = System.Drawing.Color.Transparent
        Me.label_spells.Font = New System.Drawing.Font("Comic Sans MS", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.label_spells.ForeColor = System.Drawing.Color.DarkCyan
        Me.label_spells.Location = New System.Drawing.Point(118, 16)
        Me.label_spells.Name = "label_spells"
        Me.label_spells.Size = New System.Drawing.Size(80, 27)
        Me.label_spells.TabIndex = 6
        Me.label_spells.Text = "SPELLS"
        Me.label_spells.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBox3
        '
        Me.PictureBox3.BackgroundImage = CType(resources.GetObject("PictureBox3.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox3.Location = New System.Drawing.Point(44, 5)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(235, 73)
        Me.PictureBox3.TabIndex = 10
        Me.PictureBox3.TabStop = False
        '
        'btn_paste_spell_file
        '
        Me.btn_paste_spell_file.BackColor = System.Drawing.Color.CadetBlue
        Me.btn_paste_spell_file.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_paste_spell_file.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btn_paste_spell_file.ForeColor = System.Drawing.Color.White
        Me.HelpProvider_spell_load.SetHelpKeyword(Me.btn_paste_spell_file, "")
        Me.HelpProvider_spell_load.SetHelpString(Me.btn_paste_spell_file, "")
        Me.HelpProvider_spell_paste.SetHelpString(Me.btn_paste_spell_file, "Button to paste a spell text structure in the clipboard and conver it into a .Spe" &
        "ll structure compatible as Roll in RuleSet5E and load it to use as spell in the " &
        "bestiary creation ")
        Me.btn_paste_spell_file.Location = New System.Drawing.Point(15, 93)
        Me.btn_paste_spell_file.Name = "btn_paste_spell_file"
        Me.HelpProvider_bertiary_converter.SetShowHelp(Me.btn_paste_spell_file, False)
        Me.HelpProvider_spell_load.SetShowHelp(Me.btn_paste_spell_file, False)
        Me.HelpProvider_spell_paste.SetShowHelp(Me.btn_paste_spell_file, True)
        Me.btn_paste_spell_file.Size = New System.Drawing.Size(133, 109)
        Me.btn_paste_spell_file.TabIndex = 9
        Me.btn_paste_spell_file.Text = "PASTE CLIPBOARD TEXT TO CONVERT INTO SPELL FILES AND LOAD THEM"
        Me.btn_paste_spell_file.UseVisualStyleBackColor = False
        '
        'textbox_spell_load
        '
        Me.textbox_spell_load.BackColor = System.Drawing.Color.GhostWhite
        Me.textbox_spell_load.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.textbox_spell_load.Font = New System.Drawing.Font("Segoe Print", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.textbox_spell_load.ForeColor = System.Drawing.Color.DarkCyan
        Me.textbox_spell_load.Location = New System.Drawing.Point(74, 260)
        Me.textbox_spell_load.Multiline = True
        Me.textbox_spell_load.Name = "textbox_spell_load"
        Me.textbox_spell_load.Size = New System.Drawing.Size(165, 17)
        Me.textbox_spell_load.TabIndex = 8
        Me.textbox_spell_load.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'PictureBox5
        '
        Me.PictureBox5.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox5.BackgroundImage = CType(resources.GetObject("PictureBox5.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox5.Location = New System.Drawing.Point(15, 228)
        Me.PictureBox5.Name = "PictureBox5"
        Me.PictureBox5.Size = New System.Drawing.Size(283, 91)
        Me.PictureBox5.TabIndex = 12
        Me.PictureBox5.TabStop = False
        '
        'PictureBox4
        '
        Me.PictureBox4.BackgroundImage = CType(resources.GetObject("PictureBox4.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox4.Location = New System.Drawing.Point(3, 5)
        Me.PictureBox4.Name = "PictureBox4"
        Me.PictureBox4.Size = New System.Drawing.Size(276, 314)
        Me.PictureBox4.TabIndex = 11
        Me.PictureBox4.TabStop = False
        '
        'panel_bestiary
        '
        Me.panel_bestiary.Controls.Add(Me.btn_paste_monster_file)
        Me.panel_bestiary.Controls.Add(Me.textbox_bestiary_load)
        Me.panel_bestiary.Controls.Add(Me.label_characters)
        Me.panel_bestiary.Controls.Add(Me.btn_load_monster_file)
        Me.panel_bestiary.Controls.Add(Me.PictureBox2)
        Me.panel_bestiary.Controls.Add(Me.PictureBox6)
        Me.panel_bestiary.Controls.Add(Me.PictureBox1)
        Me.panel_bestiary.Location = New System.Drawing.Point(12, 12)
        Me.panel_bestiary.Name = "panel_bestiary"
        Me.panel_bestiary.Size = New System.Drawing.Size(239, 322)
        Me.panel_bestiary.TabIndex = 4
        '
        'btn_paste_monster_file
        '
        Me.btn_paste_monster_file.BackColor = System.Drawing.Color.SteelBlue
        Me.btn_paste_monster_file.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_paste_monster_file.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btn_paste_monster_file.ForeColor = System.Drawing.Color.White
        Me.HelpProvider_bertiary_converter.SetHelpKeyword(Me.btn_paste_monster_file, "Characters")
        Me.HelpProvider_bestiary_paste.SetHelpString(Me.btn_paste_monster_file, "Button to paste a monster text structure in the clipboard and conver it into a .D" &
        "nd5e structure compatible as Character in RuleSet5E")
        Me.HelpProvider_bertiary_converter.SetHelpString(Me.btn_paste_monster_file, "")
        Me.btn_paste_monster_file.Location = New System.Drawing.Point(18, 141)
        Me.btn_paste_monster_file.Name = "btn_paste_monster_file"
        Me.HelpProvider_bertiary_converter.SetShowHelp(Me.btn_paste_monster_file, False)
        Me.HelpProvider_bestiary_paste.SetShowHelp(Me.btn_paste_monster_file, True)
        Me.btn_paste_monster_file.Size = New System.Drawing.Size(204, 61)
        Me.btn_paste_monster_file.TabIndex = 9
        Me.btn_paste_monster_file.Text = "PASTE CLIPBOARD TEXT TO CONVERT INTO DN5E FILES"
        Me.btn_paste_monster_file.UseVisualStyleBackColor = False
        '
        'textbox_bestiary_load
        '
        Me.textbox_bestiary_load.BackColor = System.Drawing.Color.GhostWhite
        Me.textbox_bestiary_load.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.textbox_bestiary_load.Font = New System.Drawing.Font("Segoe Print", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.textbox_bestiary_load.ForeColor = System.Drawing.Color.LightSlateGray
        Me.textbox_bestiary_load.Location = New System.Drawing.Point(39, 258)
        Me.textbox_bestiary_load.Multiline = True
        Me.textbox_bestiary_load.Name = "textbox_bestiary_load"
        Me.textbox_bestiary_load.Size = New System.Drawing.Size(162, 17)
        Me.textbox_bestiary_load.TabIndex = 8
        Me.textbox_bestiary_load.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'label_characters
        '
        Me.label_characters.AutoSize = True
        Me.label_characters.BackColor = System.Drawing.Color.GhostWhite
        Me.label_characters.Font = New System.Drawing.Font("Comic Sans MS", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.label_characters.ForeColor = System.Drawing.Color.SteelBlue
        Me.label_characters.Location = New System.Drawing.Point(75, 35)
        Me.label_characters.Name = "label_characters"
        Me.label_characters.Size = New System.Drawing.Size(110, 27)
        Me.label_characters.TabIndex = 1
        Me.label_characters.Text = "BESTIARY"
        Me.label_characters.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PictureBox2
        '
        Me.PictureBox2.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox2.BackgroundImage = CType(resources.GetObject("PictureBox2.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.PictureBox2.Location = New System.Drawing.Point(3, 228)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(233, 87)
        Me.PictureBox2.TabIndex = 11
        Me.PictureBox2.TabStop = False
        '
        'PictureBox6
        '
        Me.PictureBox6.BackgroundImage = CType(resources.GetObject("PictureBox6.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox6.Location = New System.Drawing.Point(168, 26)
        Me.PictureBox6.Name = "PictureBox6"
        Me.PictureBox6.Size = New System.Drawing.Size(54, 43)
        Me.PictureBox6.TabIndex = 12
        Me.PictureBox6.TabStop = False
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.BackgroundImage = CType(resources.GetObject("PictureBox1.BackgroundImage"), System.Drawing.Image)
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.PictureBox1.Location = New System.Drawing.Point(3, 5)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(236, 73)
        Me.PictureBox1.TabIndex = 10
        Me.PictureBox1.TabStop = False
        '
        'HelpProvider_bertiary_converter
        '
        Me.HelpProvider_bertiary_converter.Tag = ""
        '
        'Dnd5ECharacterBuilder
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.ClientSize = New System.Drawing.Size(566, 348)
        Me.Controls.Add(Me.panel_bestiary)
        Me.Controls.Add(Me.panle_spell)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.HelpButton = True
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Dnd5ECharacterBuilder"
        Me.Text = "DND5E CONVERTER"
        Me.panle_spell.ResumeLayout(False)
        Me.panle_spell.PerformLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panel_bestiary.ResumeLayout(False)
        Me.panel_bestiary.PerformLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btn_load_monster_file As Button
    Friend WithEvents btn_load_spell As Button
    Friend WithEvents btn_load_spell_file As Button
    Friend WithEvents panle_spell As Panel
    Friend WithEvents label_spells As Label
    Friend WithEvents panel_bestiary As Panel
    Friend WithEvents label_characters As Label
    Friend WithEvents HelpProvider_bertiary_converter As HelpProvider
    Friend WithEvents textbox_bestiary_load As TextBox
    Friend WithEvents HelpProvider_spell_load As HelpProvider
    Friend WithEvents HelpProvider_spell_conveter As HelpProvider
    Friend WithEvents textbox_spell_load As TextBox
    Friend WithEvents btn_paste_spell_file As Button
    Friend WithEvents btn_paste_monster_file As Button
    Friend WithEvents HelpProvider_bestiary_paste As HelpProvider
    Friend WithEvents HelpProvider_spell_paste As HelpProvider
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
End Class
