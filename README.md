# Jobsity Financial Chat – Full Challenge Implementation

This repository contains the full implementation of the **Jobsity Financial Chat Challenge**, including the back-end API, message bot worker, front-end client, messaging infrastructure, and a complete Docker-based deployment setup.

The solution was built with a strong emphasis on reliability, clean architecture, and production‑ready engineering practices.

---

## Project Overview

The project delivers a real-time financial chat application where authenticated users can:

* Join chat rooms
* Exchange messages
* Trigger a stock command (`/stock=XXXX`)
* Receive responses asynchronously from a background bot

The system is composed of three main services:

1. **FinancialChat API** – authentication, chat rooms, messages, stock command dispatch
2. **FinancialChat Stock Bot Worker** – RabbitMQ consumer, stock quote provider
3. **Web Client (Vite + React)** – user-facing SPA served by Nginx

All services are orchestrated via **Docker Compose**, including:

* MySQL database
* RabbitMQ message broker
* API container
* Worker container
* Web container

This architecture ensures full isolation, portability, and ease of execution for evaluators.

---

## Challenge Requirements (from the original assignment)

The following requirements from the challenge specification were implemented:

* User **registration and login**
* Persistent chat messages with timestamp
* Display the **last 50 messages** when opening a room
* `/stock=XXXX` command parsing
* Bot worker fetching data from an external API
* Messages delivered asynchronously
* Support for chat rooms
* Messages ordered by timestamp
* Graceful error handling
* Proper architectural separation between components

The original specification is provided in the challenge document.

---

## Features Implemented

### Authentication & Identity

* .NET Identity Core implementation
* JWT-based authentication
* Configurable password policy per environment

### Chat Functionality

* Multiple chat rooms
* Persistence using MySQL
* Last 50 messages per room when entering a room
* Messages ordered by timestamp
* User-friendly interaction flow simulating real-time chat behavior

### Stock Bot (Worker Service)

* Dedicated .NET 9 Worker Service
* RabbitMQ queue consumer for stock commands
* Integration with an external stock provider API
* Robust `/stock=XXXX` command parsing
* Error handling for invalid tickers or provider failures

### Infrastructure & Deployment

* Full Docker Compose environment
* Automatic EF Core migrations and database seeding on API startup
* Retry logic for MySQL readiness before applying migrations
* Clear network configuration and service discovery via Docker networks
* Static front-end hosting through Nginx serving the Vite build output

---

## Architecture Summary

The solution follows a **Clean Architecture** style with segregated projects:

* **Domain** – Core entities and domain models
* **Application** – Use cases, business logic, DTOs, and service contracts
* **Infrastructure** – EF Core, MySQL, RabbitMQ, Identity, repositories, and configuration
* **API** – ASP.NET Core Web API layer (authentication, chat endpoints)
* **Bot Worker** – Background service responsible for processing stock commands
* **Web Client** – Vite + React single-page application, hosted by Nginx in its container

This architecture mirrors common enterprise patterns and is designed to be maintainable and extensible.

---

## Tech Stack

### Backend

* **.NET 9**
* ASP.NET Core Web API
* **Entity Framework Core**
* **MySQL 8**
* **RabbitMQ** as message broker

### Frontend

* **Vite + React**
* Static hosting with **Nginx** inside the front-end container

### Deployment & Tooling

* **Docker** and **Docker Compose**
* EF Core migrations and seeding on startup
* xUnit for automated tests

---

## Running the Project (Docker Compose)

Clone the repository:

```bash
git clone https://github.com//IngDavNav/Jobsity.FinancialChat.git
cd Jobsity.FinancialChat
```

Build and start the full stack:

```bash
docker compose up --build
```

Once the containers are running, the services will be available at:

| Service     | URL                                              |
| ----------- | ------------------------------------------------ |
| Web Client  | [http://localhost:3000](http://localhost:3000)   |
| API         | [http://localhost:5000](http://localhost:5000)   |
| RabbitMQ UI | [http://localhost:15672](http://localhost:15672) |
| MySQL       | localhost:3307 (host)                            |

Default RabbitMQ credentials:

```text
guest / guest
```

---

## Environment Configuration

Key environment variables are passed via `docker-compose.yml`.

### API

* `ConnectionStrings__Default` – MySQL connection string
* `Jwt__Key` – Signing key for JWT tokens
* `Jwt__Issuer` – JWT issuer
* `Jwt__Audience` – JWT audience
* `RabbitMq__HostName` – RabbitMQ host name
* `RabbitMq__UserName` – RabbitMQ user
* `RabbitMq__Password` – RabbitMQ password

### Web

* `VITE_API_BASE_URL` – Base URL of the API for local/frontend calls

---

## Database Migrations & Seeding

On API startup, the following steps occur automatically:

1. The API waits for MySQL to become available using a retry loop.
2. EF Core applies any pending migrations to the configured database.
3. Initial seed data is inserted:

   * Default chat room: **General**
   * Default bot user: **ChatBot**

This process ensures that a fresh environment is always in a consistent state and ready to be tested.

---

## Supported Commands

Within any chat room, the following command is supported:

```text
/stock=APPL
```

When a user sends this command:

1. The API publishes a message to RabbitMQ with the requested stock symbol.
2. The Worker Service consumes the message, calls the stock provider API, and prepares a response.
3. The bot response is persisted in the database and made available to the chat room.

Errors (invalid symbols, provider failures, etc.) are handled gracefully and reported back in a user-friendly message.

---

## Testing

The solution includes automated tests focused on:

* Application use cases
* Validation logic
* Message/command parsing
* Basic integration logic for the stock command workflow

Tests are written using **xUnit** and follow a clear project segregation.

---

## Engineering Decisions & Trade-offs

### Docker Compose instead of an Installer

Although the original challenge mentions the possibility of an installer as a bonus, a traditional desktop-style installer is not a natural fit for a web-based, multi-service solution. Instead, this project uses **Docker Compose** to:

* Provide a reproducible, self-contained environment
* Remove friction for evaluators (no manual installation of DB, RabbitMQ, SDKs, etc.)
* Reflect how such a system would typically be deployed in a real-world setting

### Separate Bot Worker Service

The stock bot is hosted in a dedicated Worker Service instead of being embedded into the API:

* Avoids blocking API threads during external API calls
* Makes the system more scalable, as the worker can be scaled independently
* Aligns with a message‑driven, microservice‑friendly architecture

### EF Core Migrations & Startup Seeding

* Ensures reliable schema evolution over time
* Eliminates manual SQL setup for evaluators
* Guarantees that a default room and bot user always exist

---

## Author

**David Navarro**
Software Engineer – Backend & Cloud Architecture
LinkedIn: https://www.linkedin.com/in/david-navarro-web-developer/

---

## License

MIT License
Copyright ©
