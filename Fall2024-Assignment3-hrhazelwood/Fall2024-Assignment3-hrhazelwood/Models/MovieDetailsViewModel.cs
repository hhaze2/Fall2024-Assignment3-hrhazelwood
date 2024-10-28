using System;
namespace Fall2024_Assignment3_hrhazelwood.Models
{
	public class MovieDetailsViewModel
	{
        public Movie Movie { get; set; }
        public IEnumerable<Actor> Actors { get; set; }
        public string[]? Reviews { get; set; }
        public string[]? ReviewSentiment { get; set; }
        public string? OverallSentiment { get; set; }

        public MovieDetailsViewModel(Movie movie, IEnumerable<Actor> actors, string[] reviews, string[] reviewSentiments, string overallSentiment)
		{
			Movie = movie;
			Actors = actors;
			Reviews = reviews;
			ReviewSentiment = reviewSentiments;
			OverallSentiment = overallSentiment;
		}
	}
}

