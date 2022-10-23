using MoviesAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Text.Json;
using MongoDB.Bson;

namespace MoviesAPI.Services;

public class MovieService
{
    private readonly IMongoCollection<Movie> _movieCollection;
    private readonly IConfiguration _configuration;
    private readonly string _inputFile;

    public MovieService(
        IOptions<MongoDBSettings> mongoDBSettings,
        IConfiguration configuration)
    {
        var mongoClient = new MongoClient(
            mongoDBSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            mongoDBSettings.Value.DatabaseName);

        _movieCollection = mongoDatabase.GetCollection<Movie>(
            mongoDBSettings.Value.CollectionName);

        _configuration = configuration;
        _inputFile = _configuration.GetValue<string>("PathToJSONData");
    }

    public async Task<List<Movie>> GetAsync() =>
        await _movieCollection.Find(new BsonDocument()).ToListAsync();

    public async Task<Movie?> GetAsync(string id) =>
        await _movieCollection.Find(x => x._id == new ObjectId(id)).FirstOrDefaultAsync();

    public async Task<List<Movie>> FindAsync(SearchTerms searchTerms)
    {
        var results = new List<Movie>();
        var filter = Builders<Movie>.Filter.Empty;
        if (searchTerms.FreeText != null)
        {
            var filterTerm = searchTerms.FreeText.ToUpper();
            filter &= Builders<Movie>.Filter.Or(
                Builders<Movie>.Filter.Where(x => x.name.ToUpper().Contains(filterTerm)),
                Builders<Movie>.Filter.Where(x => x.synopsis.ToUpper().Contains(filterTerm)),
                Builders<Movie>.Filter.Where(x => x.actors != null && x.actors.Any(
                    y => y.firstName != null && y.firstName.ToUpper().Contains(searchTerms.FreeText) ||
                    y.lastName != null && y.lastName.ToUpper().Contains(searchTerms.FreeText))),
                Builders<Movie>.Filter.Where(x => x.director.firstName.ToUpper().Contains(searchTerms.FreeText)
                || x.director.lastName.ToUpper().Contains(searchTerms.FreeText)));
        }
        if(searchTerms.AgeLimit != null)
        {
            filter &= Builders<Movie>.Filter.Where(x => x.ageLimit == searchTerms.AgeLimit);
        }
        if (searchTerms.Year != null)
        {
            filter &= Builders<Movie>.Filter.Where(x => x.year == searchTerms.Year);
        }
        if (searchTerms.Rating != null)
        {
            filter &= Builders<Movie>.Filter.Where(x => x.rating == searchTerms.Rating);
        }
        results = await _movieCollection.Find(filter).ToListAsync();

        return results;
    }

    public async Task CreateAsync(Movie newMovie) =>
        await _movieCollection.InsertOneAsync(newMovie);

    public async Task UpdateAsync(string id, Movie updatedMovie) =>
        await _movieCollection.ReplaceOneAsync(x => x._id == new ObjectId(id), updatedMovie);

    public async Task RemoveAsync(string id) =>
        await _movieCollection.DeleteOneAsync(x => x._id == new ObjectId(id));

    public async Task SeedDB()
    {
        string fileName = _configuration.GetValue<string>("PathToJSONData");
        string jsonString = await File.ReadAllTextAsync(fileName);
        List<Movie> movies = JsonSerializer.Deserialize<List<Movie>>(jsonString)!;
        movies.ForEach(async movie => await CreateAsync(movie));
    }
}