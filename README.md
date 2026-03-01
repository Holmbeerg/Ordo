# Ordo
Ordo is a web-based Scrabble game built around the idea of timed competitive 
play. Inspired by chess time formats, players choose a time control (blitz, 
rapid, etc.) before each game.

## Background
I enjoy playing Wordfeud but found that no good online Scrabble platform 
supports Swedish. I'm developing Ordo as a fun side project to expand my 
knowledge in C#, .NET, Azure and modern web development.

## Tech Stack

**Backend:** ASP.NET Core (.NET 10), Azure SignalR, Redis  
**Frontend:** Vue 3, TypeScript, Pinia, Vue I18n, Tailwind CSS  
**Infrastructure:** Azure App Service, GitHub Actions (CI/CD), Docker

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
