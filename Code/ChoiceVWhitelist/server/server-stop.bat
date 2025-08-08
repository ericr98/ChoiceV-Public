pushd %~dp0
FOR /F "tokens=4 delims= " %%P IN ('netstat -a -n -o ^| findstr :9911') DO TaskKill.exe /PID %%P
popd