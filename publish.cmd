@ECHO OFF

IF %1.==. GOTO USAGE

SET pkg_ver=%1

REM Some digital twin components continues to use the DT major version line (the last DT release was 5.4.0).
REM Convert 3.0.X (or 3.0.X-<suffix>) -> 6.0.X (or 6.0.X-<suffix>)
IF "%pkg_ver:~0,4%"=="3.0." (
    SET "version_legacy_component=6.0.%pkg_ver:~4%"
) ELSE (
    REM Fallback: if the version isn't in the expected 3.0.* format, use it as-is.
    SET "version_legacy_component=%pkg_ver%"
)

dotnet nuget push PackageBuild\Scaleout.Modules.DigitalTwin.Abstractions.%version_legacy_component%.nupkg -k %NUGET_API_KEY% -s https://nuget.scaleoutsoftware.com/nuget
dotnet nuget push PackageBuild\Scaleout.DigitalTwin.Workbench.%version_legacy_component%.nupkg -k %NUGET_API_KEY% -s https://nuget.scaleoutsoftware.com/nuget
GOTO END


:USAGE
ECHO Publish.cmd version
ECHO version: the version of the packages you want to publish (e.g. 0.6.5-alpha)
:END
