function remove-authorTags($xmiFile){
    (Get-Content $xmiFile) -replace "tag=`"author`" value=`"([^`"]*)`"", "tag=`"author`" value=`"`"" | Set-Content $xmiFile
}

function remove-decisionDateValue($xmiFile){
    (Get-Content $xmiFile) -replace "tag=`"Decision Date`" xmi.id=`"([^`"]*)`" value=`"([^`"]*)`"", "tag=`"Decision Date`" xmi.id=`"`$1`" value=`"`"" | Set-Content $xmiFile
}

function remove-dateTags($xmiFile){
	(Get-Content $xmiFile) -replace "<UML:TaggedValue tag=`"date_([^`"]*)`" value=`"([^`"]*)`"/>", "" | Set-Content $xmiFile
}

$modelTemplates = get-childitem ADMentor/ADTechnology/*Template.xml
$patterns = get-childitem ADMentor/ADTechnology/*Pattern.xml

$modelTemplates | ForEach-Object { remove-authorTags $_ }
$modelTemplates | ForEach-Object { remove-decisionDateValue $_ }
$modelTemplates | ForEach-Object { remove-dateTags $_ }
$patterns | ForEach-Object { remove-authorTags $_ }
$patterns | ForEach-Object { remove-decisionDateValue $_ }
$patterns | ForEach-Object { remove-dateTags $_ }