using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.ComponentModel.DataAnnotations;


namespace LifeSprints.Data
{
    #region Data Transfer Objects
    /// <summary>
    /// DTO for creating a new story.
    /// The contract of information to send to database to create the thing.
    /// </summary>
    public class CreateStoryDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public int Year { get; set; }
        [Range(0, 3)]
        public int Priority { get; set; } = 0;
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// DTO for story summary (lightweight view)
    /// </summary>
    public class StoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsCompleted { get; set; }
        public int Priority { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for user year statistics
    /// </summary>
    public class YearStatsDto
    {
        public int Year { get; set; }
        public long TotalStories { get; set; }
        public long CompletedStories { get; set; }
        public decimal CompletionPercentage { get; set; }
        public decimal TotalEstimatedHours { get; set; }
        public decimal TotalActualHours { get; set; }
    }

    /// <summary>
    /// DTO for user year statistics
    /// </summary>
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; }
    }
    #endregion

    public interface IStoredProcedureService
    {
        Task<int> CreateStoryAsync(Guid userId, CreateStoryDto createStoryDto);
        Task<bool> ToggleStoryCompletionAsync(int storyId, Guid? userId = null);
        Task<List<StoryDto>> GetUserStoriesByYearAsync(Guid userId, int year);
        Task<YearStatsDto> GetUserYearStatsAsync(Guid userId, int year);
        Task<Guid> CreateUserAsync(CreateUserDto createUserDto);
    }

    public class StoredProcedureService : IStoredProcedureService
    {

        #region Properties
        private readonly LifeSprintDbContext _context;
        private readonly string _connectionString;
        #endregion

        #region Constructors
        // Microsoft.Extensions.Configuration::IConfiguration
        public StoredProcedureService(LifeSprintDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("Connection String 'DefaultConnection' Not Found.");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new story using stored procedure
        /// </summary>
        public async Task<int> CreateStoryAsync(Guid userId, CreateStoryDto createStoryDto)
        {
            using var connection = new NpgsqlConnection(this._connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT sp_create_story(@p_user_id, @p_title, @p_year, @p_description, @p_priority, @p_estimated_hours, @p_due_date)",
                connection);
            command.Parameters.AddWithValue("@p_user_id", userId);
            command.Parameters.AddWithValue("@p_title", createStoryDto.Title);
            command.Parameters.AddWithValue("@p_year", createStoryDto.Year);
            command.Parameters.AddWithValue("@p_description", (object?)createStoryDto.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_priority", createStoryDto.Priority);
            command.Parameters.AddWithValue("@p_estimated_hours", (object?)createStoryDto.EstimatedHours ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_due_date", (object?)createStoryDto.DueDate ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }


        /// <summary>
        /// Toggle story completion status
        /// </summary>
        public async Task<bool> ToggleStoryCompletionAsync(int storyId, Guid? userId = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT sp_toggle_story_completion(@p_story_id, @p_user_id)",
                connection);

            command.Parameters.AddWithValue("@p_story_id", storyId);
            command.Parameters.AddWithValue("@p_user_id", (object?)userId ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Get all stories for a user in a specific year
        /// </summary>
        public async Task<List<StoryDto>> GetUserStoriesByYearAsync(Guid userId, int year)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT * FROM sp_get_user_stories_by_year(@p_user_id, @p_year)",
                connection);

            command.Parameters.AddWithValue("@p_user_id", userId);
            command.Parameters.AddWithValue("@p_year", year);

            using var reader = await command.ExecuteReaderAsync();
            var stories = new List<StoryDto>();

            while (await reader.ReadAsync())
            {
                var story = new StoryDto
                {
                    Id = reader.GetInt32("story_id"),
                    Title = reader.GetString("title"),
                    Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                    Year = reader.GetInt32("year"),
                    IsCompleted = reader.GetBoolean("is_completed"),
                    Priority = reader.GetInt32("priority"),
                    EstimatedHours = reader.IsDBNull("estimated_hours") ? null : reader.GetDecimal("estimated_hours"),
                    ActualHours = reader.IsDBNull("actual_hours") ? null : reader.GetDecimal("actual_hours"),
                    DueDate = reader.IsDBNull("due_date") ? null : reader.GetDateTime("due_date"),
                    CompletedAt = reader.IsDBNull("completed_at") ? null : reader.GetDateTime("completed_at"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
                stories.Add(story);
            }

            return stories;
        }

        /// <summary>
        /// Get completion statistics for a user's year
        /// </summary>
        public async Task<YearStatsDto> GetUserYearStatsAsync(Guid userId, int year)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT * FROM sp_get_user_year_stats(@p_user_id, @p_year)",
                connection);

            command.Parameters.AddWithValue("@p_user_id", userId);
            command.Parameters.AddWithValue("@p_year", year);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new YearStatsDto
                {
                    Year = reader.GetInt32("year"),
                    TotalStories = reader.GetInt64("total_stories"),
                    CompletedStories = reader.GetInt64("completed_stories"),
                    CompletionPercentage = reader.GetDecimal("completion_percentage"),
                    TotalEstimatedHours = reader.GetDecimal("total_estimated_hours"),
                    TotalActualHours = reader.GetDecimal("total_actual_hours")
                };
            }

            // Return empty stats if no data found
            return new YearStatsDto
            {
                Year = year,
                TotalStories = 0,
                CompletedStories = 0,
                CompletionPercentage = 0,
                TotalEstimatedHours = 0,
                TotalActualHours = 0
            };
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<Guid> CreateUserAsync(CreateUserDto createUserDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT sp_create_user(@p_email, @p_display_name)",
                connection);

            command.Parameters.AddWithValue("@p_email", createUserDto.Email);
            command.Parameters.AddWithValue("@p_display_name", createUserDto.DisplayName);

            var result = await command.ExecuteScalarAsync();
            return (Guid)result!;
        }
        #endregion
    }
}
