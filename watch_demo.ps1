$env:DOTNET_WATCH_SUPPRESS_EMOJIS=1
dotnet watch --project .\Haondt.Web.Demo\Haondt.Web.Demo.csproj --no-hot-reload
Remove-Item Env:DOTNET_WATCH_SUPPRESS_EMOJIS
