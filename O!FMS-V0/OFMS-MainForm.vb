﻿Imports System.Data.SqlClient
Imports O_FMS_V0.PLC_Handler
Imports O_FMS_V0.Field
Imports O_FMS_V0.AccessPoint
Imports O_FMS_V0.Elimination_Matches
Imports O_FMS_V0.DisplayServer
Imports O_FMS_V0.Tba


Public Class Main_Panel

    Public Shared PLC_Thread As New Threading.Thread(AddressOf handlePLC)
    Dim scoreHandler As New Threading.Thread(AddressOf updateScores)

    Dim connection As New SqlConnection("data source=MY-PC\OFMS; Initial Catalog=OpenFMS; Integrated Security = true")
    Dim i As Integer = 0

    Public Shared ElimMode As Boolean = False
    Public Shared Field_Estopped As Boolean = False

    Public Shared Red1Bypass
    Public Shared Red2Bypass
    Public Shared Red3Bypass
    Public Shared Blue1Bypass
    Public Shared Blue2Bypass
    Public Shared Blue3Bypass
    Public Shared Red1_Estop
    Public Shared Red2_Estop
    Public Shared Red3_Estop
    Public Shared Blue1_Estop
    Public Shared Blue2_Estop
    Public Shared Blue3_Estop
    Public Shared R1_Estop_Count = 0
    Public Shared R2_Estop_Count = 0
    Public Shared R3_Estop_Count = 0
    Public Shared B1_Estop_Count = 0
    Public Shared B2_Estop_Count = 0
    Public Shared B3_Estop_Count = 0

    Public Shared alliance_num1
    Public Shared alliance_num2

    'Red Scoring Varibles'
    Public Shared RedScore As Integer


    'Blue Scoring Varibles
    Public Shared BlueScore As Integer


    Public Shared redWin
    Public Shared blueWin
    Public Shared tie
    Public Shared alliance1 As String
    Public Shared alliance2 As String
    Public Shared type As String = "qm"
    Public Shared auto_score As Boolean = False

    Public Shared D_Server As DisplayServer

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the '_O_FMSDataSet.FMSMaster' table. You can move, or remove it, as needed.
        Timer1.Interval = 100 '100ms loop time'
        Timer1.Enabled = True
        'Timer1.AutoReset = True
        BackColor = System.Drawing.Color.Yellow
        ' Me.FMSMasterTableAdapter.Fill(Me._O_FMSDataSet.FMSMaster)
        Call CenterToScreen()
        Me.FormBorderStyle = Windows.Forms.BorderStyle.Fixed3D
        Me.WindowState = FormWindowState.Normal
        resetScore()
        scoreHandler.Start()

        'Sets the settings for the AP and Switch'
        SetSettings("10.0.100.1", "OFMS", "OFMS", 12, 5, "ofmsrocks")
        Switch.Switch.address = "10.0.100.2"
        Field_Comms.dsThread = New Threading.Thread(AddressOf Field_Comms.dsConnectThread)
        Field_Comms.dsThread.Start()
    End Sub



    Private Sub BindingSource1_CurrentChanged(sender As Object, e As EventArgs)

    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Save_btn.Click
        'Dim insertquery As String = "INSERT INTO FMSMaster([Match], [MatchTime], [Blue1], [B1Sur],  [Blue1DQ], [Blue1Volt], [Blue1Estop], [Blue1RL], [Blue1DS], [Blue1Bypass], [Blue2], [B2Sur], [Blue2DQ], [Blue2Volt], [Blue2Estop], [Blue2RL], [Blue2DS], [Blue2Bypass], [Blue3], [B3Sur], [Blue3DQ], [Blue3Volt], [Blue3Estop], [Blue3RL], [Blue3DS], [Blue3Bypass], [Red1], [R1Sur], [Red1DQ], [Red1Volt], [Red1Estop], [Red1RL], [Red1DS], [Red1Bypass], [Red2], [R2Sur], [Red2DQ], [Red2Volt], [Red2Estop], [Red2RL], [Red2DS], [Red2Bypass], [Red3], [R3Sur], [Red3DQ], [Red3Volt], [Red3Estop], [Red3RL], [Red3DS], [Red3Bypass]) VALUES('" & MatchNum.Text & "', '" & Ctime.Text & "', '" & BlueTeam1.Text & "', '" & Blue1Sur.Text & "', '" & BDQ1.Checked & "', '" & BlueVolt1.Text & "','" & PLC_Estop_Red1 & "' ,'" & Robot_Linked_Red1 & "' , '" & DS_Linked_Red1 & "', '" & BBypass1.Checked & "', '" & BlueTeam2.Text & "','" & Blue2Sur.Text & "', '" & BDQ2.Checked & "', '" & BlueVolt2.Text & "', '" & PLC_Estop_Blue2 & "','" & Robot_Linked_Blue2 & "' ,'" & DS_Linked_Blue2 & "' , '" & BBypass2.Checked & "', '" & BlueTeam3.Text & "','" & Blue3Sur.Text & "' , '" & BDQ3.Checked & "', '" & BlueVolt3.Text & "','" & PLC_Estop_Blue3 & "' , '" & Robot_Linked_Blue3 & "', '" & DS_Linked_Blue3 & "', '" & BBypass3.Checked & "', '" & RedTeam1.Text & "', '" & Red1Sur.Text & "', '" & RDQ1.Checked & "', '" & RedVolt1.Text & "', '" & PLC_Estop_Red1 & "', '" & Robot_Linked_Red1 & "', '" & DS_Linked_Red1 & "', '" & RBypass1.Checked & "', '" & RedTeam2.Text & "', '" & Red2Sur.Text & "', '" & RDQ2.Checked & "', '" & RedVolt2.Text & "', '" & PLC_Estop_Red2 & "', '" & Robot_Linked_Red2 & "','" & DS_Linked_Red2 & "' , '" & RBypass2.Checked & "', '" & RedTeam3.Text & "', '" & Red3Sur.Text & "', '" & RDQ3.Checked & "', '" & RedVolt3.Text & "','" & PLC_Estop_Red3 & "' ,'" & Robot_Linked_Red3 & "' ,'" & DS_Linked_Red3 & "' , '" & RBypass3.Checked & "', '" & SandStormMessage.Text & "')"

        'ExecuteQuery(insertquery)

        'MessageBox.Show("Data Saved")

        If ElimMode = True Then
            'Add scores back
            'Dim insertQuery As String = "INSERT INTO ElimanationResults([alliance1], [alliance2], [round], [type], [red1], [red2], [red3], [blue1], [blue2], [blue3], [redscore], [bluescore]) VALUES('" & alliance1 & "', '" & alliance2 & "', '" & MatchNum.Text & "', '" & type & "', '" & RedTeam1.Text & "', '" & RedTeam2.Text & "', '" & RedTeam3.Text & "', '" & BlueTeam1.Text & "', '" & BlueTeam2.Text & "', '" & BlueTeam3.Text & "', '" & RedScoreLbl.Text & "', '" & BlueScoreLbl.Text & "')"
            'ExecuteQuery(insertQuery)

            Dim winner = calculateWinner()
            'This updates the wins in the alliances columns'
            Dim insertWins As String = ""
            If winner = "Red" Then
                Dim amount = getWins(alliance1)
                amount = amount + 1
                insertWins = "UPDATE alliances SET wins = '" & amount & "' Where rank = '" & alliance1 & "'"
                ExecuteQuery(insertWins)
            ElseIf winner = "Blue" Then
                Dim amount = getWins(alliance2)
                amount = amount + 1
                insertWins = "UPDATE alliances SET wins = '" & amount & "' Where rank = '" & alliance2 & "'"
                ExecuteQuery(insertWins)
            ElseIf winner = "tie" Then
                Dim amount1 = getWins(alliance1)
                Dim amount2 = getWins(alliance2)
                insertWins = "UPDATE alliances SET wins = '" & amount1 & "' Where rank = '" & alliance1 & "'"
                ExecuteQuery(insertWins)
                insertWins = "UPDATE alliances SET wins = '" & amount2 & "' Where rank = '" & alliance2 & "'"
                ExecuteQuery(insertWins)
            End If

            If type.Substring(0, 2) = "QF" And MatchNum.Text >= 8 Then
                updateQuarterFinalMatches(round:=MatchNum.Text)
                MessageBox.Show("Updated the schedule")
            ElseIf type.Substring(0, 2) = "SF" And MatchNum.Text >= 4 Then
                updateSemifinalMatches(round:=MatchNum.Text)
                MessageBox.Show("Updated the schedule")
            ElseIf type.Substring(0, 1) = "F" And MatchNum.Text >= 2 Then
                updateFinalMatches(round:=MatchNum.Text)
                MessageBox.Show("Updated the schedule")
            End If
        End If

        resetScore()
        resetUI()
        Field_Estop = False
    End Sub
    Function calculateWinner()
        Dim winner = ""
        'If the blue wins
        If RedScore < BlueScore Then
            winner = "Blue"
            AudianceDisplay.WinningAlliance.Text = "Blue"
        End If

        'if red wins
        If RedScore > BlueScore Then
            winner = "Red"
            AudianceDisplay.WinningAlliance.Text = "Red"
        End If
        'if there is a tie
        If RedScore = BlueScore Then
            winner = "Tie"
            AudianceDisplay.WinningAlliance.Text = "Tie"
        End If

        Return winner
    End Function
    Sub resetUI()
        Red1Bypass = False
        Red2Bypass = False
        Red3Bypass = False
        Blue1Bypass = False
        Blue2Bypass = False
        Blue3Bypass = False
    End Sub

    Public Sub ExecuteQuery(query As String)
        Dim command As New SqlCommand(query, connection)
        connection.Open()
        command.ExecuteNonQuery()
        connection.Close()
    End Sub

    Public Sub MatchLoad_Btn_Click(sender As Object, e As EventArgs) Handles MatchLoad_Btn.Click
        If ElimMode = False Then
            Dim selectQuery As New SqlCommand("Select Match, Blue1, B1Sur, Blue2, B2Sur, Blue3, B3Sur, Red1, R1Sur, Red2, R2Sur, Red3, R3Sur FROM matches where Match= @Matchnum", connection)
            selectQuery.Parameters.Add("@Matchnum", SqlDbType.Int).Value = MatchNum.Text
            Dim adapter As New SqlDataAdapter(selectQuery)
            Dim table As New DataTable()
            adapter.Fill(table)
            If table.Rows.Count() > 0 Then
                RedTeam1.Text = table.Rows(0)(7).ToString()
                Red1Sur.Text = table.Rows(0)(8).ToString()
                RedTeam2.Text = table.Rows(0)(9).ToString()
                Red2Sur.Text = table.Rows(0)(10).ToString()
                RedTeam3.Text = table.Rows(0)(11).ToString()
                Red3Sur.Text = table.Rows(0)(12).ToString()
                BlueTeam1.Text = table.Rows(0)(1).ToString()
                Blue1Sur.Text = table.Rows(0)(2).ToString()
                BlueTeam2.Text = table.Rows(0)(3).ToString()
                Blue2Sur.Text = table.Rows(0)(4).ToString()
                BlueTeam3.Text = table.Rows(0)(5).ToString()
                Blue3Sur.Text = table.Rows(0)(6).ToString()




                'updates the audience display with team numbers'
                AudianceDisplay.RedTeam1.Text = table.Rows(0)(7).ToString
                AudianceDisplay.RedTeam2lbl.Text = table.Rows(0)(9).ToString
                AudianceDisplay.RedTeam3.Text = table.Rows(0)(11).ToString
                AudianceDisplay.BlueTeam1lbl.Text = table.Rows(0)(1).ToString
                AudianceDisplay.BlueTeam2.Text = table.Rows(0)(3).ToString
                AudianceDisplay.BlueTeam3.Text = table.Rows(0)(5).ToString

                'Updates the team numbers for the pre-match screen'
                AudianceDisplay.RedTeam1lbl.Text = RedTeam1.Text
                AudianceDisplay.Red2Lbl.Text = RedTeam2.Text
                AudianceDisplay.Red3Lbl.Text = RedTeam3.Text
                AudianceDisplay.Blue1Lbl.Text = BlueTeam1.Text
                AudianceDisplay.Blue2Lbl.Text = BlueTeam2.Text
                AudianceDisplay.BlueTeam3.Text = BlueTeam3.Text

                'Updates the audience display with match number'
                AudianceDisplay.MatchNumb.Text = MatchNum.Text
                'Updates the audience displays team names'
                AudianceDisplay.Label2.Text = Schedule_Generator.getTeamName(RedTeam1.Text)
                AudianceDisplay.Label4.Text = Schedule_Generator.getTeamName(RedTeam2.Text)
                AudianceDisplay.Label3.Text = Schedule_Generator.getTeamName(RedTeam3.Text)
                AudianceDisplay.Label7.Text = Schedule_Generator.getTeamName(BlueTeam1.Text)
                AudianceDisplay.Label6.Text = Schedule_Generator.getTeamName(BlueTeam2.Text)
                AudianceDisplay.Label5.Text = Schedule_Generator.getTeamName(BlueTeam3.Text)

                AudianceDisplay.Label8.Text = AudianceDisplay.Label2.Text
                AudianceDisplay.Label9.Text = AudianceDisplay.Label4.Text
                AudianceDisplay.Label10.Text = AudianceDisplay.Label3.Text
                AudianceDisplay.Label11.Text = AudianceDisplay.Label7.Text
                AudianceDisplay.Label12.Text = AudianceDisplay.Label6.Text
                AudianceDisplay.Label13.Text = AudianceDisplay.Label5.Text

                handleTeamWifiConfiguration()
                MessageBox.Show("Data Loaded")

            Else
                MessageBox.Show("Not Loaded")
            End If
        End If

        'Handles the match pulling during the elimanation matches'
        If ElimMode = True Then
            Dim selectQuery As New SqlCommand("Select type, round, red1, red2, red3, blue1, blue2, blue3, alliance1, alliance2 From elimination Where round = @MatchNum", connection)
            selectQuery.Parameters.Add("@Matchnum", SqlDbType.Int).Value = MatchNum.Text
            Dim adapter As New SqlDataAdapter(selectQuery)
            Dim table As New DataTable()
            adapter.Fill(table)

            If table.Rows.Count() > 0 Then
                RedTeam1.Text = table.Rows(0)(2)
                RedTeam2.Text = table.Rows(0)(3)
                RedTeam3.Text = table.Rows(0)(4)
                BlueTeam1.Text = table.Rows(0)(5)
                BlueTeam2.Text = table.Rows(0)(6)
                BlueTeam3.Text = table.Rows(0)(7)

                'updates the audience display with team numbers'
                AudianceDisplay.RedTeam1.Text = table.Rows(0)(2).ToString
                AudianceDisplay.RedTeam2lbl.Text = table.Rows(0)(3).ToString
                AudianceDisplay.RedTeam3.Text = table.Rows(0)(4).ToString
                AudianceDisplay.BlueTeam1lbl.Text = table.Rows(0)(5).ToString
                AudianceDisplay.BlueTeam2.Text = table.Rows(0)(6).ToString
                AudianceDisplay.BlueTeam3.Text = table.Rows(0)(7).ToString
                AudianceDisplay.Label14.Show()
                AudianceDisplay.Label15.Show()
                AudianceDisplay.Label14.Text = table.Rows(0)(8).ToString
                AudianceDisplay.Label15.Text = table.Rows(0)(9).ToString

                'Updates the team numbers for the pre-match screen'
                AudianceDisplay.RedTeam1lbl.Text = RedTeam1.Text
                AudianceDisplay.Red2Lbl.Text = RedTeam2.Text
                AudianceDisplay.Red3Lbl.Text = RedTeam3.Text
                AudianceDisplay.Blue1Lbl.Text = BlueTeam1.Text
                AudianceDisplay.Blue2Lbl.Text = BlueTeam2.Text
                AudianceDisplay.BlueTeam3.Text = BlueTeam3.Text

                'Updates the audience display with match number'
                AudianceDisplay.MatchNumb.Text = MatchNum.Text
                'Updates the audience displays team names'
                AudianceDisplay.Label2.Text = Schedule_Generator.getTeamName(RedTeam1.Text)
                AudianceDisplay.Label4.Text = Schedule_Generator.getTeamName(RedTeam2.Text)
                AudianceDisplay.Label3.Text = Schedule_Generator.getTeamName(RedTeam3.Text)
                AudianceDisplay.Label7.Text = Schedule_Generator.getTeamName(BlueTeam1.Text)
                AudianceDisplay.Label6.Text = Schedule_Generator.getTeamName(BlueTeam2.Text)
                AudianceDisplay.Label5.Text = Schedule_Generator.getTeamName(BlueTeam3.Text)

                AudianceDisplay.Label8.Text = AudianceDisplay.Label2.Text
                AudianceDisplay.Label9.Text = AudianceDisplay.Label4.Text
                AudianceDisplay.Label10.Text = AudianceDisplay.Label3.Text
                AudianceDisplay.Label11.Text = AudianceDisplay.Label7.Text
                AudianceDisplay.Label12.Text = AudianceDisplay.Label6.Text
                AudianceDisplay.Label13.Text = AudianceDisplay.Label5.Text

                alliance1 = table.Rows(0)(8)
                alliance2 = table.Rows(0)(9)
                type = table.Rows(0)(0)

                handleTeamWifiConfiguration()
                MessageBox.Show("Data Loaded")

            Else
                MessageBox.Show("Not Loaded")

            End If
        End If

        'updates the PLC team numbers'
        RedT1 = RedTeam1.Text
        RedT2 = RedTeam2.Text
        RedT3 = RedTeam3.Text
        BlueT1 = BlueTeam1.Text
        BlueT2 = BlueTeam2.Text
        BlueT3 = BlueTeam3.Text

        'Updates the team numbers for the switch configuration'
        Switch.Red1 = RedTeam1.Text
        Switch.Red2 = RedTeam2.Text
        Switch.Red3 = RedTeam3.Text
        Switch.Blue1 = BlueTeam1.Text
        Switch.Blue2 = BlueTeam2.Text
        Switch.Blue3 = BlueTeam3.Text


    End Sub

    Private Sub RDQ1_CheckedChanged(sender As Object, e As EventArgs)
        If PLC_Estop_Red1 = False Then
            PLC_Estop_Red1 = True
        Else : PLC_Estop_Red1 = True
        End If
    End Sub

    Private WithEvents Timer1 As New System.Windows.Forms.Timer
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        Ctime.Text = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        'Updates the DS Data on the UI'
        updateStatus()

        'Estops
        If PLC_Estop_Red1 = True Then
            R1Estop.FillColor = System.Drawing.Color.Red
        ElseIf Red_1_Estop = True Then
            R1Estop.FillColor = System.Drawing.Color.Red
        Else : R1Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        If PLC_Estop_Red2 = True Then
            R2Estop.FillColor = System.Drawing.Color.Red
        ElseIf Red_2_Estop = True Then
            R2Estop.FillColor = System.Drawing.Color.Red
        Else : R2Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        If PLC_Estop_Red3 = True Then
            R3Estop.FillColor = System.Drawing.Color.Red
        ElseIf Red_3_Estop = True Then
            R3Estop.FillColor = System.Drawing.Color.Red
        Else : R3Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        If PLC_Estop_Blue1 = True Then
            B1Estop.FillColor = System.Drawing.Color.Red
        ElseIf Blue_1_Estop = True Then
            B1Estop.FillColor = System.Drawing.Color.Red
        Else : B1Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        If PLC_Estop_Blue2 = True Then
            B2Estop.FillColor = System.Drawing.Color.Red
        ElseIf Blue_2_Estop = True Then
            B2Estop.FillColor = System.Drawing.Color.Red
        Else : B2Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        If PLC_Estop_Blue3 = True Then
            B3Estop.FillColor = System.Drawing.Color.Red
        ElseIf Blue_3_Estop = True Then
            B3Estop.FillColor = System.Drawing.Color.Red
        Else : B3Estop.FillColor = System.Drawing.Color.LimeGreen
        End If

        'Driver Stations (DS) Linked
        If DS_Linked_Red1 Then
            R1DS.FillColor = System.Drawing.Color.LimeGreen
        Else : R1DS.FillColor = System.Drawing.Color.Red
        End If
        If DS_Linked_Red2 Then
            R2DS.FillColor = System.Drawing.Color.LimeGreen
        Else : R2DS.FillColor = System.Drawing.Color.Red
        End If
        If DS_Linked_Red3 Then
            R3DS.FillColor = System.Drawing.Color.LimeGreen
        Else : R3DS.FillColor = System.Drawing.Color.Red
        End If

        If DS_Linked_Blue1 Then
            B1DS.FillColor = System.Drawing.Color.LimeGreen
        Else : B1DS.FillColor = System.Drawing.Color.Red
        End If
        If DS_Linked_Blue2 Then
            B2DS.FillColor = System.Drawing.Color.LimeGreen
        Else : B2DS.FillColor = System.Drawing.Color.Red
        End If
        If DS_Linked_Blue3 Then
            B3DS.FillColor = System.Drawing.Color.LimeGreen
        Else : B3DS.FillColor = System.Drawing.Color.Red
        End If

        'Robot Linked
        If Robot_Linked_Red1 = True Then
            R1Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : R1Robot.FillColor = System.Drawing.Color.Red
        End If
        If Robot_Linked_Red2 = True Then
            R2Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : R2Robot.FillColor = System.Drawing.Color.Red
        End If
        If Robot_Linked_Red3 = True Then
            R3Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : R3Robot.FillColor = System.Drawing.Color.Red
        End If

        If Robot_Linked_Blue1 = True Then
            B1Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : B1Robot.FillColor = System.Drawing.Color.Red
        End If
        If Robot_Linked_Blue2 = True Then
            B2Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : B2Robot.FillColor = System.Drawing.Color.Red
        End If
        If Robot_Linked_Blue3 = True Then
            B3Robot.FillColor = System.Drawing.Color.LimeGreen
        Else : B3Robot.FillColor = System.Drawing.Color.Red
        End If

        If Red1Bypass = True Then
            R1DS.FillColor = Color.Blue
            R1Robot.FillColor = Color.Blue
        End If

        If Red2Bypass = True Then
            R2DS.FillColor = Color.Blue
            R2Robot.FillColor = Color.Blue
        End If

        If Red3Bypass = True Then
            R3DS.FillColor = Color.Blue
            R3Robot.FillColor = Color.Blue
        End If

        If Blue1Bypass = True Then
            B1DS.FillColor = Color.Blue
            B1Robot.FillColor = Color.Blue
        End If

        If Blue2Bypass = True Then
            B2DS.FillColor = Color.Blue
            B2Robot.FillColor = Color.Blue
        End If

        If Blue3Bypass = True Then
            B3DS.FillColor = Color.Blue
            B3Robot.FillColor = Color.Blue
        End If

        PLC_Match_Timer = matchTimerLbl.Text
        RedT1 = RedTeam1.Text
        RedT2 = RedTeam2.Text
        RedT3 = RedTeam3.Text
        BlueT1 = BlueTeam1.Text
        BlueT2 = BlueTeam2.Text
        BlueT3 = BlueTeam3.Text

        'Updates the audience display with time and scores'
        AudianceDisplay.Timerlbl.Text = matchTimerLbl.Text
        AudianceDisplay.RedScoreLbl.Text = RedScore
        AudianceDisplay.BlueScoreLbl.Text = BlueScore

        If Field_Estop = True Then
            HandleAbortedMatch()
        End If

    End Sub

    Private Sub Pre_Start_btn_Click(sender As Object, e As EventArgs) Handles Pre_Start_btn.Click
        Field_Estop = False

        If Field.handleLighting IsNot Nothing Then
            Field.handleLighting.Abort()
        Else
            Field.handleLighting.Start()
        End If

        updateField(MatchEnums.PreMatch)
        matchTimerLbl.Text = SandStormTime
        AutoTimer.Enabled = False
        MatchMessages.Text = "Field Pre-Started"
        ResetPLC()
        resetUI()

        Pre_Match_Selector.Show()
    End Sub

    Private Sub StartMatch_btn_Click(sender As Object, e As EventArgs) Handles StartMatch_btn.Click
        updateField(MatchEnums.SandStorm)
        AutoTimer.Start()
    End Sub

    Private Sub AutoTimer_Tick(sender As Object, e As EventArgs) Handles AutoTimer.Tick
        AutoTimer.Start()
        AutoTimer.Interval = 1000
        matchTimerLbl.Text = Val(matchTimerLbl.Text) - 1
        MatchMessages.Text = "Sand Storm"
        auto_score = True

        If matchTimerLbl.Text = 0 Then
            updateField(MatchEnums.TeleOp)
            matchTimerLbl.Text = TeleTime
            AutoTimer.Stop()
            TeleTimer.Enabled = True
            TeleTimer.Start()
        End If
    End Sub

    Private Sub TeleTimer_Tick(sender As Object, e As EventArgs) Handles TeleTimer.Tick
        TeleTimer.Start()
        TeleTimer.Interval = 1000
        matchTimerLbl.Text = Val(matchTimerLbl.Text) - 1
        MatchMessages.Text = "Tele-Operated"

        If matchTimerLbl.Text = 130 Then
            auto_score = False
        End If

        If matchTimerLbl.Text = 30 Then
            updateField(MatchEnums.EndGameWarning)
            matchTimerLbl.Text = EndgameWarningTime
            TeleTimer.Stop()
            EndGameTimer.Enabled = True
            EndGameTimer.Start()
        End If
    End Sub

    Private Sub EndGameTimer_Tick(sender As Object, e As EventArgs) Handles EndGameTimer.Tick
        EndGameTimer.Start()
        EndGameTimer.Interval = 1000
        matchTimerLbl.Text = Val(matchTimerLbl.Text) - 1
        MatchMessages.Text = "EndGame Warning"

        If matchTimerLbl.Text = 20 Then
            updateField(MatchEnums.EndGame)
            MatchMessages.Text = "EndGame"
        End If

        If matchTimerLbl.Text = 0 Then

            updateField(MatchEnums.PostMatch)
            EndGameTimer.Stop()
            SandStormMessage.Text = ""
            MatchMessages.Text = "Match Ended"
        End If
    End Sub

    Private Sub AbortMatch_btn_Click(sender As Object, e As EventArgs) Handles AbortMatch_btn.Click
        HandleAbortedMatch()
        Field_Estop = True
        Field.updateField(MatchEnums.AbortMatch)
        MatchMessages.Text = "Match Aborted"
        AutoTimer.Stop()
        TeleTimer.Stop()
        EndGameTimer.Stop()
        matchTimerLbl.Text = 0
    End Sub

    Public Sub HandleAbortedMatch()
        If Field_Estop = True Then
            If i < 1 Then
                i = i + 1
                AutoTimer.Stop()
                TeleTimer.Stop()
                EndGameTimer.Stop()
                matchTimerLbl.Text = 0
                Field.updateField(MatchEnums.AbortMatch)
            End If
        End If
    End Sub

    Public Shared Sub ResetPLC()
        PLC_Reset = True
        PLC_Estop_Field = False
        PLC_Estop_Red1 = False
        PLC_Estop_Red2 = False
        PLC_Estop_Red3 = False
        PLC_Estop_Blue1 = False
        PLC_Estop_Blue2 = False
        PLC_Estop_Blue3 = False
        Red_1_Estop = False
        Red_2_Estop = False
        Red_3_Estop = False
        Blue_1_Estop = False
        Blue_2_Estop = False
        Blue_3_Estop = False
    End Sub


    'FTA Group Buttons'
    Private Sub ConnectPLCBtn_Click(sender As Object, e As EventArgs) Handles ConnectPLCBtn.Click
        ConnectPLC()
    End Sub

    Private Sub DSLightTestBtn_Click(sender As Object, e As EventArgs) Handles DSLightTestBtn.Click
        'Alliance_Light_Test = True
    End Sub

    Private Sub ScoringTableLightTestBtn_Click(sender As Object, e As EventArgs) Handles ScoringTableLightTestBtn.Click
        Scoring_Light_Test = True
    End Sub

    Private Sub LedPatternTestBtn_Click(sender As Object, e As EventArgs) Handles LedPatternTestBtn.Click
        'Add Led Pattern Test'
    End Sub

    'Display Box Buttons'
    Private Sub PreMatchBtn_Click(sender As Object, e As EventArgs) Handles PreMatchBtn.Click
        AudianceDisplay.PrestartCover.Show()
        AudianceDisplay.PreStartPanel.Show()
        AudianceDisplay.FinalScoreBox.Hide()
        AudianceDisplay.Winner.Hide()
        AudianceDisplay.WinningAlliance.Hide()
        'AudianceDisplay.Label16.Hide()
    End Sub

    Private Sub MatchPlay_Click(sender As Object, e As EventArgs) Handles MatchPlay.Click
        AudianceDisplay.PrestartCover.Hide()
        AudianceDisplay.PreStartPanel.Hide()
        AudianceDisplay.FinalScoreBox.Hide()
        AudianceDisplay.Winner.Hide()
        AudianceDisplay.WinningAlliance.Hide()
        'AudianceDisplay.Label16.Hide()
    End Sub

    Private Sub FinalScoreBtn_Click(sender As Object, e As EventArgs) Handles FinalScoreBtn.Click
        AudianceDisplay.PrestartCover.Show()
        AudianceDisplay.PreStartPanel.Show()
        AudianceDisplay.FinalScoreBox.Show()
        AudianceDisplay.Winner.Show()
        AudianceDisplay.WinningAlliance.Show()
        'AudianceDisplay.Label16.Show()
    End Sub

    Private Sub RBypass1_CheckedChanged(sender As Object, e As EventArgs) Handles RBypass1.CheckedChanged
        Red1Bypass = True
    End Sub

    Private Sub RBypass2_CheckedChanged(sender As Object, e As EventArgs) Handles RBypass2.CheckedChanged
        Red2Bypass = True
    End Sub

    Private Sub RBypass3_CheckedChanged(sender As Object, e As EventArgs) Handles RBypass3.CheckedChanged
        Red3Bypass = True
    End Sub

    Private Sub BBypass1_CheckedChanged(sender As Object, e As EventArgs) Handles BBypass1.CheckedChanged
        Blue1Bypass = True
    End Sub

    Private Sub BBypass2_CheckedChanged(sender As Object, e As EventArgs) Handles BBypass2.CheckedChanged
        Blue2Bypass = True
    End Sub

    Private Sub BBypass3_CheckedChanged(sender As Object, e As EventArgs) Handles BBypass3.CheckedChanged
        Blue3Bypass = True
    End Sub

    Private Sub resetScore()

        R1_Estop_Count = 0
    End Sub

    Public Sub updateScores()
        AudianceDisplay.Timerlbl.Text = matchTimerLbl.Text
        AudianceDisplay.RedScoreLbl.Text = RedScore
        AudianceDisplay.BlueScoreLbl.Text = BlueScore
        AudianceDisplay.BlueTeam1lbl.Text = BlueTeam1.Text
        AudianceDisplay.BlueTeam2.Text = BlueTeam2.Text
        AudianceDisplay.BlueTeam3.Text = BlueTeam3.Text
    End Sub

    Private Sub Button27_Click(sender As Object, e As EventArgs) Handles Button27.Click
        Red1DS.Estop = True
        Red_1_Estop = True
        R1Estop.BackColor = Color.Red
    End Sub

    Private Sub Button28_Click(sender As Object, e As EventArgs) Handles Button28.Click
        Red2DS.Estop = True
        Red_2_Estop = True
        R2Estop.BackColor = Color.Red
    End Sub

    Private Sub Button29_Click(sender As Object, e As EventArgs) Handles Button29.Click
        Red3DS.Estop = True
        Red_3_Estop = True
        R3Estop.BackColor = Color.Red
    End Sub

    Private Sub Button30_Click(sender As Object, e As EventArgs) Handles Button30.Click
        Blue1DS.Estop = True
        Blue_1_Estop = True
        B1Estop.BackColor = Color.Red
    End Sub

    Private Sub Button31_Click(sender As Object, e As EventArgs) Handles Button31.Click
        Blue2DS.Estop = True
        Blue_2_Estop = True
        B2Estop.BackColor = Color.Red
    End Sub

    Private Sub Button32_Click(sender As Object, e As EventArgs) Handles Button32.Click
        Blue3DS.Estop = True
        Blue_3_Estop = True
        B3Estop.BackColor = Color.Red
    End Sub

    Private Sub Button34_Click(sender As Object, e As EventArgs) Handles Button34.Click
        createBaseMatchJSON()
    End Sub

    'Manual Scoring Area'
    'Blue Varibles
    'Cargoship Bays'
    Public Shared blueBay1 = ""
    Public Shared blueBay2 = ""
    Public Shared blueBay3 = ""
    Public Shared blueBay4 = ""
    Public Shared blueBay5 = ""
    Public Shared blueBay6 = ""
    Public Shared blueBay7 = ""
    Public Shared blueBay8 = ""
    'Rocket Bays'
    Public Shared blueLowLeftRocketFar = ""
    Public Shared blueLowLeftRocketNear = ""
    Public Shared blueMidLeftRocketFar = ""
    Public Shared blueMidLeftRocketNear = ""
    Public Shared blueTopLeftRocketFar = ""
    Public Shared blueTopLeftRocketNear = ""
    Public Shared blueLowRightRocketFar = ""
    Public Shared blueLowRightRocketNear = ""
    Public Shared blueMidRightRocketFar = ""
    Public Shared blueMidRightRocketNear = ""
    Public Shared blueTopRightRocketFar = ""
    Public Shared blueTopRightRocketNear = ""
    'Completed Rockets'
    Public Shared blueCompleteRocketFar As Boolean
    Public Shared blueCompleteRocketNear As Boolean
    'Robot HAB Line Status'
    Public Shared blueHABLineRobot1 = ""
    Public Shared blueHABLineRobot2 = ""
    Public Shared blueHABLineRobot3 = ""
    'Endgame Robot Status'
    Public Shared blueEndgameRobot1 = ""
    Public Shared blueEndgameRobot2 = ""
    Public Shared blueEndgameRobot3 = ""
    'Total Rocket Ranking Point'
    Public Shared blueRocketRP As Integer
    'HAB Ranking Point'
    Public Shared blueHABRP As Integer
    'Total Hatch Panel Points'
    Public Shared blueHatchPanelPoints As Integer
    'Total Cargo Points'
    Public Shared blueCargoPoints As Integer
    'HAB Climb Points'
    Public Shared blueHABClimbPoints As Integer
    'Total Auto Points'
    Public Shared blueAutoPoints As Integer
    'Total Teleop Points'
    Public Shared blueTelePoints As Integer
    'Total Fouls Count'
    Public Shared blueFoulCount As Integer
    'Total Foul Points'
    Public Shared blueFoulPoints As Integer
    'Total Tech Foul Count
    Public Shared blueTechCount As Integer
    'Total RP'
    Public Shared blueRP As Integer
    'Total Match Points'
    Public Shared blueTotalPoints As Integer
    'Total Sand Storm Bonus
    Public Shared blueSandStormBonus As Integer

    'Red Varibles'

    'Cargoship Bays'
    Public Shared redBay1 = ""
    Public Shared redBay2 = ""
    Public Shared redBay3 = ""
    Public Shared redBay4 = ""
    Public Shared redBay5 = ""
    Public Shared redBay6 = ""
    Public Shared redBay7 = ""
    Public Shared redBay8 = ""
    'Rocket Bays'
    Public Shared redLowLeftRocketFar = ""
    Public Shared redLowLeftRocketNear = ""
    Public Shared redMidLeftRocketFar = ""
    Public Shared redMidLeftRocketNear = ""
    Public Shared redTopLeftRocketFar = ""
    Public Shared redTopLeftRocketNear = ""
    Public Shared redLowRightRocketFar = ""
    Public Shared redLowRightRocketNear = ""
    Public Shared redMidRightRocketFar = ""
    Public Shared redMidRightRocketNear = ""
    Public Shared redTopRightRocketFar = ""
    Public Shared redTopRightRocketNear = ""
    'Completed Rockets'
    Public Shared redCompleteRocketFar As Boolean
    Public Shared redCompleteRocketNear As Boolean
    'Robot HAB Line Status'
    Public Shared redHABLineRobot1 = ""
    Public Shared redHABLineRobot2 = ""
    Public Shared redHABLineRobot3 = ""
    'Endgame Robot Status'
    Public Shared redEndgameRobot1 = ""
    Public Shared redEndgameRobot2 = ""
    Public Shared redEndgameRobot3 = ""
    'Total Rocket Ranking Point'
    Public Shared redRocketRP As Integer
    'HAB Ranking Point'
    Public Shared redHABRP As Integer
    'Total Hatch Panel Points'
    Public Shared redHatchPanelPoints As Integer
    'Total Cargo Points'
    Public Shared redCargoPoints As Integer
    'HAB Climb Points'
    Public Shared redHABClimbPoints As Integer
    'Total Auto Points'
    Public Shared redAutoPoints As Integer
    'Total Teleop Points'
    Public Shared redTelePoints As Integer
    'Total Fouls Count'
    Public Shared redFoulCount As Integer
    'Total Foul Points'
    Public Shared redFoulPoints As Integer
    'Total Tech Foul Count
    Public Shared redTechCount As Integer
    'Total RP'
    Public Shared redRP As Integer
    'Total Match Points'
    Public Shared redTotalPoints As Integer
    'Total Sand Storm Bonus
    Public Shared redSandStormBonus As Integer

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        Scoring_Panel.Show()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Server()
    End Sub
End Class