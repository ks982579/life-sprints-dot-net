-- Create database schema for Life Sprints application
-- Simplified schema for start
-- Just Users and Stories


-- Users Table
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(225) UNIQUE,
  display_name VARCHAR(100),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  is_active BOOLEAN DEFAULT true
)

-- Stories table
CREATE TABLE stories (
  id SERIAL PRIMARY KEY,
  -- delete stories is user is deleted
  user_id UUID NOT NULL REFERENCES user(id) ON DELETE CASCADE,
  title VARCHAR(500) NOT NULL,
  description TEXT,
  year INTEGER NOT NULL, -- annual backlog year
  is_completed BOOLEAN DEFAULT false,
  priority INTEGER DEFAULT 0, -- 0=low, 1=medium, 2=high
  estimated_hours DECIMAL(5,2),
  actual_hours DECIMAL(5,2),
  due_date DATE,
  completed at TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
)

-- Indexes for Performance
-- SELECT story WHERE user_id = 'some-uuid'
CREATE INDEX idx_stories_user ON stories(user_id);
CREATE INDEX idx_stories_year ON stories(year);
CREATE INDEX idx_stories_completed ON stories(is_completed);

-- Update timestamp trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ language 'plpgsql';

-- Update timestamp trigger function
CREATE OR REPLACE FUNCTION update_completed_at_column()
RETURNS TRIGGER AS $$
BEGIN
  -- Story marked as compeleted, set timestamp
  IF NEW.is_completed = true AND OLD.is_completed = false THEN
    NEW.completed_at = CURRENT_TIMESTAMP;
  -- Story unmarked completed, clear timestamp
  ELSIF NEW.is_completed = false AND OLD.is_completed = true THEN
    NEW.completed_at = NULL;
  END IF;

  RETURN NEW;
END;
$$ language 'plpgsql';
-- There are apparently extension for Python or JavaScript!

-- Apply update triggers
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_stories_updated_at BEFORE UPDATE ON stories FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_stories_completed_at BEFORE UPDATE ON stories FOR EACH ROW EXECUTE FUNCTION update_completed_at_column();
