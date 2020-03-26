namespace S031.MetaStack.ORM
{
	public interface IJMXFactory
	{
		IJMXRepo CreateJMXRepo();
		IJMXProvider CreateJMXProvider();
		IJMXTypeMapping CreateJMXTypeMapping();
		JMXObject CreateObject(string objectName);
		IJMXFactory SchemaFactory { get; }
	}
}
