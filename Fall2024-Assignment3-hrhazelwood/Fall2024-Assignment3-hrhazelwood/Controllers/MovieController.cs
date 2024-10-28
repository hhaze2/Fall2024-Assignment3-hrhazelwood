using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ClientModel;
using System.Text.Json.Nodes;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VaderSharp2;
using Fall2024_Assignment3_hrhazelwood.Data;
using Fall2024_Assignment3_hrhazelwood.Models;

namespace Fall2024_Assignment3_hrhazelwood.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        //private const string ApiKey = "API_KEY";
        private const string ApiEndpoint = "https://fall2024-hrhazelwood-openai.openai.azure.com/";
        private const string AiDeployment = "gpt-35-turbo";
        //private static readonly ApiKeyCredential ApiCredential = new(ApiKey);

        public MovieController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        

        public async Task<IActionResult> GetMoviePhoto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null || movie.Media == null)
            {
                return NotFound();
            }

            var data = movie.Media;
            return File(data, "image/jpg");
        }

        // GET: Movie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var actors = await _context.MovieActor
                .Include(cs => cs.Actor)
                .Where(cs => cs.MovieId == movie.Id)
                .Select(cs => cs.Actor)
                .ToListAsync();

            //section to add the AI reviews
            var secret = _config["OpenAi:API_Key"] ?? throw new Exception("OpenAI:API_Key does not exist in the current Configuration");
            ApiKeyCredential ApiCredential = new(secret);
            ChatClient client = new AzureOpenAIClient(new Uri(ApiEndpoint), ApiCredential).GetChatClient(AiDeployment);

            string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy", "loves classics", "is a social activist", "loves symbolism", "is a casual viewer", "is an intellectual" };
            var messages = new ChatMessage[]
            {
                    new SystemChatMessage($"You represent a group of {personas.Length} film critics who have the following personalities: {string.Join(",", personas)}. When you receive a question, respond as each member of the group. Each response must be separated by a '|'. Do not say which member you are. Do not list your member number. Each response must be separated by a '|'."),
                    new UserChatMessage($"How would you rate the movie {movie.Title} released in {movie.ReleaseYear} out of 10 in 150 words or less?")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
            string[] reviews = result.Value.Content[0].Text.Split('|').Select(s => s.Trim()).ToArray();
            int reviewLength = reviews.Length;

            //foreach (var review in reviews)
            //{
            //    Console.WriteLine("REVIEW");
            //    Console.WriteLine(review);
            //}

            //Console.WriteLine("RAW RESULT");
            //Console.WriteLine(result.Value.Content[0].Text);

            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;

            string[] sentiments = new string[reviewLength];
            for (int i = 0; i < reviews.Length; i++)
            {
                string review = reviews[i];
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);
                sentimentTotal += sentiment.Compound;

                sentiments[i] = sentiment.Compound.ToString();

            }
            //movie.ReviewSentiment = sentiments;

            double sentimentAverage = sentimentTotal / reviews.Length;
            //movie.OverallSentiment = sentimentAverage.ToString();

            var vm = new MovieDetailsViewModel(movie, actors, reviews, sentiments, sentimentAverage.ToString());

            return View(vm);
        }

        // GET: Movie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ImdbLink,Genre,ReleaseYear")] Movie movie, IFormFile? media)
        {
            if (ModelState.IsValid)
            {
                //adding the photo
                if (media != null && media.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    media.CopyTo(memoryStream);
                    movie.Media = memoryStream.ToArray();
                }


                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImdbLink,Genre,ReleaseYear, Media")] Movie movie, IFormFile? media)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (media != null && media.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        media.CopyTo(memoryStream);
                        movie.Media = memoryStream.ToArray();
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
