# SOC2

SOC-platform met een ASP.NET Core 10 Minimal API en een React/Vite-frontend.

- `backend/coreApi/`: API, Identity, entiteiten en EF Core-migraties.
- `backend/coreApi.Tests/`: tests voor login en het ophalen van de huidige gebruiker.
- `frontend/`: React-app met het inlogscherm op `/login` en de bestaande demo-dashboardpagina's.

## Benodigdheden

- .NET 10 SDK en Node.js/npm.
- Microsoft SQL Server: lokaal, via LocalDB of in een container.
- De EF Core CLI. Controleer met `dotnet ef --version`. Installeer deze indien nodig:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

SSMS is alleen een beheerprogramma; er moet ook een bereikbare SQL Server zijn.
De backend gebruikt de SQL Server-provider en kan niet rechtstreeks op MySQL aansluiten.

## Eigen database instellen

**Ieder groepslid stelt een eigen connection string in en gebruikt een eigen ontwikkeldatabase.**
Je haalt de code en migraties uit Git, maar je haalt geen databaseserver, accounts of testgegevens op.
Iedereen mag de database `SOC2Db` noemen als die op een eigen server/container draait.

De standaardverbinding in `appsettings.json` gebruikt `(localdb)\MSSQLLocalDB` met database `SOC2Db`.
Dit werkt alleen als SQL Server LocalDB geinstalleerd is.

Gebruik je een andere SQL Server, stel de verbinding dan lokaal in via User Secrets.
Voer vanuit de repositorymap uit:

```powershell
cd backend\coreApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SOC2Db;User Id=JOUW_GEBRUIKER;Password=JOUW_WACHTWOORD;Encrypt=True;TrustServerCertificate=True"
```

Vervang server, poort, gebruikersnaam en wachtwoord door je eigen gegevens.
Bij een container gebruik je de poort die op je computer gepubliceerd is; dat kan bijvoorbeeld 1434 zijn.
`TrustServerCertificate=True` is hier bedoeld voor de lokale ontwikkelomgeving.
User Secrets worden in Development geladen en staan buiten de repository. Commit geen echte wachtwoorden.
Het gedeelde `UserSecretsId` in de projectfile bevat zelf geen inloggegevens.

Als alternatief kun je lokaal de omgevingsvariabele `ConnectionStrings__DefaultConnection` instellen.
Die heeft voorrang op User Secrets en `appsettings.json`.

## Database aanmaken en bijwerken

Voer vanuit `backend/coreApi/` uit:

```powershell
dotnet ef database update -- --environment Development
```

Dit maakt de ingestelde database en tabellen aan als ze nog niet bestaan en past ontbrekende migraties toe.
Controleer dus dat je connection string naar je eigen ontwikkeldatabase wijst.

Na een pull met nieuwe migraties voer je hetzelfde commando opnieuw uit.
Migraties en de model snapshot horen in Git. Maak bij het ophalen van code geen nieuwe initial migration aan.
Stem nieuwe modelwijzigingen en migraties af met de groep.

## Backend starten

Voer vanuit `backend/coreApi/` uit:

```powershell
dotnet run
```

Met het meegeleverde startprofiel draait de API in Development op `http://localhost:5294`.
Swagger staat op `http://localhost:5294/swagger`; de healthcheck staat op `/health`.
Herstart de backend na het wijzigen van je connection string.

## Testaccount maken

Open Swagger en stuur naar `POST /api/gebruikers`:

```json
{
  "naam": "Testgebruiker",
  "gebruikersnaam": "testanalist",
  "email": "testanalist@example.test",
  "wachtwoord": "dit is mijn testwachtwoord",
  "isActief": true,
  "klantId": null,
  "rol": "Testanalist",
  "rechten": []
}
```

Een nieuwe gebruiker geeft HTTP 201. Identity hasht het wachtwoord; de hash komt niet in de response.
Nieuwe wachtwoorden moeten minimaal 15 tekens hebben, zonder verplichte hoofdletters, cijfers of symbolen.
De huidige request accepteert maximaal 1024 tekens voor het wachtwoord.

De handler zoekt de rol op en maakt die aan als hij ontbreekt.
Bestaat de rol al, dan moeten de opgegeven rechten overeenkomen.
Een klantgebruiker krijgt het ID van een bestaande klant; voor een interne analist mag `klantId` null zijn.
Klanten kunnen in Development via `POST /api/klanten` aangemaakt worden.
De aanmaakroutes voor gebruikers en klanten zijn alleen in Development geregistreerd.
Dit is lokale testvoorziening, geen openbare registratie.

## Frontend starten

Open een tweede terminal in de repositorymap:

```powershell
npm.cmd --prefix frontend ci
npm.cmd --prefix frontend run dev
```

Open de URL die Vite toont, gevolgd door `/login`, meestal `http://localhost:5173/login`.
Het inlogformulier heeft geen eigen CSS. De bestaande globale stijlen blijven gelden.
Vite stuurt `/api`-verzoeken door naar `http://localhost:5294`.
Gebruik je een andere backendpoort, wijzig dan de proxy in `frontend/vite.config.js` en herstart Vite.

## Huidige inlogflow (US-01)

1. React stuurt gebruikersnaam en wachtwoord naar `POST /api/auth/login`.
2. De API controleert de invoer en zoekt het account via Identity op.
3. Onbekende, inactieve of geblokkeerde accounts en verkeerde wachtwoorden krijgen dezelfde HTTP 401-melding.
4. Identity blokkeert het account na vijf foute wachtwoordpogingen gedurende vijf minuten.
   Daarnaast begrenst de API loginverzoeken tot tien per minuut per IP-adres (HTTP 429).
5. Bij een geldig wachtwoord geeft de API HTTP 202 met `mfaVereist`, `challengeId` en `verlooptOp`.
   De challenge is vijf minuten geldig en wordt tijdelijk in het geheugen van dit API-proces bewaard.
6. React toont dat extra verificatie nodig is. Er wordt op dit moment nog geen ingelogde sessie gemaakt.

**MFA-verificatie en het activeren van de sessie moeten nog op US-02 aangesloten worden.**
Een API-herstart verwijdert de tijdelijke challenges.
De bestaande routes `GET /api/auth/me` en `POST /api/auth/logout` vereisen een ingelogde sessie.
`/me` geeft de rol, rechten en klanttoegang terug en meldt ontbrekende actieve klanttoegang.
Het demo-dashboard is nog rechtstreeks bereikbaar; de React-routebeveiliging en volledige koppeling met
RBAC/tenantafscherming uit US-03 en US-05 zijn nog niet afgerond.

## Controleren

Voer vanuit de repositorymap uit:

```powershell
dotnet test backend/coreApi.Tests/coreApi.Tests.csproj
npm.cmd --prefix frontend run build
npm.cmd --prefix frontend run lint
```

De backendtests gebruiken een aparte EF InMemory-testdatabase en hebben geen lokale SQL Server nodig.
Test handmatig ook een geldig account, een fout wachtwoord, een inactief account en een onbereikbare backend.
