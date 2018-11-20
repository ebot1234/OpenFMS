Imports O_FMS_V0.PLC_Comms_Server
Imports O_FMS_V0.RandomString
Imports O_FMS_V0.Team_Networks
Imports O_FMS_V0.Lighting
Imports System.Net

Public Class Field
    'PLC Field Types'
    Public Shared FieldReset As Boolean
    Public Shared Volunteers As Boolean
    'Driver Station instances'
    Public Shared Red1DS As New DriverStations
    Public Shared Red2DS As New DriverStations
    Public Shared Red3DS As New DriverStations
    Public Shared Blue1DS As New DriverStations
    Public Shared Blue2DS As New DriverStations
    Public Shared Blue3DS As New DriverStations
    'Led Controllers'
    Public Shared ScaleLeds As New Lighting
    Public Shared RedSwitchLeds As New Lighting
    Public Shared BlueSwitchLeds As New Lighting
    '2018 timing'
    Public Shared WarmUpTime As Integer = 4
    Public Shared AutoTime As Integer = 15
    Public Shared PauseTime As Integer = 3
    Public Shared TeleTime As Integer = 135
    Public Shared EndgameTime As Integer = 30
    Public Shared GameTime As Integer = 0
    'Match Type enums'
    Public Enum MatchEnums
        PreMatch
        WarmUp
        Auto
        Pause
        TeleOp
        EndGame
        PostMatch
        AbortMatch
    End Enum


    Public Shared Sub ConnectDriverStations()
        Red1DS.Connect(IPAddress.Parse(Red1Network))
        Red2DS.Connect(IPAddress.Parse(Red2Network))
        Red3DS.Connect(IPAddress.Parse(Red3Network))
        Blue1DS.Connect(IPAddress.Parse(Blue1Network))
        Blue2DS.Connect(IPAddress.Parse(Blue2Network))
        Blue3DS.Connect(IPAddress.Parse(Blue3Network))
    End Sub
    Public Shared Sub ConnectLeds()
        Try
            RedSwitchLeds.ConnectController("10.0.0.23")
            BlueSwitchLeds.ConnectController("10.0.0.24")
            ScaleLeds.ConnectController("10.0.0.25")
        Catch ex As Exception
            MessageBox.Show("Led Controllers Not Connected, Tell FTA before continuing")
        End Try

    End Sub



    Public Shared Function SendDS(Auto As Boolean, Enabled As Boolean)
        If Auto = True Then
            Red1DS.Auto = True
            Red2DS.Auto = True
            Red3DS.Auto = True
            Blue1DS.Auto = True
            Blue2DS.Auto = True
            Blue3DS.Auto = True

            'Red1DS.sendPacketDS()
            ' Red2DS.sendPacketDS()
            ' Red3DS.sendPacketDS()
            ' Blue1DS.sendPacketDS()
            ' Blue2DS.sendPacketDS()
            ' Blue3DS.sendPacketDS()

        Else
            Red1DS.Auto = False
            Red2DS.Auto = False
            Red3DS.Auto = False
            Blue1DS.Auto = False
            Blue2DS.Auto = False
            Blue3DS.Auto = False

            ' Red1DS.sendPacketDS()
            ' Red2DS.sendPacketDS()
            'Red3DS.sendPacketDS()
            ' Blue1DS.sendPacketDS()
            ' Blue2DS.sendPacketDS()
            'Blue3DS.sendPacketDS()
        End If

        If Enabled = True Then
            Red1DS.Enabled = True
            Red2DS.Enabled = True
            Red3DS.Enabled = True
            Blue1DS.Enabled = True
            Blue2DS.Enabled = True
            Blue3DS.Enabled = True

            ' Red1DS.sendPacketDS()
            ' Red2DS.sendPacketDS()
            ' Red3DS.sendPacketDS()
            ' Blue1DS.sendPacketDS()
            'Blue2DS.sendPacketDS()
            ' Blue3DS.sendPacketDS()
        Else
            Red1DS.Enabled = False
            Red2DS.Enabled = False
            Red3DS.Enabled = False
            Blue1DS.Enabled = False
            Blue2DS.Enabled = False
            Blue3DS.Enabled = False

            ' Red1DS.sendPacketDS()
            ' Red2DS.sendPacketDS()
            ' Red3DS.sendPacketDS()
            'Blue1DS.sendPacketDS()
            'Blue2DS.sendPacketDS()
            'Blue3DS.sendPacketDS()
        End If
        Return 0
    End Function

    Public Shared Sub updateField(MatchEnums As MatchEnums)
        Select Case (MatchEnums)
            Case MatchEnums.PreMatch
                PLC_Reset = True
                Match_PreStart = True

                'ConnectDriverStations()
                'pingDSConnections()
                SendDS(Auto:=True, Enabled:=False)

            Case MatchEnums.WarmUp
                SendDS(Auto:=True, Enabled:=False)
                GameDataGen()
                Match_Start = True
                My.Computer.Audio.Play(My.Resources.match_warmup, AudioPlayMode.Background)
            Case MatchEnums.Auto
                SendDS(Auto:=True, Enabled:=True)
                My.Computer.Audio.Play(My.Resources.match_start, AudioPlayMode.Background)

            Case MatchEnums.Pause
                My.Computer.Audio.Play(My.Resources.match_end, AudioPlayMode.Background)
            Case MatchEnums.TeleOp
                My.Computer.Audio.Play(My.Resources.match_resume, AudioPlayMode.Background)
            Case MatchEnums.EndGame
                My.Computer.Audio.Play(My.Resources.match_endgame, AudioPlayMode.Background)
            Case MatchEnums.PostMatch
                My.Computer.Audio.Play(My.Resources.match_end, AudioPlayMode.Background)
                Match_Stop = True
            Case MatchEnums.AbortMatch
                SendDS(Auto:=False, Enabled:=False)
                Match_Stop = True
            Case Else
                MatchEnums = MatchEnums.PreMatch
        End Select
    End Sub
End Class