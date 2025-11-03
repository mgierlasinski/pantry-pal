# UI Components Architecture - MAUI Application

<architecture_analysis>

## UI Components Analysis

Based on the PRD and authentication specification, I identify the following UI components for the PantryPal MAUI application:

### 1. Authentication Components:
- **LoginPage**: Email/password authentication form
- **RegisterPage**: New user registration with email verification
- **ForgotPasswordPage**: Password recovery via email
- **LoginPageViewModel**: Handles login validation and authentication
- **RegisterPageViewModel**: Manages registration process
- **ForgotPasswordPageViewModel**: Processes password reset requests

### 2. Main Application Components:
- **PantryPage**: Display and manage pantry items
- **RecipeGenerationPage**: AI recipe generation interface
- **SavedRecipesPage**: View saved recipes with pagination
- **RecipeDetailPage**: Display full recipe content
- **ProfilePage**: User preferences and settings
- **MainPage**: Shell container with tab navigation

### 3. Services and Data Flow:
- **IAuthService**: Authentication operations (login, register, logout)
- **IPantryService**: Pantry item CRUD operations
- **IRecipeService**: Recipe generation and management
- **IUserPreferencesService**: User profile data
- **HttpClient**: API communication with JWT authentication
- **SecureStorage**: JWT token persistence

### 4. Navigation and State Management:
- **AppShell**: Main navigation container with TabBar
- **Shell Navigation**: Modal pages for auth and recipe flows
- **Authentication Guards**: Redirect unauthenticated users to login
- **Session Management**: JWT storage and automatic token refresh

### 5. Component Functionality:
- **Views**: XAML UI definitions with data binding
- **ViewModels**: Business logic and state management
- **Services**: Data access and external API communication
- **Converters**: Data transformation for UI binding
- **Resources**: Styles, templates, and localization strings

</architecture_analysis>

<mermaid_diagram>
```mermaid
flowchart TD
    %% Authentication Components
    subgraph "Authentication Module"
        A1["LoginPage<br/>Email/Password Form"]
        A2["RegisterPage<br/>Registration Form"]
        A3["ForgotPasswordPage<br/>Password Reset"]
        A4["LoginPageViewModel<br/>Auth Validation"]
        A5["RegisterPageViewModel<br/>Registration Logic"]
        A6["ForgotPasswordPageViewModel<br/>Reset Logic"]
    end

    %% Main Application Components
    subgraph "Main App Module"
        M1["PantryPage<br/>Item Management"]
        M2["RecipeGenerationPage<br/>AI Recipe Generation"]
        M3["SavedRecipesPage<br/>Recipe Library"]
        M4["RecipeDetailPage<br/>Recipe Display"]
        M5["ProfilePage<br/>User Preferences"]
        M6["AppShell<br/>Navigation Container"]
    end

    %% ViewModels
    subgraph "ViewModels"
        V1["PantryPageViewModel<br/>Pantry Logic"]
        V2["RecipeGenerationViewModel<br/>Generation Logic"]
        V3["SavedRecipesViewModel<br/>Recipe Management"]
        V4["ProfileViewModel<br/>Preferences Logic"]
    end

    %% Services
    subgraph "Services"
        S1["IAuthService<br/>Authentication"]
        S2["IPantryService<br/>Pantry Data"]
        S3["IRecipeService<br/>Recipe Operations"]
        S4["IUserPreferencesService<br/>User Data"]
        S5["HttpClient<br/>API Communication"]
        S6["SecureStorage<br/>Token Storage"]
    end

    %% Data Flow
    A1 --> A4
    A2 --> A5
    A3 --> A6

    A4 --> S1
    A5 --> S1
    A6 --> S1

    M1 --> V1
    M2 --> V2
    M3 --> V3
    M5 --> V4

    V1 --> S2
    V2 --> S3
    V3 --> S3
    V4 --> S4

    S1 --> S5
    S2 --> S5
    S3 --> S5
    S4 --> S5

    S5 --> S6

    %% Navigation Flow
    M6 --> M1
    M6 --> M3
    M6 --> M5

    M1 -.-> M2
    M2 -.-> M4
    M3 -.-> M4

    %% Authentication Integration
    S1 -.-> M6
    S6 -.-> A1

    %% Styling
    classDef authClass fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef appClass fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef serviceClass fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef vmClass fill:#fff3e0,stroke:#e65100,stroke-width:2px

    class A1,A2,A3,A4,A5,A6 authClass
    class M1,M2,M3,M4,M5,M6 appClass
    class V1,V2,V3,V4 vmClass
    class S1,S2,S3,S4,S5,S6 serviceClass
```
</mermaid_diagram>
