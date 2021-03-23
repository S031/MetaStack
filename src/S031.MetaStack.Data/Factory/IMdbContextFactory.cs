namespace S031.MetaStack.Data
{
	public interface IMdbContextFactory
	{
		MdbContext GetContext(string connectionName);
	}
}
