# PantryPal Authentication - Technical Specification

## 1. Overview

This document outlines the architecture for implementing user authentication and authorization in the PantryPal application. The solution leverages Supabase for core authentication services and integrates with both the .NET MAUI mobile client and the ASP.NET backend API, as per requirements US-011 and US-014 in the PRD.

The primary goals are:
- To provide secure user registration, login, and password recovery.
- To protect user-specific data (pantry, recipes) by restricting access to authenticated users.
- To create a seamless user experience for both authenticated and unauthenticated states.

## 2. User Interface Architecture (MAUI)

The MAUI application will be responsible for all user-facing authentication screens and for managing the user's session state.

### 2.0. Project Structure (`PantryPal.Mobile`)
New files will be organized according to the established project structure:
- **Views:**
  - `src/PantryPal.Mobile/Views/LoginPage.xaml`
  - `src/PantryPal.Mobile/Views/RegisterPage.xaml`
  - `src/PantryPal.Mobile/Views/ForgotPasswordPage.xaml`
- **ViewModels:**
  - `src/PantryPal.Mobile/ViewModels/LoginPageViewModel.cs`
  - `src/PantryPal.Mobile/ViewModels/RegisterPageViewModel.cs`
  - `src/PantryPal.Mobile/ViewModels/ForgotPasswordPageViewModel.cs`
- **Services:**
  - `src/PantryPal.Mobile/Services/IAuthService.cs`
  - `src/PantryPal.Mobile/Services/SupabaseAuthService.cs`
- **Models:**
  - `src/PantryPal.Mobile/Models/AuthResult.cs`

### 2.1. Pages (Views & ViewModels)

New pages will be created to handle the authentication flows. These will follow the MVVM pattern.

#### `LoginPage`
- **View:** `LoginPage.xaml` will contain input fields for email and password, a "Log In" button, and links to the registration and password recovery pages.
- **ViewModel:** `LoginPageViewModel.cs` will handle user input, data validation, and will call the `IAuthService` to perform the login. It will manage loading states and display any errors returned from the service.

#### `RegisterPage`
- **View:** `RegisterPage.xaml` will have fields for email, password, and password confirmation, along with a "Sign Up" button.
- **ViewModel:** `RegisterPageViewModel.cs` will validate user input (e.g., matching passwords, valid email format), call the `IAuthService` to register the user, and handle success/error responses. Upon successful registration, it may show a message prompting the user to check their email for confirmation.

#### `ForgotPasswordPage`
- **View:** `ForgotPasswordPage.xaml` will provide an input field for the user's email and a "Send Reset Link" button.
- **ViewModel:** `ForgotPasswordPageViewModel.cs` will take the user's email, call the `IAuthService` to initiate the password reset process, and provide feedback to the user (e.g., "If an account with this email exists, a password reset link has been sent.").

### 2.2. Navigation and Routing (`AppShell.xaml`)

`AppShell` will be updated to manage different routes for authenticated and unauthenticated users. The visibility of routes and flyout items will be dynamically updated by subscribing to the `IAuthService.AuthStateChanged` observable. This ensures the UI reacts immediately to login and logout events.

- **Unauthenticated State:** The user will only have access to `LoginPage`, `RegisterPage`, and `ForgotPasswordPage`. The default route will be `LoginPage`.
- **Authenticated State:** The user will have access to the main application features (Pantry, Recipes, Profile). The `FlyoutItem`s for these pages will be visible. A "Logout" button will be added to the flyout menu or profile page.
- **Protected Routes:** Navigation to protected pages (e.g., `/pantry`) will be guarded. An `IAuthService` check will be performed before navigation. If the user is not authenticated, they will be redirected to the `LoginPage`. This can be implemented using a custom `Shell.BackButtonBehavior` or by checking auth state in the `OnNavigating` method of protected pages' ViewModels.

### 2.3. Services

A dedicated service will abstract the authentication logic away from the ViewModels.

#### `IAuthService`
This interface will define the contract for authentication operations.
- `Task<bool> IsAuthenticatedAsync()`
- `Task<AuthResult> LoginAsync(string email, string password)`
- `Task<AuthResult> RegisterAsync(string email, string password)`
- `Task<AuthResult> LogoutAsync()`
- `Task<AuthResult> SendPasswordResetEmailAsync(string email)`
- `IObservable<bool> AuthStateChanged`

#### `SupabaseAuthService`
This class will implement `IAuthService` using the `supabase-csharp` client.
- It will initialize the Supabase client with the project URL and public key.
- It will handle session persistence and retrieval securely using `Microsoft.Maui.Storage.SecureStorage`.
- It will map Supabase responses to a generic `AuthResult` object containing success status and error messages.

### 2.4. Validation and Error Handling

