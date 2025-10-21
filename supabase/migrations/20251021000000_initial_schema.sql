-- =====================================================
-- Migration: Initial Schema Setup for PantryPal
-- =====================================================
-- Purpose: Create the core database schema for PantryPal application
-- 
-- This migration creates:
--   - Dictionary tables (diet_types, preferred_cuisines, recipe_reject_reasons)
--   - Main application tables (pantry_items, recipes, recipes_generations, user_preferences)
--   - Indexes for performance optimization
--   - Row-level security policies for data isolation
--   - Triggers for automatic timestamp management
--
-- Affected tables: ALL (initial creation)
-- Special considerations: 
--   - Requires pgcrypto extension for UUID generation
--   - Requires pg_trgm extension for trigram-based text search
--   - All tables have RLS enabled with user-based isolation
-- =====================================================

-- =====================================================
-- 1. EXTENSIONS
-- =====================================================
-- Enable pgcrypto for gen_random_uuid() function
create extension if not exists pgcrypto;

-- Enable pg_trgm for trigram-based text search and similarity
create extension if not exists pg_trgm;

-- =====================================================
-- 2. DICTIONARY TABLES
-- =====================================================

-- -----------------------------------------------------
-- 2.1. diet_types
-- -----------------------------------------------------
-- Stores available dietary restriction options
-- Used by user_preferences to define user's diet type
create table diet_types (
  id smallserial primary key,
  name varchar(50) not null unique
);

-- Seed default diet types
insert into diet_types (name) values
  ('standard'),
  ('vegetarian'),
  ('vegan'),
  ('gluten-free');

-- -----------------------------------------------------
-- 2.2. preferred_cuisines
-- -----------------------------------------------------
-- Stores available cuisine preference options
-- Used by user_preferences to define user's preferred cuisine
create table preferred_cuisines (
  id smallserial primary key,
  name varchar(50) not null unique
);

-- Seed default cuisine options
insert into preferred_cuisines (name) values
  ('Polish'),
  ('Italian'),
  ('Asian'),
  ('Mexican'),
  ('Indian'),
  ('None');

-- -----------------------------------------------------
-- 2.3. recipe_reject_reasons
-- -----------------------------------------------------
-- Stores predefined reasons why a user might reject a recipe
-- Used by recipes_generations to track rejection feedback
create table recipe_reject_reasons (
  id smallserial primary key,
  description varchar(100) not null unique
);

-- Seed default reject reasons
insert into recipe_reject_reasons (description) values
  ('I don''t have these ingredients'),
  ('I don''t like this dish'),
  ('Other');

-- =====================================================
-- 3. MAIN APPLICATION TABLES
-- =====================================================

