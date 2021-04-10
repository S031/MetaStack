using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
	public interface IStartup
	{
		void ConfigureServices(IServiceCollection services);
		void Configure(HostBuilderContext context);
	}

	public static class HostBuilderExtensions
	{
		public static IHostBuilder UseStartup<T>(this IHostBuilder host) where T : IStartup, new()
		{
			var t = new T();
			host.ConfigureServices((context, services) =>
			{
				t.Configure(context);
				t.ConfigureServices(services);
			});
			return host;
		}
	}
}
