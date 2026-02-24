# Ordo
Ordo is a web-based Scrabble game built around the idea of timed competitive 
play. Inspired by chess time formats, players choose a time control (blitz, 
rapid, etc.) before each game.

## Background
I enjoy playing Wordfeud but found that no good online Scrabble platform 
supports Swedish. I'm developing Ordo as a fun side project to expand my 
knowledge in .NET and Azure, with the added twist of chess-style time controls.

## Tech Stack

**Backend:** ASP.NET Core (.NET 9), Azure SignalR, Redis  
**Frontend:** Vue 3, TypeScript  
**Infrastructure:** Azure App Service, GitHub Actions (CI/CD), Docker

## Features

- Real-time multiplayer via SignalR
- Redis-backed matchmaking and game state management
- Chess-style time controls (blitz, rapid etc)
- Full tile placement and move validation logic
- Word validation against dictionary
- Automated CI/CD pipeline deploying to Azure on every push to main

## Status

Active development.

## Roadmap

- [ ] Frontend game board with full gameplay UI
- [ ] Time control integration in SignalR hub
- [ ] User accounts and authentication
- [ ] English language support
- [ ] Additional language support
