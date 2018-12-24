﻿Imports System.Net.Sockets
Imports System.Text
Imports O_FMS_V0.RandomString
'This class is for interating with the led controllers for the 2018 FRC game FIRST POWERUP'

Public Class Lighting
    'UdpClient for the controller'
    Public Shared ControllerConnection As New UdpClient
    'Port Number for the communication to the controllers'
    Public Shared Port As Integer = 5555
    'Varible for using the game data for 2018, for warmup'
    Public Shared GameData = gamedatause

    Public Shared mode As String = "mode"
    'Led Types'
    Public Enum LightingModes
        Green
        Purple
        Red
        Blue
        Awards
        Test
        Off
        Owned_Red
        Owned_Blue
        NotOwned
        Red_Force
        Red_Boost
        Red_Levitate
        Blue_Force
        Blue_Boost
        Blue_Levitate
    End Enum

    'Connects to the controller'
    Public Sub ConnectController(ip As String)
        If ControllerConnection Is Nothing Then
            ControllerConnection = New UdpClient(ip, Port)
            ControllerConnection.Connect(ip, Port)
        End If

    End Sub

    'sets the mode of the leds'
    Public Sub SetMode(LightingModes)
        Select Case (LightingModes)
            Case LightingModes.Owned_Red
                SendPacket("R")
            Case LightingModes.Owned_Blue
                SendPacket("B")
            Case LightingModes.NotOwned
                SendPacket("N")
            Case LightingModes.Blue_Force
                SendPacket("BF")
            Case LightingModes.Blue_Boost
                SendPacket("BB")
            Case LightingModes.Red_Force
                SendPacket("RF")
            Case LightingModes.Red_Boost
                SendPacket("RB")
            Case LightingModes.Purple
                SendPacket("P")
            Case LightingModes.Green
                SendPacket("G")
            Case LightingModes.Awards
                SendPacket("A")
            Case LightingModes.Test
                SendPacket("T")
            Case LightingModes.Off
                SendPacket("O")
            Case Else
                LightingModes.Off
        End Select

    End Sub

    'Sends the udp packet containing the mode string to the led controller'
    Public Sub SendPacket(mode)
        If ControllerConnection Is Nothing Then
            'Do Nothing'
            MessageBox.Show("Led Controller not connected, check connections and firewalls")
        Else
            'Sends the mode to the controller on port 5555'
            Dim LightingPacket(1024) As Byte
            LightingPacket = Encoding.ASCII.GetBytes(mode)
            ControllerConnection.Send(LightingPacket, LightingPacket.Length)
        End If
    End Sub
End Class