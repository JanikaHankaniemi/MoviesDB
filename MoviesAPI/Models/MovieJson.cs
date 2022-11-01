using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MoviesAPI.Models
{
    public class MovieJson
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string name { get; set; } = null!;
        public int year { get; set; } 
        public List<string> genres { get; set; } = null!;
        public int ageLimit { get; set; }   
        public int rating { get; set; } 
        public List<Person> actors { get; set; } = null!;
        public Person director { get; set; } = null!;
        public string synopsis { get; set; } = null!;
    }
}