Attribute VB_Name = "HashModule"
Option Explicit

Const UTIL As String = "C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe"
Const ARGS As String = " -logfile %2 -mk -alg GR3411_2012_256 %1"

Public Sub CalcHash()
    Dim Answer As Variant
    Dim Pdf As String, Hash As String
    
    Answer = Application.GetOpenFilename("PDF (*.pdf), *.pdf", , "Выберите файл для расчета ХэшКода")
    If Answer <> False Then
        Pdf = Answer
        Hash = GetHash(Pdf)
    End If
    
    'MsgBox Pdf & vbCrLf & Hash, , "ХэшКод"
    Answer = InputBox(Pdf & vbCrLf & "скопируйте и вставьте", "ХэшКод", Hash)
End Sub

Public Function GetHash(Pdf) As String
    Dim cmd As String, txt As String, s As String
    Dim fn As Integer
    
    txt = Pdf & ".txt"
    
    cmd = Q(UTIL) & ARGS
    cmd = Replace(cmd, "%1", Q(Pdf))
    cmd = Replace(cmd, "%2", Q(txt))
    
    Shell cmd, 1
    
    Application.Wait Now + TimeValue("0:00:05")
    
    fn = FreeFile
    Open txt For Input As fn
    s = Input(LOF(fn) - 2, fn)
    Close fn
    
    GetHash = LCase(s)
End Function

Public Function Q(s As String) As String
    Q = """" & s & """"
End Function
