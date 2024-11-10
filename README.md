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
For å koble applikasjonen til databasen, kjør applikasjonen via Docker Compose:
1. Finn **dropdown-menyen** øverst i Visual Studio, ved siden av "Any CPU".
2. Velg **docker-compose** fra listen.
3. Trykk på **Start** (play-ikonet) for å kjøre prosjektet med Docker Compose.

## Komponenter av applikasjonen
- Beskrivelse av applikasjonens hovedkomponenter.

## Funksjonaliteter i applikasjonen
Liste over funksjonaliteter i applikasjonen:
- Kartvisning og redigering
