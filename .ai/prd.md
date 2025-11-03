# Product Requirements Document (PRD) - PantryPal
## 1. Product Overview
PantryPal is a mobile application designed to help busy individuals simplify meal planning by utilizing ingredients they already have at home. The MVP enables users to manage their virtual pantry, define dietary preferences in their profile, and generate AI-powered recipes based on available ingredients, core pantry items, and user preferences.

## 2. User Problem
After a long day at work, users often lack the time and creativity to plan meals, leading to food waste and repetitive menus. They need a straightforward way to discover recipes using existing ingredients, both at home and while shopping, without the burden of manual cross-referencing or complex input methods.

## 3. Functional Requirements
1. Pantry Management
   - Users can add new ingredients to their virtual pantry via free-text input.
   - Users can edit or delete existing pantry entries.
   - Users can mark pantry items as "favorite" for recipe prioritization.

2. Recipe Generation
   - One-tap AI recipe generation based on:
     - Current pantry contents
     - Predefined core ingredient list
     - User’s dietary preferences (diet type, cuisine, disliked ingredients)
   - AI may suggest up to three additional ingredients not in the pantry.
   - Recipes are returned in standardized Markdown format.
   - No visual distinction between owned and missing ingredients; users compare manually.

3. Recipe Interaction
   - Accept recipe: saves to user’s saved recipe list (chronologically sorted).
   - Reject recipe: discards recipe and prompts for a predefined rejection reason ([I don’t have these ingredients], [I don’t like this dish], [Other]). Reasons are logged for analytics.

4. Saved Recipes
   - Users can view a list of previously saved recipes.
   - Users can delete saved recipes.

5. User Accounts & Authentication
   - Sign up and log in via email and password.
   - Recover forgotten password.
   - Authentication backed by Supabase; data sync on server.
   - Access to pantry and saved recipes is restricted to authenticated users.

6. Profile & Preferences
   - Profile page with:
     - Diet type (standard, vegetarian, vegan, gluten-free)
     - Preferred cuisine (Polish, Italian, Asian, Mexican, Indian, none)
     - Open-text field for disliked ingredients
   - UI banner prompting users without completed preferences to fill out their profile.

7. Feedback & Analytics
   - Log rejection reasons for each discarded recipe.
   - Monitor recipe accept-to-reject ratios and preference completion rates.

## 4. Product Boundaries
In scope for MVP:
- Free-text pantry input without validation or quantity tracking.
- Basic AI recipe generation with Markdown output.
- Email/password authentication via Supabase.
- Manual comparison of missing ingredients.
- Profile-based preference filtering.

Out of scope for MVP:
- Camera-based input (barcode scanning or image recognition).
- Autocomplete or suggestion for ingredient input.
- Rich media (recipe images, multimedia content).
- Social features (sharing, community).
- Detailed quantity or stock tracking.
- Automatic visual differentiation of missing ingredients in recipes.

## 5. User Stories
- US-001: Add Ingredient to Pantry
  - Description: As a user, I want to add a new ingredient by typing its name so that my pantry reflects what I have at home.
  - Acceptance Criteria:
    - Given I am logged in, when I enter a text and submit, then the ingredient appears in my pantry list.
    - The entry persists after app reload.

- US-002: Edit Ingredient
  - Description: As a user, I want to edit the name of an existing ingredient so that I can correct typos or unify naming.
  - Acceptance Criteria:
    - Given an existing pantry item, when I tap edit, change text, and save, then the modified name is displayed and persisted.

- US-003: Delete Ingredient
  - Description: As a user, I want to delete an ingredient from my pantry so that I can remove items I no longer have.
  - Acceptance Criteria:
    - Given an existing pantry item, when I tap delete and confirm, then the item is removed from my pantry list.

- US-004: Mark Favorite
  - Description: As a user, I want to mark a pantry item as favorite so that recipes prioritize that ingredient.
  - Acceptance Criteria:
    - Given a pantry item, when I tap the favorite icon, then the item is visually indicated as favorite and used preferentially.

- US-005: Generate Recipe
  - Description: As a user with at least one pantry item, I want to generate a recipe with a single tap so that I can quickly decide what to cook.
  - Acceptance Criteria:
    - Given I have one or more ingredients, when I tap generate, then an AI-generated recipe in Markdown is shown.
    - Recipe may include up to three additional suggested ingredients.

- US-006: Empty Pantry State
  - Description: As a new user with no pantry items, I want to see an empty state prompting me to add ingredients.
  - Acceptance Criteria:
    - Given an empty pantry, when I open the app, then I see a call-to-action to add my first ingredient.

- US-007: Accept Recipe
  - Description: As a user, I want to save a generated recipe by accepting it so that I can access it later.
  - Acceptance Criteria:
    - Given a displayed recipe, when I tap accept, then the recipe is added to my saved list and persists.

- US-008: Reject Recipe with Reason
  - Description: As a user, I want to reject an unsatisfactory recipe and provide a reason so that the app logs feedback.
  - Acceptance Criteria:
    - Given a displayed recipe, when I tap reject and select a reason, then the recipe is dismissed and the reason is logged.

- US-009: View Saved Recipes
  - Description: As a user, I want to view my list of saved recipes so that I can revisit them.
  - Acceptance Criteria:
    - Given I have saved recipes, when I navigate to saved recipes, then I see a chronologically sorted list.

- US-010: Delete Saved Recipe
  - Description: As a user, I want to delete a saved recipe so that I can manage my recipe list.
  - Acceptance Criteria:
    - Given a saved recipe entry, when I tap delete and confirm, then the recipe is removed.

- US-011: User Authentication
  - Description: As a user, I want to sign up and log in via email and password so that my pantry and recipes are private.
  - Acceptance Criteria:
    - Login and registration are done on dedicated pages.
    - Logging in requires entering an email address and password.
    - Registration requires providing an email address, password, and password confirmation.
    - We do not use external login services (e.g., Google, GitHub).
    - Password recovery should be possible.
    - Given valid credentials, when I sign up or log in, then I gain access to my data.
    - Invalid credentials show an error.

- US-012: Define Dietary Preferences
  - Description: As a user, I want to set my diet type, preferred cuisines, and disliked ingredients in profile so that recipes match my needs.
  - Acceptance Criteria:
    - Given profile page, when I select options and save, then preferences persist and influence future recipes.

- US-013: Preferences Prompt
  - Description: As a user without completed preferences, I want to see a banner prompting me to fill my profile so that I customize recipes.
  - Acceptance Criteria:
    - Given incomplete preferences, when I view pantry, then I see a prompt linked to profile page.

- US-014: Secure Access Control
  - Description: As a user, I want protected routes for pantry and recipes so that only authenticated users can access them.
  - Acceptance Criteria:
    - Given an unauthenticated session, when navigating to protected pages, then I am redirected to login.

## 6. Success Metrics
- Profile Completion: 90% of active users have completed dietary preferences. Measured via user profile data.
- Engagement: 75% of active users generate at least one recipe per week. Measured via generation logs.
- Quality Indicator: Ratio of accepted to rejected recipes tracked; high rejection reasons analyzed for prompt improvements.
