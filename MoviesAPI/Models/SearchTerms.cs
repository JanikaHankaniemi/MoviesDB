using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MoviesAPI.Models
{
    public class SearchTerms
    {
        public string? FreeText { get; set; }
        public string? Person { get; set; }
        public int? Year { get; set; }
        public string? Genre { get; set; }
        public int? AgeLimit { get; set; }  
        public int? Rating { get; set; } 
    }
}