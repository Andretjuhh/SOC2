# Rollen en rechten

Nieuwe accounts krijgen automatisch de rol **Geen toegang**, zonder rechten.
Zij kunnen inloggen. Het dashboard toont een wachtmelding totdat een beheerder toegang toekent.
De bestaande Development-seedaccounts krijgen expliciet hun testrollen.

| Actie | Geen toegang | Klant | SOC-analist | Beheerder |
| --- | --- | --- | --- | --- |
| Inloggen | Ja | Ja | Ja | Ja |
| Dashboard | Nee | Eigen actieve klant | Toegewezen actieve klant | Alle klanten |
| Alerts bekijken | Nee | Eigen actieve klant | Toegewezen actieve klant | Alle klanten |
| Alerts aanmaken en claimen | Nee | Nee | Toegewezen actieve klant | Alle klanten |
| Tickets en historie bekijken | Nee | Eigen actieve klant | Toegewezen actieve klant | Alle klanten |
| Tickets aanmaken, wijzigen en afronden | Nee | Nee | Toegewezen actieve klant | Alle klanten |
| Gebruikers bekijken en rollen toekennen | Nee | Nee | Nee | Ja |

Een gebruiker heeft momenteel één klantkoppeling. De beheerder kan klantoverschrijdend lezen,
wijzigen en claimen. Bij het aanmaken van een alert of ticket kan de beheerder via `klantId`
een actieve doelklant opgeven. Zonder dit veld gebruikt de API de eigen klantkoppeling.

De centrale definitie staat in `backend/coreApi/Domain/RechtenMatrix.cs`.
De standaardrollen krijgen tijdens Development-start precies de rechten uit deze matrix.
Dat kan bestaande rechtenkoppelingen van deze vier standaardrollen aanpassen.
Andere rolrecords worden door de seeder niet aangepast.

## Toegang toekennen

1. Log in als `beheerder` en open `/gebruikers` via Gebruikersbeheer.
2. Kies bij de gebruiker een standaardrol en een klant.
3. Voor Klant en SOC-analist is een actieve klant verplicht.
4. Sla op. De gebruiker kan op de wachtpagina op **Opnieuw controleren** klikken.

De drie Development-seedaccounts houden hun vaste rol en testklant.
Gebruik andere accounts om rolwijzigingen en toegang intrekken te testen.

Alle beheerroutes controleren de ingelogde beheerder in de backend.

- `GET /api/gebruikers` toont accounts zonder wachtwoordhashes of MFA-geheimen.
- `GET /api/rollen` toont de standaardrollen.
- `GET /api/klanten` toont actieve klanten voor de toewijzing.
- `PUT /api/gebruikers/{id}/rol` kent `{ "rolId": "UUID", "klantId": "UUID of null" }` toe.

Het ontwikkelendpoint `POST /api/gebruikers` accepteert naam, gebruikersnaam, e-mail,
wachtwoord en optioneel actieve status en klant-ID. Een meegegeven rol of rechtenlijst
geeft geen privileges. Roltoekenning verloopt via de beveiligde beheerroute.
