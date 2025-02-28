Attribute VB_Name = "HashModule"
Option Explicit

'Редакция: 2025-02-28

Const UTIL As String = "C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe"
Const ARGS As String = " -logfile %2 -mk -alg GR3411_2012_256 %1"

Public Sub CalcHash()
    Dim Answer As Variant
    Dim Pdf As String, Hash As String
    
    Answer = Application.GetOpenFilename("PDF (*.pdf),*.pdf,Все файлы (*.*),*.*", , "Выберите файл PDF для расчета ХэшКода")
    If Answer <> False Then
        Pdf = Answer
        Hash = GetHash(Pdf)
    End If
    
    'MsgBox Pdf & vbCrLf & Hash, , "ХэшКод"
    Answer = InputBox(Pdf & vbCrLf & "скопируйте и вставьте", "ХэшКод", Hash)
End Sub

Public Sub ReadHash()
    Dim Answer As Variant
    Dim Txt As String, Hash As String
    
    Answer = Application.GetOpenFilename("TXT (*.txt),*.txt,Все файлы (*.*),*.*", , "Выберите файл TXT с ХэшКодом")
    If Answer <> False Then
        Txt = Answer
        fn = FreeFile
        Open Txt For Input As fn
        Hash = Input(64, fn)
        Close fn
    End If
    
    'MsgBox Txt & vbCrLf & Hash, , "ХэшКод"
    Answer = InputBox(Txt & vbCrLf & "скопируйте и вставьте", "ХэшКод", Hash)
End Sub

Public Function GetHash(Pdf As String) As String
    Dim cmd As String, Txt As String, s As String
    Dim fn As Integer
    
    Txt = Pdf & ".txt"
    
    cmd = Q(UTIL) & ARGS
    cmd = Replace(cmd, "%1", Q(Pdf))
    cmd = Replace(cmd, "%2", Q(Txt))
    
    Shell cmd, 1
    
    Application.Wait Now + TimeValue("0:00:05")
    
    fn = FreeFile
    Open Txt For Input As fn
    s = Input(LOF(fn) - 2, fn)
    Close fn
    
    GetHash = LCase(s)
End Function

Public Function Q(s As String) As String
    Q = """" & s & """"
End Function
