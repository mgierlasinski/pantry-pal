# PantryPal

[![Build Status](https://github.com/yourusername/PantryPal/actions/workflows/ci.yml/badge.svg)](https://github.com/yourusername/PantryPal/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## Table of Contents

1. [Project Description](#project-description)  
2. [Tech Stack](#tech-stack)  
3. [Getting Started](#getting-started)  
4. [Available Scripts](#available-scripts)  
5. [Project Scope](#project-scope)  
6. [Project Status](#project-status)  
7. [License](#license)  

## Project Description

PantryPal is a cross-platform mobile application that simplifies meal planning by leveraging the ingredients you already have at home. With PantryPal, you can manage your virtual pantry, define dietary preferences, and generate AI-powered recipes tailored to your available ingredients and tastes.

Key features include:

- **Pantry Management**: Add, edit, delete, and favorite ingredients with free-text input.  
- **AI Recipe Generation**: One-tap recipe creation using your pantry contents, core ingredients, and preferences; suggests up to three missing items.  
- **Recipe Interaction**: Accept and save recipes, or reject them if AI did not fit your taste.  
- **Saved Recipes**: Chronologically view and delete your saved recipes.  
- **Authentication**: Secure email/password sign-up and login backed by Supabase.  
- **Profile & Preferences**: Set diet type, preferred cuisines, and disliked ingredients; profile completion prompts ensure personalized results.  

## Tech Stack

- **Language & Runtime**: .NET 9  
- **Mobile**: .NET MAUI (Android, iOS) with XAML  
- **Backend**: ASP.NET Core Minimal API  
- **Database & Auth**: Supabase (PostgreSQL, SDK, Auth)  
- **AI Integration**: Openrouter.ai (OpenAI, Anthropic, Google models)  
- **Testing**: xUnit (unit tests), Moq (mocking), ASP.NET Test Host (API integration tests), Appium (mobile UI tests)  
- **CI/CD**: GitHub Actions  
- **Hosting**: Docker on DigitalOcean  

## Getting Started

### Prerequisites

- .NET 9 SDK  
- .NET MAUI workloads installed (`dotnet workload install maui`)  
- A Supabase project (URL & API key)  
- An Openrouter.ai API key  

### Clone the Repository

```bash
git clone https://github.com/mgierlasinski/PantryPal.git
cd PantryPal
```

### Configuration

Create a `.env` file in the project root (or configure environment variables):

```bash
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your_supabase_api_key
OPENROUTER_API_KEY=your_openrouter_api_key
```

### Backend (API)

```bash
cd src/PantryPal.Api
dotnet restore
dotnet run
```

The API will launch at `https://localhost:5001` by default.

### Mobile App

In a separate terminal:

```bash
cd src/PantryPal.Mobile
dotnet restore
dotnet build
# To launch on Android emulator:
dotnet maui run -p src/PantryPal.Mobile -t Android
```

Replace `-t Android` with `-t iOS` as needed.

## Available Scripts

From the project root:

- `dotnet run --project src/PantryPal.Api`  
- `dotnet build src/PantryPal.Mobile`  
- `dotnet maui run -p src/PantryPal.Mobile -t <target>`  

Continuous integration is configured via GitHub Actions (see `.github/workflows/ci.yml`).

## Project Scope

### In Scope (MVP)

- Free-text pantry ingredient input  
- AI-generated recipes (Markdown)  
- Email/password authentication (Supabase)  
- Manual comparison of missing ingredients  
- Profile-based dietary preference filtering  

### Out of Scope

- Barcode scanning or image recognition  
- Autocomplete or ingredient suggestion  
- Rich media (recipe images, video)  
- Social/community features  
- Detailed quantity or stock tracking  
- Automatic visual differentiation of missing ingredients  

## Project Status

This project is currently in active development as an MVP.

## License

This project is licensed under the [MIT License](LICENSE).  
Please see the LICENSE file for details.
