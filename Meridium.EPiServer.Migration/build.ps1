$7z = '.\tools\7za.exe'

# Clean
if (Test-Path .\build\) { rm .\build\ -force -recurse }

# Create build dir
md .\build\lib -force
md .\build\Migration -force

# Copy build results 
cp .\*.aspx .\build\Migration\ 
cp .\bin\debug\Meridium.EPiServer.Migration.* .\build\lib\ 
cp .\bin\debug\HtmlAgilityPack.* .\build\lib\

# Zip build folder
& $7z a .\build\migration.zip .\build\*
