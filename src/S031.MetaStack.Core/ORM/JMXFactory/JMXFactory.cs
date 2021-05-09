using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Properties;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System;
using System.Linq;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXFactory : IJMXFactory
	{
		public const string DETAIL_FIELD_PREFIX = "$1_";
		/// <summary>
		/// !!! Статическое хранение MdbContext имеет смысл, только в случае локального соединения, типа SQLite
		/// или добавить в MdbContext reconnect если соединение разорвано
		/// </summary>
		private static JMXFactory _schemaFactory;
		private ILogger _logger;
		private MdbContext _mdb;
		private IServiceProvider _services;

		public IServiceProvider Services => _services;

		public virtual ILogger Logger => _logger;

		public virtual IJMXFactory WorkFactory => this;

		public IJMXFactory SchemaFactory => _schemaFactory;

		public abstract IJMXRepo CreateJMXRepo();

		public abstract IJMXProvider CreateJMXProvider();

		public abstract JMXObject CreateObject(string objectName);

		public abstract IJMXBalance CreateJMXBalance();

		public static JMXFactory Create(IServiceProvider services, MdbContext workMdbContext)
		{
			if (_schemaFactory == null)
			{
				var mdbFactory = services.GetRequiredService<IMdbContextFactory>();
				var sysCatMdb = mdbFactory.GetContext(Strings.SysCatConnection);
				_schemaFactory = CreateFactoryFromMdbContext(services, sysCatMdb);
			}
			return CreateFactoryFromMdbContext(services, workMdbContext);
		}

		public static JMXFactory Create(IServiceProvider services)
		{
			var mdbFactory = services.GetRequiredService<IMdbContextFactory>();
			var mdb = mdbFactory.GetContext(Strings.DefaultConnection);
			return CreateFactoryFromMdbContext(services, mdb);
		}

		private static JMXFactory CreateFactoryFromMdbContext(IServiceProvider services, MdbContext mdb)
		{
			
			var l = ImplementsList.GetTypes(typeof(JMXFactory));
			if (l == null)
				//No class inherited from JMXFactory defined
				throw new InvalidOperationException(Strings.S031_MetaStack_Core_ORM_JMXFactory_Create_1);

			string dbProviderName = mdb.ProviderName;
			JMXFactory factory = null;
			foreach (var t in l)
			{
				if (System.Attribute.GetCustomAttributes(t)?
					.FirstOrDefault(attr => attr.GetType() == typeof(DBRefAttribute)
						&& (attr as DBRefAttribute)
							.DBProviderName
							.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase)) is DBRefAttribute att)
				{
					factory = (JMXFactory)t.CreateInstance(services, mdb);
					break;
				}
			}
			if (factory == null)
				//No class inherited from JMXFactory contained attribute of type DBRefAttribute  defined
				throw new InvalidOperationException(Properties.Strings.S031_MetaStack_Core_ORM_JMXFactory_Create_2);
			factory._services = services;
			factory._mdb = mdb;
			factory._logger = services
				.GetRequiredService<ILoggerProvider>()
				.CreateLogger(typeof(JMXFactory).FullName);
			return factory;
		}

		public virtual SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXTypeMappingAnsi());

		public virtual IJMXTypeMapping CreateJMXTypeMapping() => new JMXTypeMappingAnsi();

		public virtual MdbContext GetMdbContext() => _mdb;

	}
}
