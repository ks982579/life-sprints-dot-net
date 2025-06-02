using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LifeSprints.Models;
using Npgsql;
using NpgsqlTypes;
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

        void ToggleStoryCompletionAsync();
        void GetUserStoriesByYear();
        void GetUserYearStats();
        void CreateUser();
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
        public async Task<int> CreateStoryAsync(Guid userId, CreateStoryDto createStoryDto)
        {
            using var connection = new NpgsqlConnection(this._connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT sp_create_story(@p_user_id, @p_title, @p_description, @p_year, @p_priority, @p_estimated_hours, @p_due_date)",
                connection);
            command.Parameters.AddWithValue("@p_user_id", userId);
            command.Parameters.AddWithValue("@p_title", createStoryDto.Title);
            command.Parameters.AddWithValue("@p_description", (object?)createStoryDto.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_year", createStoryDto.Year);
            command.Parameters.AddWithValue("@p_priority", createStoryDto.Priority);
            command.Parameters.AddWithValue("@p_estimated_hours", (object?)createStoryDto.EstimatedHours ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_due_date", (object?)createStoryDto.DueDate ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public void ToggleStoryCompletionAsync()
        {
            throw new NotImplementedException();
        }
        public void GetUserStoriesByYear()
        {
            throw new NotImplementedException();
        }
        public void GetUserYearStats()
        {
            throw new NotImplementedException();
        }
        public void CreateUser()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
