@echo off
call :init

set app1=Api6775

call :pack %app1%
exit /b 0

:pack
rem %1 - app
if exist bin rd /s /q bin
for %%i in (%1\*.csproj) do set prj=%%~dpnxi

rem Build matrix
rem 1 - Build an app with many dlls (default)
rem 2 - Build a single-file app when NET [Desktop] runtime required (my favorite)
rem 3 - Build a single-file app when no runtime required (NET embedded)
rem 4 - Build an app with many dlls for Linux
set option=2

rem call :bin %1 %option% %prj% net8.0 win-x86
rem call :bin %1 %option% %prj% net9.0 win-x86

call :bin %1 %option% %prj% net8.0 win-x64
call :bin %1 %option% %prj% net9.0 win-x64

rem Linux
set option=4
rem call :bin %1 %option% %prj% net8.0 linux-x64
rem call :bin %1 %option% %prj% net9.0 linux-x64

for /f "tokens=3 delims=<>" %%v in ('findstr "<TargetFrameworks>" %prj%') do set targets=%%v
for /f "tokens=3 delims=<>" %%v in ('findstr "<Version>" %prj%') do set version=%%v
for /f "tokens=3 delims=<>" %%v in ('findstr "<Description>" %prj%') do set description=%%v
rem Use "" if the .proj description has parentheses!
rem for /f "tokens=3 delims=<>" %%v in ('findstr "<Description>" %prj%') do set description="%%v"
for %%i in (.) do set repo=%%~nxi
call :lower %repo% repol

call :readme > bin\version.txt

set pack=%1-v%version%.zip
if exist %pack% del %pack%
echo === Pack %pack% ===

"C:\Program Files\7-Zip\7z.exe" a %pack% LICENSE *.md *.sln *.cmd bin\ XLSM\ Templates\
"C:\Program Files\7-Zip\7z.exe" a %pack% -r -x!.* -x!bin -x!obj -x!PublishProfiles -x!*.user %1\

if exist %store% copy /y %pack% %store%
goto :eof

:bin
rem %1 - app
rem %2 - option
rem %3 - project.csproj
rem %4 - net
rem %5 - x86/x64
echo === Build %1 %4 %5 ===
rem win
if /%2/==/1/ dotnet publish %3 -o bin\%4.%5 -f %4 -r %5
if /%2/==/2/ dotnet publish %3 -o bin\%4.%5 -f %4 -r %5 -p:PublishSingleFile=true --no-self-contained
if /%2/==/3/ dotnet publish %3 -o bin\%4.%5 -f %4 -r %5 -p:PublishSingleFile=true
rem linux
if /%2/==/4/ dotnet publish %3 -o bin\%4.%5 -f %4 -r %5 --self-contained
goto :eof

:init
for /f "tokens=3,3" %%a in ('reg query "hkcu\control panel\international" /v sshortdate') do set sfmt=%%a
for /f "tokens=3,3" %%a in ('reg query "hkcu\control panel\international" /v slongdate') do set lfmt=%%a

reg add "hkcu\control panel\international" /v sshortdate /t reg_sz /d yyyy-MM-dd /f >nul
reg add "hkcu\control panel\international" /v slongdate /t reg_sz /d yyyy-MM-dd /f >nul

set ymd=%date%

reg add "hkcu\control panel\international" /v sshortdate /t reg_sz /d %sfmt% /f >nul
reg add "hkcu\control panel\international" /v slongdate /t reg_sz /d %lfmt% /f >nul

set store=G:\BankApps\AppStore
goto :eof

:lower
rem %1 - Source
rem %2 - source
echo>%Temp%\%1
for /f %%f in ('dir /b/l %Temp%\%1') do set %2=%%f
del %Temp%\%1
goto :eof

:readme
echo %app1%
echo %description%
echo.
echo Version: v%version%
echo Date:    %ymd%
echo.
echo NET:     %targets%
echo Download from https://dotnet.microsoft.com/download
echo.
echo Run once to create "%app1%.config.json"
echo and correct it
echo.
echo https://github.com/diev/%repo%
echo https://gitverse.ru/diev/%repo%
echo https://gitflic.ru/project/diev/%repol%
goto :eof
