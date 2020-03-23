//using Newtonsoft.Json;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Data;
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
					j = (JsonObject)new S031.MetaStack.Json.JsonReader(str).Read();
				}
				_logger.Debug($"Finish perfomance parse string test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance parse string schema test");
				t = DateTime.Now;
				for (i = 0; i < 1_000; i++)
				{
					var schema = JMXSchema.Parse(str);
				}				
				_logger.Debug($"Finish perfomance parse string schema test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");

				//str = JSONExtensions.SerializeObject(JMXSchema.Parse(str));
				//_logger.Debug($"Start perfomance parse string schema MessagePack test");
				//t = DateTime.Now;
				//for (i = 0; i < 1_000; i++)
				//{
				//	var schema = JSONExtensions.DeserializeObject<JMXSchema>(str);
				//}
				//_logger.Debug($"Finish perfomance parse string schema MessagePack test. Time={(DateTime.Now - t).Milliseconds} ms, loop count={i}");
				var s = JMXSchema.Parse(str);
				str = s.ToString();
				var str2 = JMXSchema.Parse(str).ToString();
				Assert.Equal(str, str2);
				_logger.Debug(JMXSchema.Parse(str));
				_logger.Debug(JMXSchema.Parse(str2));

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
