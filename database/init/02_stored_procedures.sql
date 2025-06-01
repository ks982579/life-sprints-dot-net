-- Stored Procedures for Life Sprint Application

-- 1. Create a new story
CREATE OR REPLACE FUNCTION sp_create_task(
  p_user_id UUID,
  p_title VARCHAR(500),
  p_description TEXT DEFAULT NULL,
  p_year INTEGER,
  p_priority INTEGER DEFAULT 0,
  p_estimated_hours DECIMAL(5,2) DEFAULT NULL,
  p_due_date DATE DEFAULT NULL
)
Returns INTEGER AS $$
DECLARE
  new_story_id INTEGER;
BEGIN
  -- Validate user exists
  IF NOT EXISTS (SELECT 1 FROM users WHERE id = p_users_id AND is_active = true) THEN
    RAISE EXCEPTION 'User with id % not found or inactive', p_user_id;
  END IF;

  -- validate year is reasonable
  IF p_year < 2000 OR p_year > 3000 THEN
    RAISE EXCEPTION 'Year % is not valid', p_year;
  END IF;

  INSERT INTO stories(
    user_id, title, description, year, priority, estimated_hours, due_date
  ) VALUES (
    p_user_id, p_title, p_description, p_year, p_priority, p_estimated_hours, p_due_date
  ) RETURNING id INTO new_story_id;

  RETURN new_story_id;
END;
$$ language plpgsql;

-- 2. Toggle story completion status
CREATE OR REPLACE FUNCTION sp_toggle_story_completion(
  p_story_id INTEGER,
  p_user_id UUID DEFAULT NULL -- optional: verify ownership
) RETURNS BOOLEAN AS $$
DECLARE
  current_status BOOLEAN;
  new_status BOOLEAN;
  story_user_id UUID;
BEGIN
  -- Get current completion status and verify story exists
  SELECT is_completed, user_id INTO current_status, story_user_id
  FROM stories
  where id = p_story_id;

  IF current_status IS NULL THEN
    RAISE EXCEPTION 'Story with id % not found', p_story_id;
  END IF;

  -- optional: verify user owns this story
  IF p_user_id IS NOT NULL AND story_user_id != p_user_id THEN
    RAISE EXCEPTION 'User % does not own story %', p_user_id, p_story_id;
  END IF;

  new_status := NOT current_status;

  -- update story completion status (trigger handles timestamp)
  UPDATE stories
  SET is_completed = new_status
  WHERE id = p_story_id;

  RETURN new_status;
END;
$$ LANGUAGE plpgsql;

-- 3. Get stories for a user by year
CREATE OR REPLACE FUNCTION sp_get_user_stories_by_year(
    p_user_id UUID,
    p_year INTEGER
)
RETURNS TABLE (
    story_id INTEGER,
    title VARCHAR(500),
    description TEXT,
    year INTEGER,
    is_completed BOOLEAN,
    priority INTEGER,
    estimated_hours DECIMAL(5,2),
    actual_hours DECIMAL(5,2),
    due_date DATE,
    completed_at TIMESTAMP,
    created_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        s.id as story_id,
        s.title,
        s.description,
        s.year,
        s.is_completed,
        s.priority,
        s.estimated_hours,
        s.actual_hours,
        s.due_date,
        s.completed_at,
        s.created_at
    FROM stories s
    WHERE s.user_id = p_user_id 
      AND s.year = p_year
    ORDER BY s.priority DESC, s.created_at ASC;
END;
$$ LANGUAGE plpgsql;

-- 4. Get completion stats for a user by year
CREATE OR REPLACE FUNCTION sp_get_user_year_stats(
    p_user_id UUID,
    p_year INTEGER
)
RETURNS TABLE (
    year INTEGER,
    total_stories BIGINT,
    completed_stories BIGINT,
    completion_percentage DECIMAL(5,2),
    total_estimated_hours DECIMAL(8,2),
    total_actual_hours DECIMAL(8,2)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p_year as year,
        COUNT(*) as total_stories,
        COUNT(*) FILTER (WHERE s.is_completed) as completed_stories,
        CASE 
            WHEN COUNT(*) = 0 THEN 0.00
            ELSE ROUND((COUNT(*) FILTER (WHERE s.is_completed)::DECIMAL / COUNT(*)) * 100, 2)
        END as completion_percentage,
        COALESCE(SUM(s.estimated_hours), 0.00) as total_estimated_hours,
        COALESCE(SUM(s.actual_hours), 0.00) as total_actual_hours
    FROM stories s
    WHERE s.user_id = p_user_id 
      AND s.year = p_year;
END;
$$ LANGUAGE plpgsql;

-- 5. Create a new user (helper function)
CREATE OR REPLACE FUNCTION sp_create_user(
    p_email VARCHAR(255),
    p_display_name VARCHAR(100)
)
RETURNS UUID AS $$
DECLARE
    new_user_id UUID;
BEGIN
    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM users WHERE email = p_email) THEN
        RAISE EXCEPTION 'User with email % already exists', p_email;
    END IF;

    INSERT INTO users (email, display_name)
    VALUES (p_email, p_display_name)
    RETURNING id INTO new_user_id;
    
    RETURN new_user_id;
END;
$$ LANGUAGE plpgsql;
