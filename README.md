# Social Media API

A RESTful backend API for a social media platform built with **ASP.NET Core** and **.NET 10**.

Currently an API-first project with plans to evolve into a full-stack web application with a modern SPA frontend.

## ✨ Current Features

- **User Authentication** - Registration, login, email confirmation
- **User Profiles** - User management and profile endpoints
- **Posts & Comments** - Full CRUD operations
- **Social Interactions** - Follow system, likes on posts and comments
- **Media Management** - Secure image handling

## 🛠️ Tech Stack

- **.NET 10** / **C#**
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **MSSQL Server** - Database

## 📁 Project Structure

```
SocialMedia/
├── SocialMedia.App      # API Controllers & Configuration
├── SocialMedia.Data     # Entity Framework & Database
├── SocialMedia.Domain   # Domain Models & Entities
└── SocialMedia.Logic    # Business Logic & Services
```

## 🚀 Quick Start

```bash
# Clone the repository
git clone https://github.com/TamasGyarmati/social-media-api.git
cd social-media-api

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

## 🔮 Planned Features

### Phase 1 (In Progress)
- **Real-time Notifications** - SignalR integration for live updates
- **MIME-based Validation** - Proper file type validation
- **Global Error & Validation Handling** - Centralized exception management
- **Fire-and-Forget Middleware** - Hangfire + ImageSharp for async image processing
- **Docker Compose Support** - Full containerization setup

### Phase 2 - Full-Stack Integration
- Angular SPA frontend
- CI/CD pipeline
- Cloud deployment ready

## 📊 Architecture

The project follows a **layered architecture**:

- **Controllers** → **Services** → **Repositories** → **Database**

This ensures clean separation of concerns and maintainability.

---

**Status:** Early-stage API development | **Target:** Full-featured social media platform
