@echo off
"C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe" -logfile "%~1.txt" -mk -alg GR3411_2012_256 %1

rem Convert to lower case

set /p hash=<"%~1.txt"
echo %hash%

echo>%Temp%\%hash%
for /f %%f in ('dir /b/l %Temp%\%hash%') do set hash2=%%f
del %Temp%\%hash%

echo %hash2%>"%~1.txt"
echo %hash2%
