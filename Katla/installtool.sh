#!/bin/sh
#installs the katla tool to the current machine
xbuild Katla.csproj
mkdir /usr/local/bin/katlabin
cp bin/Debug/*.dll /usr/local/bin/katlabin/
cp bin/Debug/Katla.exe /usr/local/bin/katlabin/
cp katla /usr/local/bin/