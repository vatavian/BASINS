Imports System
Imports atcUtility
Imports atcData
Imports atcWDM
Imports atcHspfBinOut
Imports HspfSupport
Imports MapWinUtility
Imports atcGraph
Imports ZedGraph

Imports MapWindow.Interfaces
Imports System.Collections.Specialized

Module HSPFOutputReports
    Private pTestPath As String
    Private pBaseName As String
    Private pOutputLocations As New atcCollection
    Private pGraphSaveFormat As String
    Private pGraphAnnual As Boolean = False
    Private pCurveStepType As String = "RearwardStep"

    Private Sub Initialize()
        pOutputLocations.Clear()

        pGraphSaveFormat = ".png"
        'pGraphSaveFormat = ".emf"

        'Dim lTestName As String = "tinley"
        'Dim lTestName As String = "hspf"
        'Dim lTestName As String = "hyd_man"
        'Dim lTestName As String = "shena"
        Dim lTestName As String = "upatoi"
        'Dim lTestName As String = "calleguas_cat"
        'Dim lTestName As String = "calleguas_nocat"
        'Dim lTestName As String = "SantaClara"

        Select Case lTestName
            Case "shena"
                pTestPath = "c:\test\genscn"
                pBaseName = "base"
                pOutputLocations.Add("Lynnwood")
            Case "upatoi"
                pTestPath = "d:\Basins\modelout\Upatoi"
                pBaseName = "upatoi"
                pOutputLocations.Add("R:46")
                pGraphAnnual = True
                pCurveStepType = "NonStep" 'Tony's convention 
            Case "tinley"
                pTestPath = "c:\test\tinley"
                pBaseName = "tinley"
                pOutputLocations.Add("R:850")
            Case "calleguas_cat"
                pTestPath = "D:\MountainViewData\Calleguas\cat"
                pBaseName = "Calleg"
                pOutputLocations.Add("R:408")
                pOutputLocations.Add("R:10")
                pOutputLocations.Add("R:307")
            Case "calleguas_nocat"
                pTestPath = "D:\MountainViewData\Calleguas\nocat"
                pBaseName = "Calleg"
                pOutputLocations.Add("R:408")
                pOutputLocations.Add("R:10")
                pOutputLocations.Add("R:307")
            Case "hyd_man"
                pTestPath = "C:\test\EXP_CAL\hyd_man.net"
                pBaseName = "hyd_man"
                pOutputLocations.Add("R:5")
                pOutputLocations.Add("R:4")
            Case "hspf"
                pTestPath = "C:\test\HSPF"
                pBaseName = "test10"
            Case "SantaClara"
                pTestPath = "D:\MountainViewData\SantaClara\nocat"
                pBaseName = "SCR10"
                pOutputLocations.Add("R:70")
                pOutputLocations.Add("R:180")
                pOutputLocations.Add("R:320")
                pOutputLocations.Add("R:410")
                pOutputLocations.Add("R:880")
        End Select
    End Sub

    Public Sub ScriptMain(ByRef aMapWin As IMapWin)
        Initialize()
        ChDriveDir(pTestPath)
        If FileExists(pBaseName & "Orig.uci") Then
            IO.File.Copy(pBaseName & "Orig.uci", pBaseName & ".uci")
        End If

        'open uci file
        Dim lMsg As New atcUCI.HspfMsg
        lMsg.Open("hspfmsg.mdb")
        Dim lHspfUci As New atcUCI.HspfUci
        lHspfUci.FastReadUciForStarter(lMsg, pBaseName & ".uci")
        If pOutputLocations.Contains("Lynnwood") Then 'special case to check GenScn examples
            With lHspfUci.GlobalBlock
                .SDate(0) = 1986
                .SDate(1) = 10
                .SDate(2) = 1
                .EDate(0) = 1987
                .EDate(1) = 10
                .EDate(2) = 1
            End With
        End If
        'lHspfUci.Save()

        'open WDM file
        Dim lWdmFileName As String = pTestPath & "\" & pBaseName & ".wdm"
        Dim lWdmDataSource As New atcDataSourceWDM()
        lWdmDataSource.Open(lWdmFileName)

        Dim lOutFileName As String
        Dim lExpertSystemFileNames As New NameValueCollection
        AddFilesInDir(lExpertSystemFileNames, IO.Directory.GetCurrentDirectory, False, "*.exs")
        Dim lExpertSystem As HspfSupport.ExpertSystem
        For Each lExpertSystemFileName As String In lExpertSystemFileNames
            Try
                Dim lFileCopied As Boolean = False
                If IO.Path.GetFileNameWithoutExtension(lExpertSystemFileName).ToLower <> pBaseName.ToLower Then
                    IO.File.Copy(lExpertSystemFileName, pBaseName & ".exs")
                    lFileCopied = True
                End If
                lExpertSystem = New HspfSupport.ExpertSystem(lHspfUci, lWdmDataSource)
                Dim lStr As String = lExpertSystem.Report
                SaveFileString("outfiles\ExpertSysStats-" & IO.Path.GetFileNameWithoutExtension(lExpertSystemFileName) & ".txt", lStr)

                'lStr = lExpertSystem.AsString 'NOTE:just testing
                'SaveFileString(FilenameOnly(lHspfUci.Name) & ".exx", lStr)

                Dim lCons As String = "Flow"
                For lSiteIndex As Integer = 1 To lExpertSystem.Sites.Count
                    Dim lSite As String = lExpertSystem.Sites(lSiteIndex).Name
                    If lSite.ToLower = "lynnwood" Then 'special case to check GenScn manual graph examples
                    End If
                    Dim lArea As Double = lExpertSystem.Sites(lSiteIndex).Area
                    Dim lSimTSerInches As atcTimeseries = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(0))
                    lSimTSerInches.Attributes.SetValue("Units", "Flow (inches)")
                    Dim lSimTSer As atcTimeseries = InchesToCfs(lSimTSerInches, lArea)
                    lSimTSer.Attributes.SetValue("Units", "Flow (cfs)")
                    lSimTSer.Attributes.SetValue("YAxis", "Left")
                    lSimTSer.Attributes.SetValue("StepType", pCurveStepType)
                    Dim lObsTSer As atcTimeseries = SubsetByDate(lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(1)), lExpertSystem.SDateJ, lExpertSystem.EDateJ, Nothing)
                    lObsTSer.Attributes.SetValue("Units", "Flow (cfs)")
                    lObsTSer.Attributes.SetValue("YAxis", "Left")
                    lObsTSer.Attributes.SetValue("StepType", pCurveStepType)
                    Dim lObsTSerInches As atcTimeseries = CfsToInches(lObsTSer, lArea)
                    lObsTSerInches.Attributes.SetValue("Units", "Flow (inches)")
                    Dim lPrecTSer As atcTimeseries = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(5))
                    lPrecTSer.Attributes.SetValue("Units", "inches")

                    lStr = HspfSupport.MonthlyAverageCompareStats.Report(lHspfUci, _
                                                                         lCons, lSite, _
                                                                         "inches", _
                                                                         lSimTSerInches, lObsTSerInches, _
                                                                         lExpertSystem.SDateJ, _
                                                                         lExpertSystem.EDateJ)
                    lOutFileName = "outfiles\MonthlyAverage" & lCons & "Stats" & "-" & lSite & ".txt"
                    SaveFileString(lOutFileName, lStr)

                    lStr = HspfSupport.AnnualCompareStats.Report(lHspfUci, _
                                                                 lCons, lSite, _
                                                                 "inches", _
                                                                 lPrecTSer, lSimTSerInches, lObsTSerInches, _
                                                                 lExpertSystem.SDateJ, _
                                                                 lExpertSystem.EDateJ)
                    lOutFileName = "outfiles\Annual" & lCons & "Stats" & "-" & lSite & ".txt"
                    SaveFileString(lOutFileName, lStr)

                    lStr = HspfSupport.DailyMonthlyCompareStats.Report(lHspfUci, _
                                                                       lCons, lSite, _
                                                                       lSimTSer, lObsTSer, _
                                                                       lExpertSystem.SDateJ, _
                                                                       lExpertSystem.EDateJ)
                    lOutFileName = "outfiles\DailyMonthly" & lCons & "Stats" & "-" & lSite & ".txt"
                    SaveFileString(lOutFileName, lStr)

                    'graphics 
                    'TODO: add titles to graphs
                    Dim lDataGroup As New atcDataGroup
                    lDataGroup.Add(Aggregate(SubsetByDate(lObsTSer, _
                                                          lExpertSystem.SDateJ, _
                                                          lExpertSystem.EDateJ, Nothing), _
                                             atcTimeUnit.TUDay, 1, atcTran.TranAverSame, Nothing))
                    lDataGroup.Add(Aggregate(SubsetByDate(lSimTSer, _
                                                          lExpertSystem.SDateJ, _
                                                          lExpertSystem.EDateJ, Nothing), _
                                             atcTimeUnit.TUDay, 1, atcTran.TranAverSame, Nothing))
                    Dim lOutFileBase As String = "outfiles\" & lCons & "_" & lSite
                    Dim lZgc As ZedGraphControl
                    'duration plot
                    lZgc = CreateZgc()
                    Dim lGraphDur As New clsGraphProbability(lDataGroup, lZgc)
                    lZgc.SaveIn(lOutFileBase & "_dur" & pGraphSaveFormat)
                    lGraphDur.Dispose()
                    lZgc.Dispose()
                    'cummulative difference
                    lZgc = CreateZgc()
                    Dim lGraphCum As New clsGraphCumulativeDifference(lDataGroup, lZgc)
                    lZgc.SaveIn(lOutFileBase & "_cumDif" & pGraphSaveFormat)
                    lGraphCum.Dispose()
                    lZgc.Dispose()

                    'scatter
                    lZgc = CreateZgc()
                    Dim lGraphScatter As New clsGraphScatter(lDataGroup, lZgc)

                    'TODO: move the regression line and title to a more generic place
                    '45 degree line
                    Dim lPane As ZedGraph.GraphPane = lZgc.MasterPane.PaneList(0)
                    AddLine(lPane, 1, 0, Drawing.Drawing2D.DashStyle.Dot, "45DegLine")
                    'regression line 
                    Dim lACoef As Double
                    Dim lBCoef As Double
                    Dim lRSquare As Double
                    FitLine(lDataGroup.ItemByIndex(1), lDataGroup.ItemByIndex(0), lACoef, lBCoef, lRSquare)
                    AddLine(lPane, lACoef, lBCoef, Drawing.Drawing2D.DashStyle.Solid, "RegLine")
                    Dim lText As New TextObj
                    Dim lFmt As String = "###,##0.###"
                    lText.Text = "Y = " & DoubleToString(lACoef, , lFmt) & " X + " & DoubleToString(lBCoef, , lFmt) & Environment.NewLine & _
                                 "R = " & DoubleToString(Math.Sqrt(lRSquare), , lFmt) & vbCrLf & _
                                 "R Squared = " & DoubleToString(lRSquare, , lFmt)
                    lText.FontSpec.StringAlignment = Drawing.StringAlignment.Near
                    lText.Location = New Location(0.05, 0.05, CoordType.ChartFraction, AlignH.Left, AlignV.Top)
                    lText.FontSpec.Border.IsVisible = False
                    lPane.GraphObjList.Add(lText)
                    lPane.XAxis.Title.Text &= vbCrLf & vbCrLf & "Scatter Plot"

                    lZgc.SaveIn(lOutFileBase & "_scatDay" & pGraphSaveFormat)
                    With lZgc.MasterPane.PaneList(0)
                        .YAxis.Type = AxisType.Log
                        .XAxis.Type = AxisType.Log
                    End With
                    lZgc.SaveIn(lOutFileBase & "_scatDay_log" & pGraphSaveFormat)
                    lGraphScatter.Dispose()
                    lZgc.Dispose()

                    'scatter - LZS vs Error(cfs)
                    Dim lTSer As atcTimeseries = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(9))
                    lZgc = CreateZgc()
                    If GraphScatterError(lZgc, lDataGroup, lExpertSystem.SDateJ, lExpertSystem.EDateJ, lTSer, "LZS (in)") Then
                        lZgc.SaveIn(lOutFileBase & "_Error_LZS" & pGraphSaveFormat)
                    End If
                    'scatter - UZS vs Error(cfs)
                    lTSer = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(8))
                    lZgc = CreateZgc()
                    If GraphScatterError(lZgc, lDataGroup, lExpertSystem.SDateJ, lExpertSystem.EDateJ, lTSer, "UZS (in)") Then
                        lZgc.SaveIn(lOutFileBase & "_Error_UZS" & pGraphSaveFormat)
                    End If
                    'scatter - Observed vs Error(cfs)    
                    lZgc = CreateZgc()
                    If GraphScatterError(lZgc, lDataGroup, lExpertSystem.SDateJ, lExpertSystem.EDateJ, lObsTSer, "Observed (cfs)", AxisType.Log) Then
                        lZgc.SaveIn(lOutFileBase & "_Error_ObsFlow" & pGraphSaveFormat)
                    End If
                    'add precip to aux axis
                    Dim lPaneCount As Integer = 1
                    If Not lPrecTSer Is Nothing Then
                        lPrecTSer.Attributes.SetValue("YAxis", "Aux")
                        lDataGroup.Add(SubsetByDate(lPrecTSer, _
                                                    lExpertSystem.SDateJ, _
                                                    lExpertSystem.EDateJ, Nothing))
                        lPaneCount = 2
                    End If

                    'whole span
                    GraphTimeseries(lDataGroup, lPaneCount, lOutFileBase)
                    If pGraphAnnual Then
                        'years
                        Dim lSDateJ As Double = lExpertSystem.SDateJ
                        Dim lDate(5) As Integer
                        While lSDateJ < lExpertSystem.EDateJ
                            Dim lEDateJ As Double = TimAddJ(lSDateJ, 6, 1, 1)
                            Dim lDataGroupYear As New atcDataGroup
                            For Each lTimeseries As atcTimeseries In lDataGroup
                                lDataGroupYear.Add(SubsetByDate(lTimeseries, lSDateJ, lEDateJ, Nothing))
                            Next
                            J2Date(lSDateJ, lDate)
                            If lDate(1) <> 1 OrElse lDate(2) <> 1 Then lDate(0) += 1 'non calendar years label with ending year
                            GraphTimeseries(lDataGroupYear, lPaneCount, lOutFileBase & "_" & lDate(0))
                            lSDateJ = lEDateJ
                        End While
                    End If

                    'monthly
                    Dim lMonthDataGroup As New atcDataGroup
                    lMonthDataGroup.Add(Aggregate(lDataGroup.Item(0), atcTimeUnit.TUMonth, 1, atcTran.TranAverSame))
                    lMonthDataGroup.Add(Aggregate(lDataGroup.Item(1), atcTimeUnit.TUMonth, 1, atcTran.TranAverSame))
                    If lPaneCount = 2 Then lMonthDataGroup.Add(Aggregate(lDataGroup.Item(2), atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv))
                    lZgc = CreateZgc()
                    lZgc.Width *= 2
                    Dim lGrapher As New clsGraphTime(lMonthDataGroup, lZgc)
                    If lPaneCount = 2 Then lZgc.MasterPane.PaneList(0).YAxis.Title.Text = "Precip (in)"
                    Dim lDualDateScale As Object = lZgc.MasterPane.PaneList(0).XAxis.Scale
                    lDualDateScale.MaxDaysMonthLabeled = 1200
                    lZgc.SaveIn(lOutFileBase & "_month" & pGraphSaveFormat)
                    'monthly timeseries - log
                    With lZgc.MasterPane.PaneList(lPaneCount - 1) 'main pane, not aux
                        .YAxis.Type = ZedGraph.AxisType.Log
                        .YAxis.Scale.Max *= 4 'wag!
                        .YAxis.Scale.MaxAuto = False
                        .YAxis.Scale.IsUseTenPower = False
                    End With
                    lZgc.SaveIn(lOutFileBase & "_month_log " & pGraphSaveFormat)
                    lZgc.Dispose()
                    lGrapher.Dispose()
                    'weekly ET - pet vs act
                    lDataGroup.Clear()
                    For lIndex As Integer = 6 To 7 'pet:6, act:7
                        lTSer = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(lIndex))
                        lTSer.Attributes.SetValue("Units", "ET (in)")
                        lTSer.Attributes.SetValue("YAxis", "Left")
                        If lIndex = 6 Then ' force pet to be observed
                            lTSer.Attributes.SetValue("Scenario", "Observed")
                        End If
                        lDataGroup.Add(SubsetByDate(Aggregate(lTSer, atcTimeUnit.TUDay, 7, atcTran.TranSumDiv), _
                                                    lExpertSystem.SDateJ, _
                                                    lExpertSystem.EDateJ, Nothing))
                    Next
                    lZgc = CreateZgc()
                    lGrapher = New clsGraphTime(lDataGroup, lZgc)
                    lZgc.Width *= 2
                    lDualDateScale = lZgc.MasterPane.PaneList(0).XAxis.Scale
                    lDualDateScale.MaxDaysMonthLabeled = 1200
                    lZgc.SaveIn(lOutFileBase & "_ET" & pGraphSaveFormat)
                    lZgc.Dispose()
                    lGrapher.Dispose()

                    'flow components
                    lDataGroup.Clear()
                    For lIndex As Integer = 4 To 2 Step -1 'baseflow:4,interflow:3,surface:2
                        lTSer = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(lIndex))
                        lTSer.Attributes.SetValue("Units", "Flow (in)")
                        lTSer.Attributes.SetValue("YAxis", "Left")
                        lDataGroup.Add(SubsetByDate(lTSer, _
                                                    lExpertSystem.SDateJ, _
                                                    lExpertSystem.EDateJ, Nothing))
                    Next
                    'precip
                    lPrecTSer = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(5))
                    lPrecTSer.Attributes.SetValue("YAxis", "Aux")
                    lDataGroup.Add(SubsetByDate(lPrecTSer, _
                                                lExpertSystem.SDateJ, _
                                                lExpertSystem.EDateJ, Nothing))
                    'actual et
                    Dim lETTSer As atcTimeseries = lWdmDataSource.DataSets.ItemByKey(lExpertSystem.Sites(lSiteIndex).Dsn(7))
                    lETTSer.Attributes.SetValue("YAxis", "Left")
                    lDataGroup.Add(SubsetByDate(lETTSer, _
                                                lExpertSystem.SDateJ, _
                                                lExpertSystem.EDateJ, Nothing))
                    'do the graphs
                    GraphFlowComponents(lDataGroup, lOutFileBase)
                Next lSiteIndex
                lExpertSystem = Nothing
                If lFileCopied Then
                    IO.File.Delete(pBaseName & ".exs")
                End If
            Catch lEx As ApplicationException
                Logger.Dbg(lEx.Message)
            End Try
        Next lExpertSystemFileName

        'open HBN file
        'TODO: need to allow additional binary output files!
        Dim lHspfBinFileName As String = pTestPath & "\" & pBaseName & ".hbn"
        Dim lHspfBinDataSource As New atcTimeseriesFileHspfBinOut()
        lHspfBinDataSource.Open(lHspfBinFileName)

        Dim lSummaryType As String = "Water"

        'watershed summary
        Dim lHspfBinFileInfo As System.IO.FileInfo = New System.IO.FileInfo(lHspfBinFileName)
        Dim lString As Text.StringBuilder = HspfSupport.WatershedSummary.Report(lHspfUci, lHspfBinDataSource, lHspfBinFileInfo.LastWriteTime, lSummaryType)
        lOutFileName = "outfiles\" & lSummaryType & "_" & "WatershedSummary.txt"
        SaveFileString(lOutFileName, lString.ToString)
        lString = Nothing

        'build collection of operation types to report
        Dim lOperationTypes As New atcCollection
        lOperationTypes.Add("P:", "PERLND")
        lOperationTypes.Add("I:", "IMPLND")
        lOperationTypes.Add("R:", "RCHRES")
        Dim lLocations As atcCollection = lHspfBinDataSource.DataSets.SortedAttributeValues("Location")

        'constituent balance
        lString = HspfSupport.ConstituentBalance.Report _
           (lHspfUci, lSummaryType, lOperationTypes, pBaseName, _
            lHspfBinDataSource, lLocations, lHspfBinFileInfo.LastWriteTime)
        lOutFileName = "outfiles\" & lSummaryType & "_" & "ConstituentBalance.txt"
        SaveFileString(lOutFileName, lString.ToString)

        'watershed constituent balance 
        lString = HspfSupport.WatershedConstituentBalance.Report _
           (lHspfUci, lSummaryType, lOperationTypes, pBaseName, _
            lHspfBinDataSource, lHspfBinFileInfo.LastWriteTime)
        lOutFileName = "outfiles\" & lSummaryType & "_" & "WatershedConstituentBalance.txt"
        SaveFileString(lOutFileName, lString.ToString)

        If pOutputLocations.Count > 0 Then 'subwatershed constituent balance 
            HspfSupport.WatershedConstituentBalance.ReportsToFiles _
               (lHspfUci, lSummaryType, lOperationTypes, pBaseName, _
                lHspfBinDataSource, pOutputLocations, lHspfBinFileInfo.LastWriteTime, _
                "outfiles\")
        End If
    End Sub

    Sub GraphTimeseries(ByVal aDataGroup As atcDataGroup, _
                        ByVal aPaneCount As Integer, _
                        ByVal aOutFileBase As String)
        'timeseries - arith
        Dim lZgc As ZedGraphControl
        lZgc = CreateZgc()
        Dim lGrapher As New clsGraphTime(aDataGroup, lZgc)
        If aPaneCount = 2 Then lZgc.MasterPane.PaneList(0).YAxis.Title.Text = "Precip (in)"
        lZgc.SaveIn(aOutFileBase & pGraphSaveFormat)
        'timeseries - log
        With lZgc.MasterPane.PaneList(aPaneCount - 1)
            .YAxis.Type = ZedGraph.AxisType.Log
            'ScaleAxis(lDataGroup, .YAxis)
            .YAxis.Scale.Max *= 4 'wag!
            .YAxis.Scale.MaxAuto = False
            .YAxis.Scale.IsUseTenPower = False
        End With
        lZgc.SaveIn(aOutFileBase & "_log " & pGraphSaveFormat)
        lGrapher.Dispose()
        lZgc.Dispose()
    End Sub

    Sub GraphFlowComponents(ByVal aDataGroup As atcDataGroup, _
                            ByVal aOutFileBase As String)
        Dim lDataGroupOutput As New atcDataGroup

        'baseflow + interflow
        Dim lMathBaseInterTSer As atcTimeseries = atcTimeseriesMath.atcTimeseriesMath.Compute("add", aDataGroup.Item(0), aDataGroup.Item(1))
        lMathBaseInterTSer.Attributes.SetValue("Constituent", "Interflow+baseflow")
        lDataGroupOutput.Add(lMathBaseInterTSer)

        'total - add surface runoff
        Dim lMathTSer As atcTimeseries = atcTimeseriesMath.atcTimeseriesMath.Compute("add", lMathBaseInterTSer, aDataGroup.Item(2))
        lMathTSer.Attributes.SetValue("Constituent", "Simulated")
        lDataGroupOutput.Add(lMathTSer)

        'precip - actual et
        Dim lMathPrecEtTSer As atcTimeseries = _
            atcTimeseriesMath.atcTimeseriesMath.Compute("subtract", _
                                Aggregate(aDataGroup.Item(3), atcTimeUnit.TUDay, 1, atcTran.TranSumDiv), _
                                Aggregate(aDataGroup.Item(4), atcTimeUnit.TUDay, 1, atcTran.TranSumDiv))
        lMathPrecEtTSer.Attributes.SetValue("Constituent", "Precip-ActET")
        lMathPrecEtTSer.Attributes.SetValue("YAxis", "Left")

        aDataGroup.Item(0).Attributes.SetValue("Constituent", "Baseflow")
        lDataGroupOutput.Add(aDataGroup.Item(0)) 'baseflow

        lDataGroupOutput.Add(aDataGroup.Item(3)) 'precip

        Dim lZgc As ZedGraphControl = CreateZgc()
        lZgc.Width *= 3
        Dim lGrapher As New clsGraphTime(lDataGroupOutput, lZgc)
        Dim lDualDateScale As Object = lZgc.MasterPane.PaneList(1).XAxis.Scale
        lDualDateScale.MaxDaysMonthLabeled = 1200
        lZgc.MasterPane.PaneList(1).YAxis.Title.Text = "Flow (in)"
        lZgc.MasterPane.PaneList(0).YAxis.Title.Text = "Precip (in)"
        lZgc.SaveIn(aOutFileBase & "_Components" & pGraphSaveFormat)
        With lZgc.MasterPane.PaneList(1) 'main pane, not aux
            .YAxis.Type = ZedGraph.AxisType.Log
            .YAxis.Scale.Max = 4
            .YAxis.Scale.Min = 0.001
            .YAxis.Scale.MaxAuto = False
            .YAxis.Scale.IsUseTenPower = False
        End With
        lZgc.SaveIn(aOutFileBase & "_Components_Log" & pGraphSaveFormat)
        lZgc.Dispose()
        lGrapher.Dispose()

        'now monthly
        Dim lMonthDataGroup As New atcDataGroup
        'one axis test
        lDataGroupOutput.Item(3).Attributes.SetValue("YAxis", "Left")
        lMonthDataGroup.Add(Aggregate(lDataGroupOutput.Item(0), atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv))
        lMonthDataGroup.Add(Aggregate(lDataGroupOutput.Item(1), atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv))
        lMonthDataGroup.Add(Aggregate(lDataGroupOutput.Item(2), atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv))
        lMonthDataGroup.Add(Aggregate(lDataGroupOutput.Item(3), atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv)) 'prec
        lMonthDataGroup.Add(Aggregate(lMathPrecEtTSer, atcTimeUnit.TUMonth, 1, atcTran.TranSumDiv)) 'prec -act et
        lZgc = CreateZgc()
        lZgc.Width *= 3
        lGrapher = New clsGraphTime(lMonthDataGroup, lZgc)
        lDualDateScale = lZgc.MasterPane.PaneList(0).XAxis.Scale
        lDualDateScale.MaxDaysMonthLabeled = 1200
        lZgc.MasterPane.PaneList(0).YAxis.Scale.Max = 10
        lZgc.SaveIn(aOutFileBase & "_Components_month" & pGraphSaveFormat)
        'monthly timeseries - log
        lGrapher.Datasets.RemoveAt(4) 'has negative values, looks wierd
        With lZgc.MasterPane.PaneList(0) 'main pane, not aux
            'With lZgc.MasterPane.PaneList(1) 'main pane, not aux
            .YAxis.Type = ZedGraph.AxisType.Log
            .YAxis.Scale.Max = 10
            .YAxis.Scale.Min = 0.001
            .YAxis.Scale.MaxAuto = False
            .YAxis.Scale.IsUseTenPower = False
        End With
        lZgc.SaveIn(aOutFileBase & "_Components_month_log " & pGraphSaveFormat)
        lZgc.Dispose()
        lGrapher.Dispose()
    End Sub

    Function GraphScatterError(ByVal aZgc As ZedGraphControl, ByVal aDataGroup As atcDataGroup, _
                               ByVal aSDateJ As Double, ByVal aEDateJ As Double, _
                               ByVal aXAxisTser As atcTimeseries, ByVal aXAxisTitle As String, _
                               Optional ByVal aXAxisType As ZedGraph.AxisType = AxisType.Linear) As Boolean
        Dim lMath As New atcTimeseriesMath.atcTimeseriesMath
        Dim lMathArgs As New atcDataAttributes
        lMathArgs.SetValue("timeseries", aDataGroup)
        If lMath.Open("subtract", lMathArgs) Then
            Dim lDataGroupError As New atcDataGroup
            lDataGroupError.Add(SubsetByDate(aXAxisTser, aSDateJ, aEDateJ, Nothing))
            lDataGroupError.Add(SubsetByDate(lMath.DataSets(0), aSDateJ, aEDateJ, Nothing))
            Dim lGraphScatter As clsGraphScatter = New clsGraphScatter(lDataGroupError, aZgc)
            With aZgc.MasterPane.PaneList(0)
                .XAxis.Title.Text = aXAxisTitle
                .XAxis.Type = aXAxisType
                If aXAxisType = AxisType.Linear Then
                    Scalit(aXAxisTser.Attributes.GetValue("Minimum"), _
                           aXAxisTser.Attributes.GetValue("Maximum"), _
                           False, .XAxis.Scale.Min, .XAxis.Scale.Max)
                Else
                    .XAxis.Scale.Min = 1
                    .XAxis.Scale.Max = aXAxisTser.Attributes.GetDefinedValue("Maximum").Value * 2
                    .XAxis.Scale.IsUseTenPower = False
                End If
                .YAxis.Title.Text = "Error (cfs)"
                If Math.Abs(.YAxis.Scale.Min) > .YAxis.Scale.Max Then
                    .YAxis.Scale.Max = -.YAxis.Scale.Min
                Else
                    .YAxis.Scale.Min = -.YAxis.Scale.Max
                End If
            End With
            lGraphScatter.Dispose()
            Return True
        Else 'TODO:need error message
            Return False
        End If
    End Function
End Module
