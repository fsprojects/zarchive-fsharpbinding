
$script:thisDir = split-path $MyInvocation.MyCommand.Path -parent
$script:serverDir = resolve-path((join-path $thisDir "../fsac"))
$script:bundledDir = resolve-path((join-path $thisDir "../bundled"))

remove-item (join-path $bundledDir "*") -erroraction silentlycontinue
push-location $bundledDir
    wget.exe "https://bitbucket.org/guillermooo/fsac/downloads/fsautocomplete.zip"
pop-location
