﻿Imports System.Collections
Imports atcUtility
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports atcData

'''<summary>
'''This is a test class for atcDataGroupTest and is intended
'''to contain all atcDataGroupTest Unit Tests
'''</summary>
<TestClass()> _
Public Class atcDataGroupTest
    Private testContextInstance As TestContext

    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Additional test attributes"
    '
    'You can use the following additional attributes as you write your tests:
    '
    'Use ClassInitialize to run code before running the first test in the class
    '<ClassInitialize()>  _
    'Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    'End Sub
    '
    'Use ClassCleanup to run code after all tests in a class have run
    '<ClassCleanup()>  _
    'Public Shared Sub MyClassCleanup()
    'End Sub
    '
    'Use TestInitialize to run code before running each test
    '<TestInitialize()>  _
    'Public Sub MyTestInitialize()
    'End Sub
    '
    'Use TestCleanup to run code after each test has run
    '<TestCleanup()>  _
    'Public Sub MyTestCleanup()
    'End Sub
    '
#End Region

    '''<summary>Test atcDataGroup Constructor</summary>
    <TestMethod()> Public Sub atcDataGroupConstructorTest()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
    End Sub

    '''<summary>Test atcDataGroup Constructor</summary>
    <TestMethod()> Public Sub atcDataGroupConstructorTest1()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lDataGroup As atcDataGroup = New atcDataGroup(lDataSet)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(1, lDataGroup.Count)
    End Sub

    '''<summary>Test atcDataGroup Constructor</summary>
    <TestMethod()> Public Sub atcDataGroupConstructorTest2()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lDataSets() As atcDataSet = {lDataSet, lDataSet.Clone}
        Dim lDataGroup As atcDataGroup = New atcDataGroup(lDataSets)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
    End Sub

    '''<summary>Test Add</summary>
    <TestMethod()> Public Sub AddTest()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Dim lKey As String = "Key1"
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lResult As Integer = lDataGroup.Add(lKey, lDataSet)
        Assert.AreEqual(0, lResult)
    End Sub

    '''<summary>Test Add</summary>
    <TestMethod()> Public Sub AddTest1()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lResult As Integer = lDataGroup.Add(lDataSet)
        Assert.AreEqual(0, lResult)
    End Sub

    '''<summary>Test Add</summary>
    <TestMethod()> Public Sub AddTest2()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lDataSetCollection As New atcCollection
        lDataSetCollection.Add(lDataSet.Clone)
        lDataSetCollection.Add(lDataSet)
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.Add(lDataSetCollection)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
    End Sub

    '''<summary>Test AddRange</summary>
    <TestMethod()> Public Sub AddRangeTest()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        lDataGroup.AddRange(lAddThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
    End Sub

    '''<summary>Test AddRange</summary>
    <TestMethod()> Public Sub AddRangeTest1()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        Dim lAddKeys As IEnumerable = {"Key1", "Key2"}
        lDataGroup.AddRange(lAddKeys, lAddThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
    End Sub

    '''<summary>Test ChangeTo</summary>
    <TestMethod()> Public Sub ChangeToTest()
        Dim lNewDataGroup As atcDataGroup = New atcDataGroup()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        lNewDataGroup.AddRange(lAddThese)
        Assert.IsInstanceOfType(lNewDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lNewDataGroup.Count)
        Dim lDataGroup As atcDataGroup = New atcDataGroup
        lDataGroup.Add(lDataSet.Clone)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(1, lDataGroup.Count)
        lDataGroup.ChangeTo(lNewDataGroup)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
    End Sub

    '''<summary>Test Clear</summary>
    <TestMethod()> Public Sub ClearTest()
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        lDataGroup.AddRange(lAddThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
        lDataGroup.Clear()
        Assert.AreEqual(0, lDataGroup.Count)
    End Sub

    '''<summary>Test Clone</summary>
    <TestMethod()> Public Sub CloneTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
        Dim lNewDataGroup As atcDataGroup = lDataGroup.Clone()
        Assert.IsInstanceOfType(lNewDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lNewDataGroup.Count)
        CollectionAssert.AreEqual(lDataGroup, lNewDataGroup)
    End Sub

    '''<summary>Test CommonAttributeValue</summary>
    <TestMethod()> Public Sub CommonAttributeValueTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataSet.Attributes.Add("ID", 1)
        Dim lValue As String = "TestDataSet"
        lDataSet.Attributes.Add("Name", lValue)
        Dim lDataSet2 As atcDataSet = lDataSet.Clone
        lDataSet2.Attributes.Add("ID", 2)
        Dim lAddThese As IEnumerable = {lDataSet2, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Assert.AreEqual(lValue, lDataGroup.CommonAttributeValue("Name", "Missing"))
        Assert.AreEqual("Missing", lDataGroup.CommonAttributeValue("ID", "Missing"))
    End Sub

    '''<summary>Test Dispose</summary>
    <TestMethod()> Public Sub DisposeTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lDataGroup As atcDataGroup = New atcDataGroup(lDataSet)
        lDataGroup.Dispose()
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(0, lDataGroup.Count)
    End Sub

    '''<summary>Test FindData</summary>
    <TestMethod()> Public Sub FindDataTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataSet.Attributes.Add("ID", 1)
        Dim lValue As String = "TestDataSet"
        lDataSet.Attributes.Add("Name", lValue)
        Dim lDataSet2 As atcDataSet = lDataSet.Clone
        lDataSet2.Attributes.Add("ID", 2)
        Dim lAddThese As IEnumerable = {lDataSet2, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Assert.AreEqual(2, lDataGroup.FindData("Name", "TestDataSet").Count)
        Assert.AreEqual(1, lDataGroup.FindData("ID", 2).Count)
        Assert.AreEqual(0, lDataGroup.FindData("ID", 3).Count)
    End Sub

    '''<summary>Test FindData</summary>
    <TestMethod()> Public Sub FindDataTest1()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataSet.Attributes.Add("ID", 1)
        Dim lValue As String = "TestDataSet"
        lDataSet.Attributes.Add("Name", lValue)
        Dim lDataSet2 As atcDataSet = lDataSet.Clone
        lDataSet2.Attributes.Add("ID", 2)
        Dim lAddThese As IEnumerable = {lDataSet2, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Assert.AreEqual(1, lDataGroup.FindData("Name", "TestDataSet", 1).Count)
        Assert.AreEqual(0, lDataGroup.FindData("X", "TestDataSet", 1).Count)
    End Sub

    '''<summary>Test IndexOfSerial</summary>
    <TestMethod()> Public Sub IndexOfSerialTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataSet.Attributes.Add("ID", 1)
        Dim lValue As String = "TestDataSet"
        lDataSet.Attributes.Add("Name", lValue)
        Dim lDataSet2 As atcDataSet = lDataSet.Clone
        lDataSet2.Attributes.Add("ID", 2)
        Dim lAddThese As IEnumerable = {lDataSet2, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Assert.AreEqual(-1, lDataGroup.IndexOfSerial(0))
        Assert.AreEqual(1, lDataGroup.IndexOfSerial(1))
        Assert.AreEqual(0, lDataGroup.IndexOfSerial(2))
        Assert.AreEqual(-1, lDataGroup.IndexOfSerial(3))
    End Sub

    '''<summary>Test Insert</summary>
    <TestMethod()> Public Sub InsertTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataSet.Attributes.Add("ID", 1)
        Dim lValue As String = "TestDataSet"
        lDataSet.Attributes.Add("Name", lValue)
        Dim lDataSet2 As atcDataSet = lDataSet.Clone
        lDataSet2.Attributes.Add("ID", 2)
        Dim lAddThese As IEnumerable = {lDataSet2, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup()
        lDataGroup.AddRange(lAddThese)
        Dim lIndex As Integer = 0 ' TODO: Initialize to an appropriate value
        Dim lDataSet3 As atcDataSet = lDataSet.Clone
        lDataSet3.Attributes.Add("ID", 3)
        lDataGroup.Insert(0, lDataSet3)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(3, lDataGroup.Count)
        Assert.AreEqual(3, lDataGroup.ItemByIndex(0).Attributes.GetDefinedValue("ID").Value)
        Assert.AreEqual(2, lDataGroup.ItemByIndex(1).Attributes.GetDefinedValue("ID").Value)
    End Sub

    Private WithEvents lDataGroupWithEvents As atcDataGroup
  
    '''<summary>Test RaiseAddedOne</summary>
    <TestMethod(), DeploymentItem("atcData.dll")> _
    Public Sub RaiseAddedOneTest()
        lDataGroupWithEvents = New atcDataGroup
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataGroupWithEvents.Add(lDataSet)
       End Sub
    Private Sub AddedEvent(ByVal aAddedDatasets As atcCollection) Handles lDataGroupWithEvents.Added
        Assert.AreEqual(1, aAddedDatasets.Count)
    End Sub


    '''<summary>Test RaiseRemovedOne</summary>
    <TestMethod(), DeploymentItem("atcData.dll")> _
    Public Sub RaiseRemovedOneTest()
        Dim lDataGroupAccessor As atcDataGroup_Accessor = New atcDataGroup_Accessor()
        Dim lDataSet As atcDataSet = New atcDataSet
        lDataGroupAccessor.RaiseRemovedOne(lDataSet)
        Assert.Inconclusive("Need A Test Here")
    End Sub

    '''<summary>Test Remove</summary>
    <TestMethod()> Public Sub RemoveTest()
        Dim lDataSet As atcDataSet = New atcDataSet
        Dim lAddThese As IEnumerable = {lDataSet.Clone, lDataSet}
        Dim lDataGroup As atcDataGroup = New atcDataGroup
        lDataGroup.AddRange(lAddThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(2, lDataGroup.Count)
        Dim lRemoveThese As New atcCollection
        lRemoveThese.Add(lDataSet)
        lDataGroup.Remove(lRemoveThese)
        Assert.IsInstanceOfType(lDataGroup, GetType(atcDataGroup))
        Assert.AreEqual(1, lDataGroup.Count)
    End Sub

    '''<summary>Test Remove</summary>
    <TestMethod()> Public Sub RemoveTest1()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aDataSet As atcDataSet = Nothing ' TODO: Initialize to an appropriate value
        target.Remove(aDataSet)
        Assert.Inconclusive("A method that does not return a value cannot be verified.")
    End Sub

    '''<summary>Test RemoveAt</summary>
    <TestMethod()> Public Sub RemoveAtTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aIndex As Integer = 0 ' TODO: Initialize to an appropriate value
        target.RemoveAt(aIndex)
        Assert.Inconclusive("A method that does not return a value cannot be verified.")
    End Sub

    '''<summary>Test RemoveRange</summary>
    <TestMethod()> Public Sub RemoveRangeTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aIndex As Integer = 0 ' TODO: Initialize to an appropriate value
        Dim aNumber As Integer = 0 ' TODO: Initialize to an appropriate value
        target.RemoveRange(aIndex, aNumber)
        Assert.Inconclusive("A method that does not return a value cannot be verified.")
    End Sub

    '''<summary>Test SortedAttributeValues</summary>
    <TestMethod()> Public Sub SortedAttributeValuesTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aAttributeDefinition As atcAttributeDefinition = Nothing ' TODO: Initialize to an appropriate value
        Dim aMissingValue As Object = Nothing ' TODO: Initialize to an appropriate value
        Dim expected As atcCollection = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcCollection
        actual = target.SortedAttributeValues(aAttributeDefinition, aMissingValue)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test SortedAttributeValues</summary>
    <TestMethod()> Public Sub SortedAttributeValuesTest1()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aAttributeName As String = String.Empty ' TODO: Initialize to an appropriate value
        Dim aMissingValue As Object = Nothing ' TODO: Initialize to an appropriate value
        Dim expected As atcCollection = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcCollection
        actual = target.SortedAttributeValues(aAttributeName, aMissingValue)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test ToString</summary>
    <TestMethod()> Public Sub ToStringTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim expected As String = String.Empty ' TODO: Initialize to an appropriate value
        Dim actual As String
        actual = target.ToString
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test __ENCAddToList</summary>
    <TestMethod(), DeploymentItem("atcData.dll")> _
    Public Sub __ENCAddToListTest()
        Dim value As Object = Nothing ' TODO: Initialize to an appropriate value
        atcDataGroup_Accessor.__ENCAddToList(value)
        Assert.Inconclusive("A method that does not return a value cannot be verified.")
    End Sub

    '''<summary>Test Item</summary>
    <TestMethod()> Public Sub ItemTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aIndex As Integer = 0 ' TODO: Initialize to an appropriate value
        Dim expected As atcDataSet = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcDataSet
        target(aIndex) = expected
        actual = target(aIndex)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test ItemByIndex</summary>
    <TestMethod()> Public Sub ItemByIndexTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aIndex As Integer = 0 ' TODO: Initialize to an appropriate value
        Dim expected As atcDataSet = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcDataSet
        target.ItemByIndex(aIndex) = expected
        actual = target.ItemByIndex(aIndex)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test ItemByKey</summary>
    <TestMethod()> Public Sub ItemByKeyTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim aKey As Object = Nothing ' TODO: Initialize to an appropriate value
        Dim expected As atcDataSet = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcDataSet
        target.ItemByKey(aKey) = expected
        actual = target.ItemByKey(aKey)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub

    '''<summary>Test SelectedData</summary>
    <TestMethod()> Public Sub SelectedDataTest()
        Dim target As atcDataGroup = New atcDataGroup() ' TODO: Initialize to an appropriate value
        Dim expected As atcDataGroup = Nothing ' TODO: Initialize to an appropriate value
        Dim actual As atcDataGroup
        target.SelectedData = expected
        actual = target.SelectedData
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Verify the correctness of this test method.")
    End Sub
End Class
