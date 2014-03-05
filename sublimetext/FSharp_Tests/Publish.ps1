$script:thisDir = split-path $MyInvocation.MyCommand.Path -parent

if (-not (test-path variable:STDataPath)) {
    write-error "You must define `$STDataPath"
    exit 1
}

remove-item "$STDataPath/Packages/FSharp_tests/*" -recurse -force -erroraction silentlycontinue

push-location $thisDir -erroraction stop
    copy-item "test_runner.py" "$STDataPath/Packages/FSharp_Tests" -force
pop-location

push-location (join-path $thisDir 'tests') -erroraction stop
    copy-item "*" "$STDataPath/Packages/FSharp_Tests" -force -recurse
    copy-item "*" "$STDataPath/Packages/FSharp_Tests" -force -recurse
pop-location

'done'
