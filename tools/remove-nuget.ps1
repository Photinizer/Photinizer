# remove-nuget.ps1
param(
    [Parameter(Mandatory = $true)][string]$PackageId,
    [string]$Version
)

function Get-GlobalPackagesPath {
    # 0) dotnet reports the actual global packages path
    try {
        $p = & dotnet nuget locals global-packages --list 2>$null
        if ($LASTEXITCODE -eq 0 -and $p) {
            # expected like: "global-packages: D:\.nuget\packages\"
            $line = ($p -split "`r?`n") | Where-Object { $_ -match ':' } | Select-Object -First 1
            if ($line) {
                $afterColon = $line.Substring($line.IndexOf(':') + 1).Trim()
                if ($afterColon) {
                    $rp = Resolve-Path -LiteralPath $afterColon -ErrorAction SilentlyContinue
                    return ($(if ($rp) { $rp.Path } else { $afterColon }))
                }
            }
        }
    } catch {}

    # 1) env override
    if ($env:NUGET_PACKAGES) {
        $rp = Resolve-Path -LiteralPath $env:NUGET_PACKAGES -ErrorAction SilentlyContinue
        return ($(if ($rp) { $rp.Path } else { $env:NUGET_PACKAGES }))
    }

    # 2) walk up for solution-level configs
    $probe = (Get-Location).Path
    while ($true) {
        $c1 = Join-Path $probe 'NuGet.config'
        $c2 = Join-Path $probe '.nuget\NuGet.config'
        foreach ($cfg in @($c1, $c2)) {
            if (Test-Path -LiteralPath $cfg) {
                try {
                    [xml]$x = Get-Content -LiteralPath $cfg -ErrorAction Stop
                    $node = $x.configuration.config.add | Where-Object { $_.key -ieq 'globalPackagesFolder' }
                    if ($node -and $node.value) {
                        $p  = $ExecutionContext.SessionState.Path.ExpandString($node.value.Trim())
                        $rp = Resolve-Path -LiteralPath $p -ErrorAction SilentlyContinue
                        return ($(if ($rp) { $rp.Path } else { $p }))
                    }
                } catch {}
            }
        }
        $parent = Split-Path $probe -Parent
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $probe) { break }
        $probe = $parent
    }

    # 3) user-level %APPDATA%\NuGet\NuGet.Config
    $userCfg = Join-Path $env:APPDATA 'NuGet\NuGet.Config'
    if (Test-Path -LiteralPath $userCfg) {
        try {
            [xml]$ux = Get-Content -LiteralPath $userCfg -ErrorAction Stop
            $unode = $ux.configuration.config.add | Where-Object { $_.key -ieq 'globalPackagesFolder' }
            if ($unode -and $unode.value) {
                $p  = $ExecutionContext.SessionState.Path.ExpandString($unode.value.Trim())
                $rp = Resolve-Path -LiteralPath $p -ErrorAction SilentlyContinue
                return ($(if ($rp) { $rp.Path } else { $p }))
            }
        } catch {}
    }

    # 4) default
    return (Join-Path $env:USERPROFILE '.nuget\packages')
}

# --- main ---
$root = Get-GlobalPackagesPath
$pkgFolderName = $PackageId.ToLowerInvariant()
$pkgPath = Join-Path $root $pkgFolderName

Write-Host "NuGet global cache: $root"

if (-not (Test-Path -LiteralPath $pkgPath)) {
    Write-Host "Package not found:`n$pkgPath"
    exit 0
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    Remove-Item -LiteralPath $pkgPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Removed: $pkgPath"
} else {
    $verPath = Join-Path $pkgPath $Version
    if (Test-Path -LiteralPath $verPath) {
        Remove-Item -LiteralPath $verPath -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed: $verPath"
        $hasChildren = Get-ChildItem -LiteralPath $pkgPath -Force -ErrorAction SilentlyContinue | Where-Object { $_ }
        if (-not $hasChildren) {
            Remove-Item -LiteralPath $pkgPath -Recurse -Force -ErrorAction SilentlyContinue
        }
    } else {
        Write-Host "Version not found:`n$verPath"
    }
}

# optional: HTTP v3 cache cleanup
$http = Join-Path $env:LOCALAPPDATA 'NuGet\v3-cache'
if (Test-Path -LiteralPath $http) {
    $needle = $pkgFolderName
    Get-ChildItem $http -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "*$needle*" } |
        ForEach-Object {
            Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "HTTP cache removed: $($_.FullName)"
        }
}

Write-Host "Done."