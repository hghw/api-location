@echo ON
cd HP_Learning.Web

dotnet publish -c Release /p:PublishProfile=Production /p:Username=Administrator /p:Password=vietgis@3404@2021 /p:AllowUntrustedCertificate=True