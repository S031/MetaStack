using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;
using System.IO;
using Xunit;


namespace MetaStack.Test.Common
{
	public class DeclensionTest
    {
		public DeclensionTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		void DeclensionFileTest()
		{
			using (FileLog l = new FileLog("declensionFileTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				string[] decl = { "И", "Р", "Д", "В", "Т", "П" };
				string[] test = CommonTest.testData1.declensionTestData.Split("\r\n".ToCharArray());
				foreach (string s in test)
				{
					string[] a = s.Split('\t');
					if (a.Length > 3)
					{
						DeclensionCase _dc = (DeclensionCase)(decl.IndexOf(a[1]) + 1);
						if (_dc != DeclensionCase.NotDefind)
						{
							string result = Declension.GetDeclension(a[0], _dc).TrimEnd();
							if (!result.Equals(a[2].Trim(), StringComparison.CurrentCultureIgnoreCase))
								l.Debug('\n' + a[0] + '\t' + a[1] + '\t' + a[2] + '\t' + result);
						}
					}
				}
			}
		}
	}
}
