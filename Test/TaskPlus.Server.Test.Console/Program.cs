using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlus.Server.Test.Console
{
	class Program
	{
		private static JsonObject _appSettings;
		private static string _baseUrl;
		static void Main()
		{
			_appSettings = (JsonObject)new JsonReader(File.ReadAllText("Test.App.json")).Read()?["AppSettings"];
			_baseUrl = (string)_appSettings?["BaseUrl"] ?? "http://localhost:5000";

			System.Console.WriteLine($"Start tests on {_baseUrl}");
			Login("svostrikov@metib.ru", "@test").GetAwaiter().GetResult();
			DateTime start = DateTime.Now;

			string result;
			result = RequesSpeedTest();
			System.Console.WriteLine($"Finsh RequesSpeedTest (100 calls from 100 clients) with {(DateTime.Now - start).TotalSeconds} ms\nResult: {result}\n\n");

			//result = Test("GetSettingsSpeedTest", 100000);
			//System.Console.WriteLine($"Finsh GetSettingsSpeedTest tests with Result:\n{result}\n\n");

			//result = Test("RecursiveCallExecuteAsync", 100000);
			//System.Console.WriteLine($"Finsh RecursiveCallExecuteAsync tests with Result:\n{result}\n\n");

			//start = DateTime.Now;
			//result = PerformClientSearchTest();
			//System.Console.WriteLine($"Finsh PerformClientSearchTest with {(DateTime.Now - start).TotalSeconds} ms\nResult: {result}\n\n");
			//System.Console.ReadLine();

			//start = DateTime.Now;
			//result = GetClientInfo();
			//System.Console.WriteLine($"Finsh GetClientInfo with {(DateTime.Now - start).TotalSeconds} ms\nResult: {result}\n\n");
			System.Console.ReadLine();

		}
		
		private static readonly HttpClient _client = new HttpClient();
		
		private static async Task<string> Login(string login, string password)
		{
			JsonObject j = new JsonObject() { ["UserName"] = login, ["Password"] = password };
			var tokenJson = await RunAsync("login", j.ToString());

			j = (JsonObject)new JsonReader(tokenJson).Read();
			var token = (string)j["JwtToken"];
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return token;
		}
		
		private static string RequesSpeedTest()
		{
			List<Task> ts = new List<Task>(100);
			string result = string.Empty;

			for (int j = 0; j < 100; j++)
			{
				ts.Add(Task.Run(async ()=>
				{
					for (int i = 1; i <= 100; i++)
						result = await RunAsync("cks_test", 
							new JsonObject(){["@ObjectName"] = "Test", ["@IDs"] = "0"}
							.ToString());
				}));
			}
			Task.WaitAll(ts.ToArray());
			return result;
		}
		
		private static string PerformClientSearchTest()
		{
			JsonArray a = new JsonArray
			{
				new JsonObject() { ["@Key"] = "inn", ["@Value"] = "7714606819" }
			};
			return RunAsync("cks_perform_client_search", a.ToString())
				.GetAwaiter()
				.GetResult();
		}

		private static string GetClientInfo()
		{
			JsonArray a = new JsonArray
			{
				new JsonObject() { ["@Key"] = "id", ["@Value"] = 1872024L }
			};
			return RunAsync("cks_get_client_info", a.ToString())
				.GetAwaiter()
				.GetResult();
		}

		private static string Test(string testID, int loopCount)
		{
			return RunAsync("cks_test", new JsonObject() { ["@ObjectName"] = testID, ["@IDs"] = $"{loopCount}" }.ToString())
				.GetAwaiter()
				.GetResult();
		}

		private static async Task<string> RunAsync(string method, string json)
		{
			// Update port # in the following line.
			var url = new Uri($"{_baseUrl}/api/v1/{method}");
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _client.PostAsync(url, data);
			return await response.Content.ReadAsStringAsync();
		}
	}
}
