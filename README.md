# 💊 PharmaCare

A full-stack pharmacy consultation platform that connects patients with pharmacists for remote consultations, prescription management, and medication guidance.

---

## Overview

PharmaCare streamlines the patient-pharmacist consultation process. Patients can submit consultation requests describing their symptoms or medication questions, and pharmacists can review, respond, and complete consultations — all through a clean web interface. The system handles the full lifecycle: registration, consultation requests, pharmacist review, and automated email notifications at each step.

---

## Features

- **Patient portal** — register, log in, submit consultation requests, view consultation history
- **Pharmacist dashboard** — review pending consultations, add notes, mark consultations as complete
- **Email notifications** — automated emails on consultation submission, review, and completion
- **Authentication & authorization** — role-based access (Patient / Pharmacist) using ASP.NET Core Identity
- **Dockerized** — runs in containers for consistent local and production environments

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 8), C# |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Frontend | Razor Views, HTML, SCSS, JavaScript |
| Auth | ASP.NET Core Identity |
| Email | SMTP / MailKit |
| Containerization | Docker |

---

## Architecture

The solution follows a clean layered architecture:

```
PharmaCare/
├── PharmaCare.Web/         # Presentation layer — controllers, Razor views, wwwroot
├── PharmaCare.API/         # API layer — REST endpoints
├── PharmaCare.Services/    # Business logic — consultation, patient, pharmacist services
├── PharmaCare.Data/        # Data access — EF Core DbContext, migrations, repositories
└── PharmaCare.sln
```

Each layer depends only on the layer below it, keeping business logic independent of web concerns.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (or Docker)
- [Docker](https://www.docker.com/) (optional)

### 1. Clone the repo

```bash
git clone https://github.com/noorsh/PharmaCare.git
cd PharmaCare
```

### 2. Configure the database

Update the connection string in `PharmaCare.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pharmacare;Username=your_user;Password=your_password"
  }
}
```

### 3. Apply migrations

```bash
dotnet ef database update --project PharmaCare.Data --startup-project PharmaCare.Web
```

### 4. Run the app

```bash
cd PharmaCare.Web
dotnet run
```

Visit `http://localhost:5000`

### Run with Docker

```bash
docker build -t pharmacare .
docker run -p 8080:80 pharmacare
```

---

## Database Schema

The full database schema is available in [`schema.sql`](./schema.sql).

Core entities: `Users`, `Patients`, `Pharmacists`, `Consultations`

---

## Email Notifications

The system sends automated emails for:
- ✅ Consultation submitted
- 🔍 Consultation under review
- ✔️ Consultation completed

Email templates are defined in `PharmaCare.Services` and sent via SMTP (configurable in `appsettings.json`).

---

## Roadmap

- [ ] Real-time chat between patient and pharmacist (SignalR)
- [ ] Prescription upload (PDF/image)
- [ ] Mobile-responsive UI improvements
- [ ] Admin panel for user management

---

## Author

**Noor** — [github.com/noorsh](https://github.com/noorsh)
