# Needs to be at least that version, or mmm can't read the archive
#Requires -Modules @{ ModuleName="Microsoft.PowerShell.Archive"; ModuleVersion="1.2.3" }
$Name = "BrokenController" # Replace with your mods name
$Version = "v1.1.0"
mkdir BepInEx\plugins\$Name
cp bin\Release\netstandard2.0\$Name.dll BepInEx\plugins\$Name\
Compress-Archive -force .\BepInEx\ $Name-$Version.zip
rmdir .\BepInEx\ -Recurse