using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace S031.MetaStack.AppServer
{
	public class Startup : IStartup
	{
		void IStartup.Configure(HostBuilderContext context)
		{
			//throw new NotImplementedException
		}

		void IStartup.ConfigureServices(IServiceCollection services)
		{
			//throw new NotImplementedException();
		}
	}
}
