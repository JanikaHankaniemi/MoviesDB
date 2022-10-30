using MoviesAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;
using MongoDB.Bson;

namespace MoviesAPI.Services;

public class MovieService
{
    private readonly IMongoCollection<Movie> _movieCollection;
    private readonly IMongoDatabase _database;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MovieService> _logger;

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

        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Movie>> GetAsync(int? nbrOfEntries, int? skip)
    {
        return await _movieCollection.Find(movie => true).Skip(skip).Limit(nbrOfEntries).ToListAsync();
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
                movie.name.ToUpper().Contains(filterTerm) ||
                movie.synopsis.ToUpper().Contains(filterTerm));
        }
        if (searchTerms.Person != null) {
            filter &= Builders<Movie>.Filter.Or(
                    Builders<Movie>.Filter.Where(movie =>
                        movie.actors != null && movie.actors.Any(
                        actor => actor.fullName.ToUpper().Contains(searchTerms.Person))),
                    Builders<Movie>.Filter.Where(movie =>
                        movie.director != null && movie.director.fullName.ToUpper().Contains(searchTerms.Person)));
        }
        if(searchTerms.Genre != null)
        {
            filter &= Builders<Movie>.Filter.Where(movie => movie.genres.Any(genre => genre == searchTerms.Genre));
        }
        if(searchTerms.AgeLimit != null)
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

    public async Task<Movie> CreateAsync(Movie newMovie)
    {
        AddFullNames(newMovie);
        await _movieCollection.InsertOneAsync(newMovie);
        return newMovie;
        
    }
    public async Task UpdateAsync(string id, Movie updatedMovie) =>
        await _movieCollection.ReplaceOneAsync(movie => movie._id == new ObjectId(id), updatedMovie);

    public async Task RemoveAsync(string id) =>
        await _movieCollection.DeleteOneAsync(movie => movie._id == new ObjectId(id));

    public async Task DropCollection()
    {
        await _database.DropCollectionAsync("Movies");
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
                List<Movie> movies = JsonSerializer.Deserialize<List<Movie>>(jsonString)!;
                movies.ForEach(async movie => {
                    AddFullNames(movie);
                    await CreateAsync(movie); 
                });
                var indexBuilder = Builders<Movie>.IndexKeys;
                var indexModel = new CreateIndexModel<Movie>(indexBuilder.Text(x => x.name));
                await _movieCollection.Indexes.CreateOneAsync(indexModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not seed db");
            }
        }
    }
    private static void AddFullNames(Movie movie)
    {
        movie.actors = movie.actors.Select(actor => new Person()
        {
            firstName = actor.firstName,
            lastName = actor.lastName,
            fullName = $"{actor.firstName} {actor.lastName}"
        }).ToList();
        movie.director.fullName = $"{movie.director.firstName} {movie.director.lastName}";
    }
}