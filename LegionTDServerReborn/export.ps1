dotnet.exe publish -c release -r linux-x64 /p:PublishSingleFile=true /p:PublishSingleFile=true /p:PublishTrimmed=true
explorer.exe .\bin\release\netcoreapp3.1\linux-x64\publish
