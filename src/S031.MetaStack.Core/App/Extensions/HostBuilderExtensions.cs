namespace Microsoft.Extensions.Hosting
{
	public static class HostBuilderExtensions
	{
		public static IHostBuilder UseStartup<T>(this IHostBuilder host) where T : new()
		{
			return host;
		}
	}
}
