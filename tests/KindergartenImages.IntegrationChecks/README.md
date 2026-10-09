# Kindergarten piltide integratsioonikontrollid

Vajalikud on .NET 10 SDK ja SQL Server LocalDB. Käivita käsud repositooriumi juurkaustas:

```powershell
dotnet build TARge25Shop/Targe25Shop.slnx
dotnet run --project tests/KindergartenImages.IntegrationChecks/KindergartenImages.IntegrationChecks.csproj
```

Kontrollid loovad eraldi juhusliku nimega andmebaasi, käivitavad veebirakenduse ja
kustutavad testandmebaasi lõpetamisel. Rakenduse TARge25Shop andmebaasi ei muudeta.

Kontrollitakse migratsiooni olemasolevate andmetega, mitme pildi lisamist,
andmebaasis salvestatud baite, galerii kuvamist, serveri taaskäivitamist,
valideerimist, ühe pildi kustutamist, ankeedi kaskaadkustutust ning Spaceship CRUD-i.
