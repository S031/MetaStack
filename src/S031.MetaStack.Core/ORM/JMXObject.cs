using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using S031.MetaStack.Common;


#if NETCOREAPP
using S031.MetaStack.Core.Json;
namespace S031.MetaStack.Core.ORM
#else
using S031.MetaStack.WinForms.Json;
namespace S031.MetaStack.WinForms.ORM
#endif
{
	//[JsonConverter(typeof(JsonConverter))]
	public class JMXObject : JObject
	{
		private JMXSchema _schema;
		private readonly JMXFactory _factory;

		// Call internal methods of JsonReader && JObjects for parse json
		static readonly FastMethodInfo.ReturnValueDelegate _moveToContent = new FastMethodInfo(typeof(JsonReader)
			.GetMethod("MoveToContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)).Delegate;
		static readonly FastMethodInfo.ReturnValueDelegate _setLineInfo = new FastMethodInfo(typeof(JObject)
			.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Single(
			m => m.Name == "SetLineInfo" && m.GetParameters()[0].ParameterType == typeof(IJsonLineInfo))).Delegate;
		static readonly FastMethodInfo.ReturnValueDelegate _readTokenFrom = new FastMethodInfo(typeof(JObject)
			.GetMethod("ReadTokenFrom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)).Delegate;

		JMXObject()
		{
		}
		public JMXObject(string objectName) : 
			this(objectName, ObjectFactories.GetDefault<JMXFactory>())
		{
		}

		public JMXObject(string objectName, JMXFactory schemaFactory)
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
				throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchema.ctor.1", objectName));
			foreach (var a in _schema.Attributes)
			{
				if (!vbo.IsEmpty(a.DefaultValue))
					this[a.AttribName] = JToken.FromObject(a.DefaultValue);
				else if (a.ConstName.ToUpper() == "DATE_CURRENT")
					this[a.AttribName] = DateTime.Now;

			}
		}
		public static async Task<JMXObject> CreateObjectAsync(string objectName, JMXFactory schemaFactory)
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
				throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchema.ctor.1", objectName));
			return instance;
		}
		public static JMXObject CreateFrom(string json)
		{
			return CreateFrom(json, ObjectFactories.GetDefault<JMXFactory>());
		}
		public static JMXObject CreateFrom(string json, JMXFactory schemaFactory)
		{
			JMXObject instance = new JMXObject();
			instance.ParseJson(json);
			instance.ObjectName = (string)instance["ObjectName"];
			if (!instance.TryGetValue("ID", StringComparison.CurrentCultureIgnoreCase, out var j))
				instance.ID = 0;
			try
			{
				instance._schema = schemaFactory
					.CreateJMXRepo()
					.GetSchema(instance.ObjectName);
			}
			catch { }
			if (instance._schema == null)
				throw new InvalidOperationException(Translater.GetTranslate("S031.MetaStack.Core.ORM.JMXSchema.ctor.1", instance.ObjectName));

			return instance;
		}
		public string ObjectName
		{
			get => (string)this["ObjectName"];
			private set => this["ObjectName"] = value;
		}
		public int ID
		{
			get => Convert.ToInt32(this["ID"].ToIntOrDefault());
			set => this["ID"] = value;
		}
		public JMXSchema Schema => _schema;
		public JMXFactory GetJMXFactory() => _factory;
		public override string ToString()
		{
			return base.ToString();
		}
		public void ParseJson(string json, JsonLoadSettings settings=null)
		{
			using (JsonReader reader = new JsonTextReader(new StringReader(json)))
			{
				load(reader, settings);
			}
		}
		private void load(JsonReader reader, JsonLoadSettings settings)
		{
			if (reader.TokenType == JsonToken.None)
			{
				if (!reader.Read())
				{
					throw new JsonReaderException("Error reading JObject from JsonReader.");
				}
			}

			_moveToContent(reader, null);
			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new JsonReaderException($"Error reading JObject from JsonReader. Current JsonReader item is not an object: {reader.TokenType}");
			}

			_setLineInfo(this, new object[] { reader as IJsonLineInfo, settings });
			_readTokenFrom(this, new object[] { reader, settings });
		}

	}
}
