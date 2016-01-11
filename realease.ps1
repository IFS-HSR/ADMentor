function remove-authorTags($xmiFile){
    (Get-Content $xmiFile) -replace "tag=`"author`" value=`"([^`"]*)`"", "tag=`"author`" value=`"`"" | Set-Content $xmiFile
}

$modelTemplates = get-childitem ADMentor/ADTechnology/*Template.xml
$patterns = get-childitem ADMentor/ADTechnology/*Pattern.xml

$modelTemplates | ForEach-Object { remove-authorTags $_ }
$patterns | ForEach-Object { remove-authorTags $_ }