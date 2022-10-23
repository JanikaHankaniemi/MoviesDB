using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;
        private readonly MovieService _movieService;

        public MoviesController(
            ILogger<MoviesController> logger,
            MovieService movieService
            )
        {
            _logger = logger;
            _movieService = movieService;
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovies()
        {
            try
            {
                var movies = await _movieService.GetAsync();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when fetching movies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovie(string id)
        {
            try
            {
                var movie = await _movieService.GetAsync(id);
                return Ok(movie);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred when fetching movie {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }
        }

        [HttpPost("Find")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindMovie(SearchTerms searchTerm)
        {
            try
            {
                var movies = await _movieService.FindAsync(searchTerm);
                return Ok(movies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred when finding movies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }
        }


        [HttpPost(Name = "AddMovie")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMovie(Movie newMovie)
        {
            try
            {
                await _movieService.CreateAsync(newMovie);
                return Ok(newMovie);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred when adding movie");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }
        }
    }
}