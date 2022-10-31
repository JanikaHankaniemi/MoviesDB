using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using Moq;
using MoviesAPI.Models;
using MoviesAPI.Services;

namespace MoviesAPI.Tests
{
    [TestClass]
    public class MovieServiceTests
    {
        public TestContext TestContext { get; set; } = null!;
        private static MovieService _movieService = null!;

        private readonly Movie testMovie = new()
        {
            name = "TestMovie",
            synopsis = "TestSynopsis",
            year = 2022,
            rating = 4,
            ageLimit = 12,
            actors = "Testi Testaaja 1, Testi Testaaja 2",
            director = "Testi Testaaja 3",
            genres = new List<string> { "Adventure" }
        };

        [ClassInitialize]
        public static async Task Setup(TestContext context)
        {
            var settings = Options.Create(new MongoDBSettings()
            {
                ConnectionString = (string)context.Properties["ConnectionString"]!,
                DatabaseName = (string)context.Properties["DatabaseName"]!,
                CollectionName = (string)context.Properties["CollectionName"]!,
            });
            ILogger<MovieService> logger = Mock.Of<ILogger<MovieService>>();
            var inMemorySettings = new Dictionary<string, string> {
                {"PathToJSONData", (string)context.Properties["PathToJSONData"]!}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _movieService = new MovieService(settings, configuration, logger);

            await _movieService.DropCollection();
            await _movieService.SeedDB();
        }
        [ClassCleanup]
        public static async Task Cleanup()
        {
            await _movieService.DropCollection();
        }

        [TestMethod]
        public async Task GetMovies()
        {
            var movies = await _movieService.GetAsync(null, null);
            Assert.IsNotNull(movies);            
        }
        [TestMethod]
        public async Task AddMovie()
        {
            Movie? newMovie = null;
            try
            {
                newMovie = await _movieService.CreateAsync(testMovie);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
            }
            Assert.IsNotNull(newMovie);
        }
        [TestMethod]
        public async Task GetMovie()
        {
            Movie? newMovie = null;
            Movie? getResult = null;
            try
            {
                newMovie = await _movieService.CreateAsync(testMovie);
                getResult = await _movieService.GetAsync(newMovie._id.ToString());
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
            }
            Assert.IsNotNull(getResult);
            var getResultJson = JsonConvert.SerializeObject(getResult);
            var newMovieJson = JsonConvert.SerializeObject(newMovie);
            Assert.AreEqual(getResultJson, newMovieJson);
        }
        [TestMethod]
        public async Task FindMovie()
        {
            List<Movie> searchResult = null!;
            try
            {
                _ = await _movieService.CreateAsync(testMovie);
                searchResult = await _movieService.SearchAsync(new SearchTerms
                {
                    FreeText = testMovie.name
                });
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
            }
            Assert.IsNotNull(searchResult);
            Assert.IsTrue(searchResult?.Count > 0);
            await _movieService.RemoveAsync(testMovie._id.ToString());
        }
        [TestMethod]
        public async Task RemoveMovie()
        {
            Movie? removeResult = null;
            try
            {
                Movie? newMovie = await _movieService.CreateAsync(testMovie);
                await _movieService.RemoveAsync(newMovie._id.ToString());
                removeResult = await _movieService.GetAsync(newMovie._id.ToString());
                
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
            }
            Assert.IsNull(removeResult);
        }
    }
}