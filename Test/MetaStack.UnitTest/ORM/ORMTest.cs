﻿using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Json;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Data;
using S031.MetaStack.Core.Data;

namespace MetaStack.UnitTest.ORM
{
	[TestClass]
	public class ORMSchemaTest
	{
		public ORMSchemaTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[TestMethod]
		public void SerializeTest()
		{
			using (FileLog _logger = new FileLog("ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXAttribute a = new JMXAttribute("ID")
				{
					DataType = MdbType.@int,
					IsNullable = false
				};
				string sa = a.ToString();
				//_logger.Debug(sa);
				a = JsonConvert.DeserializeObject<JMXAttribute>(sa);
				//_logger.Debug(a.ToString());
				Assert.AreEqual(sa, a.ToString());
				JMXPrimaryKey pk = new JMXPrimaryKey("PK_SomeObjects", "SomeAtt", "NextAtt");
				string spk = pk.ToString();
				//_logger.Debug(spk);
				pk = JsonConvert.DeserializeObject<JMXPrimaryKey>(spk);
				Assert.AreEqual(spk, pk.ToString());
				JMXSchema s = new JMXSchema("SysSchemas")
				{
					DbObjectName = new JMXObjectName("SysCat", "SysSchemas")
				};
				s.Attributes.Add(a);
				s.PrimaryKey = pk;
				s.Indexes.Add(new JMXIndex("IE1_SysSchemas", "ID"));
				var fk = new JMXForeignKey("FK1")
				{
					RefObjectName = "SysCat.SysArea",
					RefDbObjectName = new JMXObjectName("SysCat", "SysAreas")
				};
				fk.AddKeyMember("AreaID");
				fk.AddRefKeyMember("ID");
				s.ForeignKeys.Add(fk);
				string ss = s.ToString();
				_logger.Debug(ss);
				s = JsonConvert.DeserializeObject<JMXSchema>(ss);
				Assert.AreEqual(ss, s.ToString());
				_logger.Debug("Start speed test fo JMXObject parse from json string");


				JMXSchemaProviderFactory.RegisterProvider<JMXSchemaProviderMemory>();
				JMXSchemaProviderFactory.SetDefault(JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderMemory>());
				JMXSchemaProviderFactory.Default.SaveSchema(creaateTestSchema());
				JMXObject o = new JMXObject("SysCat.SysSchema");
				for (int i = 0; i < 1000; i++)
				{
					o.ParseJson(ss, null);
				}
				_logger.Debug("Finish speed test");
				_logger.Debug(o.ToString());
				_logger.Debug("Start speed test fo JMXSchema serialize to json string");
				for (int i = 0; i < 1000; i++)
				{
					ss = JsonConvert.SerializeObject(s);
				}
				_logger.Debug("Finish speed test");
				_logger.Debug("Start speed test fo JMXSchema ToString method to json string");
				for (int i = 0; i < 1000; i++)
				{
					ss = s.ToString();
				}
				_logger.Debug("Finish speed test");
				_logger.Debug("Start speed test fo JMXSchema parse from json string");
				for (int i = 0; i < 1000; i++)
				{
					s = JsonConvert.DeserializeObject<JMXSchema>(ss);
				}
				_logger.Debug("Finish speed test");

				//JMXObject o = new JMXObject(JObject.Parse(ss));
				//_logger.Debug((o as JObject).ToString());
				//JsonSerializer serializer = new JsonSerializer();
				//JMXObject o = (JMXObject)serializer.Deserialize(new JTokenReader(JObject.Parse(ss)), typeof(JMXObject));
				//JMXObject o = new JMXObject("Test");
			}
		}
		[TestMethod]
		public void JMXObjectTest()
		{
			JMXSchemaProviderFactory.RegisterProvider<JMXSchemaProviderMemory>();
			IJMXSchemaProvider sp = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderMemory>();
			JMXSchemaProviderFactory.SetDefault(sp);
			using (FileLog _logger = new FileLog("ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				sp.SaveSchema(creaateTestSchema());

				JMXObject o = new JMXObject("SysCat.SysSchema")
				{
					["ID"] = 1
				};
				_logger.Debug(o.ToString());
				string s = o.ToString();
				o.ParseJson(s, null);
				_logger.Debug(o.ToString());
				o = JMXObject.CreateFrom(o.ToString());
				_logger.Debug(o.ToString());
			}
		}
		[TestMethod]
		public void JMXParameterTest()
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

		[TestMethod]
		public void CreateInstaceSpeedTest()
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
		[TestMethod]
		public void CreateTypesListSpeedTest()
		{
			using (FileLog _logger = typeof(FileLog).CreateInstance<FileLog>(new object[]
				{"ORMSchemaTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }}))
			{
				_logger.Debug("List implements IDataReader");
				ImplementsList.Add(typeof(IDataReader));
				foreach (var s in ImplementsList.GetTypes(typeof(IDataReader)))
					_logger.Debug(s.FullName);

				_logger.Debug("Start speed test for find implements IDataReader");
				for (int i = 0; i < 1000000; i++)
				{
					var l = ImplementsList.GetTypes(typeof(IDataReader)).ToArray();
				}
				_logger.Debug("Finish speed test");
			}
		}

		JMXSchema creaateTestSchema()
		{

			JMXSchema s = new JMXSchema("SysSchema")
			{
				DbObjectName = new JMXObjectName("SysCat", "SysSchemas")
			};
			JMXAttribute a = new JMXAttribute("ID")
			{
				DataType = MdbType.@int
			};
			s.Attributes.Add(new JMXAttribute("ID") { DataType = MdbType.@int, Caption = "Identifier", IsNullable =false });
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
