set DOTNET_EnableAVX=0
set DOTNET_EnableAVX2=0
set DOTNET_EnableSSE41=0

dotnet publish -c Release -r win-x64
pause