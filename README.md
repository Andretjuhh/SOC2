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

## Standaardaccounts seeden

Bij het starten in Development controleert de backend automatisch de drie testaccounts.
Ontbrekende accounts worden aangemaakt. Het vaste wachtwoord staat in `AccountSeeder.cs`.
Voer vanuit `backend/coreApi/` uit nadat je je eigen databaseverbinding hebt ingesteld.

```powershell
dotnet ef database update -- --environment Development
dotnet run
```

De seeder maakt de rollen `Klant`, `SOC-analist` en `Beheerder` en de accounts
`klant`, `analist` en `beheerder`. Ze zijn gekoppeld aan dezelfde actieve ontwikkelklant.
Alle drie gebruiken het wachtwoord uit `TestWachtwoord` in `AccountSeeder.cs`.
Bij een wijziging van deze constante werkt de volgende Development-start ook bestaande seedaccounts bij.
De klant krijgt `tickets.lezen`; de analist en beheerder krijgen ook `tickets.schrijven`.
Voor een testsessie zonder wachtwoord en MFA gebruik je in Development via Swagger
`POST /api/dev/login/klant`, `POST /api/dev/login/analist` of `POST /api/dev/login/beheerder`.
Deze route werkt alleen voor de actieve, niet-geblokkeerde seedaccounts van de ontwikkelklant.
Open daarna de frontend op `/overview`. Voor de normale login via `/login`
loggen de drie seedaccounts in Development direct in met hun wachtwoord, zonder MFA.
Andere accounts en omgevingen gebruiken de normale MFA-stap. Koppel die accounts
eenmalig via `/mfa-setup` aan je eigen authenticator in de ontwikkelomgeving.

Opnieuw uitvoeren maakt geen duplicaten en bewaart MFA en accountstatus.
Alleen het wachtwoord van de drie seedaccounts wordt zo nodig gelijkgezet aan `TestWachtwoord`.
Een bestaande accountnaam met een andere rol of klant geeft een fout.
Bij bestaande standaardrollen vult de seeder ontbrekende ticketrechten aan en behoudt hij extra rechten.
Met `dotnet run -- --seed` kun je ook alleen seeden en daarna stoppen.
De SQL Server-uitvoering gebruikt een serialiseerbare transactie. Bij een fout worden de wijzigingen teruggedraaid.
De automatische seeder draait alleen in Development.

De rolcomments beschrijven de bedoelde toegang. De huidige alertcode controleert alleen actieve
klanttoegang. De beheerder heeft voorlopig de werkende ticketrechten voor de ontwikkelklant;
globaal systeembeheer en verdere rollencontroles worden bij de betreffende functies uitgewerkt.

## Testaccount maken

Open Swagger en stuur naar `POST /api/gebruikers`:

```json
{
  "naam": "Testgebruiker",
  "gebruikersnaam": "testanalist",
  "email": "testanalist@example.test",
  "wachtwoord": "dit is mijn testwachtwoord",
  "isActief": true,
  "klantId": null
}
```

Een nieuwe gebruiker geeft HTTP 201. Identity hasht het wachtwoord; de hash komt niet in de response.
Nieuwe wachtwoorden moeten minimaal 15 tekens hebben, zonder verplichte hoofdletters, cijfers of symbolen.
De huidige request accepteert maximaal 1024 tekens voor het wachtwoord.

Nieuwe accounts krijgen altijd `Geen toegang`, zonder rechten.
Een beheerder kan via `/gebruikers` een rol en klant toekennen.
De actuele rollen, rechten en API-controles staan in [de rechtenmatrix](docs/rechtenmatrix.md).
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
6. De drie seedaccounts krijgen in Development na een geldig wachtwoord direct een sessie.
   Andere accounts gaan naar `/mfa`, waar de authenticatorcode wordt gecontroleerd voordat een sessie wordt gemaakt.
7. Het dashboard controleert `/api/auth/me`. Een nieuw account zonder toegekende rechten ziet een wachtmelding.

De MFA-routes zijn aangesloten. Voor een nieuw account koppel je de authenticator in Development via `/mfa-setup`.
Een API-herstart verwijdert de tijdelijke challenges.
De bestaande routes `GET /api/auth/me` en `POST /api/auth/logout` vereisen een ingelogde sessie.
`/me` geeft de rol, rechten en klanttoegang terug en meldt ontbrekende actieve klanttoegang.
De backend controleert rechten en klanttoegang voor alerts en tickets.
Beheerders kunnen via `/gebruikers` standaardrollen en klanten toekennen.

## Controleren

Voer vanuit de repositorymap uit:

```powershell
dotnet run --project backend\coreApi\coreApi.csproj

dotnet test backend/coreApi.Tests/coreApi.Tests.csproj

npm.cmd --prefix frontend run build
npm.cmd --prefix frontend run dev

```

De backend is beschikbaar op `http://localhost:5294`. Open Swagger handmatig via
`http://localhost:5294/swagger`; de browser wordt niet automatisch geopend.

De backendtests gebruiken een aparte EF InMemory-testdatabase en hebben geen lokale SQL Server nodig.
Test handmatig ook een geldig account, een fout wachtwoord, een inactief account en een onbereikbare backend.

BACKEND STARTEN
dotnet run --project backend/coreApi

GUIDO ACOUNT:
"naam": "Guido",
"wachtwoord": "123456789ABCDEFG",
