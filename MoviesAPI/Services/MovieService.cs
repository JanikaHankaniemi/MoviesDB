using MoviesAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;
using MongoDB.Bson;

namespace MoviesAPI.Services;

public class MovieService
{
    private readonly IMongoCollection<Movie> _movieCollection;
    private readonly IMongoCollection<Genre> _genreCollection;
    private readonly IMongoDatabase _database;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MovieService> _logger;
    private readonly string _genreCollectionName = "Genres";
    public MovieService(
        IOptions<MongoDBSettings> mongoDBSettings,
        IConfiguration configuration,
        ILogger<MovieService> logger)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        _database = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _movieCollection = _database.GetCollection<Movie>(
            mongoDBSettings.Value.CollectionName);

        _genreCollection = _database.GetCollection<Genre>(_genreCollectionName);
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Movie>> GetAsync(int? nbrOfEntries, int? skip)
    {
        return await _movieCollection.Find(movie => true).Skip(skip).Limit(nbrOfEntries).ToListAsync();
    }

    public async Task<List<Genre>> GetGenresAsync()
    {
        return await _genreCollection.Find(genre => true).ToListAsync();
    }
    public async Task<Movie?> GetAsync(string id) =>
        await _movieCollection.Find(movie => movie._id == new ObjectId(id)).FirstOrDefaultAsync();

    public async Task<List<Movie>> SearchAsync(SearchTerms searchTerms)
    {
        var results = new List<Movie>();
        var filter = Builders<Movie>.Filter.Empty;
        if (searchTerms.FreeText != null)
        {
            var filterTerm = searchTerms.FreeText.ToUpper();
            filter &= Builders<Movie>.Filter.Where(movie =>
                movie.aggregate.ToUpper().Contains(filterTerm));
        }
        if (searchTerms.Genre != null)
        {
            filter &= Builders<Movie>.Filter.Where(movie => movie.genres.Any(genre => genre == searchTerms.Genre));
        }
        if (searchTerms.AgeLimit != null)
        {
            filter &= Builders<Movie>.Filter.Where(movie => movie.ageLimit <= searchTerms.AgeLimit);
        }
        if (searchTerms.Year != null)
        {
            filter &= Builders<Movie>.Filter.Where(movie => movie.year == searchTerms.Year);
        }
        if (searchTerms.Rating != null)
        {
            filter &= Builders<Movie>.Filter.Where(movie => movie.rating == searchTerms.Rating);
        }
        results = await _movieCollection.Find(filter).Skip(searchTerms.Skip).Limit(searchTerms.NbrOfEntries).ToListAsync();
        return results;
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        await UpdateGenres(movie);
        await _movieCollection.InsertOneAsync(movie);
        return movie;
    }
    public async Task UpdateAsync(string id, Movie updatedMovie) =>
        await _movieCollection.ReplaceOneAsync(movie => movie._id == new ObjectId(id), updatedMovie);

    public async Task RemoveAsync(string id) =>
        await _movieCollection.DeleteOneAsync(movie => movie._id == new ObjectId(id));

    public async Task DropCollection()
    {
        await _database.DropCollectionAsync("Movies");
        await _database.DropCollectionAsync("Genres");
    }
    public async Task SeedDB()
    {
        var inputFile = _configuration.GetValue<string>("PathToJSONData");
        //only seed an empty collection
        if (_movieCollection.Find(movie => true).Limit(1).CountDocuments() == 0)
        {
            if (inputFile == null || inputFile.Length == 0)
            {
                _logger.LogError("No input file defined");
                throw new ArgumentNullException("No input file defined");
            }
            try
            {
                string fileName = _configuration.GetValue<string>("PathToJSONData");
                string jsonString = await File.ReadAllTextAsync(fileName);
                List<MovieJson> movies = JsonSerializer.Deserialize<List<MovieJson>>(jsonString)!;
                movies.ForEach(async movieJson =>
                {
                    await CreateAsync(MapJsonToMovie(movieJson));
                });
                var indexBuilder = Builders<Movie>.IndexKeys;
                var indexModel = new CreateIndexModel<Movie>(indexBuilder.Text(x => x.name));
                await _movieCollection.Indexes.CreateOneAsync(indexModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not seed db");
                throw new Exception("Could not seed db");
            }
        }
    }

    private Movie MapJsonToMovie(MovieJson movieJson)
    {
        string directorstr = $"{movieJson.director.firstName} {movieJson.director.lastName}";
        string actorstr = String.Join(", ", movieJson.actors.Select(actor => $"{actor.firstName} {actor.lastName}").ToList());

        return new Movie()
        {
            name = movieJson.name,
            year = movieJson.year,
            synopsis = movieJson.synopsis,
            rating = movieJson.rating,
            ageLimit = movieJson.ageLimit,
            actors = actorstr,
            director = directorstr,
            aggregate = $"{movieJson.name} {movieJson.synopsis} {actorstr} {directorstr}",
            genres = movieJson.genres,
        };
    }
    private async Task UpdateGenres(Movie movie)
    {

        for (int i = 0; i < movie.genres.Count; i++)
        {
            var genre = movie.genres[i];
            var result = _genreCollection.Find(genreEntity => genreEntity.name == genre).Any();
            if (result == false)
            {
                await _genreCollection.InsertOneAsync(new Genre() { name = genre });
            }
        }
    }
}