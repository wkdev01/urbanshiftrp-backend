# UrbanShiftRP Backend

Backend API for my Roblox roleplay project, UrbanShiftRP.

I started this project while learning C# and .NET backend development. Since I already had experience with Roblox, it was a good way to learn APIs, databases and authentication by building something I could actually use with a game.

Right now, the API handles player accounts, sessions, game events, in-game currency and some admin features.

## What it has

* Roblox user lookup
* Player login with JWT
* Player data stored in PostgreSQL
* Session tracking
* Kill and death events
* In-game currency
* Transaction history
* Player bans
* Ban logs
* Admin player listing
* Basic server statistics
* Swagger

## Tech

* C#
* .NET 8
* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* Npgsql
* JWT
* Swagger
* Docker

## How it works

The game sends requests to the API and the API handles the data and business logic before saving it to PostgreSQL.

```text
Roblox
   |
   v
ASP.NET Core API
   |
   v
PostgreSQL
```

The login also uses the Roblox Users API to get the player's username.

## Main endpoints

### Auth

```http
POST /auth/login
POST /auth/logout
```

### Player

```http
GET /player/{id}
GET /player/{id}/eventos
POST /player/{id}/evento
GET /player/{id}/transacoes
POST /player/{id}/transacao
```

### Admin

```http
GET /admin/players
GET /admin/players/{id}
GET /admin/eventos
GET /admin/stats

POST /admin/ban/{id}
DELETE /admin/ban/{id}
```

## Running

You need:

* .NET 8 SDK
* PostgreSQL or Docker
* Git

Clone the project:

```bash
git clone https://github.com/wkdev01/urbanshiftrp-backend.git
cd urbanshiftrp-backend
```

Copy the example configuration:

```cmd
copy appsettings.example.json appsettings.json
```

Set your database password and JWT secret in `appsettings.json`.

Start PostgreSQL with Docker:

```bash
docker compose up -d
```

Then run the API:

```bash
dotnet restore
dotnet run
```

Swagger will be available at:

```text
http://localhost:5260/swagger
```

## Project status

This is still an ongoing project.

The first version was mainly focused on getting the backend working with Roblox. I'm now using the same project to learn more about backend development and improve the code as I go.
