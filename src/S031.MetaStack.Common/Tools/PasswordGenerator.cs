using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Common
{
	public static class PasswordGenerator
	{
		private const string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
		private const string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
		private const string PASSWORD_CHARS_NUMERIC = "123456789";
		internal const string PASSWORD_CHARS_SPECIAL = "~!@#$%^&*-+_=";
		private const string PASSWORD_CHARS = PASSWORD_CHARS_LCASE + PASSWORD_CHARS_UCASE + PASSWORD_CHARS_NUMERIC;

		public static string Generate()
		{
			return Generate(new PasswordGeneratorOptions());
		}

		public static string Generate(PasswordGeneratorOptions options)
		{
			if (options.ValidLen < 6)
				//Password length can not be less than 6 characters
				throw new ArgumentException(Properties.Strings.PasswordGenerator_Generate_1);

			int totalReqs = options.MinUpper + options.MinLower + options.MinDigit + options.MinSpecial;
			if (options.ValidLen < totalReqs)
				//For specified conditions (PasswordGeneratorOptions), the password length can not be less than {totalReqs} characters
				throw new ArgumentException(Properties.Strings.PasswordGenerator_Generate_2.ToFormat(totalReqs));

			char[] reqChars = new char[totalReqs];
			Random r = new Random(Guid.NewGuid().GetHashCode());
			int i;
			int j;

			int len = PASSWORD_CHARS_NUMERIC.Length;
			for (i = 0; i < options.MinDigit; i++)
				reqChars[i] = PASSWORD_CHARS_NUMERIC[r.Next(0, len - 1)];

			j = i;
			len = PASSWORD_CHARS_LCASE.Length;
			for (i = j; i < j + options.MinLower; i++)
				reqChars[i] = PASSWORD_CHARS_LCASE[r.Next(0, len - 1)];

			j = i;
			len = PASSWORD_CHARS_UCASE.Length;
			for (i = j; i < j + options.MinUpper; i++)
				reqChars[i] = PASSWORD_CHARS_UCASE[r.Next(0, len - 1)];

			j = i;
			len = options.SpecialCharacters.Length;
			for (i = j; i < j + options.MinSpecial; i++)
				reqChars[i] = options.SpecialCharacters[r.Next(0, len - 1)];

			char[] pwd = new char[options.ValidLen];
			string source = options.MinSpecial > 0 ? PASSWORD_CHARS+options.SpecialCharacters : PASSWORD_CHARS;
			len = source.Length;
			for (i = 0; i < options.ValidLen; i++)
				pwd[i] = source[r.Next(0, len - 1)];

			List<int> l = new List<int>();
			foreach (char c in reqChars)
			{
				int n = r.Next(0, options.ValidLen - 1);
				for (; l.Contains(n);)
					if (n < options.ValidLen - 1)
						n++;
					else
						n = 0;
				l.Add(n);
			}
			for (i = 0; i < reqChars.Length; i++)
			{
				pwd[l[i]] = reqChars[i];
			}

			return new string(pwd);
		}
	}
	public class PasswordGeneratorOptions
	{
		public int ValidLen { get; set; }
		public int MinDigit { get; set; }
		public int MinSpecial { get; set; }
		public int MinUpper { get; set; }
		public int MinLower { get; set; }
		public string SpecialCharacters { get; set; }
		public PasswordGeneratorOptions()
		{
			ValidLen = 8;
			MinDigit = MinUpper = MinLower = 1;
			MinSpecial = 0;
			SpecialCharacters = PasswordGenerator.PASSWORD_CHARS_SPECIAL;
		}
	}
}
