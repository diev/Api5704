@echo off
rem version 2024-06-24
rem Add this section to .csproj:

rem <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(OS)' == 'Windows_NT' and '$(ConfigurationName)' == 'Release'">
rem   <Exec Command="call PostBuild.cmd $(ProjectPath)"/>
rem </Target>

rem if '%APPVEYOR%' == 'True' exit /b 0
setlocal
chcp 65001
rem $(ProjectPath)
if '%1' == '' exit /b 0
rem C:\Repos\Repo\src\project.csproj
set ProjectPath=%1
rem project.csproj
set ProjectFileName=%~nx1
rem project
set ProjectName=%~n1
rem src
for %%i in (.) do set ProjectDirName=%%~nxi
for %%i in (..) do (
 rem C:\Repos\Repo
 set Repo=%%~dpnxi
 rem Repo
 set RepoName=%%~nxi
)
rem Version X.X.X.X
for /f "tokens=3 delims=<>" %%v in ('findstr "<Version>" %ProjectPath%') do set Version=%%v
for /f "tokens=3 delims=<>" %%v in ('findstr "<Description>" %ProjectPath%') do set Description=%%v
rem Date yyyy-mm-dd
set Ymd=%date:~-4%-%date:~3,2%-%date:~0,2%

rem Test build folder
set Test=$

echo === Pack sources ===

set SrcPack=%ProjectName%-v%Version%-src.zip

echo Pack sources to %SrcPack%

rem Go to the solution level
pushd ..

set Packer="C:\Program Files\7-Zip\7z.exe" a -tzip %SrcPack% -xr!bin -xr!obj
if exist %SrcPack% del %SrcPack%

rem Pack files and folders at the solution level
%Packer% *.sln *.md LICENSE Templates XLSM

rem Pack sources in folders at the solution level
call :pack %ProjectDirName%

echo === Test build ===

"C:\Program Files\7-Zip\7z.exe" x -y %SrcPack% -o%Test%
cd %Test%

call :build_cmd > build.cmd
call :version_txt > version.txt
call :postbuild_cmd > %ProjectDirName%\PostBuild.cmd

"C:\Program Files\7-Zip\7z.exe" a ..\%SrcPack% build.cmd version.txt %ProjectDirName%\PostBuild.cmd

call build.cmd

echo === Pack binaries ===

set BinPack=%ProjectName%-v%Version%.zip

cd %ProjectDirName%\bin\Distr
copy ..\..\..\version.txt

echo Pack binary application to %BinPack%

if exist ..\..\..\..\%BinPack% del ..\..\..\..\%BinPack%
"C:\Program Files\7-Zip\7z.exe" a -tzip ..\..\..\..\%BinPack%
cd ..\..\..\..

rem Pack files and folders at the solution level
"C:\Program Files\7-Zip\7z.exe" a %BinPack% Templates XLSM

echo === Backup ===

set Store=G:\BankApps\AppStore
if not exist %Store% goto :nobackup
copy /y %SrcPack% %Store%
copy /y %BinPack% %Store%
:nobackup

echo === All done ===

rd /s /q %Test%
popd
endlocal
exit /b 0

:pack
if '%1' == '' goto :eof

echo === Pack %1 ===

%Packer% -r %1\*.cs %1\*.resx
%Packer% %1\*.csproj %1\*.json %1\*.cmd
shift
goto pack

:lower
echo>%Temp%\%2
for /f %%f in ('dir /b/l %Temp%\%2') do set %1=%%f
del %Temp%\%2
goto :eof

:build_cmd
echo rem Build an app with many dlls
echo rem dotnet publish %ProjectDirName%\%ProjectFileName% -o %ProjectDirName%\bin\Distr
echo.
echo rem Build a single-file app when NET Desktop runtime required 
echo dotnet publish %ProjectDirName%\%ProjectFileName% -o %ProjectDirName%\bin\Distr -r win-x64 -p:PublishSingleFile=true --self-contained false
echo.
echo rem Build a single-file app when no runtime required
echo rem dotnet publish %ProjectDirName%\%ProjectFileName% -o %ProjectDirName%\bin\Distr -r win-x64 -p:PublishSingleFile=true
goto :eof

:version_txt
call :lower RepoLName %RepoName%
echo %ProjectName%
echo %Description%
echo.
echo Version: v%Version%
echo Date:    %Ymd%
echo.
echo Requires SDK .NET 8.0 to build
echo Requires .NET Desktop Runtime 8.0 to run
echo Download from https://dotnet.microsoft.com/download
echo.
echo Open source code, Issues, Releases:
echo https://github.com/diev/%RepoName%
echo Mirrors:
echo https://gitverse.ru/diev/%RepoName%
echo https://gitflic.ru/project/diev/%RepoLName%
goto :eof

:postbuild_cmd
echo exit /b 0
goto :eof
