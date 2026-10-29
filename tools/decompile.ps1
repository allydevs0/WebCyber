$ErrorActionPreference = 'Stop'
$Game = 'E:\SteamLibrary\steamapps\common\BombRushCyberfunk'
$Dll = Join-Path $Game 'Bomb Rush Cyberfunk_Data\Managed\Assembly-CSharp.dll'
$OutDir = Join-Path $PSScriptRoot '..\analysis\managed-code\Assembly-CSharp'
$OutDir = [System.IO.Path]::GetFullPath($OutDir)

if (!(Test-Path -LiteralPath $Dll)) {
    throw "Assembly-CSharp.dll não encontrado em: $Dll"
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

Write-Host "Iniciando descompilação de $Dll para $OutDir..."
& ilspycmd -p -o $OutDir $Dll

Write-Host "Descompilação concluída com sucesso em: $OutDir"
