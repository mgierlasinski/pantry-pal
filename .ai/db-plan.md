# Database Schema Plan

## 1. Tables

### 1.1. pantry_items
- **id** UUID PRIMARY KEY DEFAULT gen_random_uuid()
- **user_id** UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE
- **name** VARCHAR(100) NOT NULL CHECK (char_length(name) BETWEEN 1 AND 100)
- **is_favorite** BOOLEAN NOT NULL DEFAULT FALSE
- **created_at** TIMESTAMPTZ NOT NULL DEFAULT now()
- **updated_at** TIMESTAMPTZ NOT NULL DEFAULT now()

**Constraints & Indexes:**
- UNIQUE(user_id, lower(name))
- GIN index on unaccent(lower(name)) for fast searches:  
  `CREATE INDEX idx_pantry_unaccent_name ON pantry_items USING GIN (unaccent(lower(name)));`
- B-Tree index for pagination:  
  `CREATE INDEX idx_pantry_user_created ON pantry_items(user_id, created_at);`

### 1.2. recipes
- **id** UUID PRIMARY KEY DEFAULT gen_random_uuid()
- **user_id** UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE
- **recipe_text** TEXT NOT NULL
- **created_at** TIMESTAMPTZ NOT NULL DEFAULT now()
- **updated_at** TIMESTAMPTZ NOT NULL DEFAULT now()

**Indexes:**
- B-Tree index for pagination:  
  `CREATE INDEX idx_recipes_user_created ON recipes(user_id, created_at);`

### 1.3. recipes_generations
- **id** UUID PRIMARY KEY DEFAULT gen_random_uuid()
- **user_id** UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE
- **model** VARCHAR(100) NOT NULL
- **duration_ms** INTEGER NOT NULL
- **generated_recipe_id** UUID REFERENCES recipes(id) ON DELETE CASCADE
- **reason** recipe_reject_reason
- **error_code** TEXT
- **error_message** TEXT
- **created_at** TIMESTAMPTZ NOT NULL DEFAULT now()

**Indexes:**
- B-Tree index for pagination:  
  `CREATE INDEX idx_gen_user_created ON recipes_generations(user_id, created_at);`

### 1.4. user_preferences
- **user_id** UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE
- **diet_type_id** SMALLINT NOT NULL REFERENCES diet_types(id)
- **preferred_cuisine_id** SMALLINT NOT NULL REFERENCES preferred_cuisines(id)
- **disliked_ingredients** TEXT[] NOT NULL DEFAULT '{}'
- **created_at** TIMESTAMPTZ NOT NULL DEFAULT now()
- **updated_at** TIMESTAMPTZ NOT NULL DEFAULT now()

**Constraints & Indexes:**
- CHECK length of array:  
  `CHECK (array_length(disliked_ingredients, 1) <= 20)`
- CHECK element length:  
  `CHECK ( (SELECT bool_and(char_length(elem) BETWEEN 1 AND 50) FROM unnest(disliked_ingredients) AS elem) )`
- GIN index on array:  
  `CREATE INDEX idx_prefs_disliked_ing ON user_preferences USING GIN (disliked_ingredients);`

## 2. Dictionary Tables

### 2.1. diet_types
- **id** SMALLSERIAL PRIMARY KEY
- **name** VARCHAR(50) NOT NULL UNIQUE

### 2.2. preferred_cuisines
- **id** SMALLSERIAL PRIMARY KEY
- **name** VARCHAR(50) NOT NULL UNIQUE

### 2.3. recipe_reject_reasons (optional)
**Default Rows:**
  | id | description                          |
  |----|--------------------------------------|
  | 1  | I don't have these ingredients      |
  | 2  | I don't like this dish              |
  | 3  | Other                                |

### 2.4. Seed Data
```sql
-- Diet types
INSERT INTO diet_types (name) VALUES
  ('standard'),
  ('vegetarian'),
  ('vegan'),
  ('gluten-free');

-- Preferred cuisines
INSERT INTO preferred_cuisines (name) VALUES
  ('Polish'),
  ('Italian'),
  ('Asian'),
  ('Mexican'),
  ('Indian'),
  ('none');

-- Recipe reject reasons
INSERT INTO recipe_reject_reasons (description) VALUES
  ('I don''t have these ingredients'),
  ('I don''t like this dish'),
  ('Other');
```

## 3. Relationships
- **auth.users (1)** → **(N) pantry_items**
- **auth.users (1)** → **(N) recipes**
- **auth.users (1)** → **(N) recipes_generations**
- **auth.users (1)** → **(1) user_preferences**
- **recipes (1)** → **(N) recipes_generations**

## 4. Indexes Summary
- GIN on `pantry_items.unaccent(lower(name))`
- B-Tree on `(pantry_items.user_id, created_at)`
- B-Tree on `(recipes.user_id, created_at)`
- B-Tree on `(recipes_generations.user_id, created_at)`
- GIN on `user_preferences.disliked_ingredients`

## 5. Row-Level Security (RLS)
For each application table (`pantry_items`, `recipes`, `recipes_generations`, `user_preferences`):
```sql
ALTER TABLE <table> ENABLE ROW LEVEL SECURITY;

CREATE POLICY select_policy ON <table>
  FOR SELECT USING (user_id = auth.uid());
CREATE POLICY insert_policy ON <table>
  FOR INSERT WITH CHECK (user_id = auth.uid());
CREATE POLICY update_policy ON <table>
  FOR UPDATE USING (user_id = auth.uid());
CREATE POLICY delete_policy ON <table>
  FOR DELETE USING (user_id = auth.uid());
```

Replace `<table>` with each of: `pantry_items`, `recipes`, `recipes_generations`, `user_preferences`.

## 6. Additional Notes
- Requires `pgcrypto` extension for `gen_random_uuid()`:  
  `CREATE EXTENSION IF NOT EXISTS pgcrypto;`
- `updated_at` timestamps should be maintained via trigger on each table:
  ```sql
  CREATE OR REPLACE FUNCTION set_updated_at()
  RETURNS TRIGGER AS $$
  BEGIN
    NEW.updated_at = now();
    RETURN NEW;
  END;
  $$ LANGUAGE plpgsql;

  CREATE TRIGGER trg_set_updated_at
    BEFORE UPDATE ON <table>
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
  ```
- Hard delete is used; no soft-delete columns.
- All foreign keys use ON DELETE CASCADE to remove dependent rows when a user or recipe is deleted.
