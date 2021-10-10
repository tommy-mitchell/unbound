@echo off

set ASEPRITE=G:\SteamLibrary\steamapps\common\Aseprite\aseprite.exe
set   SCRIPT="C:\Users\Tommy\AppData\Roaming\Aseprite\scripts\Export Layers.lua"
set     FILE=%1

%ASEPRITE% --batch %FILE% --script %SCRIPT%