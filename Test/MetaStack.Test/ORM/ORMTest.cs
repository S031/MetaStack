//using Newtonsoft.Json;
using Newtonsoft.Json;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Json;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Json;
using System;
using System.Data;
using System.Linq;
using System.Text;
using Xunit;

namespace MetaStack.Test.ORM
{
	public class ORMSchemaTest
	{
		private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Test.Resources.TestData.TestJson);
		public ORMSchemaTest()
		{
			Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		private void SerializeTest()
		{
			using (FileLog _logger = new FileLog("ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var str = _sourceJsonString;
				_logger.Debug($"Start perfomance parse string test");
				int i;
				DateTime t = DateTime.Now;
				JsonObject j = null;
				for (i = 0; i < 1_000; i++)
				{
					j = (JsonObject)new S031.MetaStack.Json.JsonReader(ref str).Read();
				}
				_logger.Debug($"Finish perfomance parse string test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance parse string schema test");
				t = DateTime.Now;
				for (i = 0; i < 1_000; i++)
				{
					var schema = JMXSchema.Parse(str);
				}				
				_logger.Debug($"Finish perfomance parse string schema test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");

				//_logger.Debug($"Start perfomance parse string schema Json.NET test");
				//t = DateTime.Now;
				//for (i = 0; i < 1_000; i++)
				//{
				//	var schema = JsonConvert.DeserializeObject<JMXSchema>(str);
				//}				
				//_logger.Debug($"Finish perfomance parse string schema Json.NET test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");
				var s = JMXSchema.Parse(str);
				str = s.ToString();
				var str2 = JMXSchema.Parse(str).ToString();
				Assert.Equal(str, str2);
				_logger.Debug(JMXSchema.Parse(str));

				//JMXAttribute a = new JMXAttribute("ID")
				//{
				//	DataType = MdbType.@int,
				//	IsNullable = false
				//};
				//string sa = a.ToString();
				////_logger.Debug(sa);
				//a = JSONExtensions.DeserializeObject<JMXAttribute>(sa);
				////_logger.Debug(a.ToString());
				//Assert.Equal(sa, a.ToString());
				//JMXPrimaryKey pk = new JMXPrimaryKey("PK_SomeObjects", "SomeAtt", "NextAtt");
				//string spk = pk.ToString();
				////_logger.Debug(spk);
				//pk = JSONExtensions.DeserializeObject<JMXPrimaryKey>(spk);
				//Assert.Equal(spk, pk.ToString());
				//JMXSchema s = new JMXSchema("SysSchemas")
				//{
				//	DbObjectName = new JMXObjectName("SysCat", "SysSchemas")
				//};
				//s.Attributes.Add(a);
				//s.PrimaryKey = pk;
				//s.Indexes.Add(new JMXIndex("IE1_SysSchemas", "ID"));
				//var fk = new JMXForeignKey("FK1")
				//{
				//	RefObjectName = "SysCat.SysArea",
				//	RefDbObjectName = new JMXObjectName("SysCat", "SysAreas")
				//};
				//fk.AddKeyMember("AreaID");
				//fk.AddRefKeyMember("ID");
				//s.ForeignKeys.Add(fk);
				//s.Conditions.Add(new JMXCondition(JMXConditionTypes.Where, "1=1"));
				//s.Conditions.Add(new JMXCondition(JMXConditionTypes.OrderBy, "Test"));
				//s.Parameters.Add(new JMXParameter("TestParam") { DataType = MdbType.guid });
				//s.Parameters.Add(new JMXParameter("TestParam1") { DataType = MdbType.@string, Dirrect = S031.MetaStack.Core.Actions.ParamDirrect.Input });
				//s.Parameters.Add(new JMXParameter("TestParam2") { DataType = MdbType.@decimal, Dirrect = S031.MetaStack.Core.Actions.ParamDirrect.Output });
				//string ss = s.ToString();
				//_logger.Debug(ss);
				//s = JSONExtensions.DeserializeObject<JMXSchema>(ss);
				//Assert.Equal(ss, s.ToString());
				//_logger.Debug("Start speed test fo JMXObject parse from json string");


				//JMXSchemaProviderFactory.RegisterProvider<JMXSchemaProviderMemory>();
				//JMXSchemaProviderFactory.SetDefault(JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderMemory>());
				//JMXSchemaProviderFactory.Default.SaveSchema(creaateTestSchema());
				//JMXObject o = new JMXObject("SysCat.SysSchema");
				//for (int i = 0; i < 1000; i++)
				//{
				//	o.ParseJson(ss, null);
				//}
				//_logger.Debug("Finish speed test");
				//_logger.Debug(o.ToString());
				//_logger.Debug("Start speed test fo JMXSchema serialize to json string");
				//for (int i = 0; i < 1000; i++)
				//{
				//	ss = JsonConvert.SerializeObject(s);
				//}
				//_logger.Debug("Finish speed test");
				//_logger.Debug("Start speed test fo JMXSchema ToString method to json string");
				//for (int i = 0; i < 1000; i++)
				//{
				//	ss = s.ToString();
				//}
				//_logger.Debug("Finish speed test");
				//_logger.Debug("Start speed test fo JMXSchema parse from json string");
				//for (int i = 0; i < 1000; i++)
				//{
				//	s = JsonConvert.DeserializeObject<JMXSchema>(ss);
				//}
				//_logger.Debug("Finish speed test");

				//JMXObject o = new JMXObject(JObject.Parse(ss));
				//_logger.Debug((o as JObject).ToString());
				//JsonSerializer serializer = new JsonSerializer();
				//JMXObject o = (JMXObject)serializer.Deserialize(new JTokenReader(JObject.Parse(ss)), typeof(JMXObject));
				//JMXObject o = new JMXObject("Test");
			}
		}
		[Fact]
		private void JMXObjectTest()
		{
			//JMXSchemaProviderFactory.RegisterProvider<JMXSchemaProviderMemory>();
			//IJMXSchemaProvider sp = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderMemory>();
			//JMXSchemaProviderFactory.SetDefault(sp);
			//using (FileLog _logger = new FileLog("ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	sp.SaveSchema(creaateTestSchema());

			//	JMXObject o = new JMXObject("SysCat.SysSchema")
			//	{
			//		["ID"] = 1
			//	};
			//	_logger.Debug(o.ToString());
			//	string s = o.ToString();
			//	o.ParseJson(s, null);
			//	_logger.Debug(o.ToString());
			//	o = JMXObject.CreateFrom(o.ToString());
			//	_logger.Debug(o.ToString());
			//}
		}
		[Fact]
		private void JMXParameterTest()
		{
			////using (FileLog _logger = new FileLog("ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//using (FileLog _logger = typeof(FileLog).CreateInstance<FileLog>(new object[]
			//	{"ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }}))
			//{ 
			//	//JMXParameter p = new JMXParameter("FileName");
			//	JMXParameter p = typeof(JMXParameter).CreateInstance<JMXParameter>("FileName");
			//	p.PresentationType = "TextBox";
			//	string sp = p.ToString();
			//	_logger.Debug(sp);
			//	p = JsonConvert.DeserializeObject<JMXParameter>(sp);
			//	Assert.Equal(sp, p.ToString());
			//	//Type t = typeof(S031.MetaStack.Core.AttribInfo);
			//	//var a = t.CreateInstance<S031.MetaStack.Core.AttribInfo>();
			//	//_logger.Debug(JsonConvert.SerializeObject(a));
			//}
		}

		[Fact]
		private void CreateInstaceSpeedTest()
		{
			using (FileLog _logger = typeof(FileLog).CreateInstance<FileLog>(new object[]
				{"ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }}))
			{
				_logger.Debug("Start speed test for JMXParameter Type.CreateInstance");
				JMXParameter p = typeof(JMXParameter).CreateInstance<JMXParameter>("FileName");
				for (int i = 0; i < 1000000; i++)
				{
					p = typeof(JMXParameter).CreateInstance<JMXParameter>("FileName");
				}
				_logger.Debug("Finish speed test");
				_logger.Debug("Start speed test for JMXParameter ctor.Invoke ");
				for (int i = 0; i < 1000000; i++)
				{
					p = typeof(JMXParameter).CreateInstance2<JMXParameter>("FileName");
				}
				_logger.Debug("Finish speed test");
			}

		}
		[Fact]
		private void CreateTypesListSpeedTest()
		{
			using (FileLog _logger = typeof(FileLog).CreateInstance<FileLog>(new object[]
				{"ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }}))
			{
				_logger.Debug("List implements IDataReader");
				ImplementsList.Add(typeof(IDataReader));
				foreach (var s in ImplementsList.GetTypes(typeof(IDataReader)))
				{
					_logger.Debug(s.FullName);
				}

				_logger.Debug("Start speed test for find implements IDataReader");
				for (int i = 0; i < 1000000; i++)
				{
					var l = ImplementsList.GetTypes(typeof(IDataReader)).ToArray();
				}
				//ImplementsList.Add(typeof(IDataReader));
				//foreach (var s in ImplementsList.GetTypes(typeof(IDataReader)))
				//	_logger.Debug(s.FullName);

				//_logger.Debug("Start speed test for find implements IDataReader");
				//for (int i = 0; i < 1000000; i++)
				//{
				//	var l = ImplementsList.GetTypes(typeof(IDataReader)).ToArray();
				//}
				_logger.Debug("Finish speed test");
			}
		}

		private JMXSchema creaateTestSchema()
		{

			JMXSchema s = new JMXSchema("SysSchema")
			{
				DbObjectName = new JMXObjectName("SysCat", "SysSchemas")
			};
			JMXAttribute a = new JMXAttribute("ID")
			{
				DataType = MdbType.@int
			};
			s.Attributes.Add(new JMXAttribute("ID") { DataType = MdbType.@int, Caption = "Identifier", IsNullable = false });
			s.Attributes.Add(new JMXAttribute("Name") { DataType = MdbType.@string, Caption = "Name of SysSchema" });
			s.PrimaryKey = new JMXPrimaryKey("PK_SysSchemas", "ID");
			s.Indexes.Add(new JMXIndex("IE1_SysSchemas", "Name"));
			var fk = new JMXForeignKey("FK1")
			{
				RefObjectName = "SysCat.SysArea",
				RefDbObjectName = new JMXObjectName("SysCat", "SysAreas")
			};
			fk.AddKeyMember("AreaID");
			fk.AddRefKeyMember("ID");
			s.ForeignKeys.Add(fk);
			return s;
		}
	}
}
