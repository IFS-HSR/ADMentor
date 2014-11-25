function remove-authorTags($xmiFile){
    (Get-Content $xmiFile) -replace "tag=`"author`" value=`"([^`"]*)`"", "tag=`"author`" value=`"`"" | Set-Content $xmiFile
}

$modelTemplates = get-childitem ADAddIn/ADTechnology/*Template.xml

$modelTemplates | ForEach-Object { remove-authorTags $_ }