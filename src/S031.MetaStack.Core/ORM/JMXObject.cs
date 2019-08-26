using System;
using System.Threading.Tasks;
using S031.MetaStack.Common;
using S031.MetaStack.Json;


#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Json;
namespace S031.MetaStack.WinForms.ORM
#endif
{
	//[JsonConverter(typeof(JsonConverter))]
	public class JMXObject : JsonObject
	{
		private JMXSchema _schema;
		private readonly IJMXFactory _factory;

		JMXObject()
		{
		}

#if NETCOREAPP
		public JMXObject(string objectName) : 
			this(objectName, ObjectFactories.GetDefault<IJMXFactory>())
#else
		public JMXObject(string objectName) :
			this(objectName, JMXFactory.Create())
#endif
		{
		}

		public JMXObject(string objectName, IJMXFactory schemaFactory)
		{
			ID = 0;
			ObjectName = objectName;
			_factory = schemaFactory;
			try
			{
				_schema = schemaFactory
					.CreateJMXRepo()
					.GetSchema(objectName);
			}
			catch { }
			if (_schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchema_ctor_1, objectName));
			foreach (var a in _schema.Attributes)
			{
				if (!vbo.IsEmpty(a.DefaultValue)) { }
					//!!!
					//this[a.AttribName] = JToken.FromObject(a.DefaultValue);
				else if (a.ConstName.ToLower() == "date_current")
					this[a.AttribName] = DateTime.Now;
			}
		}

		public string ObjectName
		{
			get => (string)this["ObjectName"];
			private set => this["ObjectName"] = value;
		}

		public int ID
		{
			get => this.GetIntOrDefault("ID");
			set => this["ID"] = value;
		}

		public JMXSchema Schema => _schema;

		public IJMXFactory GetJMXFactory() => _factory;

		public override string ToString()
		{
			return base.ToString();
		}

		public static async Task<JMXObject> CreateAsync(string objectName, IJMXFactory schemaFactory)
		{
			JMXObject instance = new JMXObject
			{
				ObjectName = objectName
			};
			try
			{
				instance._schema = await schemaFactory
					.CreateJMXRepo()
					.GetSchemaAsync(objectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchema_ctor_1, objectName));
			return instance;
		}

		public static JMXObject Parse(string json)
		{
#if NETCOREAPP
			return Parse(json, ObjectFactories.GetDefault<IJMXFactory>());
#else
			return Parse(json, JMXFactory.Create());
#endif
		}

		public static JMXObject Parse(string json, IJMXFactory schemaFactory)
		{
			JMXObject instance = (JMXObject)new JsonReader(ref json).Read();
			try
			{
				instance._schema = schemaFactory
					.CreateJMXRepo()
					.GetSchema(instance.ObjectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchema_ctor_1, instance.ObjectName));

			return instance;
		}

		public static async Task<JMXObject> ParseAsync(string json, IJMXFactory schemaFactory)
		{
			JMXObject instance = (JMXObject)new JsonReader(ref json).Read();
			try
			{
				instance._schema = await schemaFactory
					.CreateJMXRepo()
					.GetSchemaAsync(instance.ObjectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_Core_ORM_JMXSchema_ctor_1, instance.ObjectName));

			return instance;
		}
	}
}
