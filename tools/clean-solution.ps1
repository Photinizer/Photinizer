Set-Location "$PSScriptRoot/.."

Get-ChildItem -Recurse -Directory |
  Where-Object { $_.Name -in @('bin','obj') } |
  Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

"Clean complete."