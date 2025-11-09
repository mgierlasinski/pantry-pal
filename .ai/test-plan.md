# PantryPal - Test Plan

## 1. Introduction and Testing Objectives

### 1.1. Introduction

This document outlines the testing strategy and plan for the PantryPal application. PantryPal is a mobile application (.NET MAUI) with an ASP.NET backend API that helps users generate recipes based on the ingredients they have at home. This plan covers various testing phases, methodologies, and tools to ensure the application is of high quality, reliable, and meets user expectations.

### 1.2. Testing Objectives

- To verify that all functional requirements are implemented as per the specifications.
- To ensure the application provides a seamless and consistent user experience on both Android and iOS platforms.
- To validate the backend API for functionality, performance, and security.
- To ensure the integration between the mobile app, the backend API, and external services (Supabase, Openrouter.ai) is working correctly.
- To identify and report all defects, and ensure they are resolved before release.
- To guarantee the application is secure and protects user data.

## 2. Scope of Testing

### 2.1. In Scope

- **Backend API Testing**: Unit and integration testing of all API endpoints, services, and repositories.
- **Mobile Application Functional Testing**: Testing all features of the mobile application on both Android and iOS, including user authentication, pantry management, user preferences, and recipe generation.
- **UI/UX Testing**: Verifying the mobile app's user interface and experience on different devices and screen sizes.
- **Integration Testing**: Testing the communication between the mobile app and the backend API, and between the backend API and external services.
- **Security Testing**: Basic checks for authentication, authorization, and data protection.
- **Compatibility Testing**: Ensuring the mobile app works on a range of specified Android and iOS versions and devices.

### 2.2. Out of Scope

- Advanced performance and load testing of the backend API (beyond basic checks).
- Usability testing with a large group of external end-users.
- Full security audit by a third-party firm.
- AI model accuracy testing (we test the integration and handling of AI responses, not the quality of the AI model itself).

## 3. Types of Tests to be Conducted

- **Unit Testing**: To test individual components (methods, classes) in isolation.
  - **Backend**: Services, Repositories, Validators.
  - **Mobile**: ViewModels, client-side Services.
- **Integration Testing**: To test the interaction between different components.
  - **Backend**: API endpoints interacting with services and the database.
  - **Mobile**: Mobile services making calls to the live (or test environment) backend API.
- **End-to-End (E2E) / UI Testing**: To test complete user flows from the mobile UI to the backend database and back.
- **Manual Testing**: For exploratory testing, UI/UX checks, and scenarios that are difficult to automate.
- **Compatibility Testing**: To ensure the app works correctly across different devices and operating systems.

## 4. Test Scenarios for Key Functionalities

| Feature                 | Test Scenarios                                                                                                                                                                                                                            |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **User Authentication** | - User can successfully register a new account. <br> - User can log in with valid credentials. <br> - User cannot log in with invalid credentials. <br> - User can log out. <br> - API endpoints are protected and require authentication. |
| **Pantry Management**   | - User can add a new item to their pantry. <br> - User can view all items in their pantry. <br> - User can update an existing pantry item. <br> - User can delete a pantry item. <br> - Input validation for pantry item fields.           |
| **User Preferences**    | - User can set their dietary preferences. <br> - User can set their preferred cuisines. <br> - The selected preferences are saved correctly and retrieved.                                                                                  |
| **Recipe Generation**   | - User can generate a recipe based on pantry items and preferences. <br> - The app gracefully handles errors if the AI service (Openrouter.ai) is unavailable. <br> - User can view the generated recipe details. <br> - User can save/favorite a recipe. <br> - User can reject a recipe and provide a reason. |

## 5. Test Environment

- **Backend**: Deployed to a staging environment on DigitalOcean (or a similar cloud provider) running within a Docker container.
- **Database**: A separate PostgreSQL instance on Supabase for testing purposes, seeded with test data.
- **Mobile**:
  - **Simulators/Emulators**: Android Emulator (API 31+) and iOS Simulator (latest iOS version).
  - **Real Devices**: A selection of physical Android (e.g., Samsung, Google Pixel) and iOS (e.g., iPhone) devices with different OS versions and screen sizes.
- **Network**: Testing will be conducted under various simulated network conditions (e.g., Wi-Fi, 4G, slow network, offline).

## 6. Testing Tools

| Tool                     | Purpose                                        |
| ------------------------ | ---------------------------------------------- |
| **xUnit**                | Unit testing framework for .NET.               |
| **Moq**                  | Mocking framework for unit tests.              |
| **ASP.NET Test Host**    | For in-memory integration testing of the API.  |
| **Appium**               | For automated UI testing of the mobile app.    |
| **Postman / .http files**| For manual API endpoint testing.               |
| **GitHub Actions**       | For running automated tests in a CI/CD pipeline.|
| **Jira / GitHub Issues** | For bug tracking and management.               |

## 7. Test Schedule

*(This is a tentative schedule and can be adjusted.)*

- **Phase 1: Unit & Integration Testing**: Continuous, as developers write new code.
- **Phase 2: Feature Testing**: For each new feature, a dedicated testing cycle will be conducted.
- **Phase 3: Regression Testing**: Before each release, a full regression suite will be executed (automated and manual).
- **Phase 4: Release Candidate Testing**: A final round of testing on the release candidate build.

## 8. Test Acceptance Criteria

- All critical and high-priority bugs must be fixed.
- 95% of planned test cases must pass.
- Code coverage from unit tests should be above 70%.
- The application must not crash or freeze during testing of key user flows.
- The application must be successfully tested on all target devices and platforms.

## 9. Roles and Responsibilities

| Role                | Responsibilities                                                                                                 |
| ------------------- | ---------------------------------------------------------------------------------------------------------------- |
| **Developers**      | - Write unit and integration tests for their code. <br> - Fix bugs reported by the QA team.                         |
| **QA Engineer**     | - Create and maintain the test plan and test cases. <br> - Execute manual and automated tests. <br> - Report and track bugs. <br> - Certify releases for production. |
| **Project Manager** | - Oversee the testing process. <br> - Prioritize bug fixes. <br> - Make the final decision on releases.            |

## 10. Bug Reporting Procedures

1.  **Bug Discovery**: When a bug is found, it will be reported in the project's issue tracker (e.g., GitHub Issues).
2.  **Bug Report Content**: Each bug report must include:
    - A clear and concise title.
    - Steps to reproduce the bug.
    - Expected result vs. Actual result.
    - Screenshots or video recordings.
    - Device, OS version, and app version.
    - Logs, if available.
3.  **Bug Triage**: The project manager and development lead will review new bugs, assign a priority (Critical, High, Medium, Low), and assign them to a developer.
4.  **Bug Resolution**: The developer fixes the bug and marks it as "Ready for QA".
5.  **Verification**: The QA engineer verifies the fix. If the bug is resolved, it's closed. If not, it's reopened with comments.
