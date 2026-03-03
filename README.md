# Ordo
Ordo is a web-based multiplayer word game built around the idea of timed competitive play. Inspired by chess time formats, players choose a time control (blitz, rapid, etc.) before each game.

## Background
I enjoy playing word games but found that no good online platform supports Swedish. I'm developing Ordo as a fun side project to solve that problem, 
while concurrently using it as a sandbox to explore distributed systems design, horizontal scalability, and modern CI/CD workflows in .NET and Azure.

## Tech Stack

**Backend:** ASP.NET Core (.NET 10), Azure SignalR, Redis  
**Frontend:** Vue 3, TypeScript, Pinia, Vue I18n, Tailwind CSS  
**Testing & Infrastructure:** xUnit, Azure App Service, GitHub Actions (CI/CD), Docker

## Features

- Real-time multiplayer via SignalR
- Redis-backed matchmaking and game state management
- Chess-style time controls (blitz, rapid etc)
- Full tile placement and move validation logic
- Word validation against dictionary
- Interactive game board with drag and drop tile placement
- Automated CI/CD pipeline deploying to Azure on every push to main

## Status

Active development.

## Roadmap
- [x] Frontend game board with full gameplay UI
- [ ] Timer / time control integration (backend + frontend)
- [ ] i18n — translate all hardcoded UI strings
- [ ] Private games — invite a friend via shareable link
- [ ] English language support

## Credits
**Swedish dictionary:** [DSSO (Den stora svenska ordlistan)](https://extensions.libreoffice.org/en/extensions/show/swedish-spelling-dictionary-den-stora-svenska-ordlistan) by Göran Andersson. 
* **Modifications:** The original wordlist was processed using `unmunch` to expand all inflected forms, then filtered to include only lowercase Swedish letters (a-z, å, ä, ö) with a minimum length of 2 characters to fit the constraints of the game.
* **License:** Licensed under the [GNU LGPL v3](https://www.gnu.org/licenses/lgpl-3.0.html). A copy of the license (`LICENSE_en_US.txt`) is included in the `Words` directory.
