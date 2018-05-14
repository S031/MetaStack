using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Common
{
	public static class Numeric
	{
		private static readonly Dictionary<string, string[]> numItems = new Dictionary<string, string[]>(31, StringComparer.CurrentCultureIgnoreCase)
			{   {"EMPTY", new string[7] { "", "", "", "", "", "", "M" }},
				{"MR", new string[7] { "", "", "", "", "", "", "M" }},
				{"MS", new string[7] { "", "", "", "", "", "", "W" }},
				{"RUR", new string[7] { "рубль", "рубля", "рублей", "копейка", "копейки", "копеек", "M" }},
				{"810", new string[7] { "рубль", "рубля", "рублей", "копейка", "копейки", "копеек", "M" }},
				{"USD", new string[7] { "доллар США", "доллара США", "долларов США", "цент", "цента", "центов", "M" }},
				{"840", new string[7] { "доллар США", "доллара США", "долларов США", "цент", "цента", "центов", "M" }},
				{"EUR", new string[7] { "евро", "евро", "евро", "цент", "цента", "центов", "M" }},
				{"978", new string[7] { "евро", "евро", "евро", "цент", "цента", "центов", "M" }},
				{"ШТУКА", new string[7] { "штука", "штуки", "штук", "", "", "", "W" }},
			};
		private static string _defaultItem = "EMPTY";

		static readonly string[][] aNum = {
				new string[]{"сто","двести","триста","четыреста","пятьсот","шестьсот","семьсот","восемьсот","девятьсот"},
				new string[]{"десять","двадцать","тридцать","сорок","пятьдесят","шестьдесят","семьдесят","восемьдесят","девяносто"},
				new string[]{"","","три","четыре","пять","шесть","семь","восемь","девять"},
				new string[]{"одиннадцать","двенадцать","тринадцать","четырнадцать","пятнадцать","шестнадцать","семнадцать","восемнадцать","девятнадцать"}
				};
		static readonly string[][] aWord = {
				new string[]{"","",""},
				new string[]{"триллион","триллиона","триллионов"},
				new string[]{"миллиард","миллиарда","миллиардов"},
				new string[]{"миллион","миллиона","миллионов" },
				new string[]{"тысяча","тысячи","тысяч"},
				new string[]{"","",""},
				};

		public static string DefaultItem { get { return _defaultItem; } set { _defaultItem = value; } }

		public static void Add(string key, string[] aItem)
		{
			numItems[key] = aItem;
		}

		public static string AsString()
		{
			return AsString(0, DefaultItem, true);
		}

		public static string AsString(decimal nParam)
		{
			return AsString(nParam, DefaultItem, true);
		}

		public static string AsString(decimal nParam, string cVal)
		{
			return AsString(nParam, cVal, true);
		}

		public static string AsString(decimal nParam, string cVal, bool lDec)
		{
			cVal = string.IsNullOrEmpty(cVal) || !numItems.ContainsKey(cVal) ? DefaultItem : cVal.ToUpper();

			string[] numItem = numItems[cVal];
			aWord[5][0] = numItem[0];
			aWord[5][1] = numItem[1];
			aWord[5][2] = numItem[2];

			string buf1 = ((UInt64)nParam).ToString("##0").PadLeft(15, '0');
			buf1 = buf1.Substring(buf1.Length - 15);
			for (; buf1.Substring(0, 3) == "000" && buf1.Length > 3;) buf1 = buf1.Substring(3);

			int iCount = buf1.Length / 3;
			int k = 0;
			StringBuilder result = new StringBuilder();
			byte[] buf2;
			for (int i = 1; i <= iCount; i++)
			{
				buf2 = toByteArray(buf1.Substring(k, 3));
				if ((i == iCount - 1) | (i == iCount & numItem[6] == "W"))
				{
					aNum[2][0] = "одна";
					aNum[2][1] = "две";
				}
				else
				{
					aNum[2][0] = "один";
					aNum[2][1] = "два";
				}
				for (int j = 0; j < 3; j++)
				{
					if (!(buf2[j] == 1 & j == 1))
					{
						result.Append(two(buf2, j, j));
					}
					else if (buf2[j + 1] == 0)
					{
						result.Append(two(buf2, j, j));
						break;
					}
					else
					{
						result.Append(two(buf2, 3, j));
						break;
					}
				}
				int n = 0;
				if (buf2[1] == 1) n = 2;
				else if (buf2[2] == 1) n = 0;
				else if (buf2[2] >= 2 & buf2[2] <= 4) n = 1;
				else n = 2;

				if (buf1.Substring(k, 3) != "000")
					result.Append(aWord[5 - iCount + i][n] + " ");
				else if (iCount == 1)
					result.Append("ноль " + aWord[5][n] + " ");
				else if (5 - iCount + i == 5)
					result.Append(aWord[5][n] + " ");

				k += 3;
			}
			if (lDec == true)
			{
				buf1 = nParam.ToString("0.00");
				buf1 = buf1.Substring(buf1.Length - 2);
				result.Append(buf1 + " ");
				buf2 = toByteArray(buf1);
				if (buf2[0] == 1)
					result.Append(numItem[5] + " ");
				else if (buf2[1] == 1)
					result.Append(numItem[3] + " ");
				else if (buf2[1] >= 2 & buf2[1] <= 4)
					result.Append(numItem[4] + " ");
				else
					result.Append(numItem[5] + " ");
			}
			buf1 = result.ToString();
			return buf1.Substring(0, 1).ToUpper() + buf1.Substring(1);
		}

		private static string two(byte[] buf2, int n, int j)
		{
			if (buf2[j] == 0) return "";
			else if (n == 3) return aNum[n][buf2[j + 1] - 1] + " ";
			else return aNum[n][buf2[j] - 1] + " ";
		}

		private static byte[] toByteArray(string str)
		{
			byte[] result = new Byte[str.Length];
			for (int i = 0; i < str.Length; i++) result[i] = byte.Parse(str.Substring(i, 1));
			return result;
		}
	}
}
