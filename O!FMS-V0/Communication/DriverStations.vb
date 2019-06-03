﻿Imports System.Net.Sockets
Imports System.Net
Imports System.Text.RegularExpressions
Imports O_FMS_V0.PLC_Comms_Server
Imports O_FMS_V0.RandomString
Imports System.Threading


Public Class DriverStations
    Public packetCount As Integer = 0
    Public DSUpdSendPort As Int32 = 1121
    Public DSUdpReceivePort As Int32 = 1160
    Public Bypassed As Boolean = False
    Public Auto As Boolean = False
    Public Enabled As Boolean = False
    Public Estop As Boolean = False
    Public robotIp As IPAddress
    Public radioIp As IPAddress
    Public DriverStationIP As IPAddress
    Public FMS_IP As String = "10.0.100.5"
    Public tcpClient As TcpClient
    Public udpClient As UdpClient
    Public TeamNum As Integer
    Dim IpEnd As IPEndPoint = New IPEndPoint(IPAddress.Parse("10.0.100.5"), 1750)
    Dim dsListener As TcpListener

    Public Sub newDriverStationConnection(teamNumber As String, allianceStationNumber As Integer)
        TeamNum = Convert.ToInt32(teamNumber)
        If TeamNum <> 0 Then
            Select Case teamNumber.Length
                Case 1
                Case 2
                    robotIp = IPAddress.Parse("10.00." + teamNumber + ".2")
                Case 3
                    robotIp = IPAddress.Parse("10.0" + teamNumber(0) + "." + teamNumber(1) + teamNumber(2) + ".2")
                Case 4
                    robotIp = IPAddress.Parse("10." + teamNumber.Substring(0, 2) + "." + teamNumber.Substring(2) + ".2")
                Case Else
                    robotIp = IPAddress.Parse("10.0.0.2")
            End Select
        Else
            teamNumber = 0
            robotIp = IPAddress.Parse("10.0.0.2")
        End If
        radioIp = IPAddress.Parse(robotIp.ToString().Substring(0, robotIp.ToString().Length - 1) + "1")

        Dim robotPingThread As New Thread(AddressOf robotPing)
        robotPingThread.Start()

        Dim sendDataThread As New Thread(AddressOf sendPacketDS)
        sendDataThread.Start()

    End Sub

    Public Sub robotPing(stationNumber As Integer)
        Dim timeOut As Integer = 5

        While (True)
            'Pings the robots radio to see if its connected'
            If My.Computer.Network.Ping(radioIp.ToString, timeOut) Then
                If stationNumber = 0 Then
                    Robot_Linked_Red1 = True
                ElseIf stationNumber = 1 Then
                    Robot_Linked_Red2 = True
                ElseIf stationNumber = 2 Then
                    Robot_Linked_Red3 = True
                ElseIf stationNumber = 3 Then
                    Robot_Linked_Blue1 = True
                ElseIf stationNumber = 4 Then
                    Robot_Linked_Blue2 = True
                ElseIf stationNumber = 5 Then
                    Robot_Linked_Blue3 = True
                End If
            End If
            'Pings the driver station to check if its connected'
            If DriverStationIP IsNot Nothing Then
                If My.Computer.Network.Ping(DriverStationIP.ToString, timeOut) Then
                    If stationNumber = 0 Then
                        DS_Linked_Red1 = True
                    ElseIf stationNumber = 1 Then
                        DS_Linked_Red2 = True
                    ElseIf stationNumber = 2 Then
                        DS_Linked_Red3 = True
                    ElseIf stationNumber = 3 Then
                        DS_Linked_Blue1 = True
                    ElseIf stationNumber = 4 Then
                        DS_Linked_Blue2 = True
                    ElseIf stationNumber = 5 Then
                        DS_Linked_Blue3 = True
                    End If
                End If
            End If
            'sleeps for 100ms to see if another connection has appeared'
            Thread.Sleep(100)
        End While
    End Sub

    Public Sub setConnections(dsIp As IPAddress, tcpConnection As TcpClient)
        DriverStationIP = dsIp
        tcpClient = tcpConnection
        udpClient = New UdpClient(dsIp.ToString(), 1121)
    End Sub

    Public Sub sendPacketDS(allianceStation As Integer)
        Dim packet = encodeControlPacket(allianceStation)
        udpClient.Send(packet, packet.Length)

    End Sub

    Public Function encodeControlPacket(allianceStation As Integer)
        'Using the driver station packet structure from Cheesy Arena'
        Dim data(22) As Byte

        data(0) = packetCount >> 8 And &HFF
        data(1) = packetCount And &HFF

        'Protocol version'
        data(2) = 0

        'Robot status byte'
        data(3) = 0

        If Auto = True Then
            data(3) = &H2
        End If

        If Enabled = True Then
            data(3) = &H4
        End If

        If Estop = True Then
            data(3) = &H80
        End If
        'Unused byte or unknown'
        data(4) = 0
        'Alliance Station byte, TODO add map of alliance stations'
        '0 = r1, 1 = r2, 2 = r3, 3 = b1, 4 = b2, 5 = b3'
        data(5) = allianceStation

        'driver station match type is practice for right now'
        data(6) = 1

        'Match Number'
        data(7) = 0
        data(8) = 1

        'repeat match number'
        data(9) = 1

        'Current time since 1900'
        Dim currentTime = DateAndTime.Now
        'defines the value of a nanosecond'
        Dim nanoseconds As Integer = (currentTime.Ticks / TimeSpan.TicksPerMillisecond / 10) * 100
        data(10) = (((nanoseconds / 1000) >> 24) & &HFF)
        data(11) = (((nanoseconds / 1000) >> 16) & &HFF)
        data(12) = (((nanoseconds / 1000) >> 8) & &HFF)
        data(13) = ((nanoseconds / 1000) & &HFF)
        data(14) = (currentTime.Second)
        data(15) = (currentTime.Minute)
        data(16) = (currentTime.Hour)
        data(17) = (currentTime.Day)
        data(18) = (currentTime.Month)
        data(19) = (currentTime.Year - 1900)

        'Time left in the match'
        data(20) = (Main_Panel.matchTimerLbl.Text >> 8 & &HFF)
        data(21) = (Main_Panel.matchTimerLbl.Text & &HFF)

        packetCount = packetCount + 1

        Return data
    End Function

    Public Sub ListenToDS()
        Dim listen As Boolean

        Try
            Dim port As Int32 = 1750
            Dim FMS_Address As IPAddress = IPAddress.Parse("10.0.100.5")
            dsListener = New TcpListener(FMS_Address, port)
            dsListener.Start()
            listen = True
        Catch ex As SocketException
            MessageBox.Show(ex.ToString)
            listen = False
        End Try


        While (Field.fieldStatus = Field.MatchEnums.PreMatch And listen = True)
            Dim tcpClient As TcpClient = dsListener.AcceptTcpClient
            Dim buffer(5) As Byte
            tcpClient.GetStream.Read(buffer, 0, buffer.Length)

            If buffer(0) <> 0 & buffer(1) <> 3 & buffer(2) <> 24 Then
                tcpClient.Close()

            End If

            Dim teamid_1 As Integer = buffer(3) << 8
            Dim teamid_2 As Integer = buffer(4)
            Dim teamId As Integer = teamid_1 And teamid_2

            Dim allianceStation As Integer = -1
            Dim ip As String = tcpClient.Client.RemoteEndPoint.ToString().Split(":")(0)
            Dim dsIp As IPAddress = IPAddress.Parse(ip)

            If TeamNum = teamId Then
                setConnections(dsIp, tcpClient)
            End If




            Dim assignmentPacket(5) As Byte
            assignmentPacket(0) = 0 'packet size'
            assignmentPacket(1) = 3 'packet size'
            assignmentPacket(2) = 25 'packet type'
            assignmentPacket(3) = allianceStation
            assignmentPacket(4) = 0 'station status, need to add station checking'

            tcpClient.GetStream.Write(assignmentPacket, 0, assignmentPacket.Length)
        End While

    End Sub



    Public Sub Dispose()
        If udpClient IsNot Nothing Then
            udpClient.Close()
        Else
            'Do nothing since it is nothing'
        End If

        If tcpClient IsNot Nothing Then
            tcpClient.Close()
        Else
            'Do nothing since it is nothing'
        End If

        If dsListener IsNot Nothing Then
            dsListener.Stop()
        Else
            'Do nothing since it is nothing
        End If

    End Sub

End Class
