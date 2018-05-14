using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetaStack.UnitTest
{
	[TestClass]
	public class TranslateTest
	{
		public TranslateTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[TestMethod]
		public void translateMessageTest()
		{
			using (FileLog l = new FileLog("TranslateTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				const string testKey = "S031.MetaStack.Common.PasswordGenerator.Generate.2";
				// default
				l.Debug(Translater.GetString(testKey));
				//ru-RU
				Translater.SetCurrent("ru-RU");
				l.Debug(testKey.GetTranslate(10));
				//ru-RU from file Translate\ru-RU.txt
				l.Debug(Translater.GetString("TestKeyFromFile.1"));
				l.Debug(Translater.GetString("TestKeyFromFile.2"));
				//en-US
				Translater.SetCurrent("en-US");
				l.Debug(testKey.GetTranslate(10));
				l.Debug(Translater.GetString(testKey));
				l.Debug(Translater.GetString("TestKeyFromFile.1").IsEmpty());
			}
		}
	}
}

