Option Strict Off
Option Explicit On
<System.Runtime.InteropServices.ProgId("HspfTSGroupDef_NET.HspfTSGroupDef")> Public Class HspfTSGroupDef
    'Copyright 2006 AQUA TERRA Consultants - Royalty-free use permitted under open source license
	
	Dim pName As String
	Dim pId As Integer
	Dim pBlockID As Integer
	Dim pMemberDefs As Collection 'of HspfTSMemberDef
	
	Public Property Name() As String
		Get
			Name = pName
		End Get
		Set(ByVal Value As String)
			pName = Value
		End Set
	End Property
	
	Public Property Id() As Integer
		Get
			Id = pId
		End Get
		Set(ByVal Value As Integer)
			pId = Value
		End Set
	End Property
	
	Public Property BlockId() As Integer
		Get
			BlockId = pBlockID
		End Get
		Set(ByVal Value As Integer)
			pBlockID = Value
		End Set
	End Property
	
	Public Property MemberDefs() As Collection
		Get 'of HspfTSMemberDef
			MemberDefs = pMemberDefs
		End Get
		Set(ByVal Value As Collection) 'of HspfTSMemberDef
			pMemberDefs = Value
		End Set
	End Property
End Class