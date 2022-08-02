
#Common
$headers = @{'Content-Type'='application/json'}
$pShowLog = 'N'
$pSendMail = 'N'
$pWriteLog  = 'N'
$server = "https://localhost:7073"
#Mail
$to="smorondo@bilbomatica.es"
$from="smorondo@bilbomatica.es"
$SMTPServer = "smtp.gmail.com"
$SMTPPort = "465"
$credential = New-Object System.Management.Automation.PSCredential -ArgumentList 'smorondo@bilbomatica.es', ('elputoMOE_07' | ConvertTo-SecureString -AsPlainText -Force)
#DBServer
$DBServer="backbonedb.database.windows.net"
$DataBase="n2k_backbone"
$UserName="Logger"
$Password="L1br4r14n"


function talkme($pMessaje,$pCColor,$pBColor)
{   
	if($pShowLog -eq 'Y'){ Write-Host $pMessaje -ForegroundColor $pCColor -BackgroundColor $pBColor }
}

function alert ($pBody)
{
	if($pSendMail -eq 'Y')
	{
		$subject="Harvest test"
		Send-MailMessage -From $from -To $to -Subject $subject -BodyAsHtml $pBody  -SmtpServer $SMTPServer  -port $SMTPPort -UseSsl -Credential ($credential)
	}
}

function setRow ($pLevel,$pCall,$pText)
{
	if($pWriteLog -eq 'Y')
	{
		$now = Get-Date -Format "HH:mm"
		$today = Get-Date -Format "yyyy-MM-dd"
		talkme "INSERT INTO [dbo].[HarvestLog]([HarvestDate],[Call],[Message],[Level]) VALUES('$today $now','$pCall','$pText','$pLevel')" White Black
		Invoke-Sqlcmd -ServerInstance $DBServer -Database $DataBase -Username $UserName -Password $Password -Query "INSERT INTO [dbo].[HarvestLog]([HarvestDate],[Call],[Message],[Level]) VALUES('$today $now','$pCall','$pText','$pLevel')"
		
	}
}




cls

talkme "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" DarkGreen Black
talkme "++Init the procces to preload the data in the N2KBackBone++" DarkGreen Black 
talkme "++Launching secure protocols                             ++" DarkGreen Black 
talkme "++Encription level AF+ III                               ++" DarkGreen Black
talkme "++Line is secure                                         ++" DarkGreen Black
talkme "++Starting the comunication                              ++" DarkGreen Black
talkme "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" DarkGreen Black
talkme "                                                           " DarkGreen Black
talkme "Calling to the server toretrive the new envelopes          " White Black
talkme $server White Black 
talkme "Process can take few minutes                               " White Black
talkme "...Even hours...                                           " White Black
talkme  "Please, be patient.                                       " White Black




$url = "/api/Harvesting/FullHarvest"

$webData = Invoke-RestMethod -Method 'Post' -Uri "$server$url"  -Headers $headers -ContentType "application/json"

if($webData.Success="true"){
	$number = $webData.Count
	if($number -gt 0){
		talkme "Envelopes to proccessed: $number                          " White Black
		for ($num = 0 ; $num -lt $number ; $num++){
			$country = $webData.Data[$num].CountryCode
			$version = $webData.Data[$num].VersionId
			$status = $webData.Data[$num].Status
			talkme "$country -  $version : $status" White Black  
		}
		talkme "Work complete                                             " Green Black  
		setRow "Fine" "$server$url" "No issues detected"
	}
	else{
		talkme "                                                           " Black Black 
		talkme "No envelope to process                                     " Yellow Black  
		talkme "                                                           " Black Black
		setRow "Warning" "$server$url" "No new envelopes to process"
		
	}
}
else{
	$errMsg = $webData.Message
	talkme "Error on the call                                          " Red Black
	talkme $errMsg Red Black 
	$now = Get-Date -Format "HH:mm"
	$today = Get-Date -Format "yyyy-MM-dd"
	setRow "Error" "$server$url" "$errMsg"
	
}
talkme "                                                           " Black Black
talkme "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" DarkGreen Black 
talkme "++Comunication finished                                  ++" DarkGreen Black 
talkme "++Disconecting                                           ++" DarkGreen Black 
talkme "++Mind of the day: Emperor protects                      ++"  DarkGreen Black 
talkme "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++"  DarkGreen Black 

