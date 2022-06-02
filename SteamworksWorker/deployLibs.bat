@echo off
echo %PATH%
DEL  "libsteam_api.so"
DEL  "steam_api.dll"
DEL  "Steamworks.NET"
SET path=SteamworksLibs\win64

COPY "%path%\*" "%CD%" /Y