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

        // GET api/Movies/
        /// <summary>
        /// Fetches movies from DB
        /// </summary>
        /// <param name="nbrOfEntries">Number of entries to return</param>
        /// <param name="skip">Skip number of entries from collection</param>
        /// <returns>List of movies</returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovies(int? nbrOfEntries, int? skip)
        {
            try
            {
                var movies = await _movieService.GetAsync(nbrOfEntries, skip);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when fetching movies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }

        }

        // GET api/Movies/Genres
        /// <summary>
        /// Fetches genres from DB
        /// </summary>
        /// <param name="nbrOfEntries">Number of entries to return</param>
        /// <param name="skip">Skip number of entries from collection</param>
        /// <returns>List of movies</returns>
        [HttpGet("Genres")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var genres = await _movieService.GetGenresAsync();
                return Ok(genres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when fetching genres");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }

        }

        // GET api/Movies/635bbf32bd4bd86899e1f02f
        /// <summary>
        /// Fetches movie with ID from DB
        /// </summary>
        /// <param name="id">Id of the movie</param>
        /// <returns>Movie</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovie(string id)
        {
            if (id == null) { return BadRequest("No id provided"); }
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

        /// <summary>
        /// Fetches movies from DB matching the searchTerm object
        /// </summary>
        /// <param name="searchTerm">searchTerm object</param>
        /// <returns>Movies</returns>
        [HttpGet("Search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchMovies(
            string? FreeText,
            string? Person,
            string? Genre,
            int? AgeLimit,
            string? Year,
            int? Rating,
            int? Skip, 
            int? NbrOfEntries
            )
        {
                SearchTerms searchTerms = new()
            {
                FreeText = FreeText,
                Person = Person,
                Genre = Genre,
                AgeLimit = AgeLimit,
                Year = Year,
                Rating = Rating,
                Skip = Skip,
                NbrOfEntries = NbrOfEntries
            };
            try
            {
                IList<Movie> movies;
                if (searchTerms == null)
                {
                    movies = await _movieService.GetAsync(null, null);
                }
                else
                {
                    movies = await _movieService.SearchAsync(searchTerms);
                }
                return Ok(movies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when finding movies");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }
        }

        //POST api/Movies
        /// <summary>
        /// Add a movies to the collection
        /// </summary>
        /// <param name="newMovie">Movie type object</param>
        /// <returns>Added movie entry</returns>
        [HttpPost(Name = "AddMovie")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMovie(Movie newMovie)
        {
            if(newMovie == null) { return BadRequest("Movie not defined"); }

            try
            {
                var updatedMovie = await _movieService.CreateAsync(newMovie);
                return Ok(updatedMovie);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when adding movie");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error occurred" });
            }
        }
    }
}