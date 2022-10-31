using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MoviesAPI.Models
{
    public class Genre
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string name { get; set; } = null!;
    }
}