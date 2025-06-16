using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using LifeSprints.Data;
using Testcontainers.PostgreSql;
using Npgsql;

namespace LifeSprints.Tests
{
    public class StoredProcedureServiceTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        private StoredProcedureService _service;
        private string _connectionString;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            _connectionString = _postgres.GetConnectionString();

            // Setup database schema and stored procedures
            await SetupDatabaseAsync();

            // Create service
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetConnectionString("DefaultConnection"))
                     .Returns(_connectionString);

            var mockContext = new Mock<LifeSprintDbContext>();
            _service = new StoredProcedureService(mockContext.Object, mockConfig.Object);
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private async Task SetupDatabaseAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Create schema
            var schemaScript = await File.ReadAllTextAsync("../../../../../database/init/01_schema.sql");
            using var schemaCommand = new NpgsqlCommand(schemaScript, connection);
            await schemaCommand.ExecuteNonQueryAsync();

            // Create stored procedures
            var spScript = await File.ReadAllTextAsync("../../../../../database/init/02_stored_procedures.sql");
            using var spCommand = new NpgsqlCommand(spScript, connection);
            await spCommand.ExecuteNonQueryAsync();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnUserId_WhenValidData()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "test@example.com",
                DisplayName = "Test User"
            };

            // Act
            var userId = await _service.CreateUserAsync(createUserDto);

            // Assert
            Assert.NotEqual(Guid.Empty, userId);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenDuplicateEmail()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "duplicate@example.com",
                DisplayName = "Test User 1"
            };

            await _service.CreateUserAsync(createUserDto);

            var duplicateDto = new CreateUserDto
            {
                Email = "duplicate@example.com",
                DisplayName = "Test User 2"
            };

            // Act & Assert
            await Assert.ThrowsAsync<PostgresException>(
                () => _service.CreateUserAsync(duplicateDto));
        }

        [Fact]
        public async Task CreateStoryAsync_ShouldReturnStoryId_WhenValidData()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var createStoryDto = new CreateStoryDto
            {
                Title = "Test Story",
                Description = "Test Description",
                Year = 2025,
                Priority = 1,
                EstimatedHours = 5.5m
            };

            // Act
            var storyId = await _service.CreateStoryAsync(userId, createStoryDto);

            // Assert
            Assert.True(storyId > 0);
        }

        [Fact]
        public async Task ToggleStoryCompletionAsync_ShouldReturnTrue_WhenMarkingIncompleteStoryAsComplete()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var storyId = await CreateTestStoryAsync(userId);

            // Act
            var result = await _service.ToggleStoryCompletionAsync(storyId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetUserStoriesByYearAsync_ShouldReturnStories_WhenStoriesExist()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var storyId = await CreateTestStoryAsync(userId);

            // Act
            var stories = await _service.GetUserStoriesByYearAsync(userId, 2025);

            // Assert
            Assert.Single(stories);
            Assert.Equal("Test Story", stories[0].Title);
        }

        [Fact]
        public async Task GetUserYearStatsAsync_ShouldReturnStats_WhenStoriesExist()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            await CreateTestStoryAsync(userId);
            await CreateTestStoryAsync(userId, isCompleted: true);

            // Act
            var stats = await _service.GetUserYearStatsAsync(userId, 2025);

            // Assert
            Assert.Equal(2, stats.TotalStories);
            Assert.Equal(1, stats.CompletedStories);
            Assert.Equal(50.00m, stats.CompletionPercentage);
        }

        private async Task<Guid> CreateTestUserAsync()
        {
            var createUserDto = new CreateUserDto
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                DisplayName = "Test User"
            };
            return await _service.CreateUserAsync(createUserDto);
        }

        private async Task<int> CreateTestStoryAsync(Guid userId, bool isCompleted = false)
        {
            var createStoryDto = new CreateStoryDto
            {
                Title = "Test Story",
                Description = "Test Description",
                Year = 2025,
                Priority = 1,
                EstimatedHours = 5.5m
            };

            var storyId = await _service.CreateStoryAsync(userId, createStoryDto);

            if (isCompleted)
            {
                await _service.ToggleStoryCompletionAsync(storyId, userId);
            }

            return storyId;
        }
    }
}