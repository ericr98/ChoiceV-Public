echo off
set arg1=%1
shift
pushd %~dp0
FOR /F "tokens=4 delims= " %%P IN ('netstat -a -n -o ^| findstr :8899') DO TaskKill.exe /PID %%P
popd