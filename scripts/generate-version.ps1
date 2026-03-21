param(
    [string]$SolutionDir,
    [string]$ProjectDir
)

$manifest = Get-Content "$SolutionDir/thunderstore/manifest.json" | ConvertFrom-Json
$version = $manifest.version_number
$buildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# Write Version.g.cs
$versionCs = @"
// Auto-generated from manifest.json -- do not edit manually
namespace ProliferatorMultiplier
{
    internal static class VersionInfo
    {
        public const string Value = "$version";
        public const string BuildDate = "$buildDate";
    }
}
"@
Set-Content "$ProjectDir/Properties/Version.g.cs" $versionCs

# Update AssemblyInfo.cs version lines
$assemblyInfoPath = "$ProjectDir/Properties/AssemblyInfo.cs"
$assemblyInfo = Get-Content $assemblyInfoPath
$assemblyInfo = $assemblyInfo -replace 'AssemblyVersion\("[^"]*"\)', "AssemblyVersion(`"$version`")"
$assemblyInfo = $assemblyInfo -replace 'AssemblyFileVersion\("[^"]*"\)', "AssemblyFileVersion(`"$version`")"
Set-Content $assemblyInfoPath $assemblyInfo

Write-Host "Version $version injected from manifest.json"
