# OpenWeather

Harus `OpenWeather` on lisatud Current Weather Data API. Aluseks on main-haru ChuckNorris ja Cocktail näited.

## Käivitamine

Vaja on .NET 10 SDK-d ja projekti olemasolevat SQL Server LocalDB seadistust (vt START_AND_DATABASE_FIX.md).
Loo OpenWeather kontol API võti. Käivita repositooriumi juurkaustas (asenda näidis oma võtmega):

```powershell
dotnet user-secrets set "OpenWeather:ApiKey" "SINU_API_VOTI" --project TARge25Shop/TARge25Shop/Targe25Shop.csproj
dotnet run --project TARge25Shop/TARge25Shop/Targe25Shop.csproj
```

User Secrets laaditakse Development keskkonnas. Serveris kasuta keskkonnamuutujat `OpenWeather__ApiKey`.
Ära lisa tegelikku võtit appsettings.json faili, lähtekoodi ega GitHubi. Võtit ei saadeta brauserisse.
OpenWeather HttpClienti päringute logimine on välja lülitatud, sest URL sisaldab võtit.

Ava menüüst OpenWeather või `/OpenWeather`. Sisesta näiteks `Tallinn` või `Tartu,EE` ja vajuta „Vaata ilma”.
Võtme puudumisel kuvatakse veateade. Uue võtme aktiveerimine võib võtta aega.

## Kuidas töötab

1. View saadab linna nime GET-päringuga OpenWeatherControllerile.
2. Controller kutsub IOpenWeatherServices liidese kaudu OpenWeatherServices teenust.
3. Service loeb võtme seadistusest ja teeb HTTPS-päringu `/data/2.5/weather?q=...&appid=...&units=metric&lang=et`.
4. System.Text.Json muudab vastuse OpenWeatherRootDto objektiks. JsonPropertyName seob feels_like välja FeelsLike omadusega.
5. Controller lisab vastuse ViewModelisse ja View kuvab linna, ilmastiku kirjelduse, temperatuuri, tajutava temperatuuri, õhuniiskuse, õhurõhu ja tuule kiiruse.

Käsitletakse puuduvat/vigast võtit, tundmatut linna, päringulimiiti, võrguviga, aegumist ja vigast JSON-vastust.
Andmebaasi skeemi ei muudeta. Ilmateenus ise andmebaasi ei kasuta.

Kasutatakse ülesandes antud linnapõhist päringut. OpenWeather märgib sisseehitatud linnaotsingu aegunuks, kuid see on endiselt kasutatav.
Dokumentatsioon: https://openweathermap.org/current

## Kontrollimine

```powershell
dotnet build TARge25Shop/Targe25Shop.slnx
dotnet test TARge25Shop/Targe25Shop.slnx
```

OpenWeatherTests kontrollib DTO teisendust, linna kodeerimist, veateateid, puuduva võtme ja tühja linna käsitlemist ning vigast vastust. Testid kasutavad näidisvastust ega vaja päris API võtit.
Päris ilmaandmete kuvamiseks tuleb lisada enda aktiveeritud võti.
