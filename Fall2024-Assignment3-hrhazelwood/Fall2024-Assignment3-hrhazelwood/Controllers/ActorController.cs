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
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        //private const string ApiKey = "API_KEY";
        private const string ApiEndpoint = "https://fall2024-hrhazelwood-openai.openai.azure.com/";
        private const string AiDeployment = "gpt-35-turbo";
        //private static readonly ApiKeyCredential ApiCredential = new(ApiKey);

        public ActorController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> GetActorPhoto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }

            var data = actor.Photo;
            return File(data, "image/jpg");
        }

        // GET: Actor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.MovieActor
                .Include(cs => cs.Movie)
                .Where(cs => cs.ActorId == actor.Id)
                .Select(cs => cs.Movie)
                .ToListAsync();

            //AI generation
            var secret = _config["OpenAi:API_Key"] ?? throw new Exception("OpenAI:API_Key does not exist in the current Configuration");
            ApiKeyCredential ApiCredential = new(secret);
            ChatClient client = new AzureOpenAIClient(new Uri(ApiEndpoint), ApiCredential).GetChatClient(AiDeployment);

            var messages = new ChatMessage[]
            {
                    new SystemChatMessage($"You represent the Twitter social media platform. Generate an answer with a valid JSON formatted array of objects containing the tweet and username. The response must start with [."),
                    new UserChatMessage($"Generate 20 tweets from a variety of users about the actor {actor.Name}. Start your response with [.")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);

            string tweetsJsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";
            //Console.WriteLine(tweetsJsonString);
            JsonArray json = JsonNode.Parse(tweetsJsonString)!.AsArray();

            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;

            var tweets = json.Select(t => new { Username = t!["username"]?.ToString() ?? "", Text = t!["tweet"]?.ToString() ?? "" }).ToArray();
            string[] stringTweets = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            //string[] sentiments = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            string[] sentiments = new string[stringTweets.Length];
            int i = 0;
            foreach (var tweet in tweets)
            //for (int i = 0; i < 20; i++)
            { 
                //tweet = tweets[i];
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
                sentimentTotal += sentiment.Compound;

                stringTweets[i] = tweet.Username + ": " + tweet.Text;
                sentiments[i] = sentiment.Compound.ToString();
                i++;
                //Console.WriteLine($"{tweet.Username}: \"{tweet.Text}\" (sentiment {sentiment.Compound})\n");
            }
            //actor.Tweets = stringTweets;
            //actor.TweetSentiment = sentiments;

            double sentimentAverage = sentimentTotal / tweets.Length;
            //actor.OverallSentiment = sentimentAverage.ToString();
            //Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");

            var vm = new ActorDetailsViewModel(actor, movies, stringTweets, sentiments, sentimentAverage.ToString());

            return View(vm);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Age,ImdbLink")] Actor actor, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }


                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tweets,TweetSentiment,OverallSentiment,Name,Age,ImdbLink")] Actor actor, IFormFile? photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //same issue as in movie with resetting the photo, technically if they hit back to list it wont reset but i dont trust the users
                    //if (photo != null && photo.Length > 0)
                    //{
                    //    using var memoryStream = new MemoryStream();
                    //    photo.CopyTo(memoryStream);
                    //    actor.Photo = memoryStream.ToArray();
                    //}
                    
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
