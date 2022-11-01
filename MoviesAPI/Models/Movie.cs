using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MoviesAPI.Models
{
    public class Movie
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string name { get; set; } = null!;
        public string year { get; set; } = null!;
        public List<string> genres { get; set; } = null!;
        public int ageLimit { get; set; }  
        public int rating { get; set; } 
        public string actors { get; set; } = null!;
        public string director { get; set; } = null!;
        public string synopsis { get; set; } = null!;
        public string? aggregate { get; set; } = null!;
    }
}