using Microsoft.AspNetCore.Mvc;
using LifeSprints.Data;
using LifeSprints.Models;

namespace LifeSprints.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IStoredProcedureService _storedProcedureService;

        public StoriesController(IStoredProcedureService storedProcedureService)
        {
            _storedProcedureService = storedProcedureService;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateStory([FromBody] CreateStoryRequest request)
        {
            try
            {
                var createStoryDto = new CreateStoryDto
                {
                    Title = request.Title,
                    Description = request.Description,
                    Year = request.Year,
                    Priority = request.Priority,
                    EstimatedHours = request.EstimatedHours,
                    DueDate = request.DueDate
                };

                var storyId = await _storedProcedureService.CreateStoryAsync(request.UserId, createStoryDto);
                return Ok(storyId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{storyId}/toggle")]
        public async Task<ActionResult<bool>> ToggleStoryCompletion(int storyId, [FromQuery] Guid? userId = null)
        {
            try
            {
                var result = await _storedProcedureService.ToggleStoryCompletionAsync(storyId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}/year/{year}")]
        public async Task<ActionResult<List<StoryDto>>> GetUserStoriesByYear(Guid userId, int year)
        {
            try
            {
                var stories = await _storedProcedureService.GetUserStoriesByYearAsync(userId, year);
                return Ok(stories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}/year/{year}/stats")]
        public async Task<ActionResult<YearStatsDto>> GetUserYearStats(Guid userId, int year)
        {
            try
            {
                var stats = await _storedProcedureService.GetUserYearStatsAsync(userId, year);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("user")]
        public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var userId = await _storedProcedureService.CreateUserAsync(createUserDto);
                return Ok(userId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreateStoryRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Year { get; set; }
        public int Priority { get; set; } = 0;
        public decimal? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