- **ViewModel-level Validation:** ViewModels will use the .NET Community Toolkit `[ObservableProperty]` and validation attributes (`[Required]`, `[EmailAddress]`, `[MinLength]`) to perform initial client-side validation.
- **Error Messages:**
  - "Please enter a valid email address."
  - "Password is required."
  - "Passwords do not match."
  - "Invalid login credentials." (from Supabase)
  - "An account with this email already exists." (from Supabase)
  - "An unexpected error occurred. Please try again." (for network issues or other exceptions)
  - User-friendly error messages will be displayed on the UI, for instance, below the input fields or in a summary section.

### 2.5. Key Scenarios

- **Login:** User enters credentials -> ViewModel validates -> `IAuthService.LoginAsync` is called -> On success, session is stored, and user is navigated to the main app page (e.g., Pantry) -> On failure, an error message is displayed.
- **Logout:** User taps logout -> `IAuthService.LogoutAsync` is called -> Session is cleared -> User is navigated back to the `LoginPage`.
- **Session Persistence:** When the app starts, `IAuthService.IsAuthenticatedAsync` will check `SecureStorage` for a valid session. If found, the user is navigated directly to the main app; otherwise, they are sent to the `LoginPage`.

### 2.6. Data Contracts

#### `AuthResult` Model
A simple record will be used to communicate the outcome of authentication operations from the `IAuthService` to the ViewModels. This decouples the ViewModels from the specific implementation details of the Supabase client.

```csharp
// Located in src/PantryPal.Mobile/Models/AuthResult.cs
public record AuthResult
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; }

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
```

## 3. Backend Logic (ASP.NET Minimal API)

The backend API must be secured to ensure that only authenticated users can access and modify their own data.

### 3.1. Authentication and Authorization Middleware

- A custom ASP.NET Core authentication middleware will be implemented.
- On every request to a protected endpoint, this middleware will:
  1. Extract the JWT from the `Authorization: Bearer <token>` header.
  2. Validate the JWT using the Supabase project's JWT secret. The `supabase-csharp` library or a standard JWT library can be used for this.
  3. If the token is valid, it will populate the `HttpContext.User` with claims from the token (e.g., user ID).
  4. If the token is invalid, expired, or missing, it will reject the request with a `401 Unauthorized` status.

### 3.2. Protected API Endpoints

- All existing and future endpoints that handle user-specific data (e.g., `GET /pantry`, `POST /pantry/item`, `GET /recipes`) will be decorated with the `[Authorize]` attribute.
- The `userId` required for database queries will be retrieved from the `HttpContext.User` claims, ensuring users can only access their own data. This prevents one user from accessing another user's pantry items.

```csharp
// Example of a protected endpoint
app.MapGet("/api/pantry", (HttpContext context) => {
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ... logic to fetch pantry items for userId
})
.RequireAuthorization();
```

### 3.3. Exception Handling

- A global exception handler middleware will be configured.
- If an `UnauthorizedAccessException` is thrown or the authorization middleware rejects a request, the API will consistently return a `401 Unauthorized` HTTP status code with an empty body or a generic error message.
- If an authenticated user tries to access a resource they do not own, a `403 Forbidden` status should be returned.

## 4. Authentication System (Supabase)

Supabase Auth will be the single source of truth for user identities.

### 4.1. Configuration

- **MAUI Client:** The `Supabase.Client` will be initialized once with the public URL and anon key. These will be stored in a configuration file.
- **ASP.NET API:** The API will need the Supabase project URL and the JWT Secret to validate tokens. These will be stored securely using .NET's configuration system (e.g., `appsettings.json`, environment variables, or user secrets).

### 4.2. User Flows

- **Registration:**
  1. MAUI app calls `Supabase.Auth.SignUpAsync(email, password)`.
  2. Supabase creates the user in the `auth.users` table but marks them as unconfirmed.
  3. Supabase sends a confirmation email to the user. **Note:** Email confirmation must be enabled in the Supabase project settings.
  4. The user clicks the link in the email to confirm their account.

- **Login:**
  1. MAUI app calls `Supabase.Auth.SignInWithPassword(email, password)`.
  2. On success, Supabase returns a session object containing a JWT (`AccessToken`).
  3. The `SupabaseAuthService` in the MAUI app serializes and saves this session to `SecureStorage`.
  4. For subsequent API calls, the MAUI app retrieves the `AccessToken` and includes it in the `Authorization` header.

- **Password Recovery:**
  1. MAUI app calls `Supabase.Auth.ResetPasswordForEmailAsync(email)`.
  2. Supabase sends an email with a password reset link.
  3. The user clicks the link, which takes them to a page hosted by Supabase where they can enter a new password. **Note:** The redirect URL for this page must be configured in Supabase project settings.

### 4.3. JWT Handling

- The JWTs issued by Supabase are short-lived by default. The `supabase-csharp` client handles token refreshes automatically using the provided refresh token.
- The ASP.NET API will only ever receive the short-lived `AccessToken`. Its sole responsibility is to validate the token's signature and expiration. It does not need to handle token refreshes.

