@echo off
echo Calc HashCode for Credit Bureau

if not exist "C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe" (
 echo Error: Crypto Pro not found.
 goto :eof
)

for %%f in (*.pdf) do call :hash "%%f"
goto :eof

:hash
echo File: %1

"C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe" -mk -alg GR3411_2012_256 %1 > "%~1.txt"

rem Convert to lower case

set /p hash=<"%~1.txt"
echo %hash%

echo>%Temp%\%hash%
for /f %%f in ('dir /b/l %Temp%\%hash%') do set hash2=%%f
del %Temp%\%hash%

echo %hash2%>"%~1.txt"
echo %hash2%
goto :eof
