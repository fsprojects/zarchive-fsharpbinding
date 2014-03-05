param([switch]$Release, [switch]$Refresh)

$script:thisDir = split-path $MyInvocation.MyCommand.Path -parent

$getDependencies = join-path $script:thisDir "GetDependencies.ps1"
$publishRelease = join-path $script:thisDir "Publish.ps1"

if ($Refresh) {
    # Download stuff.
    'downloading depencdencies...'
    & $getDependencies
}

'publishing...'
# XXX: Use @boundparams instead?
& $publishRelease -Release:$Release

if ($LASTEXITCODE -ne 0) {
    write-error "Could not publish package."
    exit 1
}

'done'

try {
    get-process "sublime_text" | stop-process
    start-sleep -milliseconds 250
}
catch [Exception] {
    # ignore
}
sublime_text
