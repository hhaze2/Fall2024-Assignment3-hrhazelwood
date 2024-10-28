using System;
namespace Fall2024_Assignment3_hrhazelwood.Models
{
	public class Actor
	{
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Age { get; set; }
        public string? ImdbLink { get; set; }
        public byte[]? Photo { get; set; }

        public Actor()
		{
		}
	}
}

