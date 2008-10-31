Imports atcData
Imports atcData.atcDataGroup
Imports atcUtility
Imports MapWindow.Interfaces
Imports MapWinUtility

Imports Microsoft.VisualBasic
Imports System

Public Module CatSummary
    Private Const pFieldWidth As Integer = 12
    Private Const pTestPath As String = "C:\mono_luChange\output\lu2030a2\"
    Private Const pCatSummaryFileName As String = "CatSummary.txt"
    Private pCatXMLFile As String = "VaryPrecTempHbnPrepare.xml"
    Private WithEvents pCat As New atcClimateAssessmentTool.clsCat
    Private pCatList As ArrayList


    Public Sub ScriptMain(ByRef aMapWin As IMapWin)

        'For each UCI file in the specified (pTestPath) folder, 
        'do a Cat Summary Report 

        'Requirements:
        'UCI, WDM and HBN must reside in the same folder,
        'base name of UCI = base name of HBN file = base name of WDM file

        'Output:
        'writes to the pTestPath folder, a file named "CatSummary.txt",
        'appending to the file for each UCI (scenario)

        Logger.Dbg("Start")

        'change to the directory of the current project
        ChDriveDir(pTestPath)
        Logger.Dbg(" CurDir:" & CurDir())

        'delete the output file if it already exists
        If FileExists(pCatSummaryFileName) Then
            Kill(pCatSummaryFileName)
        End If

        'read in the xml for the endpoints and parse out the variations
        Logger.Dbg("read in endpoints")
        With pCat
            .XML = IO.File.ReadAllText(pCatXMLFile) ' TODO: ZZ deal with no need for uci files
            '.StartRun("Modified")
            'Logger.Dbg("RunsComplete")
            'IO.File.WriteAllText("CatRunnerResults.txt", .ResultsGrid.ToString)
            '.Inputs.Clear()
            '.Endpoints.Clear()
            '.PreparedInputs.Clear()
        End With

        'build collection of location::constituent<->operation list

        pCatList = New ArrayList
        Try
            For Each lVar As atcClimateAssessmentTool.atcVariation In pCat.Endpoints
                'ATTENTION:
                'Put the selected months here assuming there is only one dataset per variation
                'If not, then the selected months for the first dataset within the current variation is added into all datasets
                'So has to pay attention here
                Dim lSelectedMonths As String = ""
                If lVar.Seasons IsNot Nothing Then
                    For li As Integer = 1 To 12
                        If lVar.Seasons.SeasonSelected(li) Then lSelectedMonths &= li & ","
                    Next
                    'Rid of the last , at the end of the lSelectedMonths string
                    lSelectedMonths = lSelectedMonths.TrimEnd(",")
                End If

                For Each ldataset As atcDataSet In lVar.DataSets
                    Dim lCatTemp(3) As String
                    Dim lChkSeasons As Boolean = True
                    lCatTemp(0) = ldataset.Attributes.GetFormattedValue("location")  ' e.g. Seg1
                    lCatTemp(1) = ldataset.Attributes.GetFormattedValue("constituent") ' e.g. HPRC
                    lCatTemp(2) = lVar.Operation.ToString ' e.g. Sum
                    lCatTemp(3) = lSelectedMonths ' either this is empty string or a comma separated list of Months
                    If lCatTemp(1) = "HPRC" Or lCatTemp(1) = "ATMP" Or lCatTemp(1) = "EVAP" Then lChkSeasons = False
                    If Not foundMatchingEndpoint(pCatList, lCatTemp, lChkSeasons) Then
                        pCatList.Add(lCatTemp)
                    End If
                Next
            Next
        Catch ex As Exception
            Logger.Msg("Cat structure failed: " & ex.ToString)
        End Try

        'build collection of scenarios (uci base names) to report
        Dim lUcis As New System.Collections.Specialized.NameValueCollection
        'do we want the 'base.uci'?
        AddFilesInDir(lUcis, pTestPath, False, "*.uci")
        Dim lScenarios As New atcCollection
        For Each lUci As String In lUcis
            lScenarios.Add(FilenameNoPath(FilenameNoExt(lUci)))
        Next

        'declare a new data manager to manage the hbn and wdm files
        'Dim lDataManager As New atcDataManager(aMapWin)

        'loop thru each scenario (uci name)
        For Each lScenario As String In lScenarios
            If atcDataManager.DataSources.Count > 0 Then
                Logger.Dbg("Clearing " & atcDataManager.DataSources.Count & " datasources.")
                atcDataManager.DataSources.Clear()
            End If

            'open the corresponding hbn file
            Dim lHspfBinFile As atcDataSource = New atcHspfBinOut.atcTimeseriesFileHspfBinOut
            Dim lHspfBinFileName As String = lScenario & ".hbn"
            Logger.Dbg(" AboutToOpen " & lHspfBinFileName)
            If Not FileExists(lHspfBinFileName) Then
                'if hbn doesnt exist, make a guess at what the name might be
                lHspfBinFileName = lHspfBinFileName.Replace(".hbn", ".base.hbn")
                Logger.Dbg("  NameUpdated " & lHspfBinFileName)
            End If
            Dim lHspfBinFileInfo As System.IO.FileInfo = New System.IO.FileInfo(lHspfBinFileName)
            atcDataManager.OpenDataSource(lHspfBinFile, lHspfBinFileName, Nothing)
            Logger.Dbg(" DataSetCount " & lHspfBinFile.DataSets.Count)

            'open the corresponding wdm file
            Dim lHspfWdmFile As atcDataSource = New atcWDM.atcDataSourceWDM
            Dim lHspfWdmFileName As String = lScenario & ".wdm"
            Logger.Dbg(" AboutToOpen " & lHspfWdmFileName)
            If Not FileExists(lHspfWdmFileName) Then
                'if wdm doesnt exist, make a guess at what the name might be
                lHspfWdmFileName = lHspfWdmFileName.Replace(".wdm", ".base.wdm")
                Logger.Dbg("  NameUpdated " & lHspfWdmFileName)
            End If
            atcDataManager.OpenDataSource(lHspfWdmFile, lHspfWdmFileName, Nothing)
            Logger.Dbg(" DataSetCount " & lHspfWdmFile.DataSets.Count)

            'call main cat summary routine
            DoCatSummary(lScenario)

            atcDataManager.DataSources.Remove(lHspfBinFile)
            lHspfBinFile.DataSets.Clear()
            lHspfBinFile = Nothing
            atcDataManager.DataSources.Remove(lHspfWdmFile)
            lHspfWdmFile.DataSets.Clear()
            lHspfWdmFile = Nothing

        Next lScenario

        pCatList.Clear()
        pCatList = Nothing

        Logger.Msg("Done summary of CAT run results")

    End Sub

    Public Function foundMatchingEndpoint(ByRef aVarList As ArrayList, ByRef aCat() As String, ByVal aChkSeasons As Boolean) As Boolean
        'This function loops through the Endpoints-Variation collection to search for the matching one as user specified entry
        'Found it, return True
        'Not found it, return False
        'This function is used to make sure there is only unique set of variations are used to do the cat summary from run results
        Dim foundMatch As Boolean = False
        If aVarList Is Nothing Then Return False
        For Each lCatTemp() As String In aVarList
            If lCatTemp(0) = aCat(0) And lCatTemp(1) = aCat(1) And lCatTemp(2) = aCat(2) Then
                foundMatch = True
                If aChkSeasons Then
                    If Not String.Compare(lCatTemp(3), aCat(3)) = 0 Then foundMatch = False
                End If
            End If
        Next
        Return foundMatch
    End Function

    Friend Sub DoCatSummary(ByVal aScenario As String)
        Logger.Dbg("DoCatSummary for " & aScenario)

        Dim lString As New Text.StringBuilder
        lString.Append(aScenario)

        'Get this hard coded stuff from CAT endpoints/variations!
        Dim lMetDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "SEG1")
        Logger.Dbg("     MetMatchingDatasetCount " & lMetDataGroup.Count)
        lMetDataGroup.Add(atcDataManager.DataSets.FindData("Location", "_A24013"))
        lMetDataGroup.Add(atcDataManager.DataSets.FindData("Location", "A24013"))
        Logger.Dbg("     AdditionalMetMatchingDatasetCount " & lMetDataGroup.Count)
        For Each lCat() As String In pCatList
            If lCat(0) = "SEG1" Then
                If lMetDataGroup.Count > 0 Then
                    lString.Append(AnnualAndSeasonalValues(lMetDataGroup, lCat(1), lCat(2)))
                End If
            End If
        Next

        Dim lRchDataGroupW As atcDataGroup = atcDataManager.DataSets.FindData("Location", "RIV9")
        For Each lCat() As String In pCatList
            If lCat(0) = "RIV9" Then
                If lRchDataGroupW.Count > 0 Then
                    lString.Append(AnnualAndSeasonalValues(lRchDataGroupW, lCat(1), lCat(2)))
                End If
            End If
        Next

        Dim lRchDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "R:9")
        For Each lCat() As String In pCatList
            If lCat(0) = "R:9" Then
                If lRchDataGroup.Count > 0 Then
                    lString.Append(AnnualValue(lRchDataGroup, lCat(1), lCat(2)))
                    If lCat(3).Length > 1 Then
                        lString.Append(AnnualValue(lRchDataGroup, lCat(1), lCat(2), lCat(3), True))
                    End If
                End If
            End If
        Next

        '******** Second try ********
        'For Each lCat() As String In pCatList
        '    'lCat is an array of 3 elements, 0: Location (e.g. Seg1, RIV9) 1: Constituent (HPRC, RO) 2: Operation/Transaction (Sum, Mean)
        '    If lCat(1) = "FLOW" And Not (lCat(2) = "7Q10") Then lCat(1) = "WATR" 'This adjustment is done based on originals
        '    Select Case lCat(0)
        '        Case "SEG1"
        '            Dim lMetDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "SEG1")
        '            Logger.Dbg("     MetMatchingDatasetCount " & lMetDataGroup.Count)
        '            If lMetDataGroup.Count > 0 Then
        '                lString.Append(AnnualAndSeasonalValues(lMetDataGroup, lCat(1), lCat(2)))
        '            End If
        '        Case "RIV9"
        '            Dim lRchDataGroupW As atcDataGroup = atcDataManager.DataSets.FindData("Location", "RIV9")
        '            If lRchDataGroupW.Count > 0 Then
        '                lString.Append(AnnualAndSeasonalValues(lRchDataGroupW, lCat(1), lCat(2)))
        '            End If
        '        Case "R:9"
        '            Dim lRchDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "R:9")
        '            If lRchDataGroup.Count > 0 Then
        '                lString.Append(AnnualValue(lRchDataGroup, lCat(1), lCat(2)))
        '            End If
        '    End Select
        'Next


        '******* Original ********
        'Dim lMetDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "SEG1")
        'Logger.Dbg("     MetMatchingDatasetCount " & lMetDataGroup.Count)
        'lMetDataGroup.Add(atcDataManager.DataSets.FindData("Location", "_A24013"))
        'lMetDataGroup.Add(atcDataManager.DataSets.FindData("Location", "A24013"))
        'Logger.Dbg("     AdditionalMetMatchingDatasetCount " & lMetDataGroup.Count)

        'If lMetDataGroup.Count > 0 Then
        '    lString.Append(AnnualAndSeasonalValues(lMetDataGroup, "HPRC", "Sum"))
        '    lString.Append(AnnualAndSeasonalValues(lMetDataGroup, "ATMP", "Mean"))
        '    lString.Append(AnnualAndSeasonalValues(lMetDataGroup, "EVAP", "Sum"))
        'End If

        'Dim lRchDataGroupW As atcDataGroup = atcDataManager.DataSets.FindData("Location", "RIV9")
        'If lRchDataGroupW.Count > 0 Then
        '    lString.Append(AnnualAndSeasonalValues(lRchDataGroupW, "WATR", "Mean"))
        '    lString.Append(AnnualAndSeasonalValues(lRchDataGroupW, "WATR", "1Hi100"))
        '    lString.Append(AnnualAndSeasonalValues(lRchDataGroupW, "FLOW", "7Q10"))
        'End If

        'Dim lRchDataGroup As atcDataGroup = atcDataManager.DataSets.FindData("Location", "R:9")
        'If lRchDataGroup.Count > 0 Then
        '    lString.Append(AnnualValue(lRchDataGroup, "RO", "Mean"))
        '    lString.Append(AnnualValue(lRchDataGroup, "ROSED-TOT", "SumAnnual"))
        '    lString.Append(AnnualValue(lRchDataGroup, "ROSED-TOT", "SumAnnual", True))
        '    lString.Append(AnnualValue(lRchDataGroup, "ROSED-TOT", "Mean"))
        '    lString.Append(AnnualValue(lRchDataGroup, "ROSED-TOT", "Mean", True))
        '    lString.Append(AnnualValue(lRchDataGroup, "P-TOT-OUT", "SumAnnual"))
        '    lString.Append(AnnualValue(lRchDataGroup, "N-TOT-OUT", "SumAnnual"))
        'End If

        lString.AppendLine()

        Logger.Dbg("CAT Report Add - " & lString.ToString)
        AppendFileString(pCatSummaryFileName, lString.ToString)
    End Sub

    Private Function AnnualValue(ByVal aDataGroup As atcDataGroup, ByVal aCons As String, ByVal aTrans As String, Optional ByVal aMonthsList As String = "", Optional ByVal aSummer As Boolean = False) As String
        Dim lString As String = ""

        Try
            Dim lConsDataGroup As atcDataGroup = aDataGroup.FindData("Constituent", aCons)
            Logger.Dbg("     " & aCons & "MatchingDatasetCount " & lConsDataGroup.Count)
            If lConsDataGroup.Count > 0 Then
                Dim lTempDataSet As atcDataSet = lConsDataGroup.Item(0)
                Dim lValue As Double = lTempDataSet.Attributes.GetDefinedValue(aTrans).Value
                lString = vbTab & DecimalAlign(lValue)
                If aSummer Then
                    Dim lSeasons As New atcSeasons.atcSeasonsMonth
                    Dim lSeasonalAttributes As New atcDataAttributes
                    Dim lCalculatedAttributes As New atcDataAttributes
                    lSeasonalAttributes.SetValue(aTrans, 0) 'fluxes are summed from daily, monthly or annual to annual
                    lSeasons.SetSeasonalAttributes(lTempDataSet, lSeasonalAttributes, lCalculatedAttributes)
                    lValue = 0
                    Dim lMonthsStrArray() As String = aMonthsList.Split(",")
                    For Each ls As String In lMonthsStrArray
                        lValue += lCalculatedAttributes(Integer.Parse(ls) - 1).Value
                    Next
                    'For lIndex As Integer = 3 To 9
                    '    lValue += lCalculatedAttributes(lIndex).Value 'AMJJASO
                    'Next lIndex
                    If aTrans.StartsWith("Mean") Then
                        lValue = lValue / lMonthsStrArray.Length ' to get the Mean of the seasonal months involved
                    End If
                    lString &= vbTab & DecimalAlign(lValue)
                End If
            End If
        Catch ex As Exception
            Logger.Msg("AnnualValue Error: " & ex.ToString)
        End Try

        Return lString
    End Function

    Private Function AnnualAndSeasonalValues(ByVal aDataGroup As atcDataGroup, ByVal aCons As String, ByVal aTrans As String) As String
        Dim lString As String = ""
        Try
            Dim lConsDataGroup As atcDataGroup = aDataGroup.FindData("Constituent", aCons)
            Logger.Dbg("     " & aCons & "MatchingDatasetCount " & lConsDataGroup.Count)
            If lConsDataGroup.Count > 0 Then
                Dim lTempDataSet As atcDataSet = Nothing
                lTempDataSet = lConsDataGroup.Item(0)
                If lTempDataSet Is Nothing Then
                    Logger.Msg("AnnualAndSeasonalValues Error: The dataset: " & lConsDataGroup.Item(0).ToString & " is Empty")
                End If
                Dim lAttributeDefinedValue As atcDefinedValue = lTempDataSet.Attributes.GetDefinedValue(aTrans)
                Dim lValue As Double
                If lAttributeDefinedValue Is Nothing Then
                    lValue = Double.NaN
                Else
                    lValue = lAttributeDefinedValue.Value
                End If
                lString = vbTab & DecimalAlign(lValue)
                Dim lSeasons As New atcSeasons.atcSeasonsMonth
                Dim lSeasonalAttributes As New atcDataAttributes
                Dim lCalculatedAttributes As New atcDataAttributes
                lSeasonalAttributes.SetValue(aTrans, 0) 'fluxes are summed from daily, monthly or annual to annual
                lSeasons.SetSeasonalAttributes(lTempDataSet, lSeasonalAttributes, lCalculatedAttributes)
                Try
                    lValue = lCalculatedAttributes(0).Value + lCalculatedAttributes(1).Value + lCalculatedAttributes(11).Value   'DJF
                    If aTrans = "Mean" Then lValue = lValue / 3
                Catch ex As Exception
                    lValue = Double.NaN
                End Try
                lString &= vbTab & DecimalAlign(lValue)

                Try
                    lValue = lCalculatedAttributes(2).Value + lCalculatedAttributes(3).Value + lCalculatedAttributes(4).Value  'MAM
                    If aTrans = "Mean" Then lValue = lValue / 3
                Catch ex As Exception
                    lValue = Double.NaN
                End Try
                lString &= vbTab & DecimalAlign(lValue)
                Try
                    lValue = lCalculatedAttributes(5).Value + lCalculatedAttributes(6).Value + lCalculatedAttributes(7).Value  'JJA
                    If aTrans = "Mean" Then lValue = lValue / 3
                Catch ex As Exception
                    lValue = Double.NaN
                End Try
                lString &= vbTab & DecimalAlign(lValue)
                Try
                    lValue = lCalculatedAttributes(8).Value + lCalculatedAttributes(9).Value + lCalculatedAttributes(10).Value  'SON
                    If aTrans = "Mean" Then lValue = lValue / 3
                Catch ex As Exception
                    lValue = Double.NaN
                End Try
                lString &= vbTab & DecimalAlign(lValue)
            End If
        Catch ex As Exception
            Logger.Msg("AnnualAndSeasonalValues Error: " & ex.ToString)
        End Try

        Return lString

    End Function
End Module
