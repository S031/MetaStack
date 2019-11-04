using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.IO;
using System.Linq;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// Basic functionality to support multiple languages. 
	/// You can then expand
	/// </summary>
	public static class Translater
    {

		static readonly Dictionary<string, KeyValueList> _languages = new Dictionary<string, KeyValueList>();
		const string _default = "en-US";
		const string _russian = "ru-RU";
		private static string _current;

		public static string GetCurrent() => _current;

		public static void SetCurrent(string value)
		{
			value.NullTest(nameof(value));
			KeyValueList l;
			if (!_languages.ContainsKey(value))
			{
				if (value == _default || value == _russian)
					l = new KeyValueList(GetEmbeddedResource(value), Environment.NewLine, "=");
				else
					l = new KeyValueList(_languages[_default].ToArray(), Environment.NewLine, "=");

				string path = Path.Combine(System.AppContext.BaseDirectory, "Translate", $"{value}.txt");
				if (File.Exists(path))
				{
					KeyValueList l2 = new KeyValueList(File.ReadAllText(path), Environment.NewLine, "=");
					foreach (var i in l2)
						l[i.Key] = i.Value;
				}
				_languages.Add(value, l);
			}
			_current = value;
		}

		static Translater()
		{
			SetCurrent(_default);
			SetCurrent(CultureInfo.CurrentCulture.Name);
		}

		static string GetEmbeddedResource(string lang)
		{
			Assembly asm = typeof(Translater).Assembly;
			return new System.IO.StreamReader(asm.GetManifestResourceStream(
					$"{asm.GetWorkName() }.Translater.{lang}.txt")).ReadToEnd();
		}

		public static string GetString(string index) => _languages[_current][index];

		public static string GetTranslate(this string index, params object[] args)
		{
			return  string.Format(GetString(index), args);
		}
	}
}
