using S031.MetaStack.Data;

namespace TaskPlus.Server.Data
{
	public interface IMdbContextFactory
	{
		MdbContext GetContext(string connectionName);
	}
}
