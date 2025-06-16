using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;
using System.Net;
using LifeSprints.Data;
using LifeSprints.Api.Controllers;
using Testcontainers.PostgreSql;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace LifeSprints.Tests
{
    public class StoriesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly PostgreSqlContainer _postgres;

        public StoriesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .WithDatabase("test_db")
                .WithUsername("test_user")
                .WithPassword("test_pass")
                .Build();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing database configuration
                    services.RemoveAll(typeof(IStoredProcedureService));
                    
                    // Add test database configuration
                    services.AddScoped<IStoredProcedureService>(provider =>
                    {
                        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>
                            {
                                {"ConnectionStrings:DefaultConnection", _postgres.GetConnectionString()}
                            })
                            .Build();
                        
                        return new StoredProcedureService(null!, config);
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            await SetupDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _postgres.DisposeAsync();
        }

        private async Task SetupDatabaseAsync()
        {
            using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
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
        public async Task CreateUser_ShouldReturnOk_WhenValidData()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "integration@example.com",
                DisplayName = "Integration Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/stories/user", createUserDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, userId);
        }

        [Fact]
        public async Task CreateStory_ShouldReturnOk_WhenValidData()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var createStoryRequest = new CreateStoryRequest
            {
                UserId = userId,
                Title = "Integration Test Story",
                Description = "Test Description",
                Year = 2025,
                Priority = 1,
                EstimatedHours = 8.0m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/stories", createStoryRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var storyId = await response.Content.ReadFromJsonAsync<int>();
            Assert.True(storyId > 0);
        }

        [Fact]
        public async Task ToggleStoryCompletion_ShouldReturnOk_WhenValidStoryId()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var storyId = await CreateTestStoryAsync(userId);

            // Act
            var response = await _client.PatchAsync($"/api/stories/{storyId}/toggle?userId={userId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            Assert.True(result);
        }

        [Fact]
        public async Task GetUserStoriesByYear_ShouldReturnStories_WhenStoriesExist()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var storyId = await CreateTestStoryAsync(userId);

            // Act
            var response = await _client.GetAsync($"/api/stories/user/{userId}/year/2025");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stories = await response.Content.ReadFromJsonAsync<List<StoryDto>>();
            Assert.NotNull(stories);
            Assert.Single(stories);
            Assert.Equal("Integration Test Story", stories[0].Title);
        }

        [Fact]
        public async Task GetUserYearStats_ShouldReturnStats_WhenStoriesExist()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            await CreateTestStoryAsync(userId);
            var completedStoryId = await CreateTestStoryAsync(userId);
            
            // Complete one story
            await _client.PatchAsync($"/api/stories/{completedStoryId}/toggle?userId={userId}", null);

            // Act
            var response = await _client.GetAsync($"/api/stories/user/{userId}/year/2025");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<Guid> CreateTestUserAsync()
        {
            var createUserDto = new CreateUserDto
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                DisplayName = "Test User"
            };

            var response = await _client.PostAsJsonAsync("/api/stories/user", createUserDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        private async Task<int> CreateTestStoryAsync(Guid userId)
        {
            var createStoryRequest = new CreateStoryRequest
            {
                UserId = userId,
                Title = "Integration Test Story",
                Description = "Test Description",
                Year = 2025,
                Priority = 1,
                EstimatedHours = 5.5m
            };

            var response = await _client.PostAsJsonAsync("/api/stories", createStoryRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}