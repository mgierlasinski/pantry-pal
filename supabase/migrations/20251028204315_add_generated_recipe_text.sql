-- =====================================================
-- Migration: Add generated_recipe_text to recipes_generations
-- =====================================================
-- Purpose: Store generated recipe text temporarily in recipes_generations table
-- 
-- This migration enables a hybrid approach where:
--   - Generated recipes are stored temporarily in recipes_generations
--   - Recipes are only moved to the recipes table when user accepts them
--   - If client crashes, recipe text can be retrieved from generations table
--   - Provides flexibility for accept endpoint implementation
--
-- Affected tables: recipes_generations
-- Special considerations:
--   - Column is nullable (existing records won't break)
--   - Text can be large (full markdown recipe)
--   - Consider cleanup policy for old unaccepted recipes (future enhancement)
-- =====================================================

-- Add generated_recipe_text column to store the AI-generated recipe temporarily
-- This allows the accept endpoint to work even if client loses the recipe text
alter table recipes_generations
  add column generated_recipe_text text;

-- Add comment to document the purpose of this column
comment on column recipes_generations.generated_recipe_text is 
  'Temporary storage for generated recipe text. Allows recipe retrieval if client crashes before accepting. Can be cleaned up after 30 days if not accepted.';

-- =====================================================
-- END OF MIGRATION
-- =====================================================

