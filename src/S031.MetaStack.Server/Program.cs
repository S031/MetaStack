using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace S031.MetaStack.Server
{
	public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
				.UseUrls("http://0.0.0.0:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
