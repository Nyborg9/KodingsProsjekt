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
- Installere en connector til mysql eller mariadb og restart visual studio (i visual studio brukte vi extensionen dotConnect for MySQL & MariaDB)
- Gå til View/Server Explorer i toppen av Visual Studio og høyreklikk på Data Connections og trykk på Add Connection
- Velg MySQLServer og legg in informasjonen til databasen (Host = localhost, Port = 3306, User Id = root, Password = 123, Database = geochangesdb)

### Steg 1 - Klon prosjektet
1. Kopier linken til repository på GitHub.
2. Åpne Visual Studio og klikk på **"Clone a repository"**.
3. Lim inn linken og klon prosjektet.

### Steg 2 - Koble til databasen
For å koble applikasjonen til databasen, kjør applikasjonen via Docker Compose:
1. Gå til Package Manager Console og skriv inn kommandoene Drop-Database også Update-Database
2. Velg **Build soultion** fra Build menyen øverst i Visual Studio. 
3. Kjør applikasjonen med Docker Compose.

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
Bruker github repository som lagringsted for applikasjonens kildekode og for å organisere arbeidet. Det vil inneholde all koden, konfigurasjoner og dokumentasjon som er nødvendig for å bygge og kjøre applikasjonen. 

## Test av applikasjon
Det er utført manuell testing etter hver implementasjon av nye funskjoner for å sikre applikasjonens stabilitet og funksjonalitet.
[Testing-dokumentasjon i Wiki](https://github.com/Nyborg9/KodingsProsjekt/wiki/Test-av-applikasjon).


## Funksjonaliteter i applikasjonen
Liste over funksjonaliteter i applikasjonen:
- Brukerregistrering: Mulighet for å registrere seg som bruker, og logge in.
- Kartvisning: Innloggede brukere kan se kart, i ulike kartvarianter.
- Sende inn forslag til endriger i Kart: Innloggede brukere kan markere områder i kart og sende inn forslag til endringer i kart.
- Oversikt over innsendte forslag: En oversikt over alle innsendte forslag, samt en spesifikk oversikt over forslagene sendt inn av hver bruker. 
- Redigere/Slette rapport: Mulighet for å redigere og slette innsendte rapporter.
- Promotere: Som admin: Endre rollen til en bruker. Vanlig bruker kan promoteres til saksbehandler og saksbehandler kan nedgraderes til vanlig bruker. 
- Kommune og fylke: Innsendte rapporter blir automatisk tilegnet kommune og fylke informasjon basert på markeringer i kart. 
- Slette bruker: Mulighet for å slette bruker. Admin bruker kan ikke fjernes. 
- Status oppdatering: Mulighet for saksbehandler til å oppdatere status og prioritering for en rapport. 