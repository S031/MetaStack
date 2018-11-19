using System.Collections.Generic;

namespace S031.MetaStack.WinForms
{
	public enum ReportDestinations
	{
		None = 0,
		Screen = 1,
		Printer = 2,
		File = 3,
		Designer = 4,
		DataSource = 5
	}

	public enum ReportExportCharsets
	{
		Default = 0,
		Windows_1251 = 1
	}
	public interface IReporter
	{
		void PrintCurrentForm(IObjectHost objectHost);
		void DesignCurrentForm(IObjectHost objectHost);
		void ExportCurrentForm(IObjectHost objectHost, ReportExportFormat format);
		string ConvertReport(string source);
		void OutputReport(string reportName, System.Data.DataTable dataSource);
		void OutputReport(string reportName, System.Data.DataTable dataSource, ReportDestinations outputTarget);
		void OutputReport(string reportName, System.Data.DataTable dataSource, ActionParameters reportParameters);
		void OutputReport(string reportName, System.Data.DataTable dataSource,
			ReportDestinations outputTarget, ActionParameters reportParameters);
		void OutputReport(string reportName, System.Data.DataTable dataSource,
			 ReportExportFormat format, ActionParameters reportParameters);
		void Design(IObjectHost objectHost);
	}

	public class ReportCreateParam
	{
		public string ExportFileName { get; set; }
		public ReportExportCharsets Charset { get; set; }
		public float FontSize { get; set; }
		public bool TitleOnPage { get; set; }
		public bool Period { get; set; }
		public bool MultiLine { get; set; }
		public int Border { get; set; }
		public bool Sign { get; set; }

		public ReportCreateParam()
		{
			ExportFileName = string.Empty;
			FontSize = 8f;
			TitleOnPage = false;
			Period = true;
			MultiLine = false;
			Border = 0;
			Sign = false;
			Charset = ReportExportCharsets.Default;
		}
	}

	public class ReportExportFormat
	{
		public string ID { get; set; }
		public string Description { get; set; }
		public string FileMask { get; set; }
		public string FileMaskDescription { get; set; }
		public ReportCreateParam CreateParam { get; set; }
	}

	public static class ReportExportFormats
	{
		private static readonly Dictionary<string, ReportExportFormat> formats = new Dictionary<string, ReportExportFormat>(System.StringComparer.CurrentCultureIgnoreCase)
		{
			{"xls",new ReportExportFormat(){ID = "xls", Description = "MS Excel", FileMask="*.xls", FileMaskDescription="(*.xls)",
				CreateParam=new ReportCreateParam(){ Border=2, Period=true, MultiLine=true, Sign=false, TitleOnPage=false}}},
			{"txt",new ReportExportFormat(){ID = "txt", Description = "Текст Windows", FileMask="*.txt", FileMaskDescription="(*.txt)",
				CreateParam=new ReportCreateParam(){ Border=0, Period=false, MultiLine=false, Sign=false, TitleOnPage=false}}},
			{"html",new ReportExportFormat(){ID = "html", Description = "HTML формат", FileMask="*.html", FileMaskDescription="(*.html)",
				CreateParam=new ReportCreateParam(){ Border=2, Period=true, MultiLine=true, Sign=false, TitleOnPage=false}}},
			{"pdf",new ReportExportFormat(){ID = "pdf", Description = "Acrobat reader", FileMask="*.pdf", FileMaskDescription="(*.pdf)",
				CreateParam=new ReportCreateParam(){ Border=2, Period=true, MultiLine=true, Sign=false, TitleOnPage=true}}}
		};

		public static Dictionary<string, ReportExportFormat> Formats
		{
			get { return formats; }
		}

		public static ReportExportFormat GetFormat(string formatID)
		{
			return formats[formatID];
		}
	}
}
