# Authentication Architecture - Sequence Diagram

<authentication_analysis>

## Authentication Requirements Analysis

Based on the PRD documentation and authentication specification, I identify the following authentication flows:

### 1. Main actors and their interactions:
- **User**: MAUI Client (mobile application)
- **MAUI Client**: Mobile application handling the user interface
- **ASP.NET API**: Backend minimal API
- **Supabase Auth**: Authentication service

### 2. Authentication flows:
- **Registration**: User creates account with email/password
- **Login**: User logs in with existing credentials
- **Password Reset**: User resets password via email
- **API Authorization**: API requests require valid JWT
- **Session Management**: Token storage and renewal

### 3. Token verification and refresh processes:
- JWT tokens are issued by Supabase Auth
- API validates JWT on every protected request
- Session is stored in MAUI SecureStorage
- Automatic token refresh by Supabase client

### 4. Authentication steps description:
1. **Registration**: Email/password → Supabase creates user → Verification email
2. **Login**: Email/password → Supabase verifies → Returns session with JWT
3. **API Access**: Client attaches JWT to Authorization header
4. **API Verification**: Checking JWT signature and expiration
5. **Session Expiration**: Automatic token refresh by client

</authentication_analysis>

<mermaid_diagram>
```mermaid
sequenceDiagram
    autonumber

    participant User as User
    participant MAUI as MAUI Client
    participant API as ASP.NET API
    participant Supabase as Supabase Auth

    %% User registration
    rect rgb(240, 248, 255)
        note over User,Supabase: Registration process
        User->>MAUI: Enter email and password
        MAUI->>Supabase: SignUpAsync(email, password)
        Supabase-->>MAUI: User created (unconfirmed)
        Supabase-->>User: Verification email sent
        User->>Supabase: Click verification link
        Supabase-->>Supabase: Account confirmed
    end

    %% User login
    rect rgb(255, 248, 220)
        note over User,Supabase: Login process
        User->>MAUI: Enter email and password
        MAUI->>Supabase: SignInWithPassword(email, password)
        alt Authentication successful
            Supabase-->>MAUI: Session with JWT (AccessToken, RefreshToken)
            MAUI->>MAUI: Save session to SecureStorage
            MAUI-->>User: Redirect to main app
        else Authentication failed
            Supabase-->>MAUI: Authentication error
            MAUI-->>User: Display error message
        end
    end

    %% API protected resources access
    rect rgb(240, 255, 240)
        note over MAUI,API: API access with authentication
        MAUI->>API: GET /pantry-items (Authorization: Bearer JWT)
        API->>API: Validate JWT and expiration
        alt JWT valid
            API->>API: Extract userId from token
            API->>API: Fetch user data
            API-->>MAUI: Pantry data (200 OK)
        else JWT invalid/expired
            API-->>MAUI: 401 Unauthorized
            MAUI->>Supabase: Automatic token refresh
            alt Refresh successful
                Supabase-->>MAUI: New AccessToken
                MAUI->>API: Retry request with new JWT
            else Refresh failed
                MAUI-->>User: Redirect to login
            end
        end
    end

    %% Password reset
    rect rgb(255, 240, 245)
        note over User,Supabase: Password reset
        User->>MAUI: Enter email for reset
        MAUI->>Supabase: ResetPasswordForEmail(email)
        Supabase-->>User: Password reset email sent
        User->>Supabase: Click link and enter new password
        Supabase-->>Supabase: Password updated
    end

    %% Logout
    rect rgb(255, 245, 245)
        note over MAUI,Supabase: Logout
        User->>MAUI: Click logout button
        MAUI->>Supabase: SignOut()
        MAUI->>MAUI: Clear SecureStorage
        MAUI-->>User: Redirect to login screen
    end
```
</mermaid_diagram>
