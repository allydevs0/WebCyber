$ErrorActionPreference = 'Stop'
$Game = 'E:\SteamLibrary\steamapps\common\BombRushCyberfunk'
$Data = Join-Path $Game 'Bomb Rush Cyberfunk_Data'
$Out = Join-Path $PSScriptRoot '..\analysis\executable-report'
$Out = [System.IO.Path]::GetFullPath($Out)

if (!(Test-Path -LiteralPath $Game)) {
    throw "Pasta do jogo não encontrada: $Game"
}
if (!(Test-Path -LiteralPath $Data)) {
    throw "Pasta de dados da Unity não encontrada: $Data"
}
New-Item -ItemType Directory -Force -Path $Out | Out-Null

$exe = Get-ChildItem -LiteralPath $Game -File -Filter '*.exe'
$allFiles = Get-ChildItem -LiteralPath $Data -File -Recurse

$exe |
    Select-Object Name, Length, LastWriteTime |
    Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $Out 'executables.csv')

$allFiles |
    Select-Object @{Name='RelativePath';Expression={$_.FullName.Substring($Data.Length).TrimStart('\')}},
                  Length, Extension |
    Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $Out 'data-files.csv')

$managedCandidates = $allFiles | Where-Object {
    $_.Extension -eq '.dll' -and
    ($_.FullName -match '[\\/]Managed[\\/]' -or
     $_.FullName -match '[\\/]MonoBleedingEdge[\\/]')
}
$managedCandidates |
    Select-Object FullName, Length |
    Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $Out 'managed-candidates.csv')

$interesting = $allFiles | Where-Object {
    $_.Name -match 'globalgamemanagers|global-metadata|GameAssembly|resources\.assets|boot\.config'
}
$interesting |
    Select-Object FullName, Length |
    Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $Out 'unity-markers.csv')

$bytes = ($allFiles | Measure-Object -Property Length -Sum).Sum
$summary = @(
    "Game path: $Game",
    "Data path: $Data",
    "Executable count: $($exe.Count)",
    "Data-file count: $($allFiles.Count)",
    "Managed candidates: $($managedCandidates.Count)",
    "Data bytes: $bytes",
    "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss K')"
)
$summary | Set-Content -Encoding UTF8 (Join-Path $Out 'summary.txt')
$summary
Write-Host "`nRelatórios criados em: $Out"
