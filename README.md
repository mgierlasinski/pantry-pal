# PantryPal

[![Test and Publish](https://github.com/mgierlasinski/pantry-pal/actions/workflows/main.yml/badge.svg)](https://github.com/mgierlasinski/pantry-pal/actions/workflows/main.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

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
- **Mobile**: .NET MAUI (Android, iOS) with XAML and UraniumUI
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

The project uses `appsettings.json` for configuration in both the API and Mobile projects, supplemented by environment-specific files (`appsettings.Development.json`) and user secrets for local development.

#### Backend (API)

For local development, the project uses the standard .NET User Secrets mechanism to store sensitive data like API keys and connection strings.

To add your secrets:
1.  Right-click the `PantryPal.Api` project in Visual Studio.
2.  Select **Manage User Secrets**.
3.  This will open a `secrets.json` file. Add your configuration there:

```json
{
  "Supabase": {
    "AnonKey": "your_supabase_api_key",
    "Auth": {
      "JwtSecret": "your_supabase_jwt_secret"
    }
  },
  "OpenRouter": {
    "ApiKey": "your_openrouter_api_key"
  }
}
```

The `appsettings.Development.json` file can be used to override non-sensitive configuration for the development environment.

#### Mobile App

The mobile app loads configuration from embedded `appsettings.json` files. For local development, it supports the standard .NET User Secrets mechanism.

To add your secrets:
1.  Right-click the `PantryPal.Mobile` project in Visual Studio.
2.  Select **Manage User Secrets**.
3.  This will open a `secrets.json` file where you can add your keys. The project is configured to automatically embed this file during the build process.

`secrets.json`:
```json
{
  "Supabase": {
    "AnonKey": "your_supabase_anon_key"
  }
}
```

The mobile app's environment is determined by `src/PantryPal.Mobile/Properties/MauiLaunchSettings.cs`. By default, it's set to `Development`, which loads `appsettings.Development.json`.

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

### Installing the Android App

To install the pre-built application package on an Android device:

1.  Open the web browser on your phone and navigate to the releases page:  
    [https://github.com/mgierlasinski/pantry-pal/releases](https://github.com/mgierlasinski/pantry-pal/releases)
2.  Find the latest release and download the `com.pantrypal.mobile.apk` file.
3.  Once downloaded, open the file to begin installation.
4.  You may need to allow your browser to "install unknown apps". Enable this permission when prompted.
5.  Follow the on-screen instructions to complete the installation.

### Docker (Production)

To build and run the application using Docker, follow these steps from the project root directory.

**1. Build the Docker Image:**

```bash
docker build -t mgierlasinski/pantry-pal-api -f src/PantryPal.Api/Dockerfile .
```

- `-t mgierlasinski/pantry-pal-api`: Tags the image with the specified name.
- `-f src/PantryPal.Api/Dockerfile`: Specifies the path to the Dockerfile.
- `.`: Sets the build context to the current directory (the project root).

**2. Run the Docker Container:**

This command runs the container and injects the required secrets as environment variables.

```bash
docker run --rm -p 8080:8080 \
  -e "Supabase__Url=your_supabase_url" \
  -e "Supabase__AnonKey=your_supabase_anon_key" \
  -e "Supabase__Auth__JwtSecret=your_supabase_jwt_secret" \
  -e "OpenRouter__ApiKey=your_openrouter_api_key" \
  mgierlasinski/pantry-pal-api
```

- `--rm`: Automatically removes the container when it exits.
- `-p 8080:8080`: Maps port 8080 on your local machine to port 8080 in the container.
- `-e "..."`: Sets the environment variables required by your application. **Replace the placeholder values** with your actual secrets.
- `mgierlasinski/pantry-pal-api`: The name of the image to run.

### Testing the API

The project includes a `src/PantryPal.Api/PantryPal.Api.http` file for testing all API endpoints. You can use it with the built-in editor in Visual Studio or the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension in VS Code.

The requests use variables defined in `src/PantryPal.Api/http-client.env.json` for three environments: `dev`, `test`, and `prod`. You'll need to fill in your credentials (like `Auth_ApiKey`, `UserEmail`, and `UserPassword`) in this file to run the requests successfully.

## Available Scripts

From the project root:

- `dotnet run --project src/PantryPal.Api`  
- `dotnet build src/PantryPal.Mobile`  
- `dotnet maui run -p src/PantryPal.Mobile -t <target>`  

Continuous integration and deployment are configured via GitHub Actions. See the `.github/workflows` directory for details on the following workflows:
- `main.yml`: Checks the health of the main branch by compiling the code and running tests.
- `pull-request.yml`: Runs tests on every pull request.
- `deploy-api.yml`: Deploys the API to DigitalOcean.
- `release-mobile.yml`: Creates a GitHub release for the mobile app.

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
