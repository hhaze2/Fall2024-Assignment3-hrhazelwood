using System;
namespace Fall2024_Assignment3_hrhazelwood.Models
{
	public class ActorDetailsViewModel
	{
        public Actor Actor { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public string[]? Tweets { get; set; }
        public string[]? TweetSentiment { get; set; }
        public string? OverallSentiment { get; set; }

        public ActorDetailsViewModel(Actor actor, IEnumerable<Movie> movies, string[] tweets, string[] tweetSentiment, string overallSentiment)
        { 
            Actor = actor;
            Movies = movies;
            Tweets = tweets;
            TweetSentiment = tweetSentiment;
            OverallSentiment = overallSentiment;
        }
	}
}

