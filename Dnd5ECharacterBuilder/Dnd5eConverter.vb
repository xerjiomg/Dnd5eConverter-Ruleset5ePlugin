Imports Microsoft
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Text.RegularExpressions
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Globalization
Imports System.Collections.ObjectModel
Imports System.Buffers
Imports System.Reflection.Metadata.Ecma335
Imports System.DirectoryServices.ActiveDirectory
Imports System.Net.Security
Imports System.Web
Imports System.Text.Json.Nodes
Imports System.Security.Cryptography.X509Certificates

Public Class Dnd5eConverter

    'Characters to save as Ddnd5e files
    Public Characters As New List(Of Dnd5ECharacter.Character)
    Public Characters_name As New List(Of String)
    Public Characters_source As New List(Of String)

    'Spells to save as Spell files
    Public Spells As New List(Of List(Of Dnd5ECharacter.Roll))
    Public Spells_name As New List(Of String)
    Public Spells_source As New List(Of String)
    Public Spells_level As New List(Of String)
    Public Spells_type As New List(Of String)

    'Spells loads to take in account into character's Ddnd5e files
    Public Spells_attacks As New List(Of List(Of Dnd5ECharacter.Roll))
    Public Spells_attacksDC As New List(Of List(Of Dnd5ECharacter.Roll))
    Public Spells_healing As New List(Of List(Of Dnd5ECharacter.Roll))
    Public Spells_attacks_name As New List(Of String)
    Public Spells_attacksDC_name As New List(Of String)
    Public Spells_healing_name As New List(Of String)

    'Characters and Spells with input format
    Public Spells_input As New List(Of JObject)
    Public Characters_input As New List(Of JObject)

    Private Sub btn_load_spell_click(sender As Object, e As EventArgs) Handles btn_load_spell.Click
        Try
            '1) Open dialog to select folder
            Dim obj_opendir As New FolderBrowserDialog
            obj_opendir.UseDescriptionForTitle = True
            obj_opendir.Description = "Select the folder that contains Spell files to load"
            obj_opendir.ShowDialog()
            '2) Load files selected in folder or subfolder
            Dim int_count_spell As Integer = 0
            If obj_opendir.SelectedPath <> "" Then
                For Each str_file As String In My.Computer.FileSystem.GetFiles(obj_opendir.SelectedPath, FileIO.SearchOption.SearchAllSubDirectories, "*.Spell")
                    Dim str_txtfile As String = File.ReadAllText(str_file)
                    If JsonConvert.DeserializeObject(str_txtfile).GetType.Name = "JArray" Then
                        Dim obj_jsonfile As JArray = JsonConvert.DeserializeObject(str_txtfile)
                        Dim str_txtfile_name As String = Mid(str_file, InStrRev(str_file, "\") + 1, str_file.Length).Replace(".Spell", "")
                        Dim roll_spell_attacks As New List(Of Dnd5ECharacter.Roll)
                        Dim roll_spell_attacksDC As New List(Of Dnd5ECharacter.Roll)
                        Dim roll_spell_healing As New List(Of Dnd5ECharacter.Roll)
                        For int_count As Integer = 1 To obj_jsonfile.Count
                            Dim str_roll As String = obj_jsonfile.Item(int_count - 1).ToObject(Of Dnd5ECharacter.Roll).roll
                            If str_roll = "100" Or str_roll.Contains("/") Then
                                roll_spell_attacksDC.Add(obj_jsonfile.Item(int_count - 1).ToObject(Of Dnd5ECharacter.Roll))
                            ElseIf str_roll.Contains("1D20") Then
                                roll_spell_attacks.Add(obj_jsonfile.Item(int_count - 1).ToObject(Of Dnd5ECharacter.Roll))
                            Else
                                roll_spell_healing.Add(obj_jsonfile.Item(int_count - 1).ToObject(Of Dnd5ECharacter.Roll))
                            End If
                        Next
                        If roll_spell_attacks.Count > 0 Then
                            Spells_attacks.Add(roll_spell_attacks)
                            Spells_attacks_name.Add(str_txtfile_name)
                        End If
                        If roll_spell_attacksDC.Count > 0 Then
                            Spells_attacksDC.Add(roll_spell_attacksDC)
                            Spells_attacksDC_name.Add(str_txtfile_name)
                        End If
                        If roll_spell_healing.Count > 0 Then
                            Spells_healing.Add(roll_spell_healing)
                            Spells_healing_name.Add(str_txtfile_name)
                        End If
                        int_count_spell = int_count_spell + 1
                    End If
                Next
                textbox_spell_load.Text = int_count_spell.ToString + " spell(s) loaded"
                MsgBox("Completed process")
            Else
                MsgBox("Not Spell files selected")
            End If
        Catch ex As Exception
        MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
        End Try
    End Sub

    Private Sub btn_load_monster_file_Click(sender As Object, e As EventArgs) Handles btn_load_monster_file.Click
        Try
            '1) Open dialog to select file
            Dim obj_openfile As New System.Windows.Forms.OpenFileDialog
            obj_openfile.Filter = "Json files (*.Json)|*.Json|" & "All files (*.*)|*.*"
            obj_openfile.CheckFileExists = False
            obj_openfile.Multiselect = True
            obj_openfile.Title = "Select Json files to convert into Dnd5e files"
            obj_openfile.ShowDialog()
            '2) Load files selected and convert to Dnd5e structure
            If obj_openfile.FileName <> "" Then
                For Each str_txtfile_name As String In obj_openfile.FileNames
                    Dim str_txtfile As String = File.ReadAllText(str_txtfile_name)
                    Dim obj_jsonfile As JObject = JsonConvert.DeserializeObject(str_txtfile)
                    If obj_jsonfile.Item("monster") IsNot Nothing Then
                        For Each obj_monster As JObject In obj_jsonfile.Item("monster")
                            proc_Dnd5eBuilder(obj_monster)
                        Next
                    ElseIf obj_jsonfile.Item("name") IsNot Nothing And obj_jsonfile.Item("source") IsNot Nothing And
                           obj_jsonfile.Item("source") IsNot Nothing Then
                        proc_Dnd5eBuilder(obj_jsonfile)
                    End If
                Next
                '3) Open dialog to select output folder
                Dim obj_opendir As New FolderBrowserDialog
                obj_opendir.UseDescriptionForTitle = True
                obj_opendir.Description = "Select a folder to save Dnd5e files"
                obj_opendir.ShowDialog()
                '3) Saves Dnd5e files in selected folder
                If obj_opendir.SelectedPath <> "" Then
                    Dim str_path As String = obj_opendir.SelectedPath
                    Dim settingjsonfile As New JsonSerializerSettings
                    settingjsonfile.NullValueHandling = NullValueHandling.Ignore
                    settingjsonfile.DefaultValueHandling = DefaultValueHandling.Ignore
                    Dim int_count As Integer = 0
                    For Each cha_character As Dnd5ECharacter.Character In Characters
                        Dim str_jsonfile As String = JsonConvert.SerializeObject(cha_character, Newtonsoft.Json.Formatting.Indented, settingjsonfile)
                        Dim str_dnd5efile As Byte() = New UTF8Encoding(True).GetBytes(str_jsonfile)
                        If Directory.Exists(str_path + "\" + Characters_source(int_count)) = False Then
                            My.Computer.FileSystem.CreateDirectory(str_path + "\" + Characters_source(int_count))
                        End If
                        Dim obj_fs As FileStream = File.Create(str_path + "\" + Characters_source(int_count) + "\" + Characters_name(int_count) + ".Dnd5e")
                        obj_fs.Write(str_dnd5efile, 0, str_dnd5efile.Length)
                        obj_fs.Close()
                        int_count = int_count + 1
                    Next
                    MsgBox("Completed process")
                    textbox_bestiary_load.Text = Characters.Count.ToString + " Dnd5E file(s) loaded"
                Else
                    MsgBox("Not output folder selected")
                End If
            Else
                MsgBox("Not Json files selected")
            End If
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
        End Try
    End Sub

    Private Sub btn_paste_monster_file_Click(sender As Object, e As EventArgs) Handles btn_paste_monster_file.Click
        Try
            If Clipboard.ContainsText Then
                '1) Paste clipboard info
                Dim str_txtfile As String = Clipboard.GetText
                '2) Convert info to Dnd5e structure
                Dim obj_jsonfile As JObject = JsonConvert.DeserializeObject(str_txtfile)
                If obj_jsonfile.Item("name") IsNot Nothing And obj_jsonfile.Item("source") IsNot Nothing And
                obj_jsonfile.Item("source") IsNot Nothing Then
                    proc_Dnd5eBuilder(obj_jsonfile)
                End If
                '3) Open dialog to select output folder
                Dim obj_opendir As New FolderBrowserDialog
                obj_opendir.UseDescriptionForTitle = True
                obj_opendir.Description = "Select a folder to save Dnd5e files"
                obj_opendir.ShowDialog()
                '3) Saves Dnd5e files in selected folder
                If obj_opendir.SelectedPath <> "" Then
                    Dim str_path As String = obj_opendir.SelectedPath
                    Dim settingjsonfile As New JsonSerializerSettings
                    settingjsonfile.NullValueHandling = NullValueHandling.Ignore
                    settingjsonfile.DefaultValueHandling = DefaultValueHandling.Ignore
                    Dim int_count As Integer = 0
                    For Each cha_character As Dnd5ECharacter.Character In Characters
                        Dim str_jsonfile As String = JsonConvert.SerializeObject(cha_character, Newtonsoft.Json.Formatting.Indented, settingjsonfile)
                        Dim str_dnd5efile As Byte() = New UTF8Encoding(True).GetBytes(str_jsonfile)
                        If Directory.Exists(str_path + "\" + Characters_source(int_count)) = False Then
                            My.Computer.FileSystem.CreateDirectory(str_path + "\" + Characters_source(int_count))
                        End If
                        Dim obj_fs As FileStream = File.Create(str_path + "\" + Characters_source(int_count) + "\" + Characters_name(int_count) + ".Dnd5e")
                        obj_fs.Write(str_dnd5efile, 0, str_dnd5efile.Length)
                        obj_fs.Close()
                        int_count = int_count + 1
                    Next
                    MsgBox("Completed process")
                    textbox_bestiary_load.Text = Characters.Count.ToString + " Dnd5E file(s) loaded"
                Else
                    MsgBox("Not output folder selected")
                End If
            Else
                MsgBox("Clipboard is empty")
            End If
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
        End Try
    End Sub

    Private Sub btn_load_spell_file_Click(sender As Object, e As EventArgs) Handles btn_load_spell_file.Click
        Try
            '1) Open dialog to select Json file
            Dim obj_openfile As New System.Windows.Forms.OpenFileDialog
            obj_openfile.Filter = "Json files (*.Json)|*.Json|" & "All files (*.*)|*.*"
            obj_openfile.CheckFileExists = False
            obj_openfile.Multiselect = True
            obj_openfile.Title = "Select Json files to convert into Spell files"
            obj_openfile.ShowDialog()
            '2) Load selected Json files and converted to Spell files
            If obj_openfile.FileName <> "" Then
                For Each str_txtfile_name As String In obj_openfile.FileNames
                    Dim str_txtfile As String = File.ReadAllText(str_txtfile_name)
                    Dim obj_jsonfile As JObject = JsonConvert.DeserializeObject(str_txtfile)
                    If obj_jsonfile.Item("spell") IsNot Nothing Then
                        For Each obj_spell As JObject In obj_jsonfile.Item("spell")
                            proc_SpellBuilder(obj_spell)
                        Next
                    ElseIf obj_jsonfile.Item("name") IsNot Nothing And obj_jsonfile.Item("source") IsNot Nothing Then
                        proc_SpellBuilder(obj_jsonfile)
                    End If
                Next
                '3) Open dialog to select output folder
                Dim obj_opendir As New FolderBrowserDialog
                obj_opendir.UseDescriptionForTitle = True
                obj_opendir.Description = "Select a folder to save Spell files"
                obj_opendir.ShowDialog()
                '4) Saves Spell files in selected folder
                If obj_opendir.SelectedPath <> "" Then
                    Dim str_path As String = obj_opendir.SelectedPath
                    Dim settingjsonfile As New JsonSerializerSettings
                    settingjsonfile.NullValueHandling = NullValueHandling.Ignore
                    settingjsonfile.DefaultValueHandling = DefaultValueHandling.Ignore
                    Dim int_count As Integer = 0
                    For Each str_spell As String In Spells_name
                        Dim str_jsonfile As String = JsonConvert.SerializeObject(Spells(int_count), Newtonsoft.Json.Formatting.Indented, settingjsonfile)
                        Dim str_spellfile As Byte() = New UTF8Encoding(True).GetBytes(str_jsonfile)
                        If Spells_type(int_count) = "healing" Then
                            If Directory.Exists(str_path + "\healing") = False Then
                                My.Computer.FileSystem.CreateDirectory(str_path + "\healing")
                            End If
                            Dim obj_fs As FileStream = File.Create(str_path + "\healing\" + Spells_name(int_count) + ".Spell")
                            obj_fs.Write(str_spellfile, 0, str_spellfile.Length)
                            obj_fs.Close()
                        Else
                            If Directory.Exists(str_path + "\spell\" + Spells_level(int_count)) = False Then
                                My.Computer.FileSystem.CreateDirectory(str_path + "\spell\" + Spells_level(int_count))
                            End If
                            Dim obj_fs As FileStream = File.Create(str_path + "\spell\" + Spells_level(int_count) + "\" + Spells_name(int_count) + ".Spell")
                            obj_fs.Write(str_spellfile, 0, str_spellfile.Length)
                            obj_fs.Close()
                        End If
                        int_count = int_count + 1
                    Next
                    textbox_spell_load.Text = Spells_name.Count.ToString + " Spell file(s) loaded"
                    MsgBox("Completed process")
                Else
                    MsgBox("Not output folder selected")
                End If
            Else
                MsgBox("Not Json files selected")
            End If
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
        End Try
    End Sub

    Private Sub btn_paste_spell_file_Click(sender As Object, e As EventArgs) Handles btn_paste_spell_file.Click
        Try
            If Clipboard.ContainsText Then
                '1) Paste clipboard info
                Dim str_txtfile As String = Clipboard.GetText
                '2) Convert info to Dnd5e structure
                Dim obj_jsonfile As JObject = JsonConvert.DeserializeObject(str_txtfile)
                If obj_jsonfile.Item("name") IsNot Nothing And obj_jsonfile.Item("source") IsNot Nothing And
                   obj_jsonfile.Item("source") IsNot Nothing Then
                    proc_SpellBuilder(obj_jsonfile)
                End If
                '3) Open dialog to select output folder
                Dim obj_opendir As New FolderBrowserDialog
                obj_opendir.UseDescriptionForTitle = True
                obj_opendir.Description = "Select a folder to save Spell files"
                obj_opendir.ShowDialog()
                '4) Saves Spell files in selected folder
                If obj_opendir.SelectedPath <> "" Then
                    Dim str_path As String = obj_opendir.SelectedPath
                    Dim settingjsonfile As New JsonSerializerSettings
                    settingjsonfile.NullValueHandling = NullValueHandling.Ignore
                    settingjsonfile.DefaultValueHandling = DefaultValueHandling.Ignore
                    Dim int_count As Integer = 0
                    For Each str_spell As String In Spells_name
                        Dim str_jsonfile As String = JsonConvert.SerializeObject(Spells(int_count), Newtonsoft.Json.Formatting.Indented, settingjsonfile)
                        Dim str_spellfile As Byte() = New UTF8Encoding(True).GetBytes(str_jsonfile)
                        If Spells_type(int_count) = "healing" Then
                            If Directory.Exists(str_path + "\healing") = False Then
                                My.Computer.FileSystem.CreateDirectory(str_path + "\healing")
                            End If
                            Dim obj_fs As FileStream = File.Create(str_path + "\healing\" + Spells_name(int_count) + ".Spell")
                            obj_fs.Write(str_spellfile, 0, str_spellfile.Length)
                            obj_fs.Close()
                        Else
                            If Directory.Exists(str_path + "\spell\" + Spells_level(int_count)) = False Then
                                My.Computer.FileSystem.CreateDirectory(str_path + "\spell\" + Spells_level(int_count))
                            End If
                            Dim obj_fs As FileStream = File.Create(str_path + "\spell\" + Spells_level(int_count) + "\" + Spells_name(int_count) + ".Spell")
                            obj_fs.Write(str_spellfile, 0, str_spellfile.Length)
                            obj_fs.Close()
                        End If
                        int_count = int_count + 1
                    Next
                    textbox_spell_load.Text = Spells_name.Count.ToString + " Spell file(s) loaded"
                    MsgBox("Completed process")
                Else
                    MsgBox("Not output folder selected")
                End If
            Else
                MsgBox("Clipboard is empty")
            End If
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
        End Try
    End Sub

    Function proc_Dnd5eBuilder(obj_monster As JObject)
        Try
            '1) Initial variable allocation
            '1.1) String
            Dim str_monster_name As String
            Dim str_monster_source As String
            Dim str_sneak_attack As String = ""
            Dim str_dive_attack As String = ""
            Dim str_advan_attack As String = ""
            Dim str_scroll_header As String = ""
            Dim str_scroll_text As String = ""
            '1.2) Bolean
            Dim bol_is_magic_attack As Boolean = False
            Dim bol_is_magic_rangeattack As Boolean = False
            Dim bol_is_critical_rg19 As Boolean = False
            Dim bol_is_critical_rg18 As Boolean = False
            Dim bol_is_critical_x3 As Boolean = False
            Dim bol_is_critical_x4 As Boolean = False
            Dim bol_is_critical_immune As Boolean = False
            Dim bol_is_sneak_attack As Boolean = False
            Dim bol_is_dive_attack As Boolean = False
            Dim bol_is_advan_attack As Boolean = False
            Dim bol_is_scroll As Boolean = False
            Dim bol_is_summon As Boolean = False
            '1.3) Characters
            Dim cha_character As New Dnd5ECharacter.Character
            '2) Character 
            '2.1) Name
            str_monster_name = obj_monster("name").ToString.Replace(Chr(34), "").Replace("\", "").Replace("/", "")
            '2.2) Source
            str_monster_source = obj_monster("source").ToString.Replace(":", "")
            '2.3) Traits
            If obj_monster.Item("trait") IsNot Nothing Then
                For Each obj_monster_trait As JObject In obj_monster.Item("trait")
                    Dim str_monster_trait_desc As String = ""
                    If obj_monster_trait.Item("entries").Count > 0 Then
                        str_monster_trait_desc = obj_monster_trait.Item("entries").Item(0).ToString.ToLower
                    End If
                    Dim str_monster_trait_name As String = obj_monster_trait("name")
                    If str_monster_trait_name.ToLower.Contains("magic weapons") Or
                        str_monster_trait_name.ToLower.Contains("hellish weapons") Or
                        str_monster_trait_name.ToLower.Contains("fallen weapons") Or
                        str_monster_trait_name.ToLower.Contains("angelic weapons") Or
                        str_monster_trait_name.ToLower.Contains("blazing weapons") Or
                        str_monster_trait_name.ToLower.Contains("weapons of balance") Or
                        str_monster_trait_name.ToLower.Contains("sacred weapons") Then
                        bol_is_magic_attack = True
                    ElseIf str_monster_trait_name.ToLower.Contains("unerring precision") Or
                        str_monster_trait_name.ToLower.Contains("magic arrows") Or
                        str_monster_trait_name.ToLower.Contains("magic ranged weapons") Then
                        bol_is_magic_rangeattack = True
                    ElseIf str_monster_trait_name.ToLower.Contains("improved critical") Then
                        bol_is_critical_rg19 = True
                    ElseIf str_monster_trait_name.ToLower.Contains("adamantine plating") Or
                        str_monster_trait_name.ToLower.Contains("unfavorable target") Then
                        bol_is_critical_immune = True
                    ElseIf str_monster_trait_name.ToLower.Contains("sneak attack") Or
                        str_monster_trait_name.ToLower.Contains("dive attack") Or
                        str_monster_trait_name.ToLower.Contains("martial advantage") Then
                        Dim int_key_pos As Integer = InStr(str_monster_trait_desc, "@damage")
                        Dim int_key_len As Integer = "@damage".Length
                        If int_key_pos = 0 Then
                            int_key_pos = InStr(str_monster_trait_desc, "@dice")
                            int_key_len = "@dice".Length
                        End If
                        If int_key_pos > 0 Then
                            Dim int_sep_pos As Integer = InStr(int_key_pos, str_monster_trait_desc, "}")
                            If str_monster_trait_name.ToLower.Contains("sneak attack") Then
                                bol_is_sneak_attack = True
                                str_sneak_attack = Mid(str_monster_trait_desc, int_key_pos + int_key_len + 1, int_sep_pos - (int_key_pos + int_key_len + 1))
                            ElseIf str_monster_trait_name.ToLower.Contains("dive attack") Then
                                bol_is_dive_attack = True
                                str_dive_attack = Mid(str_monster_trait_desc, int_key_pos + int_key_len + 1, int_sep_pos - (int_key_pos + int_key_len + 1))
                            ElseIf str_monster_trait_name.ToLower.Contains("martial advantage") Then
                                bol_is_advan_attack = True
                                str_advan_attack = Mid(str_monster_trait_desc, int_key_pos + int_key_len + 1, int_sep_pos - (int_key_pos + int_key_len + 1))
                            End If
                        End If
                    ElseIf str_monster_trait_name.ToLower.Contains("scrollskin") Then
                        bol_is_scroll = True
                        str_scroll_header = str_monster_trait_desc
                        For int_count_entries As Integer = 2 To obj_monster_trait.Item("entries").Count
                            str_scroll_text = str_scroll_text + " " + obj_monster_trait.Item("entries")(int_count_entries - 1).ToString
                        Next
                    End If
                Next
            End If
            '2.4) Hit points
            If obj_monster.Item("hp") IsNot Nothing Then
                If obj_monster.Item("hp").Item("special") IsNot Nothing Then
                    bol_is_summon = True
                    cha_character.hp = Regex.Replace(obj_monster.Item("hp").Item("special").ToString.Split("(")(0).Split("+")(0), "[^0-9]", "").Trim
                    If cha_character.hp = "" Then cha_character.hp = "1"
                Else
                    cha_character.hp = obj_monster.Item("hp").Item("average")
                End If
            End If
            '2.5) Armor Class
            If obj_monster.Item("ac") IsNot Nothing Then
                If obj_monster.Item("ac").Type = JTokenType.Integer Then
                    cha_character.ac = obj_monster.Item("ac").ToString
                ElseIf obj_monster.Item("ac").Type = JTokenType.Array Then
                    If obj_monster.Item("ac").Item(0).Type = JTokenType.Integer Then
                        cha_character.ac = obj_monster.Item("ac").Item(0).ToString
                    ElseIf obj_monster.Item("ac").Item(0).Type = JTokenType.Object Then
                        If obj_monster.Item("ac").Item(0).Item("ac") IsNot Nothing Then
                            cha_character.ac = obj_monster.Item("ac").Item(0).Item("ac").ToString
                        ElseIf obj_monster.Item("ac").Item(0).Item("special") IsNot Nothing Then
                            If obj_monster.Item("ac").Item(0).Item("special").ToString.Contains("+") Then
                                cha_character.ac = Regex.Replace(obj_monster.Item("ac").Item(0).Item("special").ToString.Split("+")(0), "[^0-9]", "")
                            Else
                                cha_character.ac = Regex.Replace(obj_monster.Item("ac").Item(0).Item("special").ToString, "[^0-9]", "")
                            End If
                        End If
                    End If
                End If
            End If
            '2.6) Speed
            If obj_monster.Item("speed") IsNot Nothing Then
                If obj_monster.Item("speed").Type = JTokenType.Object Then
                    If obj_monster.Item("speed").Item("walk") IsNot Nothing Then
                        If obj_monster.Item("speed").Item("walk").Type = JTokenType.Object Then
                            Dim str_key1 As String = obj_monster.Item("speed").Item("walk").First.Path.Split(".").Last
                            cha_character.speed = obj_monster.Item("speed").Item("walk").Item(str_key1).ToString
                        ElseIf obj_monster.Item("speed").Item("walk").Type = JTokenType.Integer Then
                            cha_character.speed = obj_monster.Item("speed").Item("walk").ToString
                        End If
                    ElseIf obj_monster.Item("speed").First IsNot Nothing Then
                        Dim str_key1 As String = obj_monster.Item("speed").First.Path.Split(".").Last
                        If obj_monster.Item("speed").Item(str_key1).Type = JTokenType.Object Then
                            Dim str_key2 As String = obj_monster.Item("speed").Item(str_key1).First.Path.Split(".").Last
                            cha_character.speed = obj_monster.Item("speed").Item(str_key1).Item(str_key2).ToString
                        ElseIf obj_monster.Item("speed").Item(str_key1).Type = JTokenType.Integer Then
                            cha_character.speed = obj_monster.Item("speed").Item(str_key1).ToString
                        End If
                    End If
                End If
            End If
            '2.7) Stats
            If obj_monster.Item("str") IsNot Nothing Then
                cha_character.str = obj_monster.Item("str").ToString
            End If
            If obj_monster.Item("dex") IsNot Nothing Then
                cha_character.dex = obj_monster.Item("dex").ToString
            End If
            If obj_monster.Item("con") IsNot Nothing Then
                cha_character.con = obj_monster.Item("con").ToString
            End If
            If obj_monster.Item("int") IsNot Nothing Then
                cha_character.Int = obj_monster.Item("int").ToString
            End If
            If obj_monster.Item("wis") IsNot Nothing Then
                cha_character.wis = obj_monster.Item("wis").ToString
            End If
            If obj_monster.Item("cha") IsNot Nothing Then
                cha_character.cha = obj_monster.Item("cha").ToString
            End If
            '2.8) Saving throw
            Dim array_stat() As String = {"str", "dex", "con", "int", "wis", "cha"}
            For Each str_stat As String In array_stat
                Dim roll_save As New Dnd5ECharacter.Roll
                roll_save.name = str_stat.ToUpper
                roll_save.type = "Public"
                If obj_monster.Item(str_stat) IsNot Nothing Then
                    If obj_monster.Item(str_stat).ToString <> "" Then
                        If Integer.Parse(obj_monster.Item(str_stat)) > 10 Then
                            roll_save.roll = "1D20+" + Math.Floor((Integer.Parse(obj_monster.Item(str_stat)) - 10) / 2).ToString
                        ElseIf Integer.Parse(obj_monster.Item(str_stat)) < 10 Then
                            roll_save.roll = "1D20" + Math.Floor((Integer.Parse(obj_monster.Item(str_stat)) - 10) / 2).ToString
                        Else
                            roll_save.roll = "1D20+0"
                        End If
                    End If
                End If
                If obj_monster.Item("save") IsNot Nothing Then
                    If obj_monster.Item("save").Item(str_stat) IsNot Nothing Then
                        roll_save.roll = "1D20" + obj_monster.Item("save").Item(str_stat).ToString
                    End If
                End If
                cha_character.saves.Add(roll_save)
            Next
            '2.9) Skill check
            Dim array_skill() As String = {"Initiative", "Acrobatics", "Animal Handling", "Arcana", "Athletics", "Deception", "History", "Insight",
                "Intimidation", "Investigation", "Medicine", "Nature", "Perception", "Performance", "Persuasion", "Religion", "Sleight of Hand", "Stealth", "Survival"}
            Dim array_skill_stat() As String = {"dex", "dex", "wis", "int", "str", "cha", "int", "wis", "cha", "int", "wis", "int", "wis", "cha", "cha", "int", "dex", "dex", "wis"}
            Dim int_count_stat As Integer = 0
            For Each str_skill_stat As String In array_skill_stat
                Dim roll_skill As New Dnd5ECharacter.Roll
                int_count_stat = int_count_stat + 1
                roll_skill.name = array_skill(int_count_stat - 1) + " [GM]"
                roll_skill.type = "Secret,GM"
                If obj_monster.Item(str_skill_stat) IsNot Nothing Then
                    If obj_monster.Item(str_skill_stat).ToString <> "" Then
                        If Integer.Parse(obj_monster.Item(str_skill_stat)) > 10 Then
                            roll_skill.roll = "1D20+" + Math.Floor((Integer.Parse(obj_monster.Item(str_skill_stat)) - 10) / 2).ToString
                        ElseIf Integer.Parse(obj_monster.Item(str_skill_stat)) < 10 Then
                            roll_skill.roll = "1D20" + Math.Floor((Integer.Parse(obj_monster.Item(str_skill_stat)) - 10) / 2).ToString
                        Else
                            roll_skill.roll = "1D20+0"
                        End If
                    End If
                End If
                If obj_monster.Item("skill") IsNot Nothing Then
                    If obj_monster.Item("skill").Item(array_skill(int_count_stat - 1).ToLower) IsNot Nothing Then
                        roll_skill.roll = "1D20" + obj_monster.Item("skill").Item(array_skill(int_count_stat - 1).ToLower).ToString
                    End If
                End If
                cha_character.skills.Add(roll_skill)
            Next
            '2.10) Resistances
            If obj_monster.Item("resist") IsNot Nothing Then
                For int_count As Integer = 1 To obj_monster.Item("resist").Count
                    If obj_monster.Item("resist").Item(int_count - 1).Type = JTokenType.String Then
                        If obj_monster.Item("resist").Item(int_count - 1).ToString.ToLower.Contains("piercing") Or
                            obj_monster.Item("resist").Item(int_count - 1).ToString.ToLower.Contains("bludgeoning") Or
                            obj_monster.Item("resist").Item(int_count - 1).ToString.ToLower.Contains("slashing") Then
                            cha_character.resistance.Add("magic " + obj_monster.Item("resist").Item(int_count - 1).ToString.ToLower)
                        End If
                        cha_character.resistance.Add(obj_monster.Item("resist").Item(int_count - 1).ToString.ToLower)
                    ElseIf obj_monster.Item("resist").Item(int_count - 1).Type = JTokenType.Object Then
                        If obj_monster.Item("resist").Item(int_count - 1).Item("resist") IsNot Nothing Then
                            For int_count2 As Integer = 1 To obj_monster.Item("resist").Item(int_count - 1).Item("resist").Count
                                If obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).Type = JTokenType.String Then
                                    cha_character.resistance.Add(obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).ToString.ToLower)
                                ElseIf obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).Type = JTokenType.Object Then
                                    If obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).Item("resist") IsNot Nothing Then
                                        For int_count3 As Integer = 1 To obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).Item("resist").Count
                                            cha_character.resistance.Add(obj_monster.Item("resist").Item(int_count - 1).Item("resist").Item(int_count2 - 1).Item("resist").Item(int_count3 - 1).ToString.ToLower)
                                        Next
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
            End If
            '2.11) Inmunities
            If obj_monster.Item("immune") IsNot Nothing Then
                For int_count As Integer = 1 To obj_monster.Item("immune").Count
                    If obj_monster.Item("immune").Item(int_count - 1).Type = JTokenType.String Then
                        If obj_monster.Item("immune").Item(int_count - 1).ToString.ToLower.Contains("piercing") Or
                            obj_monster.Item("immune").Item(int_count - 1).ToString.ToLower.Contains("bludgeoning") Or
                            obj_monster.Item("immune").Item(int_count - 1).ToString.ToLower.Contains("slashing") Then
                            cha_character.immunity.Add("magic " + obj_monster.Item("immune").Item(int_count - 1).ToString.ToLower)
                        End If
                        cha_character.immunity.Add(obj_monster.Item("immune").Item(int_count - 1).ToString.ToLower)
                    ElseIf obj_monster.Item("immune").Item(int_count - 1).Type = JTokenType.Object Then
                        If obj_monster.Item("immune").Item(int_count - 1).Item("immune") IsNot Nothing Then
                            For int_count2 As Integer = 1 To obj_monster.Item("immune").Item(int_count - 1).Item("immune").Count
                                cha_character.immunity.Add(obj_monster.Item("immune").Item(int_count - 1).Item("immune").Item(int_count2 - 1).ToString.ToLower)
                            Next
                        End If
                    End If
                Next
            End If
            If bol_is_critical_immune Then cha_character.immunity.Add("critical")
            '2.12) Vulnerabilities
            If obj_monster.Item("vulnerable") IsNot Nothing Then
                For int_count As Integer = 1 To obj_monster.Item("vulnerable").Count
                    If obj_monster.Item("vulnerable").Item(int_count - 1).Type = JTokenType.String Then
                        If obj_monster.Item("vulnerable").Item(int_count - 1).ToString.ToLower.Contains("piercing") Or
                            obj_monster.Item("vulnerable").Item(int_count - 1).ToString.ToLower.Contains("bludgeoning") Or
                            obj_monster.Item("vulnerable").Item(int_count - 1).ToString.ToLower.Contains("slashing") Then
                            cha_character.vulnerability.Add("magic " + obj_monster.Item("vulnerable").Item(int_count - 1).ToString.ToLower)
                        End If
                        cha_character.vulnerability.Add(obj_monster.Item("vulnerable").Item(int_count - 1).ToString.ToLower)
                    ElseIf obj_monster.Item("vulnerable").Item(int_count - 1).Type = JTokenType.Object Then
                        If obj_monster.Item("vulnerable").Item(int_count - 1).Item("vulnerable") IsNot Nothing Then
                            If obj_monster.Item("vulnerable").Item(int_count - 1).Item("note") IsNot Nothing Then
                                If obj_monster.Item("vulnerable").Item(int_count - 1).Item("note").ToString.ToLower.Contains("from magical attacks") Then
                                    For int_count2 As Integer = 1 To obj_monster.Item("vulnerable").Item(int_count - 1).Item("vulnerable").Count
                                        cha_character.vulnerability.Add("magic " + obj_monster.Item("vulnerable").Item(int_count - 1).Item("vulnerable").Item(int_count2 - 1).ToString.ToLower)
                                    Next
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            '2.13) Actions
            Dim str_actions() As String = {"action", "bonus", "reaction", "legendary", "trait"}
            For Each str_action As String In str_actions
                If obj_monster.Item(str_action) IsNot Nothing Then
                    For int_count_action As Integer = 1 To obj_monster.Item(str_action).Count
                        If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Count > 0 Then
                            Dim str_action_entries As String = ""
                            For int_count_entries As Integer = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Count
                                If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Type = JTokenType.Object Then
                                    If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("type") IsNot Nothing Then
                                        If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("type") = "list" Then
                                            For int_count_entries_list As Integer = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Count
                                                If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Type = JTokenType.String Then
                                                    If Mid(obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).ToString, 1, 10).Contains("@b ") Or
                                                       Mid(obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).ToString, 1, 10).Contains("@bold ") Then
                                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).ToString.Replace("@bold", "@name").Replace("@b", "@name")
                                                    Else
                                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).ToString
                                                    End If
                                                ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Type = JTokenType.Object Then
                                                    If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("name") IsNot Nothing Then
                                                        Dim str_name As String = obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("name").ToString.Replace(Chr(34), "").Replace(":", " ")
                                                        If str_name.Contains("(") Then str_name = str_name.Split("(")(0).Trim
                                                        If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries") IsNot Nothing Then
                                                            For int_count_subentries_list = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries").Count
                                                                If int_count_subentries_list = 1 Then
                                                                    str_action_entries = str_action_entries + " {@name " + str_name + "} " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries").Item(int_count_subentries_list - 1).ToString
                                                                Else
                                                                    str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries").Item(int_count_subentries_list - 1).ToString
                                                                End If
                                                            Next
                                                        ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entry") IsNot Nothing Then
                                                            str_action_entries = str_action_entries + " {@name " + str_name + "} " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entry").ToString
                                                        End If
                                                    Else
                                                        For int_count_subentries_list = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries").Count
                                                            str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_entries_list - 1).Item("entries").Item(int_count_subentries_list - 1).ToString
                                                        Next
                                                    End If
                                                End If
                                            Next
                                        ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("type") = "table" Then
                                            For int_count_entries_list As Integer = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("rows").Count
                                                For int_count_subentries_list = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_entries_list - 1).Count
                                                    If int_count_subentries_list = 1 Then
                                                        str_action_entries = str_action_entries + " {@name " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_entries_list - 1).Item(int_count_subentries_list - 1).ToString + "}"
                                                    Else
                                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_entries_list - 1).Item(int_count_subentries_list - 1).ToString
                                                    End If
                                                Next
                                            Next
                                        ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("type") = "item" Or
                                               obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("type") = "entries" Then
                                            Dim str_name As String = obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("name").ToString.Replace(Chr(34), "").Replace(":", " ")
                                            If str_name.Contains("(") Then str_name = str_name.Split("(")(0).Trim
                                            If obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entries") IsNot Nothing Then
                                                For int_count_subentries_list = 1 To obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entries").Count
                                                    If int_count_subentries_list = 1 Then
                                                        str_action_entries = str_action_entries + " {@name " + str_name + "} " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entries").Item(int_count_subentries_list - 1).ToString
                                                    Else
                                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entries").Item(int_count_subentries_list - 1).ToString
                                                    End If
                                                Next
                                            ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entry") IsNot Nothing Then
                                                str_action_entries = str_action_entries + " {@name " + str_name + "} " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Item("entry").ToString
                                            End If
                                        End If
                                    End If
                                ElseIf obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).Type = JTokenType.String Then
                                    If Mid(obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).ToString, 1, 10).Contains("@b ") Or
                                       Mid(obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).ToString, 1, 10).Contains("@bold ") Then
                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).ToString.Replace("@bold", "@name").Replace("@b", "@name")
                                    Else
                                        str_action_entries = str_action_entries + " " + obj_monster.Item(str_action).Item(int_count_action - 1).Item("entries").Item(int_count_entries - 1).ToString
                                    End If
                                End If
                            Next
                            Dim int_skip As Integer = 0
                            'Attacks (versus AC or DC)
                            If str_action_entries.Contains("@hit") Or str_action_entries.Contains("@dc") Then
                                For int_count_subaction As Integer = 1 To str_action_entries.Split({"@hit", "@dc"}, StringSplitOptions.RemoveEmptyEntries).Length - 1
                                    Dim str_action_text As String = str_action_entries.ToLower.Replace("@hit", "~@hit").Replace("@dc", "~@dc").Split("~")(int_count_subaction)
                                    Dim str_action_text_prev As String = str_action_entries.ToLower.Replace("@hit", "~@hit").Replace("@dc", "~@dc").Split("~")(int_count_subaction - 1)
                                    Dim bol_skip As Boolean = False
                                    Dim roll_action As New Dnd5ECharacter.Roll
                                    Dim bol_is_escape As Boolean = False
                                    'a) Name
                                    Dim str_action_name As String = obj_monster.Item(str_action).Item(int_count_action - 1).Item("name")
                                    If str_action_name <> "" And Not str_action_name.ToLower.Contains("two-hand") And
                                        Not str_action_name.ToLower.Contains("one-hand") And
                                        Not str_action_name.ToLower.Contains("thrown") Then
                                        str_action_name = str_action_name.Split({"{", "("}, StringSplitOptions.RemoveEmptyEntries)(0).Trim
                                    End If
                                    If str_action_entries.Split({"@hit", "@dc"}, StringSplitOptions.RemoveEmptyEntries).Length > 1 And
                                       str_action_text_prev.Contains("@name") Then
                                        Dim int_start As Integer = InStrRev(str_action_text_prev, "@name ") + "@name ".Length
                                        Dim int_len As Integer = InStr(int_start, str_action_text_prev, "}") - int_start
                                        If int_len > 0 Then
                                            str_action_name = str_action_name + " (" + Mid(str_action_text_prev, int_start, int_len).Replace(".", "") + ")"
                                        End If
                                    ElseIf int_count_subaction - int_skip > 1 Then
                                        If str_action_text_prev.Contains("@hit") And str_action_text.Contains("@hit") Then
                                            str_action_name = str_action_name + " (second attack)"
                                        ElseIf str_action_text_prev.Contains("@hit") And str_action_text.Contains("@dc") Then
                                            If str_action_text.Contains("check") Then
                                                str_action_name = str_action_name + " (check)"
                                            Else
                                                str_action_name = str_action_name + " (saving)"
                                            End If
                                        ElseIf str_action_text_prev.Contains("@dc") And str_action_text.Contains("@hit") Then
                                            str_action_name = str_action_name + " (extra attack)"
                                        ElseIf str_action_text_prev.Contains("@dc") And str_action_text.Contains("@dc") Then
                                            If str_action_text.Contains("action") Or str_action_text_prev.Contains("action") Then
                                                str_action_name = str_action_name + " (as action)"
                                            Else
                                                str_action_name = str_action_name + " (if condition)"
                                            End If
                                        End If
                                    End If
                                    If str_action_name = "Scrollskin" Then int_skip = int_skip + 1
                                    If str_action_name = "" Then
                                        If str_action.Contains("actions") Then
                                            str_action_name = str_action
                                        Else
                                            str_action_name = str_action + " action"
                                        End If
                                    End If
                                    If str_action_name.Contains("+1") Or str_action_name.Contains("+2") Or str_action_name.Contains("+3") Or
                                            str_action_name.Contains("+4") Or str_action_name.Contains("+5") Then
                                        bol_is_magic_attack = True
                                    End If
                                    roll_action.name = str_action_name
                                    'b) Type
                                    Dim str_action_type As String = Nothing
                                    If str_action_text.Contains("@hit") Then
                                        Dim str_action_text_ini As String = str_action_entries.Split({"@hit", "@dc"}, StringSplitOptions.RemoveEmptyEntries)(0).ToLower
                                        Dim int_start As Integer = InStr(str_action_text_ini, "@atk") + "@atk".Length + 1
                                        Dim int_len As Integer = InStr(int_start, str_action_text_ini, "}") - int_start
                                        If int_len > 0 Then
                                            If Mid(str_action_text_ini, int_start, int_len) = "mw" Or
                                                Mid(str_action_text_ini, int_start, int_len) = "m" Then
                                                str_action_type = "Melee"
                                            ElseIf Mid(str_action_text_ini, int_start, int_len) = "ms" Or
                                                Mid(str_action_text_ini, int_start, int_len).Contains("rs") Then
                                                str_action_type = "Magic"
                                            ElseIf Mid(str_action_text_ini, int_start, int_len) = "r" Or
                                                Mid(str_action_text_ini, int_start, int_len).Contains("rw") Then
                                                str_action_type = "Range"
                                            End If
                                        End If
                                        If str_action_type Is Nothing Then
                                            If str_action_text_ini.Contains("reach") Or str_action_text_ini.Contains("5 feet") Then
                                                str_action_type = "Melee"
                                            ElseIf str_action_text_ini.Contains("range") Then
                                                str_action_type = "Range"
                                            Else
                                                str_action_type = "Range"
                                            End If
                                        End If
                                    ElseIf str_action_text.Contains("@dc") Then
                                        str_action_type = "Magic"
                                    End If
                                    roll_action.type = str_action_type
                                    'c) Range
                                    Dim str_action_range As String = Nothing
                                    If str_action_text.Contains("@hit") Then
                                        If str_action_text.Contains("range") Then
                                            Dim int_start As Integer = InStr(str_action_text, "range") + "range".Length + 1
                                            Dim int_len As Integer = InStr(int_start, str_action_text, "ft.") - int_start
                                            If int_len > 0 Then
                                                str_action_range = Mid(str_action_text, int_start, int_len).Trim
                                                If Not str_action_range.Contains("/") Then str_action_range = str_action_range + "/" + str_action_range
                                            End If
                                        End If
                                        If str_action_range Is Nothing And str_action_text.Contains("reach") Then
                                            Dim int_start As Integer = InStr(str_action_text, "reach") + "reach".Length + 1
                                            Dim int_len As Integer = InStr(int_start, str_action_text, "ft.") - int_start
                                            If int_len > 0 Then
                                                str_action_range = Mid(str_action_text, int_start, int_len).Trim + "/" + Mid(str_action_text, int_start, int_len).Trim
                                                If str_action_range = "0/0" Then
                                                    str_action_range = "1/1"
                                                End If
                                            End If
                                        End If
                                    ElseIf str_action_text.Contains("@dc") Then
                                        Dim str_action_text_ini As String = str_action_entries.Split({"@hit", "@dc"}, StringSplitOptions.RemoveEmptyEntries)(0).ToLower
                                        If int_count_subaction > 1 And str_action_text_ini.Contains("@atk") Then
                                            Dim str_action_text_hit = str_action_entries.Split({"@hit", "@dc"}, StringSplitOptions.RemoveEmptyEntries)(1).ToLower
                                            If str_action_text_hit.Contains("range") Then
                                                Dim int_start As Integer = InStr(str_action_text_hit, "range") + "range".Length + 1
                                                Dim int_len As Integer = InStr(int_start, str_action_text_hit, "ft.") - int_start
                                                If int_len > 0 Then
                                                    str_action_range = Mid(str_action_text_hit, int_start, int_len).Trim
                                                    If Not str_action_range.Contains("/") Then str_action_range = str_action_range + "/" + str_action_range
                                                End If
                                            End If
                                            If str_action_range Is Nothing And str_action_text_hit.Contains("reach") Then
                                                Dim int_start As Integer = InStr(str_action_text_hit, "reach") + "reach".Length + 1
                                                Dim int_len As Integer = InStr(int_start, str_action_text_hit, "ft.") - int_start
                                                If int_len > 0 Then
                                                    str_action_range = Mid(str_action_text_hit, int_start, int_len).Trim + "/" + Mid(str_action_text_hit, int_start, int_len).Trim
                                                End If
                                            End If
                                        Else
                                            Dim int_count_word As Integer = 0
                                            For Each str_word As String In str_action_text_ini.Split(" ")
                                                If str_word.Contains("-foot") Then
                                                    str_action_range = Regex.Replace(str_word, "[^0-9]", "") + "/" + Regex.Replace(str_word, "[^0-9]", "")
                                                    Exit For
                                                ElseIf str_word = "feet" Or str_word = "ft." Then
                                                    str_action_range = str_action_text_ini.Split(" ")(int_count_word - 1) + "/" + str_action_text_ini.Split(" ")(int_count_word - 1)
                                                    Exit For
                                                End If
                                                int_count_word = int_count_word + 1
                                            Next
                                        End If
                                    End If
                                    If str_action_range Is Nothing Then
                                        If str_action_type = "Melee" Then
                                            str_action_range = "5/5"
                                        Else
                                            str_action_range = "120/120"
                                        End If
                                    End If
                                    roll_action.range = str_action_range
                                    'd) Roll
                                    Dim str_action_roll As String = Nothing
                                    If str_action_text.Contains("@hit") Then
                                        If bol_is_summon Then
                                            str_action_roll = "1D20+0"
                                        Else
                                            Dim int_start As Integer = InStr(str_action_text, "@hit") + "@hit".Length + 1
                                            Dim int_len As Integer = InStr(int_start, str_action_text, "}") - int_start
                                            If int_len > 0 Then
                                                str_action_roll = "1D20+" + Mid(str_action_text, int_start, int_len)
                                                If str_action_roll.Contains("++") Then str_action_roll = str_action_roll.Replace("++", "+")
                                            End If
                                        End If
                                    ElseIf str_action_text.Contains("@dc") Then
                                        Dim str_stats() As String = {"str", "dex", "con", "int", "wis", "cha"}
                                        Dim int_start As Integer = InStr(str_action_text, "@dc") + "@dc".Length + 1
                                        Dim int_len As Integer = InStr(int_start, str_action_text, "}") - int_start
                                        Dim str_dc As String = Nothing
                                        If int_len > 0 Then
                                            str_dc = Mid(str_action_text, int_start, int_len)
                                        End If
                                        Dim str_save As String = Nothing
                                        Dim str_save_success As String = "zero"
                                        If str_action_text.Contains("saving ") Then
                                            int_start = InStr(str_action_text, "}") + 1
                                            int_len = InStr(int_start, str_action_text, "saving ") - int_start
                                            If int_len > 0 Then
                                                str_save = Mid(Mid(str_action_text, int_start, int_len).ToLower.Trim, 1, 3)
                                            End If
                                            If Not str_stats.Contains(str_save) Then
                                                str_save = Nothing
                                            End If
                                            If (str_action_text.Contains("half") Or str_action_text.Contains("halve")) And str_action_text.Contains("success") Then str_save_success = "half"
                                        End If
                                        If str_save Is Nothing Then
                                            If Not str_action_text.Contains("check") And Not Mid(str_action_text_prev, Math.Max(str_action_text_prev.Length - 15, 1), Math.Min(15, str_action_text_prev.Length)).Contains("check") Then
                                                str_stats = {"strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"}
                                                For Each str_stat In str_stats
                                                    If str_action_text.Contains(str_stat) Or (str_action_text_prev.Contains(str_stat) And str_action_text_prev.Contains("saving ")) Then
                                                        str_save = Mid(str_stat, 1, 3)
                                                        Exit For
                                                    End If
                                                Next
                                                If (str_action_text.Contains("half") Or str_action_text.Contains("halve")) And str_action_text.Contains("success") Then str_save_success = "half"
                                            Else

                                                str_stats = {"Initiative", "Acrobatics", "Animal Handling", "Arcana", "Athletics", "Deception", "History", "Insight", "Intimidation",
                                                         "Investigation", "Medicine", "Nature", "Perception", "Performance", "Persuasion", "Religion", "Sleight of Hand", "Stealth", "Survival"}
                                                For Each str_stat In str_stats
                                                    If str_action_text.Contains(str_stat.ToLower) Or Mid(str_action_text_prev, Math.Max(str_action_text_prev.Length - 30, 1), Math.Min(30, str_action_text_prev.Length)).Contains(str_stat.ToLower) Then
                                                        str_save = str_stat
                                                        Exit For
                                                    End If
                                                Next
                                                If str_save Is Nothing Then
                                                    str_stats = {"strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"}
                                                    For Each str_stat In str_stats
                                                        If str_action_text.Contains(str_stat) Or Mid(str_action_text_prev, Math.Max(str_action_text_prev.Length - 30, 1), Math.Min(30, str_action_text_prev.Length)).Contains(str_stat) Then
                                                            str_save = Mid(str_stat, 1, 3)
                                                            Exit For
                                                        End If
                                                    Next
                                                End If
                                            End If
                                        End If
                                        If Mid(str_action_text_prev, Math.Max(str_action_text_prev.Length - 15, 1), Math.Min(15, str_action_text_prev.Length)).Contains("escape") Then
                                            bol_is_escape = True
                                            str_save = "Athletics"
                                        End If
                                        If str_save IsNot Nothing Then
                                            str_action_roll = str_dc + "/" + str_save.ToUpper + "/" + str_save_success
                                        Else
                                            bol_skip = True
                                            int_skip = int_skip + 1
                                        End If
                                    End If
                                    roll_action.roll = str_action_roll
                                    'e) Critical range and damage multiplier
                                    If str_action_text.Contains("@hit") Then
                                        Dim str_action_critrange As String = "20"
                                        If bol_is_critical_rg18 Then
                                            str_action_critrange = "18"
                                        ElseIf bol_is_critical_rg19 Then
                                            str_action_critrange = "19"
                                        End If
                                        Dim str_action_critmultip As String = "2"
                                        If bol_is_critical_x4 Then
                                            str_action_critmultip = "4"
                                        ElseIf bol_is_critical_x3 Then
                                            str_action_critmultip = "3"
                                        End If
                                        roll_action.critrangemin = str_action_critrange
                                        roll_action.critmultip = str_action_critmultip
                                    End If
                                    'f) Conditions
                                    Dim str_action_condition As String = ""
                                    For int_count_condition As Integer = 1 To str_action_text.Split("@condition").Length - 1
                                        Dim str_condition As String = str_action_text.Split("@condition")(int_count_condition)
                                        Dim int_len As Integer = InStr(str_condition, "}") - 1
                                        If int_len > 0 Then
                                            If str_action_condition = "" Then
                                                str_action_condition = Mid(str_condition, 1, int_len).Trim
                                            ElseIf Not str_action_condition.Contains(Mid(str_condition, 1, int_len).Trim) Then
                                                str_action_condition = str_action_condition + "|" + Mid(str_condition, 1, int_len).Trim
                                            End If
                                        End If
                                    Next
                                    If str_action_condition IsNot Nothing Then
                                        roll_action.conditions = str_action_condition
                                    End If
                                    'g) Icon
                                    Dim str_action_icon As String = ""
                                    Dim str_icon_name() As String = {"TENTACLE", "CROSSBOW", "AXE", "AXE", "AXE", "SHORTSWORD"}
                                    Dim str_icon_des() As String = {"tentacle", "crossbow", "greataxe", "handaxe", "battleaxe", "shortsword"}
                                    For int_count_icon As Integer = 1 To str_icon_name.Count
                                        If str_action_name.ToLower.Contains(str_icon_des(int_count_icon - 1)) Then
                                            str_action_icon = "e_" + str_icon_name(int_count_icon - 1)
                                        End If
                                    Next
                                    If str_action_type <> "Magic" And str_action_icon = "" Then
                                        str_icon_name = {"WHIP", "RAPIER", "DAGGER", "ROCK"}
                                        str_icon_des = {"whip", "rapier", "dagger", "rock"}
                                        For int_count_icon As Integer = 1 To str_icon_name.Count
                                            If roll_action.name.ToLower.Contains(str_icon_des(int_count_icon - 1)) Then
                                                str_action_icon = "e_" + str_icon_name(int_count_icon - 1)
                                            End If
                                        Next
                                    End If
                                    If str_action_type <> "Range" And str_action_icon = "" Then
                                        str_icon_name = {"BITE", "CLAW", "SLAM", "TAIL", "BITE", "SLAM"}
                                        str_icon_des = {"bite", "claw", "slam", "tail", "mandibles", "fist"}
                                        For int_count_icon As Integer = 1 To str_icon_name.Count
                                            If Mid(str_action_name, 1, str_icon_des(int_count_icon - 1).Length).ToLower = str_icon_des(int_count_icon - 1) Or
                                                   Mid(str_action_name, Math.Max(1, str_action_name.Length - str_icon_des(int_count_icon - 1).Length), str_icon_des(int_count_icon - 1).Length).ToLower = str_icon_des(int_count_icon - 1) Or
                                                   Mid(str_action_name, Math.Max(1, str_action_name.Length - str_icon_des(int_count_icon - 1).Length - 1), str_icon_des(int_count_icon - 1).Length).ToLower = str_icon_des(int_count_icon - 1) Then
                                                str_action_icon = "e_" + str_icon_name(int_count_icon - 1)
                                            End If
                                        Next
                                    End If
                                    If str_action_type = "Range" And str_action_icon = "" Then str_action_icon = roll_action.type
                                    If str_action_icon <> "" Then roll_action.futureUse_icon = str_action_icon
                                    'h) Damages (links)
                                    'If Attacks versus CA
                                    If str_action_text.Contains("@hit") Then
                                        Dim str_dmg_elem_types() As String = {"acid", "cold", "fire", "force", "lightning", "necrotic", "poison", "psychic", "radiant", "thunder"}
                                        Dim str_dmg_types() As String = {"acid", "bludgeoning", "cold", "fire", "force", "lightning", "necrotic", "piercing", "poison", "psychic", "radiant", "slashing", "thunder"}
                                        'h.1) Damage based on rolls
                                        If str_action_text.Contains("@damage") Or str_action_text.Contains("@dice") Then
                                            Dim str_key As String = "@damage"
                                            If Not str_action_text.Contains("@damage") Then str_key = "@dice"
                                            Dim str_action_dmgs() As String = str_action_text.Split(str_key)
                                            Dim int_dmgs = str_action_dmgs.Length - 1
                                            Dim int_count_dmg As Integer = 0
                                            Dim int_skip_dmg As Integer = 0
                                            Dim int_dmg_type_extra As Integer = 0
                                            Dim bol_exist_or As Boolean = False
                                            Dim bol_exist_plus As Boolean = False
                                            Dim bol_exist_if As Boolean = False
                                            Dim bol_exist_start As Boolean = False
                                            Dim roll_action_dmg_range_as_melee As New Dnd5ECharacter.Roll
                                            Dim roll_action_dmg_twohand As New Dnd5ECharacter.Roll
                                            Dim roll_action_dmg_with As New Dnd5ECharacter.Roll
                                            Dim roll_action_dmg_if As New Dnd5ECharacter.Roll
                                            Dim roll_action_dmg_extra_extra As New Dnd5ECharacter.Roll
                                            Dim roll_action_dmg_extra As New Dnd5ECharacter.Roll
                                            Dim str_dmg_extra_types() As String
                                            Dim str_action_scnd_roll As String = ""
                                            While (int_count_dmg < int_dmgs - int_skip_dmg)
                                                int_count_dmg = int_count_dmg + 1
                                                Dim bol_dmg_skip As Boolean = False
                                                Dim str_dmg_text_ini As String = str_action_dmgs(int_count_dmg + int_skip_dmg - 1)
                                                Dim str_dmg_text_end As String = str_action_dmgs(int_count_dmg + int_skip_dmg)
                                                'h.1.1) Damage roll
                                                Dim int_len As Integer = InStr(str_dmg_text_end, "}") - 1
                                                Dim str_dmg_roll As String = Mid(str_dmg_text_end, 1, int_len).Replace("‒", "-").Trim.Replace(" ", "").Replace("{@dice", "")
                                                If bol_is_summon Then str_dmg_roll = str_dmg_roll.Replace("+pb", "").Replace("+summonspelllevel", "")
                                                If Mid(str_dmg_roll, 1, 1) = "d" Then str_dmg_roll = "0"
                                                'h.1.2) Damage type
                                                Dim int_start As Integer = int_len + 3
                                                int_len = InStr(int_start, str_dmg_text_end, " damage") - int_start
                                                Dim str_dmg_type As String = ""
                                                If int_len < 0 Then int_len = InStr(int_start, str_dmg_text_end, " ") - int_start
                                                If int_len > 0 Then
                                                    str_dmg_type = Mid(str_dmg_text_end, int_start, int_len).Trim
                                                End If
                                                If str_dmg_type.Contains("or ") And int_dmg_type_extra = 0 Then
                                                    While str_dmg_type.Contains("(")
                                                        str_dmg_type = Mid(str_dmg_type, 1, InStr(str_dmg_type, "(") - 1) + Mid(str_dmg_type, InStr(str_dmg_type, ")") + 1, str_dmg_type.Length)
                                                    End While
                                                    str_dmg_extra_types = str_dmg_type.Replace(", or", "or").Split({"or ", ","}, StringSplitOptions.RemoveEmptyEntries)
                                                    str_dmg_type = str_dmg_extra_types(0).Trim
                                                    int_dmg_type_extra = int_count_dmg
                                                End If
                                                If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                                If str_dmg_type = "" & int_len > 0 Then
                                                    For Each str_type In str_dmg_types
                                                        If Mid(str_dmg_text_end, int_start, int_len).Contains(str_dmg_type) Then
                                                            str_dmg_type = str_type
                                                        End If
                                                    Next
                                                End If
                                                If str_dmg_type = "" And str_dmg_text_end.Contains("type chosen") Then
                                                    Dim str_dmgs As String = ""
                                                    For Each str_type In str_dmg_types
                                                        If str_dmg_text_end.Contains(str_type) Then
                                                            str_dmgs = str_dmgs + "," + str_type
                                                        End If
                                                    Next
                                                    If str_dmgs <> "" Then
                                                        str_dmg_extra_types = str_dmgs.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                    Else
                                                        str_dmg_extra_types = str_dmg_elem_types
                                                    End If
                                                    str_dmg_type = str_dmg_extra_types(0)
                                                    int_dmg_type_extra = int_count_dmg
                                                End If
                                                If {"bludgeoning", "slashing", "piercing"}.Contains(str_dmg_type) And
                                                    (bol_is_magic_attack Or (bol_is_magic_rangeattack And str_action_type = "Range")) Then
                                                    str_dmg_type = "magic " + str_dmg_type
                                                End If
                                                If str_dmg_type = "" And str_key = "@dice" Then bol_dmg_skip = True
                                                'h.1.3) Icon based on damage
                                                If int_count_dmg = 1 And roll_action.futureUse_icon = "" Then
                                                    If str_dmg_type <> "" Then
                                                        If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) And
                                                            str_action_type = "Magic" Then
                                                            str_action_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                        ElseIf {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) And
                                                            str_action_type <> "Magic" Then
                                                            str_action_icon = "ex_M_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                        Else
                                                            str_action_icon = "ex_" + str_dmg_type.ToUpper
                                                        End If
                                                    Else
                                                        If roll_action.type = "Magic" Then
                                                            str_action_icon = "_" + str_action_type
                                                        Else
                                                            str_action_icon = str_action_type
                                                        End If
                                                    End If
                                                    roll_action.futureUse_icon = str_action_icon
                                                End If
                                                'h.1.4) Add actions
                                                Dim int_end As Integer = str_dmg_text_end.Length
                                                If InStr(str_dmg_text_end, ".") > 0 Then int_end = InStr(str_dmg_text_end, ".")
                                                If InStr(str_dmg_text_end, " and if ") > 0 Then int_end = InStr(str_dmg_text_end, " and if ")
                                                Dim int_ini As Integer
                                                If InStrRev(str_dmg_text_ini, ".") > 0 Then
                                                    int_ini = InStrRev(str_dmg_text_ini, ".") + 1
                                                Else
                                                    int_ini = InStrRev(str_dmg_text_ini, ",") + 1
                                                End If
                                                Dim int_text_len As Integer = str_dmg_text_ini.Length
                                                If Mid(str_dmg_text_ini, InStr(str_dmg_text_ini, " damage") + " damage".Length + 1, 3).Contains("or ") Or
                                                   Mid(str_dmg_text_ini, int_text_len - Math.Min(8, int_text_len - 1), Math.Min(8, int_text_len - 1)).Contains("or ") Then
                                                    bol_exist_or = True
                                                    bol_dmg_skip = True
                                                End If
                                                If Mid(str_dmg_text_ini, int_text_len - Math.Min(10, int_text_len - 1), Math.Min(10, int_text_len - 1)).Contains("and") Or
                                                   Mid(str_dmg_text_ini, int_text_len - Math.Min(10, int_text_len - 1), Math.Min(10, int_text_len - 1)).Contains("plus") Then
                                                    If (bol_exist_or And bol_exist_plus) Or
                                                       Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                       Mid(str_dmg_text_ini, int_ini, int_text_len - int_ini).Contains(" if ") Or
                                                       Mid(str_dmg_text_end, 1, int_end).Contains("strat of each") Then
                                                        bol_dmg_skip = True
                                                    Else
                                                        bol_exist_plus = True
                                                    End If
                                                ElseIf (Mid(str_dmg_text_end, 1, int_end).Contains("melee") And str_action_type = "Range") Or
                                                        Mid(str_dmg_text_end, 1, int_end).Contains("with ") Or
                                                        Mid(str_dmg_text_end, 1, int_end).Contains("while ") Or
                                                        Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                        Mid(str_dmg_text_ini, int_ini, int_text_len - int_ini).Contains(" if ") Or
                                                        Mid(str_dmg_text_ini, int_text_len - Math.Min(15, int_text_len - 1), Math.Min(15, int_text_len - 1)).Contains("extra") Or
                                                        Mid(str_dmg_text_ini, int_text_len - Math.Min(15, int_text_len - 1), Math.Min(15, int_text_len - 1)).Contains("additional") Or
                                                        Mid(str_dmg_text_ini, int_text_len - Math.Min(15, int_text_len - 1), Math.Min(15, int_text_len - 1)).Contains("instead") Then
                                                    bol_dmg_skip = True
                                                    If Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                        Mid(str_dmg_text_ini, int_ini, int_text_len - int_ini).Contains(" if ") Then
                                                        bol_exist_if = True
                                                    End If
                                                ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("at the start of ") Or
                                                        Mid(str_dmg_text_ini, 1, int_ini).Contains("at the start of ") Then
                                                    bol_dmg_skip = True
                                                    bol_exist_start = True
                                                End If
                                                If (int_count_dmg = 1 And bol_exist_start = False) Or
                                               (int_count_dmg > 1 And bol_dmg_skip = False) Then
                                                    Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                    roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                    roll_action_dmg.roll = str_dmg_roll
                                                    roll_action_dmg.type = str_dmg_type
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg
                                                Else
                                                    int_skip_dmg = int_skip_dmg + 1
                                                    int_count_dmg = int_count_dmg - 1
                                                    If str_dmg_roll <> "0" Then
                                                        If Mid(str_dmg_text_end, 1, int_end).Contains("melee") And
                                                       str_action_type = "Range" Then
                                                            roll_action_dmg_range_as_melee.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_range_as_melee.roll = str_dmg_roll
                                                            roll_action_dmg_range_as_melee.type = str_dmg_type
                                                        ElseIf Mid(str_dmg_text_ini, 1, int_text_len).Contains("melee") And
                                                               Mid(str_dmg_text_end, 1, int_end).Contains("range") And
                                                               str_action_type = "Range" And int_count_dmg = 1 Then
                                                            roll_action_dmg_range_as_melee.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_range_as_melee.roll = roll_action.link.roll
                                                            roll_action_dmg_range_as_melee.type = roll_action.link.type
                                                            roll_action.link.roll = str_dmg_roll
                                                            If str_dmg_type <> "" Then roll_action.link.type = str_dmg_type
                                                        ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("two hand") Or
                                                           Mid(str_dmg_text_end, 1, int_end).Contains("both hand") Then
                                                            roll_action_dmg_twohand.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_twohand.roll = str_dmg_roll
                                                            roll_action_dmg_twohand.type = str_dmg_type
                                                        ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("with ") Or
                                                        Mid(str_dmg_text_ini, int_text_len - Math.Min(15, int_text_len - 1), Math.Min(15, int_text_len - 1)).Contains("instead") Or
                                                        ((Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                        Mid(str_dmg_text_ini, int_ini, int_text_len - int_ini).Contains(" if ") Or
                                                        Mid(str_dmg_text_end, 1, int_end).Contains("while ")) And
                                                        Mid(str_dmg_text_ini, int_text_len - Math.Min(10, int_text_len - 1), Math.Min(10, int_text_len - 1)).Contains("or")) Then
                                                            roll_action_dmg_with.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_with.roll = str_dmg_roll
                                                            roll_action_dmg_with.type = str_dmg_type
                                                        ElseIf Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                           Mid(str_dmg_text_ini, int_ini, int_text_len - int_ini).Contains(" if ") Or
                                                           Mid(str_dmg_text_end, 1, int_end).Contains("while ") Then
                                                            roll_action_dmg_if.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_if.roll = str_dmg_roll
                                                            roll_action_dmg_if.type = str_dmg_type
                                                        ElseIf bol_exist_or And bol_exist_plus Then
                                                            roll_action_dmg_extra_extra.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_extra_extra.roll = str_dmg_roll
                                                            roll_action_dmg_extra_extra.type = str_dmg_type
                                                        Else
                                                            roll_action_dmg_extra.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg_extra.roll = str_dmg_roll
                                                            roll_action_dmg_extra.type = str_dmg_type
                                                        End If
                                                    End If
                                                    If int_count_dmg = 0 Then
                                                        Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                        roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                        roll_action_dmg.roll = "0"
                                                        roll_action_dmg.type = ""
                                                        roll_action.link = roll_action_dmg
                                                    End If
                                                End If
                                            End While
                                            cha_character.attacks.Add(roll_action)
                                            If int_dmg_type_extra > 0 Then
                                                For Each str_type As String In str_dmg_extra_types
                                                    If str_type <> str_dmg_extra_types(0) Then
                                                        Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action)
                                                        Dim roll_link As New Dnd5ECharacter.Roll
                                                        roll_link = roll_action_dmgtype
                                                        roll_link.name = str_action_name + " (" + str_type.Trim + ")"
                                                        For int_count_link As Integer = 1 To int_dmg_type_extra
                                                            roll_link = roll_link.link
                                                        Next
                                                        If roll_link IsNot Nothing Then
                                                            roll_link.type = str_type.Trim
                                                            If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0).Trim) Then
                                                                roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                            End If
                                                            cha_character.attacks.Add(roll_action_dmgtype)
                                                        End If
                                                    End If
                                                Next
                                                roll_action.name = str_action_name + " (" + str_dmg_extra_types(0).Trim + ")"
                                            End If
                                            If roll_action.name.ToLower.Contains("second attack") Then
                                                If cha_character.attacks(cha_character.attacks.Count - 2).link.roll = "0" Then
                                                    roll_action.name = str_action_name.Split("(")(0).Trim
                                                    If roll_action_dmg_with.name <> "" Or roll_action_dmg_twohand.name <> "" Then
                                                        str_action_scnd_roll = cha_character.attacks(cha_character.attacks.Count - 1).roll
                                                    End If
                                                    cha_character.attacks(cha_character.attacks.Count - 1).roll = cha_character.attacks(cha_character.attacks.Count - 2).roll
                                                    cha_character.attacks.RemoveAt(cha_character.attacks.Count - 2)
                                                End If
                                            End If
                                            If roll_action_dmg_twohand.name <> "" Then
                                                Dim roll_action_twohand As New Dnd5ECharacter.Roll(roll_action)
                                                roll_action_twohand.name = roll_action_twohand.name + " (two hand)"
                                                If bol_exist_or And bol_exist_plus And roll_action_dmg_extra_extra.name <> "" Then
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_twohand.link.link.link
                                                    roll_action_dmg_twohand.link = roll_link
                                                    roll_action_dmg_extra_extra.link = roll_action_dmg_twohand
                                                    roll_action_twohand.link = roll_action_dmg_extra_extra
                                                Else
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_twohand.link.link
                                                    roll_action_dmg_twohand.link = roll_link
                                                    roll_action_twohand.link = roll_action_dmg_twohand
                                                End If
                                                If str_action_scnd_roll <> "" Then roll_action_twohand.roll = str_action_scnd_roll
                                                cha_character.attacks.Add(roll_action_twohand)
                                                If roll_action_dmg_if.name <> "" Then
                                                    Dim roll_action_twohand_if = New Dnd5ECharacter.Roll(roll_action_twohand)
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_twohand_if
                                                    roll_link.name = roll_link.name + "(if condition)"
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg_if
                                                    cha_character.attacks.Add(roll_action_twohand_if)
                                                End If
                                                If int_dmg_type_extra > 0 Then
                                                    For Each str_type As String In str_dmg_extra_types
                                                        If str_type <> str_dmg_extra_types(0) Then
                                                            Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action_twohand)
                                                            Dim roll_link As New Dnd5ECharacter.Roll
                                                            roll_link = roll_action_dmgtype
                                                            roll_link.name = str_action_name + " (" + str_type.Trim + ")(two hand)"
                                                            For int_count_link As Integer = 1 To int_dmg_type_extra
                                                                roll_link = roll_link.link
                                                            Next
                                                            roll_link.type = str_type.Trim
                                                            If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                                roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                            End If
                                                            cha_character.attacks.Add(roll_action_dmgtype)
                                                        End If
                                                    Next
                                                End If
                                            End If
                                            If roll_action_dmg_range_as_melee.name <> "" Then
                                                Dim roll_action_range_as_melee As New Dnd5ECharacter.Roll(roll_action)
                                                roll_action_range_as_melee.name = roll_action_range_as_melee.name + " (melee)"
                                                roll_action_range_as_melee.type = "Melee"
                                                If bol_exist_or Then
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_range_as_melee.link.link
                                                    roll_action_dmg_range_as_melee.link = roll_link
                                                    roll_action_range_as_melee.link = roll_action_dmg_range_as_melee
                                                Else
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_range_as_melee
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg_range_as_melee
                                                End If
                                                If roll_action_range_as_melee.futureUse_icon = str_action_type Then
                                                    If roll_action_range_as_melee.link.type <> "" Then
                                                        If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(roll_action_range_as_melee.link.type) Then
                                                            roll_action_range_as_melee.futureUse_icon = "ex_" + roll_action_range_as_melee.link.type.ToUpper.Split(" ")(1)
                                                        Else
                                                            roll_action_range_as_melee.futureUse_icon = "ex_" + roll_action_range_as_melee.link.type.ToUpper
                                                        End If
                                                    Else
                                                        roll_action_range_as_melee.futureUse_icon = "Melee"
                                                    End If
                                                End If
                                                cha_character.attacks.Add(roll_action_range_as_melee)
                                                If roll_action_dmg_if.name <> "" Then
                                                    Dim roll_action_range_as_melee_if As New Dnd5ECharacter.Roll(roll_action_range_as_melee)
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_range_as_melee_if
                                                    roll_link.name = roll_link.name + "(if condition)"
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg_if
                                                    cha_character.attacks.Add(roll_action_range_as_melee_if)
                                                End If
                                                If int_dmg_type_extra > 0 Then
                                                    For Each str_type As String In str_dmg_extra_types
                                                        If str_type <> str_dmg_extra_types(0) Then
                                                            Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action_range_as_melee)
                                                            Dim roll_link As New Dnd5ECharacter.Roll
                                                            roll_link = roll_action_dmgtype
                                                            roll_link.name = str_action_name + " (" + str_type.Trim + ")(melee)"
                                                            For int_count_link As Integer = 1 To int_dmg_type_extra
                                                                roll_link = roll_link.link
                                                            Next
                                                            roll_link.type = str_type.Trim
                                                            If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                                roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                            End If
                                                            cha_character.attacks.Add(roll_action_dmgtype)
                                                        End If
                                                    Next
                                                End If
                                            End If
                                            If roll_action_dmg_with.name <> "" Then
                                                Dim roll_action_with As New Dnd5ECharacter.Roll(roll_action)
                                                If bol_exist_if Then
                                                    roll_action_with.name = roll_action.name + " (while condition)"
                                                Else
                                                    roll_action_with.name = roll_action.name + " (with other)"
                                                End If
                                                If bol_exist_or And bol_exist_plus And roll_action_dmg_extra_extra.name <> "" Then
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_with.link.link.link
                                                    roll_action_dmg_with.link = roll_link
                                                    roll_action_dmg_extra_extra.link = roll_action_dmg_with
                                                    roll_action_with.link = roll_action_dmg_extra_extra
                                                Else
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_with.link.link
                                                    roll_action_dmg_with.link = roll_link
                                                    roll_action_with.link = roll_action_dmg_with
                                                End If
                                                If str_action_scnd_roll <> "" Then roll_action_with.roll = str_action_scnd_roll
                                                cha_character.attacks.Add(roll_action_with)
                                                If roll_action_dmg_if.name <> "" Then
                                                    Dim roll_action_with_if As New Dnd5ECharacter.Roll(roll_action_with)
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_with_if
                                                    roll_link.name = roll_link.name + "(if condition)"
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg_if
                                                    cha_character.attacks.Add(roll_action_with_if)
                                                End If
                                                If int_dmg_type_extra > 0 Then
                                                    For Each str_type As String In str_dmg_extra_types
                                                        If str_type <> str_dmg_extra_types(0) Then
                                                            Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action_with)
                                                            Dim roll_link As New Dnd5ECharacter.Roll
                                                            roll_link = roll_action_dmgtype
                                                            roll_link.name = str_action_name + " (" + str_type.Trim + ")(while condition)"
                                                            For int_count_link As Integer = 1 To int_dmg_type_extra
                                                                roll_link = roll_link.link
                                                            Next
                                                            roll_link.type = str_type.Trim
                                                            If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                                roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                            End If
                                                            cha_character.attacks.Add(roll_action_dmgtype)
                                                        End If
                                                    Next
                                                End If
                                            End If
                                            If roll_action_dmg_if.name <> "" Then
                                                Dim roll_action_if As New Dnd5ECharacter.Roll(roll_action)
                                                roll_action_if.name = roll_action_if.name + " (if condition)"
                                                If bol_exist_or And bol_exist_plus And roll_action_dmg_extra_extra.name <> "" Then
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_if.link.link.link
                                                    roll_action_dmg_if.link = roll_link
                                                    roll_action_dmg_extra_extra.link = roll_action_dmg_if
                                                    roll_action_if.link = roll_action_dmg_extra_extra
                                                Else
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action_if
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg_if
                                                End If
                                                cha_character.attacks.Add(roll_action_if)
                                            End If
                                            If roll_action_dmg_extra.name <> "" And
                                               (roll_action_dmg_if.name = "" Or (roll_action_dmg_if.name <> "" And (bol_exist_or = False Or bol_exist_plus = False)) Or
                                               roll_action_dmg_with.name = "" Or (roll_action_dmg_with.name <> "" And (bol_exist_or = False Or bol_exist_plus = False))) Then
                                                Dim roll_action_extra As New Dnd5ECharacter.Roll
                                                roll_action_extra.name = str_action_name + " (extra)"
                                                roll_action_extra.type = "Range"
                                                roll_action_extra.range = "120/120"
                                                roll_action_extra.roll = "100"
                                                roll_action_extra.link = roll_action_dmg_extra
                                                If roll_action_dmg_extra.type <> "" Then
                                                    roll_action_extra.futureUse_icon = "ex_" + roll_action_dmg_extra.type.ToUpper
                                                Else
                                                    roll_action_extra.futureUse_icon = "Range"
                                                End If
                                                cha_character.attacksDC.Add(roll_action_extra)
                                                If int_dmg_type_extra > 0 Then
                                                    For Each str_type As String In str_dmg_extra_types
                                                        If str_type <> str_dmg_extra_types(0) Then
                                                            Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action_extra)
                                                            roll_action_dmgtype.name = str_action_name + " (extra)(" + str_type.Trim + ")"
                                                            roll_action_dmgtype.link.type = str_type.Trim
                                                            cha_character.attacksDC.Add(roll_action_dmgtype)
                                                        End If
                                                    Next
                                                    If Not roll_action_extra.name.Contains(str_dmg_extra_types(0)) Then
                                                        roll_action_extra.name = str_action_name + " (" + str_dmg_extra_types(0).Trim + ")"
                                                    End If
                                                End If
                                            End If
                                            'h.2) Constant damages
                                        ElseIf str_action_text.Contains("@h") And str_action_text.Contains(" damage") Then
                                            'h.2.1) Damage roll
                                            Dim int_start As Integer = InStr(str_action_text, "@h}") + "@h}".Length
                                            Dim int_len As Integer = InStr(int_start, str_action_text, " ") - int_start
                                            Dim str_dmg_roll As String = ""
                                            If int_len > 0 Then
                                                str_dmg_roll = Regex.Replace(Mid(str_action_text, int_start, int_len).Trim, "[^0-9]", "")
                                            End If
                                            'h.2.2) Damage type
                                            int_start = int_start + int_len + 1
                                            int_len = InStr(int_start, str_action_text, " damage") - int_start
                                            Dim str_dmg_type As String = ""
                                            If int_len < 0 Then int_len = InStr(int_start, str_action_text, " ") - int_start
                                            If int_len > 0 Then
                                                str_dmg_type = Mid(str_action_text, int_start, int_len).Trim
                                            End If
                                            If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                            If str_dmg_type = "" And int_len > 0 Then
                                                For Each str_type In str_dmg_types
                                                    If Mid(str_action_text, int_start, int_len).Contains(str_type) Then
                                                        str_dmg_type = str_type
                                                    End If
                                                Next
                                            End If
                                            'h.2.3) Icon based on damage
                                            If str_dmg_type <> "" And str_action_icon = "" Then
                                                If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) And
                                                str_action_type = "Magic" Then
                                                    str_action_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                ElseIf {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) And
                                                str_action_type <> "Magic" Then
                                                    str_action_icon = "ex_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                Else
                                                    str_action_icon = "ex_" + str_dmg_type.ToUpper
                                                End If
                                            End If
                                            'h.4) Add actions
                                            If str_dmg_roll <> "" And str_dmg_type <> "" Then
                                                Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                roll_action_dmg.roll = str_dmg_roll
                                                roll_action_dmg.type = str_dmg_type
                                                roll_action.link = roll_action_dmg
                                                If roll_action.futureUse_icon = "" Then roll_action.futureUse_icon = str_action_icon
                                                cha_character.attacks.Add(roll_action)
                                            End If
                                        Else
                                            Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                            roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                            roll_action_dmg.roll = "0"
                                            roll_action_dmg.type = ""
                                            roll_action.link = roll_action_dmg
                                            If roll_action.futureUse_icon = "" And roll_action.type = "Magic" Then
                                                roll_action.futureUse_icon = "_" + str_action_type
                                            ElseIf roll_action.futureUse_icon = "" Then
                                                roll_action.futureUse_icon = str_action_type
                                            End If
                                            cha_character.attacks.Add(roll_action)
                                        End If
                                        'Attacks versus DC
                                    ElseIf str_action_text.Contains("@dc") And bol_skip = False Then
                                        Dim str_dmg_elem_types() As String = {"acid", "cold", "fire", "force", "lightning", "necrotic", "poison", "psychic", "radiant", "thunder"}
                                        Dim str_dmg_types() As String = {"acid", "bludgeoning", "cold", "fire", "force", "lightning", "necrotic", "piercing", "poison", "psychic", "radiant", "slashing", "thunder"}
                                        If str_action_text.Contains("@damage") Or str_action_text.Contains("@dice") Then
                                            Dim str_key As String = "@damage"
                                            If Not str_action_text.Contains("@damage") Then str_key = "@dice"
                                            Dim str_action_dmgs() As String = str_action_text.Replace("@dice {@dice", "@dice").Split(str_key)
                                            Dim int_dmgs = str_action_dmgs.Length - 1
                                            Dim int_count_dmg As Integer = 0
                                            Dim int_skip_dmg As Integer = 0
                                            Dim int_dmg_type_extra As Integer = 0
                                            Dim str_dmg_extra_types() As String
                                            While (int_count_dmg < int_dmgs - int_skip_dmg)
                                                int_count_dmg = int_count_dmg + 1
                                                Dim str_dmg_text_ini As String = str_action_dmgs(int_count_dmg + int_skip_dmg - 1)
                                                Dim str_dmg_text_end As String = str_action_dmgs(int_count_dmg + int_skip_dmg)
                                                'h.1) Damage roll
                                                Dim int_len As Integer = InStr(str_dmg_text_end, "}") - 1
                                                Dim str_dmg_roll As String = Mid(str_dmg_text_end, 1, int_len).Replace("×", "*").Replace(" ", "").Trim
                                                If Mid(str_dmg_roll, 1, 1) = "d" Then str_dmg_roll = "0"
                                                'h.2) Damage type
                                                Dim int_start As Integer = int_len + 3
                                                int_len = InStr(int_start, str_dmg_text_end, " damage") - int_start
                                                Dim str_dmg_type As String = ""
                                                If int_len > 0 Then
                                                    str_dmg_type = Mid(str_dmg_text_end, int_start, int_len).Trim
                                                Else
                                                    int_len = InStr(int_start, str_dmg_text_end, " ") - int_start
                                                    If int_len > 0 Then
                                                        str_dmg_type = Mid(str_dmg_text_end, int_start, int_len).Trim
                                                    End If
                                                End If
                                                If str_dmg_type.Contains("or ") And int_dmg_type_extra = 0 Then
                                                    While str_dmg_type.Contains("(")
                                                        str_dmg_type = Mid(str_dmg_type, 1, InStr(str_dmg_type, "(") - 1) + Mid(str_dmg_type, InStr(str_dmg_type, ")") + 1, str_dmg_type.Length)
                                                    End While
                                                    str_dmg_extra_types = str_dmg_type.Replace(", or", "or").Split({"or ", ","}, StringSplitOptions.RemoveEmptyEntries)
                                                    For int_count_type As Integer = 1 To str_dmg_extra_types.Count
                                                        If Not str_dmg_types.Contains(str_dmg_extra_types(int_count_type - 1).Trim) Then str_dmg_extra_types(int_count_type - 1) = ""
                                                    Next
                                                    str_dmg_type = String.Join(",", str_dmg_extra_types)
                                                    str_dmg_extra_types = str_dmg_type.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                    If str_dmg_extra_types.Length > 1 Then
                                                        str_dmg_type = str_dmg_extra_types(0).Trim
                                                        int_dmg_type_extra = int_count_dmg
                                                    End If
                                                End If
                                                If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                                If str_dmg_type = "" And str_dmg_text_end.Contains("type chosen") Then
                                                    Dim str_dmgs As String = ""
                                                    For Each str_type In str_dmg_types
                                                        If str_dmg_text_end.Contains(str_type) Then
                                                            str_dmgs = str_dmgs + "," + str_type
                                                        End If
                                                    Next
                                                    If str_dmgs <> "" Then
                                                        str_dmg_extra_types = str_dmgs.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                    Else
                                                        str_dmg_extra_types = str_dmg_elem_types
                                                    End If
                                                    str_dmg_type = str_dmg_extra_types(0)
                                                    int_dmg_type_extra = int_count_dmg
                                                End If
                                                If {"bludgeoning", "slashing", "piercing"}.Contains(str_dmg_type) Then str_dmg_type = "magic " + str_dmg_type
                                                'h.3) Icon based on damage
                                                If str_dmg_type <> "" Then
                                                    If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) Then
                                                        str_action_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                    Else
                                                        str_action_icon = "ex_" + str_dmg_type.ToUpper
                                                    End If
                                                Else
                                                    str_action_icon = "_" + str_action_type
                                                End If
                                                If int_count_dmg = 1 And roll_action.futureUse_icon = "" Then
                                                    roll_action.futureUse_icon = str_action_icon
                                                End If
                                                'h.4) Add actions
                                                If str_dmg_type = "" And str_key = "@dice" Then str_action_roll = ""
                                                Dim int_end As Integer = str_dmg_text_end.Length
                                                If InStr(str_dmg_text_end, ".") > 0 Then int_end = InStr(str_dmg_text_end, ".")
                                                If InStr(str_dmg_text_end, " and ") > 0 Then int_end = InStr(str_dmg_text_end, " and ")
                                                Dim int_ini As Integer = InStrRev(str_dmg_text_ini, ".") + 1
                                                If Mid(str_dmg_text_end, 1, int_end).Contains("at the start") Or Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the start") Or
                                                       Mid(str_dmg_text_end, 1, int_end).Contains("at the end") Or Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the end") Then
                                                    Dim roll_action_dmg_extra_plus As New Dnd5ECharacter.Roll
                                                    If int_count_dmg = 1 Then
                                                        Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                        roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                        roll_action_dmg.roll = "0"
                                                        roll_action_dmg.type = ""
                                                        roll_action.link = roll_action_dmg
                                                        If roll_action.futureUse_icon = "" Then roll_action.futureUse_icon = "_" + str_action_type
                                                        cha_character.attacksDC.Add(roll_action)
                                                        If bol_is_escape Then
                                                            roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                            roll_action.futureUse_icon = "e_ESCAPE"
                                                            Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                            roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                            roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                            cha_character.attacksDC.Add(roll_action_escape)
                                                        End If
                                                    Else
                                                        cha_character.attacksDC.Add(roll_action)
                                                        If bol_is_escape Then
                                                            Dim roll_action_extraplus As New Dnd5ECharacter.Roll(roll_action.link)
                                                            roll_action_dmg_extra_plus = roll_action_extraplus
                                                            roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                            roll_action.link.roll = "0"
                                                            roll_action.link.type = ""
                                                            roll_action.futureUse_icon = "e_ESCAPE"
                                                            Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                            roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                            roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                            cha_character.attacksDC.Add(roll_action_escape)
                                                        End If
                                                    End If
                                                    Dim roll_action_extra As New Dnd5ECharacter.Roll
                                                    If Mid(str_dmg_text_end, 1, int_end).Contains("at the start") Or
                                                           Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the start") Then
                                                        roll_action_extra.name = str_action_name + " (start turn)"
                                                    Else
                                                        roll_action_extra.name = str_action_name + " (end turn)"
                                                    End If
                                                    roll_action_extra.type = "Magic"
                                                    roll_action_extra.range = "120/120"
                                                    roll_action_extra.roll = "100"
                                                    Dim roll_action_dmg_extra As New Dnd5ECharacter.Roll
                                                    roll_action_dmg_extra.name = str_action_name.Split("(")(0).Trim
                                                    roll_action_dmg_extra.roll = str_dmg_roll
                                                    roll_action_dmg_extra.type = str_dmg_type
                                                    roll_action_extra.link = roll_action_dmg_extra
                                                    roll_action_extra.futureUse_icon = str_action_icon
                                                    If roll_action_dmg_extra_plus.name <> "" Then
                                                        roll_action_extra.link.link = roll_action_dmg_extra_plus
                                                    End If
                                                    cha_character.attacksDC.Add(roll_action_extra)
                                                    int_skip_dmg = int_skip_dmg + 1
                                                    int_count_dmg = int_count_dmg - 1
                                                ElseIf int_count_dmg > 1 And (Mid(str_dmg_text_end, 1, int_end).Contains(" if ") Or
                                                           Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains(" if ")) Then
                                                    If Mid(str_dmg_text_ini, str_dmg_text_ini.Length - 12, 12).Contains("extra") Or
                                                       Mid(str_dmg_text_ini, str_dmg_text_ini.Length - 12, 12).Contains("plus") Or
                                                       Mid(str_dmg_text_ini, str_dmg_text_ini.Length - 12, 12).Contains("more") Or
                                                       Mid(str_dmg_text_ini, str_dmg_text_ini.Length - 12, 12).Contains("additional") Or
                                                       Mid(str_dmg_text_ini, str_dmg_text_ini.Length - 12, 12).Contains("and") Then
                                                        roll_action.futureUse_icon = str_action_icon
                                                        cha_character.attacksDC.Add(roll_action)
                                                        Dim roll_action_extra As New Dnd5ECharacter.Roll(roll_action)
                                                        roll_action_extra.name = str_action_name + " (if condition)"
                                                        Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                        roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                        roll_action_dmg.roll = str_dmg_roll
                                                        roll_action_dmg.type = str_dmg_type
                                                        Dim roll_link As Dnd5ECharacter.Roll
                                                        roll_link = roll_action_extra
                                                        While roll_link.link IsNot Nothing
                                                            roll_link = roll_link.link
                                                        End While
                                                        roll_link.link = roll_action_dmg
                                                        If roll_action.futureUse_icon = "" Then roll_action_extra.futureUse_icon = str_action_icon
                                                        cha_character.attacksDC.Add(roll_action_extra)
                                                    Else
                                                        If int_count_dmg = 1 Then
                                                            Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                            roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                            roll_action_dmg.roll = "0"
                                                            roll_action_dmg.type = ""
                                                            roll_action.link = roll_action_dmg
                                                            If roll_action.futureUse_icon = "" Then roll_action.futureUse_icon = "_" + str_action_type
                                                            cha_character.attacksDC.Add(roll_action)
                                                            If bol_is_escape Then
                                                                roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                                roll_action.futureUse_icon = "e_ESCAPE"
                                                                Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                                roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                                roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                                cha_character.attacksDC.Add(roll_action_escape)
                                                            End If
                                                        Else
                                                            cha_character.attacksDC.Add(roll_action)
                                                            If bol_is_escape Then
                                                                Dim roll_action_extra_plus As New Dnd5ECharacter.Roll(roll_action)
                                                                roll_action_extra_plus.roll = "100"
                                                                roll_action_extra_plus.name = str_action_name.Split("(")(0).Trim + " (extra)"
                                                                roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                                roll_action.link.roll = "0"
                                                                roll_action.link.type = ""
                                                                roll_action.futureUse_icon = "e_ESCAPE"
                                                                Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                                roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                                roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                                cha_character.attacksDC.Add(roll_action_escape)
                                                                If str_key <> "@dice" And roll_action_extra_plus.link.type <> "" Then
                                                                    cha_character.attacksDC.Add(roll_action_extra_plus)
                                                                End If
                                                            End If
                                                        End If
                                                        Dim roll_action_extra As New Dnd5ECharacter.Roll
                                                        roll_action_extra.name = str_action_name + " (if condition)"
                                                        roll_action_extra.type = "Magic"
                                                        roll_action_extra.range = "120/120"
                                                        roll_action_extra.roll = "100"
                                                        Dim roll_action_dmg_extra As New Dnd5ECharacter.Roll
                                                        roll_action_dmg_extra.name = str_action_name.Split("(")(0).Trim
                                                        roll_action_dmg_extra.roll = str_dmg_roll
                                                        roll_action_dmg_extra.type = str_dmg_type
                                                        roll_action_extra.link = roll_action_dmg_extra
                                                        If roll_action_extra.futureUse_icon = "" Then roll_action_extra.futureUse_icon = str_action_icon
                                                        cha_character.attacksDC.Add(roll_action_extra)
                                                    End If
                                                    int_skip_dmg = int_skip_dmg + 1
                                                    int_count_dmg = int_count_dmg - 1
                                                Else
                                                    Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                    roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                    roll_action_dmg.roll = str_dmg_roll
                                                    roll_action_dmg.type = str_dmg_type
                                                    Dim roll_link As Dnd5ECharacter.Roll
                                                    roll_link = roll_action
                                                    While roll_link.link IsNot Nothing
                                                        roll_link = roll_link.link
                                                    End While
                                                    roll_link.link = roll_action_dmg
                                                End If
                                            End While
                                            If int_skip = 0 And int_count_dmg = int_dmgs Then
                                                cha_character.attacksDC.Add(roll_action)
                                                If int_dmg_type_extra > 0 Then
                                                    For Each str_type As String In str_dmg_extra_types
                                                        If str_type <> str_dmg_extra_types(0) Then
                                                            Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action)
                                                            Dim roll_link As New Dnd5ECharacter.Roll
                                                            roll_link = roll_action_dmgtype
                                                            roll_link.name = str_action_name + " (" + str_type.Trim + ")"
                                                            For int_count_link As Integer = 1 To int_dmg_type_extra
                                                                roll_link = roll_link.link
                                                            Next
                                                            If roll_link IsNot Nothing Then
                                                                roll_link.type = str_type.Trim
                                                                If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                                    roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                                End If
                                                                cha_character.attacksDC.Add(roll_action_dmgtype)
                                                            End If
                                                        End If
                                                    Next
                                                    roll_action.name = str_action_name + " (" + str_dmg_extra_types(0).Trim + ")"
                                                End If
                                                If bol_is_escape Then
                                                    Dim roll_action_extra As New Dnd5ECharacter.Roll(roll_action)
                                                    roll_action_extra.roll = "100"
                                                    roll_action_extra.name = str_action_name.Split("(")(0).Trim + " (extra)"
                                                    roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                    roll_action.link.roll = "0"
                                                    roll_action.link.type = ""
                                                    roll_action.futureUse_icon = "e_ESCAPE"
                                                    Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                    roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                    roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                    cha_character.attacksDC.Add(roll_action_escape)
                                                    If str_key <> "@dice" And roll_action_extra.link.type <> "" Then
                                                        cha_character.attacksDC.Add(roll_action_extra)
                                                    End If
                                                End If
                                            End If
                                        ElseIf int_count_subaction = 1 And str_action_text_prev.Contains("@damage") Then
                                            'h.1) Damage roll
                                            Dim int_start As Integer = InStr(str_action_text_prev, "@damage") + "@damage".Length
                                            Dim int_len As Integer = InStr(int_start, str_action_text_prev, "}") - int_start
                                            Dim str_dmg_roll As String = Mid(str_action_text_prev, int_start, int_len).Replace("×", "*").Replace(" ", "").Trim
                                            If Mid(str_dmg_roll, 1, 1) = "d" Then str_dmg_roll = "0"
                                            'h.2) Damage type
                                            int_start = int_start + int_len + 3
                                            int_len = InStr(int_start, str_action_text_prev, " damage") - int_start
                                            Dim str_dmg_type As String = ""
                                            If int_len > 0 Then
                                                str_dmg_type = Mid(str_action_text_prev, int_start, int_len).Trim
                                            Else
                                                int_len = InStr(int_start, str_action_text_prev, " ") - int_start
                                                If int_len > 0 Then
                                                    str_dmg_type = Mid(str_action_text_prev, int_start, int_len).Trim
                                                End If
                                            End If
                                            Dim str_dmg_extra_types() As String
                                            If str_dmg_type.Contains("or ") Then
                                                While str_dmg_type.Contains("(")
                                                    str_dmg_type = Mid(str_dmg_type, 1, InStr(str_dmg_type, "(") - 1) + Mid(str_dmg_type, InStr(str_dmg_type, ")") + 1, str_dmg_type.Length)
                                                End While
                                                str_dmg_extra_types = str_dmg_type.Replace(", or", "or").Split({"or ", ","}, StringSplitOptions.RemoveEmptyEntries)
                                                For int_count_type As Integer = 1 To str_dmg_extra_types.Count
                                                    If Not str_dmg_types.Contains(str_dmg_extra_types(int_count_type - 1).Trim) Then str_dmg_extra_types(int_count_type - 1) = ""
                                                Next
                                                str_dmg_type = String.Join(",", str_dmg_extra_types)
                                                str_dmg_extra_types = str_dmg_type.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                If str_dmg_extra_types.Length > 1 Then
                                                    str_dmg_type = str_dmg_extra_types(0).Trim
                                                End If
                                            End If
                                            If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                            If {"bludgeoning", "slashing", "piercing"}.Contains(str_dmg_type) And Not str_action_name.ToLower.Contains("charge") Then str_dmg_type = "magic " + str_dmg_type                                            'h.3) Icon based on damage
                                            If str_dmg_type <> "" Then
                                                If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) Then
                                                    str_action_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                                Else
                                                    str_action_icon = "ex_" + str_dmg_type.ToUpper
                                                End If
                                            Else
                                                str_action_icon = "_" + str_action_type
                                            End If
                                            If roll_action.futureUse_icon = "" Then
                                                roll_action.futureUse_icon = str_action_icon
                                            End If
                                            If (str_action_text_prev.Contains("or half") Or str_action_text_prev.Contains("as half") Or
                                               str_action_text.Contains("half")) And bol_is_escape = False Then
                                                Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                roll_action_dmg.roll = str_dmg_roll
                                                roll_action_dmg.type = str_dmg_type
                                                roll_action.link = roll_action_dmg
                                                If roll_action.futureUse_icon = "" And str_dmg_type <> "" Then
                                                    roll_action.futureUse_icon = "ex_" + str_dmg_type.ToUpper
                                                ElseIf roll_action.futureUse_icon = "" Then
                                                    roll_action.futureUse_icon = "_" + str_action_type
                                                End If
                                                cha_character.attacksDC.Add(roll_action)
                                                If str_dmg_extra_types IsNot Nothing Then
                                                    If str_dmg_extra_types.Count > 0 Then
                                                        For Each str_type As String In str_dmg_extra_types
                                                            If str_type <> str_dmg_extra_types(0) Then
                                                                Dim roll_action_dmgtype As New Dnd5ECharacter.Roll(roll_action)
                                                                roll_action_dmgtype.name = str_action_name + " (" + str_type.Trim + ")"
                                                                roll_action_dmgtype.link.type = str_type.Trim
                                                                If roll_action_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                                    roll_action_dmgtype.futureUse_icon = "ex_" + str_type.Trim.ToUpper
                                                                End If
                                                                cha_character.attacksDC.Add(roll_action_dmgtype)
                                                            End If
                                                        Next
                                                        roll_action.name = str_action_name + " (" + str_dmg_extra_types(0).Trim + ")"
                                                    End If
                                                End If
                                            Else
                                                Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                                roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                                roll_action_dmg.roll = "0"
                                                roll_action_dmg.type = ""
                                                roll_action.link = roll_action_dmg
                                                roll_action.name = str_action_name.Split("(")(0) + " (saving)"
                                                roll_action.futureUse_icon = "_" + str_action_type
                                                cha_character.attacksDC.Add(roll_action)
                                                If bol_is_escape Then
                                                    roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                    roll_action.futureUse_icon = "e_ESCAPE"
                                                    Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                    roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                    roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                    cha_character.attacksDC.Add(roll_action_escape)
                                                End If
                                                Dim roll_action_extra As New Dnd5ECharacter.Roll
                                                roll_action_extra.name = str_action_name.Split("(")(0)
                                                roll_action_extra.type = "Magic"
                                                roll_action_extra.range = str_action_range
                                                roll_action_extra.roll = "100"
                                                roll_action_extra.futureUse_icon = str_action_icon
                                                Dim roll_action_extra_dmg As New Dnd5ECharacter.Roll
                                                roll_action_extra_dmg.name = str_action_name.Split("(")(0).Trim
                                                roll_action_extra_dmg.roll = str_dmg_roll
                                                roll_action_extra_dmg.type = str_dmg_type
                                                roll_action_extra.link = roll_action_extra_dmg
                                                cha_character.attacksDC.Add(roll_action_extra)
                                            End If
                                        Else
                                            Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                            roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                            roll_action_dmg.roll = "0"
                                            roll_action_dmg.type = ""
                                            roll_action.link = roll_action_dmg
                                            If roll_action.futureUse_icon = "" Then roll_action.futureUse_icon = "_" + str_action_type
                                            cha_character.attacksDC.Add(roll_action)
                                            If bol_is_escape Then
                                                roll_action.name = roll_action.name.Split("(")(0).Trim + " (escape: athletics)"
                                                roll_action.futureUse_icon = "e_ESCAPE"
                                                Dim roll_action_escape As New Dnd5ECharacter.Roll(roll_action)
                                                roll_action_escape.roll = roll_action_escape.roll.Replace("ATHLETICS", "ACROBATICS")
                                                roll_action_escape.name = roll_action_escape.name.Replace("athletics", "acrobatics")
                                                cha_character.attacksDC.Add(roll_action_escape)
                                            End If
                                        End If
                                    End If
                                    If str_action_name.Contains("+1") Or str_action_name.Contains("+2") Or str_action_name.Contains("+3") Or
                                            str_action_name.Contains("+4") Or str_action_name.Contains("+5") Then
                                        bol_is_magic_attack = False
                                    End If
                                Next
                                'Damages without roll attack
                            ElseIf str_action_entries.Contains("@damage") Then
                                'a) Name
                                Dim str_action_name As String = obj_monster.Item(str_action).Item(int_count_action - 1).Item("name")
                                If str_action_name <> "" Then str_action_name = str_action_name.Split({"{", "("}, StringSplitOptions.RemoveEmptyEntries)(0).Trim
                                If Not str_action_name.ToLower.Contains("sneak attack") And Not str_action_name.ToLower.Contains("dive attack") And
                                   Not str_action_name.ToLower.Contains("martial advantage") Then
                                    Dim str_dmg_types() As String = {"acid", "bludgeoning", "cold", "fire", "force", "lightning", "necrotic", "piercing", "poison", "psychic", "radiant", "slashing", "thunder"}
                                    'b) Range
                                    Dim int_count_word As Integer = 0
                                    Dim str_action_range As String = ""
                                    For Each str_word As String In str_action_entries.Split(" ")
                                        If str_word.Contains("-foot") Then
                                            str_action_range = Regex.Replace(str_word, "[^0-9]", "") + "/" + Regex.Replace(str_word, "[^0-9]", "")
                                            Exit For
                                        ElseIf str_word = "feet" Or str_word = "ft." Then
                                            str_action_range = str_action_entries.Split(" ")(int_count_word - 1) + "/" + str_action_entries.Split(" ")(int_count_word - 1)
                                            Exit For
                                        End If
                                        int_count_word = int_count_word + 1
                                    Next
                                    If str_action_range = "" Then str_action_range = "120/120"
                                    'c) Damages
                                    'c.1) Damage roll
                                    Dim int_start As Integer = InStr(str_action_entries, "@damage") + "@damage".Length
                                    Dim int_len As Integer = InStr(int_start, str_action_entries, "}") - int_start
                                    Dim str_dmg_roll As String = Mid(str_action_entries, int_start, int_len).Replace("×", "*").Replace(" ", "").Trim
                                    If bol_is_summon Then str_dmg_roll = str_dmg_roll.ToLower.Replace("+pb", "").Replace("+summonspelllevel", "")
                                    If Mid(str_dmg_roll, 1, 1) = "d" Then str_dmg_roll = "0"
                                    'c.1) Damage type
                                    int_start = int_start + int_len + 2
                                    int_len = InStr(int_start, str_action_entries, " damage") - int_start
                                    Dim str_dmg_type As String = ""
                                    If int_len > 0 Then
                                        str_dmg_type = Mid(str_action_entries, int_start, int_len).Trim
                                    Else
                                        int_len = InStr(int_start, str_action_entries, " ") - int_start
                                        If int_len > 0 Then
                                            str_dmg_type = Mid(str_action_entries, int_start, int_len).Trim
                                        End If
                                    End If
                                    If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                    If str_dmg_type = "" & int_len > 0 Then
                                        For Each str_type In str_dmg_types
                                            If Mid(str_action_entries, int_start, int_len).Contains(str_dmg_type) Then
                                                str_dmg_type = str_type
                                            End If
                                        Next
                                    End If
                                    If {"bludgeoning", "slashing", "piercing"}.Contains(str_dmg_type) And Not str_action_name.ToLower.Contains("charge") Then str_dmg_type = "magic " + str_dmg_type
                                    'd) Add actions
                                    Dim roll_action As New Dnd5ECharacter.Roll
                                    roll_action.name = str_action_name
                                    roll_action.type = "Magic"
                                    roll_action.range = str_action_range
                                    roll_action.roll = "100"
                                    Dim roll_action_dmg As New Dnd5ECharacter.Roll
                                    roll_action_dmg.name = str_action_name.Split("(")(0).Trim
                                    roll_action_dmg.roll = str_dmg_roll
                                    roll_action_dmg.type = str_dmg_type
                                    roll_action.link = roll_action_dmg
                                    If str_dmg_type <> "" Then
                                        If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) Then
                                            roll_action.futureUse_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                        Else
                                            roll_action.futureUse_icon = "ex_" + str_dmg_type.ToUpper
                                        End If
                                    Else
                                        roll_action.futureUse_icon = "_Magic"
                                    End If
                                    cha_character.attacksDC.Add(roll_action)
                                End If
                                'Healings
                            ElseIf str_action_entries.Contains("regain") And str_action_entries.Contains("hit points") Then
                                'a) Name
                                Dim str_action_name As String = obj_monster.Item(str_action).Item(int_count_action - 1).Item("name")
                                If str_action_name <> "" Then str_action_name = str_action_name.Split({"{", "("}, StringSplitOptions.RemoveEmptyEntries)(0).Trim
                                'b) Range
                                Dim str_action_range As String = ""
                                If str_action_entries.Contains("feet") Or str_action_entries.Contains("ft.") Then
                                    Dim int_start As Integer = InStr(str_action_entries, "feet")
                                    If int_start = 0 Then int_start = InStr(str_action_entries, "ft.")
                                    str_action_range = Regex.Replace(Mid(str_action_entries, int_start - 10, 10), "[^0-9]", "") + "/" + Regex.Replace(Mid(str_action_entries, int_start - 10, 10), "[^0-9]", "")
                                ElseIf str_action_entries.Contains("touch") Or str_action_entries.Contains("reach") Then
                                    If str_action_range = "" Then str_action_range = "5/5"
                                End If
                                If str_action_range = "" Then str_action_range = "1/1"
                                If str_action_name.ToLower.Contains("hexblade's curse") Then
                                    str_action_range = "1/1"
                                End If
                                'c) Roll
                                Dim str_action_roll As String = ""
                                If str_action_entries.Contains("@dice") Then
                                    Dim int_start As Integer = InStr(str_action_entries, "@dice") + "@dice".Length
                                    Dim int_len As Integer = InStr(int_start, str_action_entries, "}") - int_start
                                    If str_action_name.ToLower.Contains("second wind") Or str_action_name.ToLower.Contains("balm of the summer court") Then
                                        str_action_roll = Mid(str_action_entries, int_start, int_len).Replace("×", "*").Trim
                                    ElseIf int_start - InStr(int_start, str_action_entries, "regain") > 0 And
                                               int_start - InStr(int_start, str_action_entries, "hit points") < 0 Then
                                        str_action_roll = Mid(str_action_entries, int_start, int_len).Replace("×", "*").Trim
                                    End If
                                    If Mid(str_action_roll, 1, 1) = "d" Then str_action_roll = ""
                                End If
                                If str_action_roll = "" Then
                                    Dim int_start As Integer = InStr(str_action_entries, "regain") + "regain".Length
                                    Dim int_len As Integer = InStr(int_start, str_action_entries, "hit point") - int_start
                                    If int_len > 0 Then
                                        str_action_roll = Regex.Replace(Mid(str_action_entries, int_start, int_len), "[^0-9]", "")
                                    End If
                                End If
                                'd) Add actions
                                If str_action_roll <> "" And Not str_action_name.ToLower.Contains("misty escape") And
                                       Not str_action_name.ToLower.Contains("blessing of bountiful generosity") Then
                                    Dim roll_action As New Dnd5ECharacter.Roll
                                    roll_action.name = str_action_name
                                    roll_action.type = "Magic"
                                    roll_action.range = str_action_range
                                    roll_action.roll = str_action_roll
                                    cha_character.healing.Add(roll_action)
                                End If
                            End If
                        End If
                    Next
                End If
            Next
            '2.14) Add actions with extra damage based on traits
            For int_count_attack As Integer = 1 To cha_character.attacks.Count
                If bol_is_sneak_attack Then
                    Dim roll_action As New Dnd5ECharacter.Roll(cha_character.attacks(int_count_attack - 1))
                    Dim roll_action_sneak As New Dnd5ECharacter.Roll
                    roll_action_sneak.name = "Sneak Attack"
                    roll_action_sneak.roll = str_sneak_attack
                    Dim roll_link As Dnd5ECharacter.Roll
                    roll_link = roll_action
                    roll_link.name = roll_link.name + " (with sneak attack)"
                    While roll_link.link IsNot Nothing
                        roll_link = roll_link.link
                    End While
                    roll_link.link = roll_action_sneak
                    roll_link.link.type = roll_link.type
                    cha_character.attacks.Add(roll_action)
                End If
                If bol_is_dive_attack And cha_character.attacks(int_count_attack - 1).type = "Melee" Then
                    Dim roll_action As New Dnd5ECharacter.Roll(cha_character.attacks(int_count_attack - 1))
                    Dim roll_action_dive As New Dnd5ECharacter.Roll
                    roll_action_dive.name = "Dive Attack"
                    roll_action_dive.roll = str_dive_attack
                    Dim roll_link As Dnd5ECharacter.Roll
                    roll_link = roll_action
                    roll_link.name = roll_link.name + " (with dive attack)"
                    While roll_link.link IsNot Nothing
                        roll_link = roll_link.link
                    End While
                    roll_link.link = roll_action_dive
                    roll_link.link.type = roll_link.type
                    cha_character.attacks.Add(roll_action)
                End If
                If bol_is_advan_attack Then
                    Dim roll_action As New Dnd5ECharacter.Roll(cha_character.attacks(int_count_attack - 1))
                    Dim roll_action_advan As New Dnd5ECharacter.Roll
                    roll_action_advan.name = "Martial advantage"
                    roll_action_advan.roll = str_advan_attack
                    Dim roll_link As Dnd5ECharacter.Roll
                    roll_link = roll_action
                    roll_link.name = roll_link.name + " (with martial advantage)"
                    While roll_link.link IsNot Nothing
                        roll_link = roll_link.link
                    End While
                    roll_link.link = roll_action_advan
                    roll_link.link.type = roll_link.type
                    cha_character.attacks.Add(roll_action)
                End If
            Next
            '3) Spells
            If obj_monster.Item("spellcasting") IsNot Nothing Then
                For int_count As Integer = 1 To obj_monster.Item("spellcasting").Count
                    Dim str_spell_dc As String = "8"
                    Dim str_spell_attack As String = "0"
                    Dim str_spell_mod As String = "0"
                    Dim str_spell_level As String = "1"
                    Dim str_spell_list As String = ""
                    If obj_monster.Item("spellcasting").Item(int_count - 1).Type = JTokenType.Object Then
                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item("headerEntries") IsNot Nothing Then
                            Dim str_spell_header As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item("headerEntries")(0)
                            'Spellcaster DC
                            Dim int_start As Integer = InStr(str_spell_header, "@dc") + "@dc".Length
                            Dim int_len As Integer = InStr(int_start, str_spell_header, "}") - int_start
                            If int_len > 0 Then str_spell_dc = Mid(str_spell_header, int_start, int_len).Trim
                            'Spellcaster attack
                            If str_spell_header.Contains("@hit") Then
                                int_start = InStr(str_spell_header, "@hit") + "@hit".Length
                                int_len = InStr(int_start, str_spell_header, "}") - int_start
                                If int_len > 0 Then str_spell_attack = Mid(str_spell_header, int_start, int_len).Trim
                            ElseIf Mid(str_spell_header, int_start + int_len + 1, 8).Contains("+") Then
                                str_spell_attack = Regex.Replace(Mid(str_spell_header, int_start + int_len + 1, 8), "[^0-9]", "")
                            Else
                                Dim a As String = ""
                            End If
                            'Spellcaster stat modifier
                            If obj_monster.Item("spellcasting").Item(int_count - 1).Item("ability") IsNot Nothing Then
                                Dim str_ability As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item("ability")
                                If obj_monster.Item(str_ability) IsNot Nothing Then
                                    str_spell_mod = Math.Floor((Integer.Parse(obj_monster.Item(str_ability)) - 10) / 2).ToString
                                End If
                            Else
                                For Each str_stat As String In {"Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma"}
                                    If str_spell_header.ToLower.Contains(str_stat.ToLower) Then
                                        If obj_monster.Item(Mid(str_stat, 1, 3).ToLower) IsNot Nothing Then
                                            str_spell_mod = Math.Floor((Integer.Parse(obj_monster.Item(Mid(str_stat, 1, 3).ToLower)) - 10) / 2).ToString
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                            'Spellcaster level
                            int_start = InStr(str_spell_header, "level")
                            If int_start > 0 Then
                                str_spell_level = Regex.Replace(Mid(str_spell_header, int_start - 6, 6), "[^0-9]", "")
                            End If
                            If str_spell_level = "" Then str_spell_level = "1"
                        End If
                    End If
                    'Spell list by type (will, daily or prepared)
                    For Each str_action As String In {"will", "daily", "spells"}
                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action) IsNot Nothing Then
                            If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Type = JTokenType.Object Then
                                If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Count > 0 Then
                                    For int_count_key As Integer = 1 To obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Count
                                        Dim str_key As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Children.ElementAt(int_count_key - 1).ToObject(Of JProperty).Name
                                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Type = JTokenType.Object Then
                                            If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Count > 0 Then
                                                For int_count_subkey As Integer = 1 To obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Count
                                                    Dim str_subkey As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Children.ElementAt(int_count_subkey - 1).ToObject(Of JProperty).Name
                                                    If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(str_subkey).Type = JTokenType.Array Then
                                                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Children.ElementAt(int_count_subkey - 1).Count > 0 Then
                                                            For int_count_action As Integer = 1 To obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(str_subkey).Count
                                                                If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(str_subkey).Item(int_count_action - 1).Type = JTokenType.String Then
                                                                    If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(str_subkey).Item(int_count_action - 1).ToString.Contains("@spell") Then
                                                                        Dim str_spells() As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(str_subkey).Item(int_count_action - 1).ToString.Split("{@spell", StringSplitOptions.RemoveEmptyEntries)
                                                                        If str_spells.Count > 0 Then
                                                                            For int_count_spell As Integer = 1 To str_spells.Count
                                                                                If str_spells(int_count_spell - 1).Contains("}") Then
                                                                                    If Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Contains("|") Then
                                                                                        str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "|") - 1).Replace("/", "-").Trim
                                                                                    Else
                                                                                        str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Replace("/", "-").Trim
                                                                                    End If
                                                                                End If
                                                                            Next
                                                                        End If
                                                                    End If
                                                                End If
                                                            Next
                                                        End If
                                                    End If
                                                Next
                                            End If
                                        ElseIf obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Type = JTokenType.Array Then
                                            If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Count > 0 Then
                                                For int_count_action As Integer = 1 To obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Count
                                                    If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(int_count_action - 1).Type = JTokenType.String Then
                                                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(int_count_action - 1).ToString.Contains("@spell") Then
                                                            Dim str_spells() As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(str_key).Item(int_count_action - 1).ToString.Split("{@spell", StringSplitOptions.RemoveEmptyEntries)
                                                            If str_spells.Count > 0 Then
                                                                For int_count_spell As Integer = 1 To str_spells.Count
                                                                    If str_spells(int_count_spell - 1).Contains("}") Then
                                                                        If Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Contains("|") Then
                                                                            str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "|") - 1).Replace("/", "-").Trim
                                                                        Else
                                                                            str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Replace("/", "-").Trim
                                                                        End If
                                                                    End If
                                                                Next
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                            End If
                                        End If
                                    Next
                                End If
                            ElseIf obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Type = JTokenType.Array Then
                                If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Count > 0 Then
                                    For int_count_action As Integer = 1 To obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Count
                                        If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(int_count_action - 1).Type = JTokenType.String Then
                                            If obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(int_count_action - 1).ToString.Contains("@spell") Then
                                                Dim str_spells() As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Item(int_count_action - 1).ToString.Split("{@spell", StringSplitOptions.RemoveEmptyEntries)
                                                If str_spells.Count > 0 Then
                                                    For int_count_spell As Integer = 1 To str_spells.Count
                                                        If str_spells(int_count_spell - 1).Contains("}") Then
                                                            If Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Contains("|") Then
                                                                str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "|") - 1).Replace("/", "-").Trim
                                                            Else
                                                                str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Replace("/", "-").Trim
                                                            End If
                                                        End If
                                                    Next
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            ElseIf obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).Type = JTokenType.String Then
                                Dim str_spells() As String = obj_monster.Item("spellcasting").Item(int_count - 1).Item(str_action).ToString.Split("{@spell", StringSplitOptions.RemoveEmptyEntries)
                                If str_spells.Count > 0 Then
                                    For int_count_spell As Integer = 1 To str_spells.Count
                                        If str_spells(int_count_spell - 1).Contains("}") Then
                                            If Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Contains("|") Then
                                                str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "|") - 1).Replace("/", "-").Trim
                                            Else
                                                str_spell_list = str_spell_list + "," + Mid(str_spells(int_count_spell - 1), 1, InStr(str_spells(int_count_spell - 1), "}") - 1).Replace("/", "-").Trim
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Next
                    'Already included actions
                    Dim str_actions_list As String = ""
                    For Each roll_action As Dnd5ECharacter.Roll In cha_character.attacks
                        str_actions_list = str_actions_list + "," + roll_action.name
                    Next
                    For Each roll_action As Dnd5ECharacter.Roll In cha_character.attacksDC
                        str_actions_list = str_actions_list + "," + roll_action.name
                    Next
                    For Each roll_action As Dnd5ECharacter.Roll In cha_character.healing
                        str_actions_list = str_actions_list + "," + roll_action.name
                    Next
                    'Include spells not already presents as action
                    For Each str_spell As String In str_spell_list.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        If Not str_actions_list.ToLower.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(str_spell) Then
                            Dim int_count_roll As Integer = 0
                            For Each str_spell_name As String In Spells_attacks_name
                                If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                    For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_attacks(int_count_roll)
                                        Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                        str_cha_spell_roll.roll = str_cha_spell_roll.roll + "+" + str_spell_attack
                                        If str_cha_spell_roll.name.Contains(" lvl ") Then
                                            Dim int_start As Integer = InStr(str_cha_spell_roll.name, " lvl ")
                                            Dim str_spell_roll_lvl As String = Regex.Replace(Mid(str_cha_spell_roll.name, int_start, " lvl ".Length + 3), "[^0-9]", "")
                                            If Integer.Parse(str_spell_roll_lvl) <= Integer.Parse(str_spell_level) Then
                                                str_cha_spell_roll.name = Mid(str_cha_spell_roll.name, 1, int_start).Trim
                                                cha_character.attacks.Item(cha_character.attacks.Count - 1) = str_cha_spell_roll
                                            End If
                                        Else
                                            cha_character.attacks.Add(str_cha_spell_roll)
                                        End If
                                    Next
                                    Exit For
                                End If
                                int_count_roll = int_count_roll + 1
                            Next
                            int_count_roll = 0
                            For Each str_spell_name As String In Spells_attacksDC_name
                                If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                    For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_attacksDC(int_count_roll)
                                        Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                        If str_cha_spell_roll.roll <> "100" Then str_cha_spell_roll.roll = str_spell_dc + str_cha_spell_roll.roll
                                        If str_cha_spell_roll.name.Contains(" lvl ") Then
                                            Dim int_start As Integer = InStr(str_cha_spell_roll.name, " lvl ")
                                            Dim str_spell_roll_lvl As String = Regex.Replace(Mid(str_cha_spell_roll.name, int_start, " lvl ".Length + 3), "[^0-9]", "")
                                            If Integer.Parse(str_spell_roll_lvl) <= Integer.Parse(str_spell_level) Then
                                                str_cha_spell_roll.name = Mid(str_cha_spell_roll.name, 1, int_start).Trim
                                                cha_character.attacksDC.Item(cha_character.attacksDC.Count - 1) = str_cha_spell_roll
                                            End If
                                        Else
                                            cha_character.attacksDC.Add(str_cha_spell_roll)
                                        End If
                                    Next
                                    Exit For
                                End If
                                int_count_roll = int_count_roll + 1
                            Next
                            int_count_roll = 0
                            For Each str_spell_name As String In Spells_healing_name
                                If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                    For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_healing(int_count_roll)
                                        Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                        str_cha_spell_roll.roll = str_cha_spell_roll.roll + "+" + str_spell_mod
                                        cha_character.healing.Add(str_cha_spell_roll)
                                    Next
                                    Exit For
                                End If
                                int_count_roll = int_count_roll + 1
                            Next
                        End If
                    Next
                Next
            End If
            If bol_is_scroll Then
                Dim str_spell_dc As String = "8"
                Dim str_spell_attack As String = "0"
                Dim str_spell_mod As String = "0"
                Dim str_spell_level As String = "1"
                Dim str_spell_list As String = ""
                'Spellcaster DC
                Dim int_start As Integer = InStr(str_scroll_header, "@dc") + "@dc".Length
                Dim int_len As Integer = InStr(int_start, str_scroll_header, "}") - int_start
                If int_len > 0 Then str_spell_dc = Mid(str_scroll_header, int_start, int_len).Trim
                'Spellcaster attack
                int_start = InStr(str_scroll_header, "@hit") + "@hit".Length
                int_len = InStr(int_start, str_scroll_header, "}") - int_start
                If int_len > 0 Then str_spell_attack = Mid(str_scroll_header, int_start, int_len).Trim
                'Spellcaster level
                int_start = InStrRev(str_scroll_header, "level")
                If int_start > 0 Then
                    str_spell_level = Regex.Replace(Mid(str_scroll_header, int_start - 6, 6), "[^0-9]", "")
                End If
                If str_spell_level = "" Then str_spell_level = "1"
                'Spell list
                If str_scroll_text.ToLower.Contains("@spell ") Then
                    For int_count_spell As Integer = 2 To str_scroll_text.ToLower.Split("@spell ").Count
                        Dim str_spell As String = str_scroll_text.ToLower.Split("@spell ")(int_count_spell - 1)
                        int_len = InStr(str_spell, "}") - 1
                        str_spell_list = str_spell_list + "," + Mid(str_spell, 1, int_len).Trim
                    Next
                End If
                'Already included actions
                Dim str_actions_list As String = ""
                For Each roll_action As Dnd5ECharacter.Roll In cha_character.attacks
                    str_actions_list = str_actions_list + "," + roll_action.name
                Next
                For Each roll_action As Dnd5ECharacter.Roll In cha_character.attacksDC
                    str_actions_list = str_actions_list + "," + roll_action.name
                Next
                For Each roll_action As Dnd5ECharacter.Roll In cha_character.healing
                    str_actions_list = str_actions_list + "," + roll_action.name
                Next
                'Include spells not already presents as action
                For Each str_spell As String In str_spell_list.Split(",", StringSplitOptions.RemoveEmptyEntries)
                    If Not str_actions_list.ToLower.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(str_spell) Then
                        Dim int_count_roll As Integer = 0
                        For Each str_spell_name As String In Spells_attacks_name
                            If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_attacks(int_count_roll)
                                    Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                    str_cha_spell_roll.roll = str_cha_spell_roll.roll + "+" + str_spell_attack
                                    If str_cha_spell_roll.name.Contains(" lvl ") Then
                                        int_start = InStr(str_cha_spell_roll.name, " lvl ")
                                        Dim str_spell_roll_lvl As String = Regex.Replace(Mid(str_cha_spell_roll.name, int_start, " lvl ".Length + 3), "[^0-9]", "")
                                        If Integer.Parse(str_spell_roll_lvl) <= Integer.Parse(str_spell_level) Then
                                            str_cha_spell_roll.name = Mid(str_cha_spell_roll.name, 1, int_start).Trim
                                            cha_character.attacks.Item(cha_character.attacks.Count - 1) = str_cha_spell_roll
                                        End If
                                    Else
                                        cha_character.attacks.Add(str_cha_spell_roll)
                                    End If
                                Next
                                Exit For
                            End If
                            int_count_roll = int_count_roll + 1
                        Next
                        int_count_roll = 0
                        For Each str_spell_name As String In Spells_attacksDC_name
                            If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_attacksDC(int_count_roll)
                                    Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                    If str_cha_spell_roll.roll <> "100" Then str_cha_spell_roll.roll = str_spell_dc + str_cha_spell_roll.roll
                                    If str_cha_spell_roll.name.Contains(" lvl ") Then
                                        int_start = InStr(str_cha_spell_roll.name, " lvl ")
                                        Dim str_spell_roll_lvl As String = Regex.Replace(Mid(str_cha_spell_roll.name, int_start, " lvl ".Length + 3), "[^0-9]", "")
                                        If Integer.Parse(str_spell_roll_lvl) <= Integer.Parse(str_spell_level) Then
                                            str_cha_spell_roll.name = Mid(str_cha_spell_roll.name, 1, int_start).Trim
                                            cha_character.attacksDC.Item(cha_character.attacksDC.Count - 1) = str_cha_spell_roll
                                        End If
                                    Else
                                        cha_character.attacksDC.Add(str_cha_spell_roll)
                                    End If
                                Next
                                Exit For
                            End If
                            int_count_roll = int_count_roll + 1
                        Next
                        int_count_roll = 0
                        For Each str_spell_name As String In Spells_healing_name
                            If str_spell_name.ToLower = str_spell.ToLower.Trim Then
                                For Each str_spell_roll As Dnd5ECharacter.Roll In Spells_healing(int_count_roll)
                                    Dim str_cha_spell_roll As New Dnd5ECharacter.Roll(str_spell_roll)
                                    str_cha_spell_roll.roll = str_cha_spell_roll.roll + "+" + str_spell_mod
                                    cha_character.healing.Add(str_cha_spell_roll)
                                Next
                                Exit For
                            End If
                            int_count_roll = int_count_roll + 1
                        Next
                    End If
                Next
            End If
            '4) Add character to colecction
            If Characters_name.Contains(str_monster_name) Then
                Characters_name.Add(str_monster_name + " (" + str_monster_source + ")")
            Else
                Characters_name.Add(str_monster_name)
            End If
            Characters_source.Add(str_monster_source)
            Characters.Add(cha_character)
            Characters_input.Add(obj_monster)
            Return 0
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
            Return 0
        End Try
    End Function

    Function proc_SpellBuilder(obj_spell As JObject)
        Try
            '1) Initial variable allocation
            '1.1) String
            Dim str_spell_name As String
            Dim str_spell_source As String
            Dim str_spell_level As String
            Dim str_spell_type As String = ""
            Dim str_spell_maintext As String = ""
            Dim str_spell_roll_DC As String = ""
            '1.2) Bolean
            Dim bol_is_noattackroll As Boolean = False
            Dim bol_is_attackwithDCroll As Boolean = False
            '1.3) Others
            Dim spell As New List(Of Dnd5ECharacter.Roll)
            '2) Spell characteristics
            '2.1) Name
            str_spell_name = obj_spell("name").ToString.Replace(Chr(34), "").Replace("\", "").Replace("/", "-").Replace("*", "").Replace("""", "")
            '2.2) Source
            str_spell_source = obj_spell("source").ToString.Replace(":", "")
            '2.3) Level
            str_spell_level = obj_spell("level")
            '2.4) Description
            If obj_spell("entries") IsNot Nothing Then
                If obj_spell.Item("entries").Count > 0 Then
                    For int_count_entries As Integer = 1 To obj_spell.Item("entries").Count
                        If obj_spell.Item("entries").Item(int_count_entries - 1).Type = JTokenType.String Then
                            str_spell_maintext = str_spell_maintext.Trim + " " + obj_spell.Item("entries").Item(int_count_entries - 1).ToString.ToLower.Replace("@b ", "@name ").Replace("@bold ", "@name ")
                        ElseIf obj_spell.Item("entries").Item(int_count_entries - 1).Type = JTokenType.Object Then
                            If obj_spell.Item("entries").Item(int_count_entries - 1).Item("type") IsNot Nothing Then
                                If obj_spell.Item("entries").Item(int_count_entries - 1).Item("type") = "entries" Then
                                    If obj_spell.Item("entries").Item(int_count_entries - 1).Item("name") IsNot Nothing Then
                                        Dim str_entries_name As String = obj_spell.Item("entries").Item(int_count_entries - 1).Item("name").ToString.ToLower
                                        For int_count_subentries As Integer = 1 To obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Count
                                            If int_count_subentries = 1 Then
                                                str_spell_maintext = str_spell_maintext.Trim + " " + "{@name " + str_entries_name + "} " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(int_count_subentries - 1).ToString.ToLower
                                            Else
                                                str_spell_maintext = str_spell_maintext.Trim + " " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(int_count_subentries - 1).ToString.ToLower
                                            End If
                                        Next
                                    ElseIf obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(0).Item("entries").Item(0).Item("name") IsNot Nothing Then
                                        Dim str_entries_name As String = obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(0).Item("entries").Item(0).Item("name").ToString.ToLower
                                        For int_count_subentries As Integer = 1 To obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(0).Item("entries").Item(0).Item("entries").Count
                                            If int_count_subentries = 1 Then
                                                str_spell_maintext = str_spell_maintext.Trim + " " + "{@name " + str_entries_name + "} " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(0).Item("entries").Item(0).Item("entries").Item(int_count_subentries - 1).ToString.ToLower
                                            Else
                                                str_spell_maintext = str_spell_maintext.Trim + " " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("entries").Item(0).Item("entries").Item(0).Item("entries").Item(int_count_subentries - 1).ToString.ToLower
                                            End If
                                        Next
                                    End If
                                ElseIf obj_spell.Item("entries").Item(int_count_entries - 1).Item("type") = "table" Then
                                    If obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Count > 0 Then
                                        For int_count_subentries As Integer = 1 To obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Count
                                            If obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).ToString.Contains("roll") Then
                                                Dim str_entries_name As String = ""
                                                If obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(0).Item("roll").Item("exact") IsNot Nothing Then
                                                    str_entries_name = obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(0).Item("roll").Item("exact").ToString
                                                ElseIf obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(0).Item("roll").Item("min") IsNot Nothing Then
                                                    str_entries_name = obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(0).Item("roll").Item("min").ToString + "-" + obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(0).Item("roll").Item("max").ToString
                                                End If
                                                str_spell_maintext = str_spell_maintext.Trim + " " + "{@name " + str_entries_name + "} " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(1).ToString.ToLower
                                            Else
                                                For int_count_subsubentries As Integer = 1 To obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Count
                                                    If int_count_subsubentries = 1 And Not obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(int_count_subsubentries - 1).ToString.ToLower.Replace("@b ", "@name ").Replace("@bold ", "@name ").Contains("@name") Then
                                                        str_spell_maintext = str_spell_maintext.Trim + " {@name }" + obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(int_count_subsubentries - 1).ToString.ToLower.Replace("@b ", "@name ").Replace("@bold ", "@name ")
                                                    Else
                                                        str_spell_maintext = str_spell_maintext.Trim + " " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("rows").Item(int_count_subentries - 1).Item(int_count_subsubentries - 1).ToString.ToLower.Replace("@b ", "@name ").Replace("@bold ", "@name ")
                                                    End If
                                                Next
                                            End If
                                        Next
                                    End If
                                ElseIf obj_spell.Item("entries").Item(int_count_entries - 1).Item("type") = "list" Then
                                    For int_count_subentries As Integer = 1 To obj_spell.Item("entries").Item(int_count_entries - 1).Item("items").Count
                                        str_spell_maintext = str_spell_maintext.Trim + " " + obj_spell.Item("entries").Item(int_count_entries - 1).Item("items").Item(int_count_subentries - 1).ToString.ToLower.Replace("@b ", "@name ").Replace("@bold ", "@name ")
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If
                If str_spell_maintext <> "" Then If Mid(str_spell_maintext, str_spell_maintext.Length, 1) <> "." Then str_spell_maintext = str_spell_maintext + "."
            End If
            '2.5) Type (versus AC, DC o healing)
            If obj_spell.Item("spellAttack") IsNot Nothing Then
                str_spell_type = "attacks"
                If obj_spell.Item("savingThrow") IsNot Nothing Then bol_is_attackwithDCroll = True
            ElseIf obj_spell.Item("savingThrow") IsNot Nothing Then
                str_spell_type = "attacksDC"
                If str_spell_maintext.Contains("next") And str_spell_maintext.Contains("weapon attack") Then bol_is_noattackroll = True
                If (obj_spell.Item("savingThrow").Count > 1 And Not str_spell_maintext.Contains("{@name")) Or bol_is_noattackroll = True Then bol_is_attackwithDCroll = True
            ElseIf str_spell_maintext.Contains("regain") And str_spell_maintext.Contains("hit point") Then
                str_spell_type = "healing"
            ElseIf str_spell_maintext.Contains("next") And str_spell_maintext.Contains("weapon attack") Then
                str_spell_type = "attacksDC"
                bol_is_noattackroll = True
            ElseIf obj_spell.Item("damageInflict") IsNot Nothing And str_spell_maintext.Contains("@damage") Then
                str_spell_type = "attacksDC"
                bol_is_noattackroll = True
            End If
            '3) Spell roll
            If str_spell_type <> "" Then
                Dim str_spell_text_split() As String
                Dim bol_is_check As Boolean = False
                Dim str_roll_prev As String = ""
                str_spell_text_split = str_spell_maintext.Split("{@name ")
                For int_count_text As Integer = 1 To str_spell_text_split.Count
                    Dim str_spell_text As String = str_spell_text_split(int_count_text - 1)
                    Dim roll_spell As New Dnd5ECharacter.Roll
                    '3.1) Name
                    If int_count_text = 1 Then
                        roll_spell.name = str_spell_name
                    Else
                        bol_is_attackwithDCroll = False
                        Dim str_spell_extra_name As String = ""
                        Dim int_len As Integer = InStr(str_spell_text, "}") - 1
                        If int_len > 0 Then str_spell_extra_name = Mid(str_spell_text, 1, int_len).Trim.Replace(".", "")
                        If str_spell_extra_name <> "" Then
                            roll_spell.name = str_spell_name + " (" + str_spell_extra_name + ")"
                        Else
                            roll_spell.name = str_spell_name + " (option " + (int_count_text - 1).ToString + ")"
                        End If
                    End If
                    '3.1) Range
                    Dim str_spell_range As String = ""
                    If obj_spell.Item("range") IsNot Nothing Then
                        If obj_spell.Item("range").Item("distance") IsNot Nothing Then
                            If obj_spell.Item("range").Item("distance").Item("amount") IsNot Nothing Then
                                str_spell_range = obj_spell.Item("range").Item("distance").Item("amount").ToString
                                If obj_spell.Item("range").Item("distance").Item("type") IsNot Nothing Then
                                    If obj_spell.Item("range").Item("distance").Item("type").ToString.ToLower = "miles" Then
                                        str_spell_range = (Integer.Parse(str_spell_range) * 1000).ToString
                                    End If
                                End If
                            ElseIf obj_spell.Item("range").Item("distance").Item("type") IsNot Nothing Then
                                If obj_spell.Item("range").Item("distance").Item("type").ToString.ToLower = "touch" Then
                                    str_spell_range = "5"
                                ElseIf obj_spell.Item("range").Item("distance").Item("type").ToString.ToLower = "self" Then
                                    If (str_spell_text.Contains("next") Or str_spell_text.Contains("your")) And
                                        str_spell_text.Contains("melee weapon attack") Then
                                        str_spell_range = "5"
                                    ElseIf (str_spell_text.Contains("next") Or str_spell_text.Contains("your")) And
                                    str_spell_text.Contains("weapon attack") Then
                                        str_spell_range = "120"
                                    ElseIf str_spell_text.Contains("feet") Then
                                        str_spell_range = Regex.Replace(Mid(str_spell_text, Math.Max(InStr(str_spell_text, "feet") - 7, 1), Math.Min(7, str_spell_text.Length)), "[^0-9]", "")
                                    ElseIf str_spell_text.Contains("-foot") Then
                                        str_spell_range = Regex.Replace(Mid(str_spell_text, Math.Max(InStr(str_spell_text, "-foot") - 7, 1), Math.Min(7, str_spell_text.Length)), "[^0-9]", "")
                                    ElseIf str_spell_text.Contains("reach") Then
                                        str_spell_range = "5"
                                    Else
                                        str_spell_range = "1"
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If str_spell_range = "" Then str_spell_range = "120"
                    roll_spell.range = str_spell_range + "/" + str_spell_range
                    '3.2) Type
                    roll_spell.type = "Magic"
                    '3.3) Critical range and damage multilied
                    If str_spell_type = "attacks" Then
                        roll_spell.critrangemin = "20"
                        roll_spell.critmultip = "2"
                    End If
                    '3.4) Roll
                    Dim str_spell_roll As String = ""
                    If int_count_text = 1 Then
                        If bol_is_noattackroll = True Then
                            str_spell_roll = "100"
                        ElseIf str_spell_type = "attacks" Then
                            str_spell_roll = "1D20"
                        ElseIf str_spell_type = "attacksDC" Then
                            If str_spell_text.Contains("half") Or str_spell_text.Contains("halve") Then
                                str_spell_roll = "/" + Mid(obj_spell.Item("savingThrow").Item(0), 1, 3).ToUpper + "/half"
                            Else
                                str_spell_roll = "/" + Mid(obj_spell.Item("savingThrow").Item(0), 1, 3).ToUpper + "/zero"
                            End If
                        ElseIf str_spell_type = "healing" Then
                            If str_spell_text.Contains("@dice") Then
                                Dim int_start As Integer = InStr(str_spell_text, "@dice") + "@dice".Length
                                Dim int_len As Integer = InStr(int_start, str_spell_text, "}") - int_start
                                str_spell_roll = Mid(str_spell_text, int_start, int_len).Replace("×", "*").Trim
                                If Mid(str_spell_roll, 1, 1) = "d" Then str_spell_roll = ""
                            End If
                            If str_spell_roll = "" Then
                                Dim int_start As Integer = InStr(str_spell_text, "regain") + "regain".Length
                                Dim int_len As Integer = InStr(int_start, str_spell_text, "hit point") - int_start
                                If int_len > 0 And int_len < 80 Then
                                    str_spell_roll = Regex.Replace(Mid(str_spell_text, int_start, int_len), "[^0-9]", "")
                                End If
                                If str_spell_roll = "0" Then str_spell_roll = ""
                            End If
                        End If
                        roll_spell.roll = str_spell_roll
                        If bol_is_attackwithDCroll Then
                            Dim str_spell_save As String
                            If str_spell_type = "attacks" Or bol_is_noattackroll Then
                                str_spell_save = Mid(obj_spell.Item("savingThrow").Item(0), 1, 3).ToUpper
                            Else
                                str_spell_save = Mid(obj_spell.Item("savingThrow").Item(1), 1, 3).ToUpper
                            End If
                            If str_spell_text.Contains("half") Or str_spell_text.Contains("halve") Then
                                str_spell_roll_DC = "/" + str_spell_save + "/half"
                            Else
                                str_spell_roll_DC = "/" + str_spell_save + "/zero"
                            End If
                        End If
                    Else
                        If str_spell_type = "healing" Then
                            If str_spell_text.Contains("regain") And str_spell_text.Contains("hit point") Then
                                If str_spell_text.Contains("@dice") Then
                                    Dim int_start As Integer = InStr(str_spell_text, "@dice") + "@dice".Length
                                    Dim int_len As Integer = InStr(int_start, str_spell_text, "}") - int_start
                                    str_spell_roll = Mid(str_spell_text, int_start, int_len).Replace("×", "*").Trim
                                    If Mid(str_spell_roll, 1, 1) = "d" Then str_spell_roll = ""
                                End If
                                If str_spell_roll = "" Then
                                    Dim int_start As Integer = InStr(str_spell_text, "regain") + "regain".Length
                                    Dim int_len As Integer = InStr(int_start, str_spell_text, "hit point") - int_start
                                    If int_len > 0 And int_len < 80 Then
                                        str_spell_roll = Regex.Replace(Mid(str_spell_text, int_start, int_len), "[^0-9]", "")
                                    End If
                                End If
                            End If
                        ElseIf str_roll_prev <> "" Then
                            str_spell_roll = str_roll_prev
                        ElseIf str_spell_text.Contains("saving throw") And Not str_spell_text.Contains("disadvantage on") And
                               Not str_spell_text.Contains("advantage on") Then
                            Dim str_spell_save As String = ""
                            Dim str_spell_stats() As String = {"strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"}
                            For Each str_spell_stat In str_spell_stats
                                If str_spell_text.Contains(str_spell_stat) Then
                                    str_spell_save = Mid(str_spell_stat, 1, 3).ToUpper
                                    Exit For
                                End If
                            Next
                            If str_spell_text.Contains("half") Or str_spell_text.Contains("halve") Then
                                str_spell_roll = "/" + str_spell_save + "/half"
                            Else
                                str_spell_roll = "/" + str_spell_save + "/zero"
                            End If
                        ElseIf str_spell_text.Contains("save") And Not str_spell_text.Contains("save dc") And
                            Not str_spell_text.Contains("disadvantage on") And
                            Not str_spell_text.Contains("advantage on") And spell.Count > 0 Then
                            str_spell_roll = spell(0).roll
                            If Not str_spell_roll.Contains("/") Then str_spell_roll = "100"
                        Else
                            str_spell_roll = "100"
                        End If
                        roll_spell.roll = str_spell_roll
                    End If
                    '3.5) Conditions
                    Dim str_spell_condition As String = ""
                    For int_count_condition As Integer = 1 To str_spell_text.Split("@condition").Length - 1
                        Dim str_condition As String = str_spell_text.Split("@condition")(int_count_condition)
                        Dim int_len As Integer = InStr(str_condition, "}") - 1
                        If int_len > 0 Then
                            If str_spell_condition = "" Then
                                str_spell_condition = Mid(str_condition, 1, int_len).Trim
                            ElseIf Not str_spell_condition.Contains(Mid(str_condition, 1, int_len).Trim) Then
                                str_spell_condition = str_spell_condition + "|" + Mid(str_condition, 1, int_len).Trim
                            End If
                        End If
                    Next
                    roll_spell.conditions = str_spell_condition
                    '3.6) Damage
                    str_roll_prev = ""
                    Dim str_dmg_elem_types() As String = {"acid", "cold", "fire", "force", "lightning", "necrotic", "poison", "psychic", "radiant", "thunder"}
                    Dim str_dmg_types() As String = {"acid", "bludgeoning", "cold", "fire", "force", "lightning", "necrotic", "piercing", "poison", "psychic", "radiant", "slashing", "thunder"}
                    If str_spell_type = "attacks" Or str_spell_type = "attacksDC" And roll_spell.roll <> "" Then
                        If str_spell_text.Contains("@damage") Or str_spell_text.Contains("@dice") Then
                            Dim str_key As String = "@damage"
                            If Not str_spell_text.Contains("@damage") Then str_key = "@dice"
                            Dim str_spell_dmgs() As String = str_spell_text.Split(str_key)
                            Dim int_dmgs = str_spell_dmgs.Length - 1
                            Dim int_count_dmg As Integer = 0
                            Dim int_skip_dmg As Integer = 0
                            Dim int_count_lvl As Integer = 0
                            Dim int_dmg_type_extra As Integer = 0
                            Dim str_dmg_extra_types() As String
                            While (int_count_dmg < int_dmgs - int_skip_dmg)
                                int_count_dmg = int_count_dmg + 1
                                Dim str_dmg_text_ini As String = str_spell_dmgs(int_count_dmg + int_skip_dmg - 1)
                                Dim str_dmg_text_end As String = str_spell_dmgs(int_count_dmg + int_skip_dmg)
                                'a) Damage roll
                                Dim int_len As Integer = InStr(str_dmg_text_end, "}") - 1
                                Dim str_dmg_roll As String = Mid(str_dmg_text_end, 1, int_len).Replace("×", "*").Replace(" ", "").Trim
                                If Mid(str_dmg_roll, 1, 1) = "d" Then str_dmg_roll = "0"
                                'b) Damage type
                                Dim int_start As Integer = int_len + 3
                                int_len = InStr(int_start, str_dmg_text_end, " damage") - int_start
                                Dim str_dmg_type As String = ""
                                If int_len > 0 Then
                                    str_dmg_type = Mid(str_dmg_text_end, int_start, int_len).Trim
                                Else
                                    int_len = InStr(int_start, str_dmg_text_end, " ") - int_start
                                    If int_len > 0 Then
                                        str_dmg_type = Mid(str_dmg_text_end, int_start, int_len).Trim
                                    End If
                                End If
                                If str_dmg_type.Contains("or ") And int_dmg_type_extra = 0 Then
                                    While str_dmg_type.Contains("(")
                                        str_dmg_type = Mid(str_dmg_type, 1, InStr(str_dmg_type, "(") - 1) + Mid(str_dmg_type, InStr(str_dmg_type, ")") + 1, str_dmg_type.Length)
                                    End While
                                    str_dmg_extra_types = str_dmg_type.Replace(", or", "or").Split({"or ", ","}, StringSplitOptions.RemoveEmptyEntries)
                                    For int_count_type As Integer = 1 To str_dmg_extra_types.Count
                                        If Not str_dmg_types.Contains(str_dmg_extra_types(int_count_type - 1).Trim) Then str_dmg_extra_types(int_count_type - 1) = ""
                                    Next
                                    str_dmg_type = String.Join(",", str_dmg_extra_types)
                                    str_dmg_extra_types = str_dmg_type.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                    If str_dmg_extra_types.Length > 1 Then
                                        str_dmg_type = str_dmg_extra_types(0).Trim
                                        int_dmg_type_extra = int_count_dmg
                                    End If
                                End If
                                If Not str_dmg_types.Contains(str_dmg_type) Then str_dmg_type = ""
                                If str_dmg_type = "" And (str_dmg_text_end.Contains("type") And str_dmg_text_end.Contains("chose")) Then
                                    If str_dmg_text_end.Contains("ammunition") Then
                                        str_dmg_extra_types = "piercing,bludgeoning,slashing".Split(",")
                                    Else
                                        Dim str_dmgs As String = ""
                                        For Each str_type In str_dmg_types
                                            If str_dmg_text_end.Contains(str_type) Then
                                                str_dmgs = str_dmgs + "," + str_type
                                            End If
                                        Next
                                        If str_dmgs = "" Then
                                            For Each str_type In str_dmg_types
                                                If str_dmg_text_ini.Contains(str_type) Then
                                                    str_dmgs = str_dmgs + "," + str_type
                                                End If
                                            Next
                                        End If
                                        If str_dmgs <> "" Then
                                            str_dmg_extra_types = str_dmgs.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                        Else
                                            str_dmg_extra_types = str_dmg_elem_types
                                        End If
                                    End If
                                    str_dmg_type = str_dmg_extra_types(0)
                                    int_dmg_type_extra = int_count_dmg
                                End If
                                If str_dmg_type = "" Then
                                    For Each str_type As String In str_dmg_types
                                        If Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 40, 1), Math.Min(str_dmg_text_ini.Length, 40)).Contains(str_type) Then
                                            str_dmg_type = str_type
                                            Exit For
                                        End If
                                    Next
                                End If
                                If {"bludgeoning", "slashing", "piercing"}.Contains(str_dmg_type) Then str_dmg_type = "magic " + str_dmg_type
                                If str_dmg_type = "" And str_key = "@dice" And str_spell_level <> "0" Then str_dmg_roll = "0"
                                'c) Icon based on damage
                                Dim str_spell_icon As String
                                If str_dmg_type <> "" Then
                                    If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_dmg_type) Then
                                        str_spell_icon = "ex_MAGIC_" + str_dmg_type.ToUpper.Split(" ")(1)
                                    Else
                                        str_spell_icon = "ex_" + str_dmg_type.ToUpper
                                    End If
                                Else
                                    str_spell_icon = "_Magic"
                                End If
                                If int_count_dmg = 1 And roll_spell.futureUse_icon = "" Then
                                    roll_spell.futureUse_icon = str_spell_icon
                                End If
                                'd) Add actions
                                Dim int_end As Integer = str_dmg_text_end.Length
                                If InStr(str_dmg_text_end, ".") > 0 Then int_end = InStr(str_dmg_text_end, ".")
                                If InStr(str_dmg_text_end, " and ") > 0 And InStr(str_dmg_text_end, " and ") < int_end Then int_end = InStr(str_dmg_text_end, " and ")
                                Dim int_ini As Integer = InStrRev(str_dmg_text_ini, ".") + 1
                                If str_spell_level = "0" And
                                   Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 8, 1), Math.Min(8, str_dmg_text_ini.Length)).Contains("or ") And
                                   Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 16, 1), Math.Min(16, str_dmg_text_ini.Length)).Contains("increase by ") And
                                   Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 16, 1), Math.Min(16, str_dmg_text_ini.Length)).Contains("increases by ") And
                                   Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 16, 1), Math.Min(16, str_dmg_text_ini.Length)).Contains("an additional ") And
                                   (((Mid(str_dmg_text_end, 1, int_end).Contains("5th level") Or
                                   Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length).Contains("5th level")) And
                                   int_count_lvl = 0) Or
                                   ((Mid(str_dmg_text_end, 1, int_end).Contains("11th level") Or
                                   Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length).Contains("11th level")) And
                                   int_count_lvl = 1) Or
                                   ((Mid(str_dmg_text_end, 1, int_end).Contains("17th level") Or
                                   Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length).Contains("17th level")) And
                                   int_count_lvl = 2)) Then
                                    int_count_lvl = int_count_lvl + 1
                                    Dim ab As String = Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 8, 1), Math.Min(8, str_dmg_text_ini.Length))
                                    If int_count_lvl = 1 And roll_spell.link Is Nothing Then
                                        Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                        roll_spell_dmg.name = str_spell_name
                                        roll_spell_dmg.roll = "0"
                                        roll_spell_dmg.type = ""
                                        roll_spell.link = roll_spell_dmg
                                    End If
                                    If int_count_lvl = 1 And int_skip_dmg = 0 Then spell.Add(roll_spell)
                                    Dim roll_spell_extra As New Dnd5ECharacter.Roll(roll_spell)
                                    roll_spell_extra.name = str_spell_name + " lvl " + "5 11 17".Split(" ")(int_count_lvl - 1)
                                    roll_spell_extra.link.roll = str_dmg_roll
                                    If roll_spell_extra.link.type = "" Then roll_spell_extra.link.type = str_dmg_type
                                    If roll_spell_extra.futureUse_icon = "" Then roll_spell_extra.futureUse_icon = "ex_" + str_spell_type.ToUpper
                                    spell.Add(roll_spell_extra)
                                    int_skip_dmg = int_skip_dmg + 1
                                    int_count_dmg = int_count_dmg - 1
                                ElseIf str_spell_level = "0" And int_count_lvl > 0 And
                                    Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 16, 1), Math.Min(16, str_dmg_text_ini.Length)).Contains("increase by ") Then
                                    Dim roll_spell_extra As New Dnd5ECharacter.Roll(spell(spell.Count - 1))
                                    Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                    roll_spell_dmg.name = str_spell_name
                                    roll_spell_dmg.roll = str_dmg_roll
                                    If str_dmg_type <> "" Then
                                        roll_spell_dmg.type = str_dmg_type
                                    Else
                                        roll_spell_dmg.type = roll_spell_extra.type
                                    End If
                                    roll_spell_extra.name = roll_spell_extra.name + " (if condition)"
                                    roll_spell_extra.link = roll_spell_dmg
                                    spell.Add(roll_spell_extra)
                                ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("at the start") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the start") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("at the end") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the end") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("starts its") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("starts its") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("ends its") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("ends its") Or
                                        ((Mid(str_dmg_text_end, 1, int_end).Contains("while ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains(" while ")) And
                                        (Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 12, 1), Math.Min(12, str_dmg_text_ini.Length)).Contains("increase") And
                                        Not Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 12, 1), Math.Min(12, str_dmg_text_ini.Length)).Contains("additional"))) Then
                                    Dim roll_spell_dmg_extra As New Dnd5ECharacter.Roll
                                    roll_spell_dmg_extra.name = str_spell_name
                                    roll_spell_dmg_extra.roll = str_dmg_roll
                                    roll_spell_dmg_extra.type = str_dmg_type
                                    If str_spell_roll = "100" Or str_dmg_roll = "0" Then
                                        roll_spell.link = roll_spell_dmg_extra
                                    Else
                                        Dim int_save_pos As Integer = InStr(str_dmg_text_ini, "saving ")
                                        Dim int_sep_pos As Integer = InStrRev(str_dmg_text_ini, ".")
                                        Dim int_save_end_pos As Integer = InStrRev(str_dmg_text_ini, "saving ")
                                        If int_count_dmg = 1 And int_skip_dmg = 0 And int_save_pos < int_sep_pos And int_save_pos > 0 Then
                                            Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                            roll_spell_dmg.name = str_spell_name
                                            roll_spell_dmg.roll = "0"
                                            roll_spell_dmg.type = ""
                                            roll_spell.link = roll_spell_dmg
                                            roll_spell.futureUse_icon = "_Magic"
                                            spell.Add(roll_spell)
                                        End If
                                        If int_skip_dmg = 0 And int_count_dmg > 1 Then
                                            spell.Add(roll_spell)
                                        End If
                                        Dim roll_spell_extra As New Dnd5ECharacter.Roll
                                        If Mid(str_dmg_text_end, 1, int_end).Contains("at the start") Or
                                            Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("at the start") Or
                                            Mid(str_dmg_text_end, 1, int_end).Contains("starts its") Or
                                            Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("starts its") Then
                                            roll_spell_extra.name = roll_spell.name + " (start turn)"
                                        ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("while ") Or
                                               Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains(" while ") Then
                                            roll_spell_extra.name = roll_spell.name + " (if condition)"
                                        Else
                                            roll_spell_extra.name = roll_spell.name + " (end turn)"
                                        End If
                                        roll_spell_extra.type = "Magic"
                                        roll_spell_extra.range = "120/120"
                                        If int_save_end_pos > int_sep_pos Then
                                            roll_spell_extra.roll = roll_spell.roll
                                        Else
                                            roll_spell_extra.roll = "100"
                                        End If
                                        roll_spell_extra.link = roll_spell_dmg_extra
                                        roll_spell_extra.futureUse_icon = str_spell_icon
                                        spell.Add(roll_spell_extra)
                                        int_skip_dmg = int_skip_dmg + 1
                                        int_count_dmg = int_count_dmg - 1
                                    End If
                                ElseIf int_count_dmg > 1 And (Mid(str_dmg_text_end, 1, int_end).Contains("if ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("if ") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("while ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains(" while ")) Or
                                        Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 6, 1), Math.Min(6, str_dmg_text_ini.Length)).Contains("or ") Then
                                    If Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 12, 1), Math.Min(12, str_dmg_text_ini.Length)).Contains("extra") Or
                                        Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 12, 1), Math.Min(12, str_dmg_text_ini.Length)).Contains("increase") Or
                                        Mid(str_dmg_text_ini, Math.Max(str_dmg_text_ini.Length - 12, 1), Math.Min(12, str_dmg_text_ini.Length)).Contains("additional") Then
                                        Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                        roll_spell_dmg.name = str_spell_name
                                        roll_spell_dmg.roll = str_dmg_roll
                                        roll_spell_dmg.type = str_dmg_type
                                        If int_count_dmg = 1 Then
                                            roll_spell.link = roll_spell_dmg
                                        Else
                                            spell.Add(roll_spell)
                                            Dim roll_spell_extra As New Dnd5ECharacter.Roll(roll_spell)
                                            roll_spell_extra.name = roll_spell.name + " (if condition)"
                                            Dim roll_link As Dnd5ECharacter.Roll
                                            roll_link = roll_spell_extra
                                            While roll_link.link IsNot Nothing
                                                roll_link = roll_link.link
                                            End While
                                            roll_link.link = roll_spell_dmg
                                            roll_spell_extra.futureUse_icon = str_spell_icon
                                            spell.Add(roll_spell_extra)
                                        End If
                                    Else
                                        Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                        roll_spell_dmg.name = str_spell_name
                                        roll_spell_dmg.roll = str_dmg_roll
                                        If str_dmg_type <> "" Then roll_spell_dmg.type = str_dmg_type
                                        If int_count_dmg = 1 Then
                                            roll_spell.link = roll_spell_dmg
                                        Else
                                            spell.Add(roll_spell)
                                            Dim roll_spell_extra As New Dnd5ECharacter.Roll(roll_spell)
                                            roll_spell_extra.name = roll_spell.name + " (if condition)"
                                            roll_spell_extra.link = roll_spell_dmg
                                            roll_spell_extra.futureUse_icon = str_spell_icon
                                            spell.Add(roll_spell_extra)
                                        End If
                                    End If
                                    If int_count_dmg > 1 Then
                                        int_skip_dmg = int_skip_dmg + 1
                                        int_count_dmg = int_count_dmg - 1
                                    End If
                                ElseIf Mid(str_dmg_text_end, 1, int_end).Contains("each creature ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("each creature ") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("every creature ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("every creature ") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("all creatures ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("all creatures ") Or
                                        Mid(str_dmg_text_end, 1, int_end).Contains("any creature ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("any creatures ") Or
                                        (Mid(str_dmg_text_end, 1, int_end).Contains("each ") And
                                        Mid(str_dmg_text_end, 1, int_end).Contains("creature")) Or
                                        (Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("each") And
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("creature")) Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("enters ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("moving through") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("wall ") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("as an action") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("as a bonus action") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("your action") Or
                                        Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("your bonus action") Then
                                    Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                    roll_spell_dmg.name = str_spell_name
                                    roll_spell_dmg.roll = str_dmg_roll
                                    roll_spell_dmg.type = str_dmg_type
                                    If int_count_dmg = 1 Then
                                        roll_spell.link = roll_spell_dmg
                                    Else
                                        spell.Add(roll_spell)
                                        Dim roll_spell_extra As New Dnd5ECharacter.Roll(roll_spell)
                                        If Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("as an action") Or
                                            Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("as a bonus action") Or
                                            Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("your action") Or
                                            Mid(str_dmg_text_ini, int_ini, str_dmg_text_ini.Length - int_ini).Contains("your bonus action") Then
                                            roll_spell_extra.name = roll_spell.name + " (as action)"
                                        Else
                                            roll_spell_extra.name = roll_spell.name + " (in zone)"
                                        End If
                                        roll_spell_extra.link = roll_spell_dmg
                                        roll_spell_extra.futureUse_icon = str_spell_icon
                                        spell.Add(roll_spell_extra)
                                        int_skip_dmg = int_skip_dmg + 1
                                        int_count_dmg = int_count_dmg - 1
                                    End If
                                ElseIf (str_spell_roll = "100" Or str_spell_roll = "1D20") And str_dmg_roll = "0" And int_count_text > 1 Then
                                    int_skip_dmg = int_skip_dmg + 1
                                    int_count_dmg = int_count_dmg - 1
                                Else
                                    Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                    roll_spell_dmg.name = str_spell_name
                                    roll_spell_dmg.roll = str_dmg_roll
                                    roll_spell_dmg.type = str_dmg_type
                                    Dim roll_link As Dnd5ECharacter.Roll
                                    roll_link = roll_spell
                                    While roll_link.link IsNot Nothing
                                        roll_link = roll_link.link
                                    End While
                                    roll_link.link = roll_spell_dmg
                                End If
                            End While
                            If int_count_dmg = int_dmgs Then
                                spell.Add(roll_spell)
                                If int_dmg_type_extra > 0 Then
                                    For Each str_type As String In str_dmg_extra_types
                                        If str_type <> str_dmg_extra_types(0) Then
                                            Dim roll_spell_dmgtype As New Dnd5ECharacter.Roll(roll_spell)
                                            Dim roll_link As New Dnd5ECharacter.Roll
                                            roll_link = roll_spell_dmgtype
                                            roll_link.name = roll_spell.name + " (" + str_type.Trim + ")"
                                            For int_count_link As Integer = 1 To int_dmg_type_extra
                                                roll_link = roll_link.link
                                            Next
                                            If roll_link IsNot Nothing Then
                                                roll_link.type = str_type.Trim
                                                If roll_spell_dmgtype.futureUse_icon.ToLower.Contains(str_dmg_extra_types(0)) Then
                                                    If {"magic bludgeoning", "magic piercing", "magic slashing"}.Contains(str_type) Then
                                                        roll_spell_dmgtype.futureUse_icon = "ex_MAGIC_" + str_type.ToUpper.Split(" ")(1)
                                                    Else
                                                        roll_spell_dmgtype.futureUse_icon = "ex_" + str_type.ToUpper
                                                    End If
                                                End If
                                                spell.Add(roll_spell_dmgtype)
                                            End If
                                        End If
                                    Next
                                    roll_spell.name = roll_spell.name + " (" + str_dmg_extra_types(0).Trim + ")"
                                End If
                                If bol_is_attackwithDCroll Then
                                    Dim roll_spell_save As New Dnd5ECharacter.Roll(roll_spell)
                                    roll_spell_save.name = roll_spell.name + " (saving)"
                                    roll_spell_save.roll = str_spell_roll_DC
                                    If roll_spell_save.critmultip <> "" Then roll_spell_save.critmultip = ""
                                    If roll_spell_save.critrangemin <> "" Then roll_spell_save.critrangemin = ""
                                    Dim roll_spell_save_dmg As New Dnd5ECharacter.Roll
                                    roll_spell_save_dmg.name = str_spell_name
                                    roll_spell_save_dmg.roll = "0"
                                    roll_spell_save_dmg.type = ""
                                    roll_spell_save.link = roll_spell_save_dmg
                                    roll_spell.futureUse_icon = "_Magic"
                                    spell.Add(roll_spell_save)
                                End If
                            End If
                        ElseIf str_spell_range <> "1" And Not str_spell_text.Contains("+ your spellcasting ability") And
                               bol_is_noattackroll = False And (int_count_text = 1 Or
                               (int_count_text > 1 And str_spell_roll.Contains("/"))) Then
                            If str_spell_text_split.Count = 1 Or (str_spell_text_split.Count > 1 And str_spell_text.Contains("saving ")) Then
                                Dim roll_spell_dmg As New Dnd5ECharacter.Roll
                                roll_spell_dmg.name = str_spell_name
                                roll_spell_dmg.roll = "0"
                                roll_spell_dmg.type = ""
                                roll_spell.link = roll_spell_dmg
                                roll_spell.futureUse_icon = "_Magic"
                                spell.Add(roll_spell)
                                If bol_is_attackwithDCroll Then
                                    Dim roll_spell_save As New Dnd5ECharacter.Roll(roll_spell)
                                    roll_spell_save.name = str_spell_name + " (saving)"
                                    roll_spell_save.roll = str_spell_roll_DC
                                    If roll_spell_save.critmultip <> "" Then roll_spell_save.critmultip = ""
                                    If roll_spell_save.critrangemin <> "" Then roll_spell_save.critrangemin = ""
                                    Dim roll_spell_save_dmg As New Dnd5ECharacter.Roll
                                    roll_spell_save_dmg.name = str_spell_name
                                    roll_spell_save_dmg.roll = "0"
                                    roll_spell_save_dmg.type = ""
                                    roll_spell_save.link = roll_spell_save_dmg
                                    roll_spell_save.futureUse_icon = "_Magic"
                                    spell.Add(roll_spell_save)
                                End If
                            ElseIf spell.Count = 0 And str_spell_text_split.Count > 1 Then
                                str_roll_prev = str_spell_roll
                            End If
                        End If
                        If obj_spell.Item("abilityCheck") IsNot Nothing And
                           ((str_spell_text_split.Count > 1 And str_spell_text.Contains("@skill")) Or
                           (str_spell_text_split.Count = 1) Or
                           (str_spell_text_split.Count = int_count_text And bol_is_check = False)) Then
                            bol_is_check = True
                            For Each str_check As String In obj_spell.Item("abilityCheck")
                                Dim str_check_stat As String = str_check
                                If str_spell_text.Contains("@skill") Then
                                    Dim int_start As Integer = InStr(str_spell_text, "@skill") + "@skill".Length
                                    Dim int_len As Integer = InStr(int_start, str_spell_text, "}") - int_start
                                    If int_len > 0 Then str_check_stat = Mid(str_spell_text, int_start, int_len).Trim
                                End If
                                If {"strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"}.Contains(str_check_stat.ToLower) Then str_check_stat = Mid(str_check, 1, 3)
                                Dim roll_spell_check As New Dnd5ECharacter.Roll(roll_spell)
                                roll_spell_check.name = roll_spell.name + " (check)"
                                roll_spell_check.roll = "/" + str_check_stat.ToUpper + "/zero"
                                Dim roll_spell_check_dmg As New Dnd5ECharacter.Roll
                                roll_spell_check_dmg.name = str_spell_name
                                roll_spell_check_dmg.roll = "0"
                                roll_spell_check_dmg.type = ""
                                roll_spell_check.link = roll_spell_check_dmg
                                roll_spell_check.futureUse_icon = "_Magic"
                                spell.Add(roll_spell_check)
                            Next
                        End If
                    ElseIf str_spell_type = "healing" Then
                        If roll_spell.roll <> "" Then spell.Add(roll_spell)
                    End If
                Next
            End If
            '4) Add spell to export spell list and to load spell list
            If spell.Count > 0 Then
                '4.1) Export list (called Spells)
                If Spells_name.Contains(str_spell_name) Then
                    Spells_name.Add(str_spell_name + " (" + str_spell_source + ")")
                Else
                    Spells_name.Add(str_spell_name)
                End If
                Spells_source.Add(str_spell_source)
                Spells_level.Add(str_spell_level)
                Spells_type.Add(str_spell_type)
                Spells.Add(spell)
                Spells_input.Add(obj_spell)
                '4.2) Load list (called Spells_#spelltype)
                If str_spell_type = "attacks" Then
                    If Spells_attacks_name.Contains(str_spell_name) Then
                        Spells_attacks_name.Add(str_spell_name + " (" + str_spell_source + ")")
                    Else
                        Spells_attacks_name.Add(str_spell_name)
                    End If
                    Spells_attacks.Add(spell)
                ElseIf str_spell_type = "attacksDC" Then
                    If Spells_attacksDC_name.Contains(str_spell_name) Then
                        Spells_attacksDC_name.Add(str_spell_name + " (" + str_spell_source + ")")
                    Else
                        Spells_attacksDC_name.Add(str_spell_name)
                    End If
                    Spells_attacksDC.Add(spell)
                ElseIf str_spell_type = "healing" Then
                    If Spells_healing_name.Contains(str_spell_name) Then
                        Spells_healing_name.Add(str_spell_name + " (" + str_spell_source + ")")
                    Else
                        Spells_healing_name.Add(str_spell_name)
                    End If
                    Spells_healing.Add(spell)
                End If
            End If
            Return 0
        Catch ex As Exception
            MsgBox("Unexpected Error! " + Chr(10) + Chr(10) + "Error: [" + ex.Message + "]", MsgBoxStyle.Critical, "Dnd5e Converter (RuleSet5EPlugin)")
            Return 0
        End Try
    End Function
End Class
