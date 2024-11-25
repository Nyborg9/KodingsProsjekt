# Kodingsprosjekt: Kartverket

## Applikasjonens Arkitektur
Applikasjonen er laget med en **Model-View-Controller (MVC)** arkitektur:
- **Models**: Administrerer data og logikk i applikasjonen.
- **Views**: Håndterer brukergrensesnittet (UI).
- **Controller**: Binder sammen models og views og håndterer kommunikasjon mellom dem.
- **Database**: Applikasjonen bruker MariaDB til å lagre data om brukere og kartendringer.

## Kjøre applikasjonen
For å kjøre applikasjonen trenger du:
- Docker Desktop
- Visual Studio (eller lignende IDE)

### Steg 1 - Klon prosjektet
1. Kopier linken til repository på GitHub.
2. Åpne Visual Studio og klikk på **"Clone a repository"**.
3. Lim inn linken og klon prosjektet.

### Steg 2 - Koble til databasen
1. Velg **Build soultion** fra Build menyen øverst i Visual Studio. 
2. Installer en connector til mysql eller mariadb og restart visual studio (i visual studio brukte vi extensionen dotConnect for MySQL & MariaDB som vi fant ved bruk av Extensions/Manage Extentions)
3. Kjør applikasjonen med Docker Compose.
4. Gå til View/Server Explorer i toppen av Visual Studio og høyreklikk på **Data Connections** og trykk på **Add Connection**
5. Velg **MySQLServer** og legg in informasjonen til databasen: (Host = localhost, Port = 3306, User Id = root, Password = 123, Database = geochangesdb)
6. Gå til Package Manager Console og skriv inn kommandoene Drop-Database også Update-Database
7. Kjør applikasjonen med Docker Compose igjen


### Steg 3 - Utforsk applikasjonen
Vanlig bruker:
1. Lag bruker.
2. Utforsk applikasjonen.

Testbruker admin:
- E-post: admin@admin.com
- Passord: Admin123


## Komponenter av applikasjonen
Applikasjonen er bygget med **Model-View-Controller (MVC)**-arkitektur. Her er en oversikt over de viktigste komponentene:

### 1. **Models**
 De håndterer applikasjonens data og forretningslogikk, som for eksempel autentisering av brukere og behandling av innsendte kartforslag. Modellene kommuniserer med databasen for å hente, lagre og oppdatere informasjon.
   
### 2. **Views**
Views er ansvarlige for hvordan data presenteres til brukeren. Dette inkluderer alt fra å vise kartet til å vise oversikter over brukerens innsendte forslag. Visningene er brukergrensesnittet (UI), og de gjør det mulig for brukeren å samhandle med applikasjonen, som å sende inn forslag eller navigere mellom sider. 

### 3. **Controllers**
Når en bruker utfører en handling, som å registrere seg eller sende inn et forslag, mottar controlleren denne inputen og håndterer den på riktig måte. Controlleren kan oppdatere modellen, for eksempel lagre et forslag i databasen.

### 4. **Database**
Databasen i **MariaDB** blir brukt for å lagrer data for applikasjonen. Dette gjelder informasjon om brukere og innsendte kartendringsforslag.

### 5. **Repository**
Github repository blir brukt som lagringsted for applikasjonens kildekode og for å organisere arbeidet. Det vil inneholde all koden, konfigurasjoner og dokumentasjon som er nødvendig for å bygge og kjøre applikasjonen. 

## Test av applikasjon
Det er utført manuell testing etter hver implementasjon av nye funskjoner for å sikre applikasjonens stabilitet og funksjonalitet. I tillegg er det implementert unit tester for å teste controllerne.

For å finne Unit testene så må mann høyreklikke på Solution også Add/Existing Project også navigere til mappen og åpne Webapplication2/Webapplication2.Tests/WebApplication2.Tests.csproj

[Test-dokumentasjon i Wiki](https://github.com/Nyborg9/KodingsProsjekt/wiki/Test-av-applikasjon).


## Funksjonaliteter i applikasjonen
Liste over noen funksjonaliteter i applikasjonen:
- **Brukerregistrering:** Mulighet for å registrere seg som bruker, og logge in.
- **Kartvisning:** Innloggede brukere kan se kart og velge mellom ulike kartvarianter.
- **Sende inn forslag til endriger i Kart:** Innloggede brukere kan markere områder i kart og sende inn forslag til endringer.
- **Oversikt over innsendte forslag:** Oversikt over egne innsendte rapporter. For Admin/Saksbehandler: Oversikt over alle innsendte rapporter. 
- **Redigere og Slette rapport:** Mulighet for å redigere og slette innsendte rapporter.
- **Roller:** Som admin: Endre rollen til en bruker. Vanlig bruker kan promoteres til saksbehandler og saksbehandler kan nedgraderes til vanlig bruker. 
- **Kommune og fylke tilkobling:** Innsendte rapporter blir automatisk tilegnet kommune og fylkes informasjon basert på markeringer i kart. 
- **Slette bruker:** Mulighet for å slette egen bruker. Admin bruker kan ikke slettes og saksbehandlere kan ikke slette andre saksbehandlere. 
- **Status oppdatering:** Mulighet for saksbehandler til å oppdatere status og prioritering for en rapport. 
