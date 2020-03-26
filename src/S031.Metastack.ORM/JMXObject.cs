using System;
using System.Threading.Tasks;
using S031.MetaStack.Common;
using S031.MetaStack.Json;

namespace S031.MetaStack.ORM
{
	public class JMXObject : JsonObject
	{
		private JMXSchema _schema;
		private readonly IJMXFactory _factory;

		JMXObject()
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
					string.Format(Properties.Strings.S031_MetaStack_ORM_JMXSchema_ctor_1, objectName));
			foreach (var a in _schema.Attributes)
			{
				if (!vbo.IsEmpty(a.DefaultValue))
					this[a.AttribName] = new JsonValue(a.DefaultValue);
				else if (a.ConstName.Equals("date_current", StringComparison.OrdinalIgnoreCase))
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
					string.Format(Properties.Strings.S031_MetaStack_ORM_JMXSchema_ctor_1, objectName));
			return instance;
		}

		public static JMXObject Parse(string json, IJMXFactory schemaFactory)
		{
			JMXObject instance = (JMXObject)new JsonReader(json).Read();
			try
			{
				instance._schema = schemaFactory
					.CreateJMXRepo()
					.GetSchema(instance.ObjectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_ORM_JMXSchema_ctor_1, instance.ObjectName));

			return instance;
		}

		public static async Task<JMXObject> ParseAsync(string json, IJMXFactory schemaFactory)
		{
			JMXObject instance = (JMXObject)new JsonReader(json).Read();
			try
			{
				instance._schema = await schemaFactory
					.CreateJMXRepo()
					.GetSchemaAsync(instance.ObjectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(
					string.Format(Properties.Strings.S031_MetaStack_ORM_JMXSchema_ctor_1, instance.ObjectName));

			return instance;
		}
	}
}
