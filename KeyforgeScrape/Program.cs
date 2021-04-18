using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KeyforgeScrape
{

    class DeckData
    {
        public string id;
        public string name;
        public List<string> cards;
    }

    class ResponseData
    {
        public List<DeckData> data;
    }

    class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task<ResponseData> GetDecksOfUser(string userId, string token, int page, int pageSize)
        {
            var domain = "https://www.keyforgegame.com";
            var apiRequest = $"/api/users/{userId}/decks/";
            var param = $"page={page+1}&page_size={pageSize}&search=&power_level=0,11&chains=0,24&only_favorites=0&ordering=-date";
            var url = $"{domain}{apiRequest}?{param}";

            try
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", token);
                var json = await client.GetStringAsync(url);
                var decks = JsonConvert.DeserializeObject<ResponseData>(json);
                return decks;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return null;
        }

        static async Task AddDeckToDecksOfKeyforge(string deckId, string token)
        {
            try
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", token);
                var url = $"https://decksofkeyforge.com/api/decks/{deckId}/import-and-add";
                var response = await client.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Adding deck {deckId}, response {content}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception! Probably deck already added");
            }
        }

        static async Task DoThings()
        {
            Console.WriteLine("Paste mastervault user id (XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX): ");
            var userId = Console.ReadLine();

            Console.WriteLine("Paste mastervault token (Token ...): ");
            var token = Console.ReadLine();

            Console.WriteLine("Paste decks of keyforge token (Bearer ...): ");
            var deckOfKeyforgeToken = Console.ReadLine();
            var response = await GetDecksOfUser(userId, token, 0, 30);

            if (response == null) return;
            var decks = response.data;

            foreach(var deck in decks)
            {
                Console.WriteLine($"{deck.id} -> {deck.name}");
                await AddDeckToDecksOfKeyforge(deck.id, deckOfKeyforgeToken);
            }

        }

        static void Main(string[] args)
        {
            DoThings().Wait();
            Console.ReadKey();
        }
    }
}
