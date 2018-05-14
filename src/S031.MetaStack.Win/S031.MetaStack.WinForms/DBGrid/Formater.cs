using System;

public class AccntFormat : IFormatProvider, ICustomFormatter
{
	private const int ACCNT_LENGTH = 20;
	private const string ACCNT_FORM = "@@@@@-@@@-@-@@@@-@@@@@@@";

	public const string ACCNTFORMAT = "{0:!@@@@@-@@@-@-@@@@-@@@@@@@}";

	public object GetFormat(Type formatType)
	{
		if (formatType == typeof(ICustomFormatter))
			return this;
		else
			return null;
	}

	public string Format(string format, object arg, IFormatProvider formatProvider)
	{
		if (arg == null) return string.Empty;
		string result = arg.ToString();
		if (result.Length >= ACCNT_LENGTH)
			return result.Substring(0, 5) + "-" + result.Substring(5, 3) + "-" + result.Substring(8, 1) +
				"-" + result.Substring(9, 4) + "-" + result.Substring(13);
		else
			return result;
	}

	public static bool ValidFormat(string fmt)
	{
		if (!string.IsNullOrEmpty(fmt))
			return fmt.Contains(ACCNT_FORM);
		else
			return false;
	}
}
