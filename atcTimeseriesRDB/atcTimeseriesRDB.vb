Option Strict Off
Option Explicit On

Imports atcData
Imports atcUtility
Imports MapWinUtility
Imports System.Collections
Imports System.IO

''' <summary>
''' Reads USGS rdb files containing daily values
''' </summary>
''' <remarks>
''' Would need to change pJulianInterval, ts and tu for non-daily values
''' Does not read provisional values into timeseries
''' </remarks>

Public Class atcTimeseriesRDB
    Inherits atcDataSource

    Private Shared pFileFilter As String = "USGS RDB Files (*.rdb, *.txt)|*.rdb;*.txt|All Files (*.*)|(*.*)"
    Private pErrorDescription As String
    Private pJulianInterval As Double = 1 'Add one day for daily values to record date at end of interval

    Public Overrides ReadOnly Property Description() As String
        Get
            Return "USGS RDB File"
        End Get
    End Property

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "Timeseries::USGS RDB"
        End Get
    End Property

    Public Overrides ReadOnly Property Category() As String
        Get
            Return "File"
        End Get
    End Property

    Public Overrides ReadOnly Property CanOpen() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides Function Open(ByVal aFileName As String, _
                          Optional ByVal aAttributes As atcData.atcDataAttributes = Nothing) As Boolean
        If aFileName Is Nothing OrElse aFileName.Length = 0 OrElse Not FileExists(aFileName) Then
            aFileName = FindFile("Select " & Name & " file to open", , , pFileFilter, True, , 1)
        End If

        If Not FileExists(aFileName) Then
            pErrorDescription = "File '" & aFileName & "' not found"
        Else
            Me.Specification = aFileName

            Try
                Dim lCurLine As String
                Dim lAttrName As String
                Dim lAttrValue As String
                Dim lAttributes As New atcDataAttributes

                Dim lInputStream As New FileStream(aFileName, FileMode.Open, FileAccess.Read)
                Dim lInputBuffer As New BufferedStream(lInputStream)
                Dim lInputReader As New BinaryReader(lInputBuffer)

                Dim lURL As String = NextLine(lInputReader)
                Dim lWQData As Boolean = False
                Dim lParmCodeIndex As Integer = 10
                If lURL.IndexOf("qwdata") > -1 Then 'TODO: need better way - only true with BASINS download
                    lWQData = True
                End If

                While lInputReader.PeekChar = Asc("#")
                    lCurLine = NextLine(lInputReader)
                    If lCurLine.Length = 1 Then Exit While
                    If lCurLine.Length > 50 Then
                        lAttrName = lCurLine.Substring(2, 12).Trim
                        lAttrValue = lCurLine.Substring(50).Trim
                        Select Case lAttrName 'translate NWIS attributes to WDM/BASINS names
                            Case "agency_cd" : lAttributes.SetValue("AGENCY", lAttrValue)
                            Case "station_nm" : lAttributes.SetValue("Description", lAttrValue)
                            Case "state_cd" : lAttributes.SetValue("STFIPS", lAttrValue)
                            Case "county_cd" : lAttributes.SetValue("CNTYFIPS", lAttrValue)
                            Case "huc_cd" : lAttributes.SetValue("HUCODE", lAttrValue)
                            Case "dec_lat_va" : lAttributes.SetValue("LATDEG", CDbl(lAttrValue))
                            Case "dec_long_va" : lAttributes.SetValue("LNGDEG", -Math.Abs(CDbl(lAttrValue)))
                            Case "alt_va" : lAttributes.SetValue("ELEV", lAttrValue)
                            Case "drain_area_va" : lAttributes.SetValue("DAREA", lAttrValue)
                        End Select
                    End If
                End While

                If lWQData Then
                Else
                    ProcessDailyValues(lInputReader, lAttributes)
                End If

                Return True
            Catch e As Exception
                Logger.Dbg("Exception reading '" & aFileName & "': " & e.Message)
                Return False
            End Try
        End If
    End Function

    Sub ProcessDailyValues(ByVal aInputReader As BinaryReader, ByVal aAttributes As atcDataAttributes)
        Dim lCurLine As String
        Dim lParmCode As String
        Dim lStatisticCode As String
        Dim lConstituentDescriptions As New atcCollection

        While aInputReader.PeekChar = Asc("#")
            lCurLine = NextLine(aInputReader)
            If lCurLine.Length > 50 Then
                'Remember extended column labels
                lParmCode = lCurLine.Substring(10, 5)
                lStatisticCode = lCurLine.Substring(20, 5)
                If IsNumeric(lParmCode) AndAlso IsNumeric(lStatisticCode) Then
                    lConstituentDescriptions.Add(lCurLine.Substring(5, 2) & "_" & lParmCode & "_" & lStatisticCode, lCurLine.Substring(30).Trim)
                End If
            End If
        End While
        Dim lRawDataSets As New atcDataGroup
        Dim lTSIndex As Integer = 0
        Dim lNCons As Integer = 0
        Dim lData As atcTimeseries = Nothing
        Dim lDateArr(6) As Integer
        lDateArr(3) = 24 'No hours in this file format, put measurement at end of day
        lDateArr(4) = 0 'No minutes in this file format
        lDateArr(5) = 0 'No seconds in this file format

        Dim lTable As New atcTableDelimited
        With lTable
            Dim lDate As Double
            Dim lLocation As String
            Dim lField As Integer
            Dim lConstituentDescription As String
            Dim lDateField As Integer = -1
            Dim lLocationField As Integer = -1
            Dim lValueFields As New ArrayList
            Dim lValueConstituentDescriptions As New atcCollection
            Dim lCurValue As String
            .Delimiter = vbTab
            .OpenStream(aInputReader.BaseStream)

            For lField = 1 To .NumFields
                Select Case .FieldName(lField)
                    Case "agency_cd"
                    Case "site_no" : lLocationField = lField
                    Case "datetime" : lDateField = lField
                    Case Else
                        If .FieldName(lField).EndsWith("_cd") Then
                            'skip code fields for now
                        Else
                            Dim lConstituentIndex As Integer = _
                                lConstituentDescriptions.IndexFromKey(.FieldName(lField))
                            If lConstituentIndex >= 0 Then
                                lValueFields.Add(lField)
                                lValueConstituentDescriptions.Add(lField, lConstituentDescriptions.ItemByIndex(lConstituentIndex))
                            Else
                                Logger.Dbg("Found column in RDB not contained in header: " & .FieldName(lField) & " (#" & lField & ")")
                            End If
                        End If
                End Select
            Next

            While lTable.CurrentRecord < lTable.NumRecords
                lTable.MoveNext()
                lDate = Date.Parse(.Value(lDateField)).ToOADate() + pJulianInterval 'add one interval to put date at end of interval

                If lDate <> 0 Then
                    lLocation = .Value(lLocationField)
                    For Each lField In lValueFields
                        lCurValue = .Value(lField).Trim
                        If lCurValue.Length = 0 Then
                            'Skip blank values
                            'If next field is code for this field, then make sure its code starts with "A" for Approved
                        ElseIf .FieldName(lField + 1) <> .FieldName(lField) & "_cd" OrElse .Value(lField + 1).StartsWith("A") Then
                            lConstituentDescription = lValueConstituentDescriptions.ItemByKey(lField)

                            Dim lDataKey As String = lLocation & ":" & lConstituentDescription
                            If Not lData Is Nothing AndAlso lData.Attributes.GetValue("DataKey") = lDataKey Then
                                'Already have correct dataset to append to
                            ElseIf lRawDataSets.Keys.Contains(lDataKey) Then
                                lData = lRawDataSets.ItemByKey(lDataKey)
                            Else
                                lData = New atcTimeseries(Me)
                                lData.Dates = New atcTimeseries(Me)
                                lData.Attributes.ChangeTo(aAttributes)
                                lData.numValues = lTable.NumRecords - 1

                                Select Case .FieldName(lField).Substring(3, 5)
                                    Case "00060" : lData.Attributes.SetValue("Constituent", "FLOW")
                                    Case Else : lData.Attributes.SetValue("Constituent", .FieldName(lField).Substring(3, 5))
                                End Select

                                Select Case .FieldName(lField).Substring(9, 5)
                                    Case "00001" : lData.Attributes.SetValue("TSFORM", "5") 'Maximum
                                    Case "00002" : lData.Attributes.SetValue("TSFORM", "4") 'Minimum
                                    Case "00003" : lData.Attributes.SetValue("TSFORM", "1") 'Mean
                                End Select
                                lData.Attributes.SetValue("Count", 0)
                                lData.Attributes.SetValue("Scenario", "OBSERVED")
                                lData.Attributes.SetValue("Location", lLocation)
                                lData.Attributes.SetValue("ConstituentDescription", lConstituentDescription)
                                lData.Attributes.SetValue("DataKey", lDataKey)

                                lRawDataSets.Add(lDataKey, lData)
                                lData.Dates.Value(0) = lDate - pJulianInterval
                            End If
                            lTSIndex = lData.Attributes.GetValue("Count") + 1
                            lData.Value(lTSIndex) = lCurValue
                            lData.Dates.Value(lTSIndex) = lDate
                            lData.Attributes.SetValue("Count", lTSIndex)
                        End If
                    Next
                End If
            End While
        End With
        '
        Dim lMissingVal As Double = -999
        For Each lData In lRawDataSets
            lTSIndex = lData.Attributes.GetValue("Count")
            If lData.numValues <> lTSIndex Then
                lData.numValues = lTSIndex
            End If
            lData.Attributes.RemoveByKey("DataKey")
            DataSets.Add(FillValues(lData, atcTimeUnit.TUDay, 1, atcUtility.GetNaN, lMissingVal, , Me))
        Next
        lRawDataSets.Clear()
    End Sub

    'Private Function GetData(ByVal aSites As ArrayList, _
    '                Optional ByVal cache_dir As String = "", _
    '                Optional ByVal base_url As String = "http://waterdata.usgs.gov/nwis/dv?cb_00060=on", _
    '                Optional ByVal begin_date As String = "1800-01-01", _
    '                Optional ByVal end_date As String = "2100-01-01", _
    '                Optional ByVal suffix As String = "_dv.txt") As Boolean

    '    Dim pLabel As String = "USGS Daily Streamflow"
    '    Dim save_filename As String
    '    Dim myDownloadFiles As New ArrayList 'of file names
    '    Dim url As String
    '    Dim iSite As Integer
    '    Dim FirstFile As Boolean
    '    Dim FilesNotCreated As String = ""
    '    Dim nFilesNotCreated As Integer
    '    Dim FileNumber As Integer

    '    Dim findPos As Integer
    '    Dim msg As String

    '    Try

    '        Logger.Dbg("  clsUsgsDaily GetData entry")

    '        'http://waterdata.usgs.gov/nwis/dv?cb_00060=on&format=rdb&begin_date=1800-01-01&end_date=2100-01-01&site_no=01591000&referred_module=sw
    '        'cache_dir = pManager.CurrentStatusGetString("cache_dir") & pClassName & "\"
    '        'project_dir = pManager.CurrentStatusGetString("project_dir")
    '        '  SHPfilename = project_dir & pManager.CurrentStatusGetString("USGSdailySHPfile", "gage.shp")
    '        'suffix = pManager.CurrentStatusGetString("USGSdailySaveSuffix", "_dv.txt")


    '        GetData = True

    '        'If Len(WDMfilename) > 0 Then
    '        '    myDownloadFiles = New Collection
    '        'Else 'Save downloaded RDB files in folder inside project_dir if we are not adding to WDM
    '        '    project_dir = project_dir & "USGSflow\"
    '        '    Logger.Dbg("Saving RDB files in " & project_dir)
    '        'End If
    '        MkDirPath(cache_dir)

    '        FirstFile = True
    '        iSite = 0
    '        For Each lSite As String In aSites 'For iSite = 1 To nSites
    '            iSite = iSite + 1
    '            url = base_url & "&format=rdb" & _
    '                        "&begin_date=" & begin_date & _
    '                        "&end_date=" & end_date & _
    '                        "&site_no=" & lSite

    '            'siteAttributes = "# " & url & vbCrLf
    '            'For iAttr = 0 To lSite.NumAttributes - 1
    '            '    siteAttributes = siteAttributes & "# " & lSite.GetAttributeName(iAttr) _
    '            '                            & Space(48 - Len(lSite.GetAttributeName(iAttr))) _
    '            '                                           & lSite.GetAttributeValue(iAttr) & vbCrLf
    '            'Next
    '            save_filename = cache_dir & lSite & suffix

    '            'If Not pManager.Download(url, save_filename, FirstFile, "Downloading " & pLabel & " (" & iSite & " of " & lstSites.Count & ")", siteAttributes) Then
    '            '    nodStatus.AddAttribute("message", "User Cancelled")

    '            '    Exit Function '!!!!!!!!!!!!!!!!!!!

    '            'End If
    '            msg = WholeFileString(save_filename)

    '            findPos = InStr(msg, "<html")
    '            If findPos > 0 Then 'Got an error message or web page, not the data we expected
    '                'msg = Mid(pManager.ResultString, findPos)
    '                Kill(save_filename)
    '                nFilesNotCreated = nFilesNotCreated + 1
    '                FilesNotCreated &= "   " & FilenameNoPath(save_filename)
    '                If InStr(msg, "No data") > 0 Then
    '                    FilesNotCreated &= " (no data)"
    '                ElseIf InStr(msg, "No site") > 0 Then
    '                    FilesNotCreated &= " (no site)"
    '                Else
    '                    FilesNotCreated &= " (error)"
    '                End If
    '                FilesNotCreated = FilesNotCreated & vbCrLf
    '            Else
    '                'Replace LF with CR/LF
    '                msg = ReplaceString(msg, vbLf, vbCrLf)
    '                'Above replacement may have added some unwanted CR
    '                msg = ReplaceString(msg, vbCr & vbCr, vbCr)
    '                SaveFileString(msg, save_filename)
    '                'If Len(WDMfilename) > 0 Then
    '                myDownloadFiles.Add(save_filename)
    '                'Else
    '                'Logger.Dbg("Copying downloaded file to " & project_dir & FilenameNoPath(save_filename))
    '                'FileCopy(save_filename, project_dir & FilenameNoPath(save_filename))
    '                'End If
    '            End If
    '            FirstFile = False
    '        Next
    '        'If Len(WDMfilename) > 0 Then
    '        '    Logger.Dbg("Saving downloaded data to " & WDMfilename)
    '        Try
    '            '    ConvertUsgsDv2Wdm(WDMfilename, myDownloadFiles)
    '        Catch
    '            Logger.Msg("Error writing WDM file" & vbCr & Err.Description & vbCr & "Libraries may need to be installed for saving WDM files", _
    '                                   pLabel & " GetData")
    '        End Try
    '        'End If
    '        'If nFilesNotCreated > 0 Then
    '        save_filename = cache_dir & "USGSflowNoData.txt"
    '        While Len(Dir(save_filename)) > 0
    '            FileNumber = FileNumber + 1
    '            save_filename = cache_dir & "USGSflowNoData(" & FileNumber & ").txt"
    '        End While
    '        If nFilesNotCreated > 10 Then
    '            findPos = 1
    '            For FileNumber = 1 To 10
    '                findPos = FilesNotCreated.IndexOf(CStr(vbCr), findPos + 1)
    '            Next
    '            msg = Left(FilesNotCreated, findPos) & " (and " & (nFilesNotCreated - 10) & " more)"
    '        Else
    '            msg = FilesNotCreated
    '        End If

    '        If Logger.Msg("Did not find data for " & nFilesNotCreated & " of " & aSites.Count & " stations: " & vbCr & vbCr _
    '                 & msg & vbCr _
    '                 & "Save this list to " & save_filename & "?", _
    '                 pLabel & " - Some data not found", "+&Yes", "-&No") = 1 Then
    '            SaveFileString(save_filename, FilesNotCreated)
    '        End If

    '        Logger.Dbg("  clsUsgsDaily GetData exit")
    '        Return True

    '    Catch ex As Exception
    '        Logger.Msg("Error '" & ex.Message & "'", pLabel & " GetData")
    '    End Try
    'End Function

End Class

