Param(
    [string] [Parameter(Mandatory=$false)] $FunctionAppPackageFilePath,
	[string] [Parameter(Mandatory=$false)] $SitePackageFilePath
)

# Login and set Azure subscription context
Import-Module AzureRm
$subscriptionId = 'b4b7d4c1-8c25-4da3-bf1c-e50f647a8130'
if ([string]::IsNullOrEmpty($(Get-AzureRmContext).Account))
{
    Login-AzureRmAccount -SubscriptionId $subscriptionId
}
else 
{
    Set-AzureRmContext -SubscriptionId $subscriptionId
}

# Get managed app deployment storage account
$resourceGroup = 'SmartSignalsDev'
$storageAccountName = 'globalsmartsignals'
$containerName = 'managedapp'
$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName

# Generate a zip file containing the managed app resources template
# Service catalog expects the package name to be app.zip
$packageName = "app.zip"
$templateFilePath = [System.IO.Path]::Combine($PSScriptRoot, "mainTemplate.json")
$uiDefinitionFilePath = [System.IO.Path]::Combine($PSScriptRoot, "createUiDefinition.json")
$packagePath = [System.IO.Path]::Combine($PSScriptRoot, $packageName)
Compress-Archive -Path $templateFilePath, $uiDefinitionFilePath -DestinationPath $packagePath -Force

# Upload the package 
$blob = Set-AzureStorageBlobContent -File $packagePath -Container $containerName -Blob $packageName -Context $storageAccount.Context -Force

# Upload new bits of the function app if provided
if ($FunctionAppPackageFilePath)
{
	$functionAppBlobName = 'smartsignals.zip'
	$functionAppPackageBlob = Set-AzureStorageBlobContent -File $FunctionAppPackageFilePath -Container $containerName -Blob $functionAppBlobName -Context $storageAccount.Context -Force
}

# Upload new bits of the management site if provided
if ($SitePackageFilePath)
{
	$siteBlobName = 'smartSignalSite.zip'
	$functionAppPackageBlob = Set-AzureStorageBlobContent -File $SitePackageFilePath -Container $containerName -Blob $siteBlobName -Context $storageAccount.Context -Force
}

# Create the managed app definition
$nettaDirectGroupId = (Get-AzureRmADGroup -SearchString "netta direct").Id.ToString()
$ownerID = (Get-AzureRmRoleDefinition -Name Owner).Id

Set-AzureRmManagedApplicationDefinition `
  -Id "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Solutions/applicationDefinitions/SmartDetectorsMonitoringAppliance" `
  -DisplayName "Smart Detectors Monitoring Appliance" `
  -Description "This application will execute and manage the Smart Detectors" `
  -Authorization "${nettaDirectGroupId}:$ownerID" `
  -PackageFileUri $blob.ICloudBlob.StorageUri.PrimaryUri.AbsoluteUri

# Delete the package
Remove-Item -Path $packagePath -Force