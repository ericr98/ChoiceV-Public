echo off
set arg1=%1
shift
pushd %~dp0
start "%arg1%" altv-server.exe %*
popd