-- -----------------------------------------------------
-- 3.1. pantry_items
-- -----------------------------------------------------
-- Stores user's pantry inventory
-- Each user maintains their own list of ingredients
-- Supports favorites and case-insensitive unique names per user
create table pantry_items (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  name varchar(100) not null check (char_length(name) between 1 and 100),
  is_favorite boolean not null default false,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- Unique index to ensure case-insensitive unique ingredient names per user
create unique index uq_pantry_user_name on pantry_items(user_id, lower(name));

-- GIN index for fast case-insensitive trigram-based ingredient search
create index idx_pantry_name_trgm on pantry_items using gin (lower(name) gin_trgm_ops);

-- B-Tree index for efficient pagination by user and creation date
create index idx_pantry_user_created on pantry_items(user_id, created_at);

-- -----------------------------------------------------
-- 3.2. recipes
-- -----------------------------------------------------
-- Stores generated recipes for users
-- Each recipe is associated with a single user
-- recipe_text contains the full AI-generated recipe content
create table recipes (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  recipe_text text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- B-Tree index for efficient pagination by user and creation date
create index idx_recipes_user_created on recipes(user_id, created_at);

-- -----------------------------------------------------
-- 3.3. recipes_generations
-- -----------------------------------------------------
-- Tracks all recipe generation attempts (successful and failed)
-- Stores metadata about the AI model, duration, and outcomes
-- Links to generated recipe on success, or stores rejection/error info on failure
create table recipes_generations (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  
  -- AI model information and performance metrics
  model varchar(100) not null,
  duration_ms integer not null,
  
  -- Reference to successfully generated recipe (null if rejected or failed)
  generated_recipe_id uuid references recipes(id) on delete cascade,
  
  -- User rejection information (null if not rejected)
  reject_reason_id smallint references recipe_reject_reasons(id),
  
  -- Error information (null if successful)
  error_code text,
  error_message text,
  
  created_at timestamptz not null default now()
);

-- B-Tree index for efficient pagination by user and creation date
create index idx_gen_user_created on recipes_generations(user_id, created_at);

-- -----------------------------------------------------
-- 3.4. user_preferences
-- -----------------------------------------------------
-- Stores user's dietary preferences and restrictions
-- One row per user with diet type, cuisine preference, and disliked ingredients
create table user_preferences (
  user_id uuid primary key references auth.users(id) on delete cascade,
  diet_type_id smallint not null references diet_types(id),
  preferred_cuisine_id smallint not null references preferred_cuisines(id),
  
  -- Nullable string of disliked ingredients (max 1000 chars)
  disliked_ingredients text,
  
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  
  -- Ensure disliked ingredients is within reasonable length if provided
  constraint chk_disliked_length check (disliked_ingredients is null or char_length(disliked_ingredients) <= 1000)
);

-- =====================================================
-- 4. TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- =====================================================

-- -----------------------------------------------------
-- 4.1. Trigger function
-- -----------------------------------------------------
-- Automatically updates the updated_at column to current timestamp
-- Called before any UPDATE operation on tables with updated_at column
create or replace function set_updated_at()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql;

-- -----------------------------------------------------
-- 4.2. Apply triggers to tables
-- -----------------------------------------------------

-- Trigger for pantry_items table
create trigger trg_pantry_items_updated_at
  before update on pantry_items
  for each row execute function set_updated_at();

-- Trigger for recipes table
create trigger trg_recipes_updated_at
  before update on recipes
  for each row execute function set_updated_at();

-- Trigger for user_preferences table
create trigger trg_user_preferences_updated_at
  before update on user_preferences
  for each row execute function set_updated_at();

-- =====================================================
-- 5. ROW LEVEL SECURITY (RLS)
-- =====================================================

-- -----------------------------------------------------
-- 5.1. pantry_items RLS
-- -----------------------------------------------------
-- Enable RLS to ensure users can only access their own pantry items
alter table pantry_items enable row level security;

-- Policy: authenticated users can view their own pantry items
-- Rationale: Users should only see ingredients they've added to their pantry
create policy select_policy_pantry_items on pantry_items
  for select 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can insert their own pantry items
-- Rationale: Users can add new ingredients to their own pantry
create policy insert_policy_pantry_items on pantry_items
  for insert 
  to authenticated
  with check (user_id = auth.uid());

-- Policy: authenticated users can update their own pantry items
-- Rationale: Users can modify (favorite, rename) their own pantry items
create policy update_policy_pantry_items on pantry_items
  for update 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can delete their own pantry items
-- Rationale: Users can remove ingredients from their pantry
create policy delete_policy_pantry_items on pantry_items
  for delete 
  to authenticated
  using (user_id = auth.uid());

-- -----------------------------------------------------
-- 5.2. recipes RLS
-- -----------------------------------------------------
-- Enable RLS to ensure users can only access their own recipes
alter table recipes enable row level security;

-- Policy: authenticated users can view their own recipes
-- Rationale: Recipe history is private to each user
create policy select_policy_recipes on recipes
  for select 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can insert their own recipes
-- Rationale: Users (via API/AI) can save new generated recipes
create policy insert_policy_recipes on recipes
  for insert 
  to authenticated
  with check (user_id = auth.uid());

-- Policy: authenticated users can update their own recipes
-- Rationale: Users might want to edit or annotate saved recipes
create policy update_policy_recipes on recipes
  for update 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can delete their own recipes
-- Rationale: Users can remove recipes they no longer want
create policy delete_policy_recipes on recipes
  for delete 
  to authenticated
  using (user_id = auth.uid());

-- -----------------------------------------------------
-- 5.3. recipes_generations RLS
-- -----------------------------------------------------
-- Enable RLS to ensure users can only access their own generation history
alter table recipes_generations enable row level security;

-- Policy: authenticated users can view their own generation history
-- Rationale: Generation metrics and history are private to each user
create policy select_policy_recipes_generations on recipes_generations
  for select 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can insert their own generation records
-- Rationale: API creates generation records when processing recipe requests
create policy insert_policy_recipes_generations on recipes_generations
  for insert 
  to authenticated
  with check (user_id = auth.uid());

-- Policy: authenticated users can update their own generation records
-- Rationale: Users can update rejection reasons or other metadata
create policy update_policy_recipes_generations on recipes_generations
  for update 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can delete their own generation records
-- Rationale: Users can clean up their generation history
create policy delete_policy_recipes_generations on recipes_generations
  for delete 
  to authenticated
  using (user_id = auth.uid());

-- -----------------------------------------------------
-- 5.4. user_preferences RLS
-- -----------------------------------------------------
-- Enable RLS to ensure users can only access their own preferences
alter table user_preferences enable row level security;

-- Policy: authenticated users can view their own preferences
-- Rationale: Preferences are private to each user
create policy select_policy_user_preferences on user_preferences
  for select 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can insert their own preferences
-- Rationale: Users can create their preference profile
create policy insert_policy_user_preferences on user_preferences
  for insert 
  to authenticated
  with check (user_id = auth.uid());

-- Policy: authenticated users can update their own preferences
-- Rationale: Users can modify their dietary preferences over time
create policy update_policy_user_preferences on user_preferences
  for update 
  to authenticated
  using (user_id = auth.uid());

-- Policy: authenticated users can delete their own preferences
-- Rationale: Users can remove their preference profile
create policy delete_policy_user_preferences on user_preferences
  for delete 
  to authenticated
  using (user_id = auth.uid());

-- -----------------------------------------------------
-- 5.5. Dictionary tables RLS (read-only for all users)
-- -----------------------------------------------------

-- diet_types: Enable RLS and allow all authenticated users to read
alter table diet_types enable row level security;

create policy select_policy_diet_types on diet_types
  for select 
  to authenticated
  using (true);

create policy select_policy_diet_types_anon on diet_types
  for select 
  to anon
  using (true);

-- preferred_cuisines: Enable RLS and allow all authenticated users to read
alter table preferred_cuisines enable row level security;

create policy select_policy_preferred_cuisines on preferred_cuisines
  for select 
  to authenticated
  using (true);

create policy select_policy_preferred_cuisines_anon on preferred_cuisines
  for select 
  to anon
  using (true);

-- recipe_reject_reasons: Enable RLS and allow all authenticated users to read
alter table recipe_reject_reasons enable row level security;

create policy select_policy_recipe_reject_reasons on recipe_reject_reasons
  for select 
  to authenticated
  using (true);

create policy select_policy_recipe_reject_reasons_anon on recipe_reject_reasons
  for select 
  to anon
  using (true);

-- =====================================================
-- END OF MIGRATION
-- =====================================================

