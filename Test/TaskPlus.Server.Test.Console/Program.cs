using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlus.Server.Test.Console
{
	class Program
	{
		static void Main()
		{
			System.Console.WriteLine("Start tests");
			_token = Login("svostrikov", "@test").GetAwaiter().GetResult();
			DateTime start = DateTime.Now;
			string result = RequesSpeedTest();
			System.Console.WriteLine($"Finsh tests with {(DateTime.Now - start).TotalSeconds} ms");
			System.Console.WriteLine($"Result: {result}");
			System.Console.ReadLine();
		}
		
		private static string RequesSpeedTest()
		{
			List<Task> ts = new List<Task>(100);
			string result = string.Empty;

			for (int j = 0; j < 1; j++)
			{
				ts.Add(Task.Run(async ()=>
				{
					for (int i = 1; i <= 1; i++)
						result = await RunAsync("cks_test", 
							new JsonObject(){["@ObjectName"] = "Test", ["@IDs"] = "0"}
							.ToString());
				}));
			}
			Task.WaitAll(ts.ToArray());
			return result;
		}

		private static readonly HttpClient _client = new HttpClient();
		private static string _token;

		private static async Task<string> Login(string login, string password)
		{
			JsonObject j = new JsonObject() { ["UserName"] = login, ["Password"] = password };
			var tokenJson = await RunAsync("login", j.ToString());

			j = (JsonObject)new JsonReader(tokenJson).Read();
			var token = (string)j["JwtToken"];
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return _token;
		}
		private static async Task<string> RunAsync(string method, string json)
		{
			// Update port # in the following line.
			var url = new Uri($"http://localhost:5000/api/v.1/{method}");
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _client.PostAsync(url, data);
			return await response.Content.ReadAsStringAsync();
		}
	}
}
