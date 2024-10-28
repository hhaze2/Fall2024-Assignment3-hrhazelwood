using System;
namespace Fall2024_Assignment3_hrhazelwood.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? ImdbLink { get; set; }
        public string? Genre { get; set; }
        public required string ReleaseYear { get; set; }
        public byte[]? Media { get; set; }

        public Movie()
		{
		}
	}
}

