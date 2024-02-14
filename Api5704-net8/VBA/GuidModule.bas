Attribute VB_Name = "GuidModule"
Option Explicit

Public Type GUID_TYPE
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(7) As Byte
End Type
 
#If VBA7 Then 'Office 2007+
  Private Declare PtrSafe Function CoCreateGuid Lib "ole32.dll" (Guid As GUID_TYPE) As LongPtr
  Private Declare PtrSafe Function StringFromGUID2 Lib "ole32.dll" (Guid As GUID_TYPE, ByVal lpStrGuid As LongPtr, ByVal cbMax As Long) As LongPtr
#Else
  Private Declare Function CoCreateGuid Lib "ole32.dll" (Guid As GUID_TYPE) As Long
  Private Declare Function StringFromGUID2 Lib "ole32.dll" (Guid As GUID_TYPE, ByVal lpStrGuid As Long, ByVal cbMax As Long) As Long
#End If
 
Public Function CreateGuidString() As String '=> "{99196FA6-B1F7-4F0D-B63D-C3A8D1A1129B} "
    Dim Guid As GUID_TYPE
    Dim strGuid As String
    Dim retValue
 
    Const guidLength As Long = 39 'registry GUID format with null terminator {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
    retValue = CoCreateGuid(Guid)
 
    If retValue = 0 Then
        strGuid = String$(guidLength, vbNullChar)
        retValue = StringFromGUID2(Guid, StrPtr(strGuid), guidLength)
        If retValue = guidLength Then ' valid GUID as a string
            CreateGuidString = strGuid
        End If
    End If
End Function

Public Function CreateGuidStr() As String '=> "99196fa6-b1f7-4f0d-b63d-c3a8d1a1129b"
    CreateGuidStr = LCase(Mid(CreateGuidString(), 2, 36))
End Function

Public Sub Guid()
    ActiveCell = CreateGuidStr()
End Sub
