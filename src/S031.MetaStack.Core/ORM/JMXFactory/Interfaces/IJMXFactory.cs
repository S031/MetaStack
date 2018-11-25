#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXFactory
	{
		IJMXRepo CreateJMXRepo();
		IJMXProvider CreateJMXProvider();
		IJMXTypeMapping CreateJMXTypeMapping();
		JMXObject CreateObject(string objectName);
	}
}
