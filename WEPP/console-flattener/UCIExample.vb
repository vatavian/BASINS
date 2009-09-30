Imports atcData
Imports atcUtility
Imports atcWDM
Imports atcSeasons
Imports MapWinUtility
Imports System.String
Imports System.IO

Public Module UCIExample

    Public Sub ScriptMain()
        Try
            Dim lWorkingDir As String = "C:\FTB\wdm\"
            Dim lFlattenPath As String = lWorkingDir & "wdm.txt"
            ChDir(lWorkingDir)
            Logger.StartToFile("WeppWrapper.log")

            Dim lTimeseriesStatistics As New atcTimeseriesStatistics.atcTimeseriesStatistics
            For Each lOperation As atcDefinedValue In lTimeseriesStatistics.AvailableOperations
                atcDataAttributes.AddDefinition(lOperation.Definition)
            Next

            Dim lWDMDataSource As New atcWDM.atcDataSourceWDM
            If lWDMDataSource.Open("FBmet.wdm") Then    'use the WDM file name
                'successfully opened the wdm
                Logger.Dbg("Opened " & lWDMDataSource.Name)

                Dim lJulianDate As Double
                Dim lDate(5) As Integer



                'Set arrays of dates for begin/end of model
                'Important: Data must begin on hour "0" of first day and end on hour "24" of last day
                Dim lStrModelBegin() As Integer = {1999, 10, 1, 0, 0, 0}
                Dim lStrModelEnd() As Integer = {2006, 9, 30, 24, 0, 0}

                Dim lTableDelimiter As String = " "
                Dim lTableCellWidth As Integer = 6
                Dim lD2SMax As Integer = 10
                Dim lD2SFormat As String = "###.#"
                Dim lD2SSig As Integer = 5

                'make timeseries

                'The 
                Dim lWDMDataSetIndex As Integer = lWDMDataSource.DataSets.IndexFromKey(102)  'use the WDM Dsn
                Dim lWDMDataSet As atcTimeseries = lWDMDataSource.DataSets.ItemByIndex(lWDMDataSetIndex)

                'PREC
                Dim lTSPREC As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(107), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)
                Dim lSeasons As atcSeasonBase = New atcSeasonsMonth
                Dim lSeasonalAttributes As New atcDataAttributes
                lSeasons.SetSeasonalAttributes(lTSPREC, lSeasonalAttributes, lTSPREC.Attributes)
                DumpAttributes(lTSPREC)

                'ATEM
                Dim lTSATEM As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(13), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)

                'DEWP
                Dim lTSDEWP As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(17), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)
                'WIND
                Dim lTSWIND As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(14), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)
                'SOLR
                Dim lTSSOLR As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(15), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)
                'lSeasonalAttributes.Clear()
                'lSeasonalAttributes.SetValue("Mean", 0)
                'lSeasons.SetSeasonalAttributes(lTSSOLR, lSeasonalAttributes, lTSSOLR.Attributes)
                'DumpAttributes(lTSSOLR)

                'PEVT
                Dim lTSPEVT As atcTimeseries = modTimeseriesMath.SubsetByDate(lWDMDataSource.DataSets.ItemByKey(16), Date2J(lStrModelBegin), Date2J(lStrModelEnd), lWDMDataSource)


                ' Create a file to write to.
                Dim lSw As System.IO.StreamWriter = System.IO.File.CreateText(lFlattenPath)

                'sring that grows recursively with each new hour. Gets written at the end of the stat computations.
                Dim lWriteBreakPointLines As String = ""

                Dim lWritePreamble(15) As String
                For i = 0 To 15
                    lWritePreamble(i) = ""
                Next

                lWritePreamble(1) = " 0.0"
                lWritePreamble(2) = "1".PadLeft(lTableCellWidth, lTableDelimiter) & "1".PadLeft(lTableCellWidth, lTableDelimiter) & "1".PadLeft(lTableCellWidth, lTableDelimiter)
                lWritePreamble(3) = " FT. Benning - " & lTSPREC.Attributes.GetDefinedValue("Parent Timeseries").Value.ToString
                lWritePreamble(4) = " Latitude Longitude Elevation (m) Obs. Years   Beginning year  Years simulated"
                lWritePreamble(5) = "32.353".PadLeft(10, lTableDelimiter) & " " & "-84.745".PadLeft(10, lTableDelimiter) & " " & "150".PadLeft(10, lTableDelimiter) & " " & lStrModelEnd(0) - lStrModelBegin(0).ToString.PadLeft(lTableCellWidth, lTableDelimiter) & " " & lStrModelBegin(0).ToString.PadLeft(lTableCellWidth, lTableDelimiter) & " " & lStrModelEnd(0) - lStrModelBegin(0).ToString.PadLeft(lTableCellWidth, lTableDelimiter)
                lWritePreamble(5) = "32.353".PadLeft(10, lTableDelimiter) & " " & "-84.745".PadLeft(10, lTableDelimiter) & " " & "150".PadLeft(10, lTableDelimiter) & " " & lStrModelEnd(0) - lStrModelBegin(0).ToString.PadLeft(lTableCellWidth, lTableDelimiter) & " " & 1 & " " & lStrModelEnd(0) - lStrModelBegin(0).ToString.PadLeft(lTableCellWidth, lTableDelimiter)
                lWritePreamble(6) = " Observed monthly ave max temperature (C)"
                lWritePreamble(7) = "##### Calculated #####"
                lWritePreamble(8) = " Observed monthly ave min temperature (C)"
                lWritePreamble(9) = "##### Calculated #####"
                lWritePreamble(10) = " Observed monthly ave solar radiation (Langleys/day)"
                lWritePreamble(11) = "##### Calculated #####"
                lWritePreamble(12) = " Observed monthly ave precipitation (mm)"
                lWritePreamble(13) = "##### Calculated #####"
                lWritePreamble(14) = " da mo  year  bpts  tmax  tmin   rad  w-vl w-dir  tdew"
                lWritePreamble(15) = "               (#)   (C)   (C) (1/d) (m/s) (Deg)   (C)"

                'TempDayStatResults indexes:
                '0: min temperature
                '1: max temperature
                '2: solar radiation
                '3: wind velocity
                '4: wind direction
                '5: dew point temp
                Dim lTempDayStatResults(5) As Double

                'Monthly statistics need to be computed for four things. We will create four collections of all monthly data, then compute stats after the master loop has run.
                Dim lMonthlyStats(3, 11) As Collection
                'initialize the collections
                For i = 0 To 3
                    For j = 0 To 11
                        lMonthlyStats(i, j) = New Collection
                    Next
                Next

                'Make array of collections to calculate stats at the end of every month.
                Dim lTempCurrentMonthStats(3) As Collection
                For i = 0 To 3
                    lTempCurrentMonthStats(i) = New Collection
                Next

                'variables for statistics
                Dim lTempRecord As Double
                Dim lDayOutString As String = ""
                Dim lMonthOutString As String = ""
                Dim lCurrentMonth As Integer
                Dim lHourIndexEnd As Integer = lTSPREC.Values.Length
                Dim lTempDayPrecipAccumulate As Double
                Dim lCurrentMonthStatCollection As Collection

                'Loop through every record (hour) in the timeseries.
                '===================================================
                For i = 0 To lHourIndexEnd - 25

                    'Set dates for very first iteration
                    If i = 0 Then
                        lJulianDate = lTSPREC.Dates.Values(i)  'date associated with the first value 
                        J2Date(lJulianDate, lDate)
                    End If

                    'Its midnight in the loop "i", so let us loop through 24 hours then calculate the stats.
                    If lDate(3) = 0 AndAlso i <> lHourIndexEnd - 1 Then

                        'set all averaged values equal to zero, wont affect mean sum because denominator used to calculate mean is set at 24.
                        lTempDayStatResults(2) = 0
                        lTempDayStatResults(3) = 0
                        lTempDayStatResults(4) = 0
                        lTempDayStatResults(5) = 0
                        lTempDayPrecipAccumulate = 0

                        For j = 1 To 24
                            'ATEM: min and max temperature for the current day
                            lTempRecord = lTSATEM.Values(i + j)
                            'convert from degrees F to C
                            lTempRecord = (lTempRecord - 32) * 5 / 9
                            If j = 1 Then
                                lTempDayStatResults(0) = lTempRecord
                                lTempDayStatResults(1) = lTempRecord
                            Else
                                If lTempRecord < lTempDayStatResults(0) Then lTempDayStatResults(0) = lTempRecord
                                If lTempRecord > lTempDayStatResults(1) Then lTempDayStatResults(1) = lTempRecord
                            End If

                            'SOLR: daily radiation
                            lTempRecord = lTSSOLR.Values(i + j)
                            lTempDayStatResults(2) = lTempDayStatResults(2) + lTempRecord

                            'WIND: wind velocity
                            lTempRecord = lTSWIND.Values(i + j)
                            lTempDayStatResults(3) = lTempDayStatResults(3) + lTempRecord

                            'wind direction (set to zero for now)
                            'DNE -- lWDMDataSetTemp = lWDMDataSource.DataSets.ItemByIndex(xx)
                            lTempDayStatResults(4) = 0

                            'DEWP: dewpoint temp
                            lTempRecord = lTSDEWP.Values(i + j)
                            lTempDayStatResults(5) = lTempDayStatResults(5) + lTempRecord

                            'write breakpoint line for this j hour
                            lTempRecord = lTSPREC.Values(i + j)
                            'Convert inches to millimeters
                            lTempRecord *= 25.4
                            lTempDayPrecipAccumulate += lTempRecord
                            lDayOutString = lDayOutString & " " & j.ToString.PadLeft(2, " ") & " " & WEPPformatMoreDetail(lTempDayPrecipAccumulate)

                            If j = 24 Then
                                'average things
                                lTempDayStatResults(2) = lTempDayStatResults(2) / 24
                                lTempDayStatResults(3) = lTempDayStatResults(3) / 24
                                lTempDayStatResults(5) = lTempDayStatResults(5) / 24
                            Else
                                lDayOutString &= vbCrLf
                            End If
                        Next

                        lDayOutString = " " & lDate(2).ToString.PadLeft(2, lTableDelimiter) _
                        & " " & lDate(1).ToString.PadLeft(2, lTableDelimiter) _
                        & " " & lDate(0) - lStrModelBegin(0) + 1 _
                        & "24".PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(1)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(0)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(2)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(3)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(4)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & WEPPformat(lTempDayStatResults(5)).PadLeft(lTableCellWidth, lTableDelimiter) _
                        & vbCrLf _
                        & lDayOutString

                        'Dont append a linefeed if this is the last hour. This prevents blank lines (which is bad, I think).
                        If i = 0 Then
                            lWriteBreakPointLines = lWriteBreakPointLines & lDayOutString
                        Else
                            lWriteBreakPointLines = lWriteBreakPointLines & vbCrLf & lDayOutString
                        End If

                        'reset the string to nothing
                        lDayOutString = ""

                        'add todays stats to the current month collections
                        lTempCurrentMonthStats(0).Add(lTempDayStatResults(1))
                        lTempCurrentMonthStats(1).Add(lTempDayStatResults(0))
                        lTempCurrentMonthStats(2).Add(lTempDayStatResults(2))
                        lTempCurrentMonthStats(3).Add(lTempDayPrecipAccumulate)

                    End If

                    'set the current month as an integer
                    lCurrentMonth = lDate(1)

                    'change lDate for the next hour and check for a month change
                    J2Date(lTSPREC.Dates.Values(i + 1), lDate)
                    If lDate(1) <> lCurrentMonth Or i = lHourIndexEnd - 25 Then
                        'Average all current month stats

                        'ATEM Max
                        lMonthlyStats(0, lCurrentMonth - 1).Add(MeanCollectionOfDoubles(lTempCurrentMonthStats(0)))

                        'ATEM Min
                        lMonthlyStats(1, lCurrentMonth - 1).Add(MeanCollectionOfDoubles(lTempCurrentMonthStats(1)))

                        'SOLR
                        lMonthlyStats(2, lCurrentMonth - 1).Add(MeanCollectionOfDoubles(lTempCurrentMonthStats(2)))

                        'PREC
                        lMonthlyStats(3, lCurrentMonth - 1).Add(MeanCollectionOfDoubles(lTempCurrentMonthStats(3)))

                    End If


                Next

                'Prep lines for the text file write

                'average the (months)averages of the (month)average of the averages(day) for everything.
                For i = 0 To 3
                    For j = 0 To 11
                        lCurrentMonthStatCollection = lMonthlyStats(i, j)
                        If j = 0 Then
                            lMonthOutString = lMonthOutString & WEPPformat(MeanCollectionOfDoubles(lCurrentMonthStatCollection)).PadLeft(lTableCellWidth, lTableDelimiter)
                        Else
                            lMonthOutString = lMonthOutString & lTableDelimiter & WEPPformat(MeanCollectionOfDoubles(lCurrentMonthStatCollection)).PadLeft(lTableCellWidth, lTableDelimiter)
                        End If
                    Next

                    'write strings to respective lines
                    Select Case i
                        Case 0
                            lWritePreamble(7) = lMonthOutString
                        Case 1
                            lWritePreamble(9) = lMonthOutString
                        Case 2
                            lWritePreamble(11) = lMonthOutString
                        Case 3
                            lWritePreamble(13) = lMonthOutString
                    End Select

                    lMonthOutString = ""
                Next

                For k = 1 To 15
                    lSw.WriteLine(lWritePreamble(k))
                Next

                'WRITE LINES 16, 17 and subsequent lines.
                lSw.WriteLine(lWriteBreakPointLines)

                'clean up file stuff in memory.
                lSw.Flush()
                lSw.Close()
                lWDMDataSet = Nothing
                lWDMDataSource = Nothing
            End If

            'Special command for author
            File.Copy(lFlattenPath, "Z:\Documents\filecabinet\employment\aquaterra\active.projects\SERDP\Roads\WEPP\wepp.run\p3.cli", True)
        Catch ex As Exception
            'uh oh
            Logger.Dbg("Badness message: " & ex.ToString)
        End Try

    End Sub
    Private Function MeanCollectionOfDoubles(ByRef aCollection As Collection) As Double
        Dim lTempAccumulator As Double = 0

        If aCollection.Count > 0 Then
            For i = 1 To aCollection.Count
                If IsNumeric(aCollection.Item(i)) Then lTempAccumulator += aCollection.Item(i)
            Next
            lTempAccumulator /= aCollection.Count
            Return lTempAccumulator
        Else
            Return -9999999
        End If

    End Function

    Private Sub DumpAttributes(ByVal aTimeSeries As atcTimeseries)
        Dim lAttributeValues As SortedList = aTimeSeries.Attributes.ValuesSortedByName
        For lAttributeIndex As Integer = 0 To lAttributeValues.Count - 1
            Dim lAttributeName As String = lAttributeValues.GetKey(lAttributeIndex)
            Dim lAttributeValue As String = aTimeSeries.Attributes.GetFormattedValue(lAttributeName)
            Logger.Dbg(lAttributeName & " = " & lAttributeValue)
        Next
    End Sub
    Function WEPPformat(ByVal aNumber As Double) As String
        Dim lStr As String = DoubleToString(aNumber, 10, , "0", , 3)
        If Not lStr.Contains(".") Then lStr &= "."
        Return lStr
    End Function

    Function WEPPformatMoreDetail(ByVal aNumber As Double) As String
        Dim lStr As String = DoubleToString(aNumber, 10, , "#.####", , 6)
        If Not lStr.Contains(".") Then lStr &= "."
        Return lStr
    End Function

End Module