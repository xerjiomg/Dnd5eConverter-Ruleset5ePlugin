Imports System.ComponentModel

Partial Public Class Dnd5ECharacter
    Public Class Character
        <DefaultValue("")>
        Public Property NPC As Boolean = True
        Public Property hp As String = "10"
        Public Property ac As String = "10"
        Public Property speed As String = "30"
        Public Property str As String = "10"
        Public Property dex As String = "10"
        Public Property con As String = "10"
        Public Property Int As String = "10"
        Public Property wis As String = "10"
        Public Property cha As String = "10"
        Public Property attacks As List(Of Roll) = New List(Of Roll)()
        Public Property attacksDC As List(Of Roll) = New List(Of Roll)()
        Public Property healing As List(Of Roll) = New List(Of Roll)()
        Public Property saves As List(Of Roll) = New List(Of Roll)()
        Public Property skills As List(Of Roll) = New List(Of Roll)()
        Public Property resistance As List(Of String) = New List(Of String)()
        Public Property vulnerability As List(Of String) = New List(Of String)()
        Public Property immunity As List(Of String) = New List(Of String)()

    End Class

    Public Class Roll
        Public Property name As String = ""
        Public Property type As String = ""
        <DefaultValue("")>
        Public Property range As String = ""
        <DefaultValue("")>
        Public Property roll As String = "100"
        <DefaultValue("")>
        Public Property critrangemin As String = ""
        <DefaultValue("")>
        Public Property critmultip As String = ""
        <DefaultValue("")>
        Public Property info As String = ""
        <DefaultValue("")>
        Public Property conditions As String = ""
        <DefaultValue("")>
        Public Property futureUse_icon As String = ""
        Public Property link As Roll = Nothing

        Public Sub New()

        End Sub

        Public Sub New(ByVal source As Roll)
            name = source.name
            type = source.type
            roll = source.roll
            critrangemin = source.critrangemin
            critmultip = source.critmultip
            range = source.range
            info = source.info
            futureUse_icon = source.futureUse_icon
            If source.link Is Nothing Then
                link = Nothing
            Else
                link = New Roll(source.link)
            End If
        End Sub
    End Class

End Class


