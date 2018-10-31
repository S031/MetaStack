using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public static class ReportManager
	{
		static string assemblyID = "MetApp.Report";

		static IReporter _reporter = loadReporter();

		public static void PrintCurrentForm(DBGrid grid)
		{
			if (_reporter != null)
				_reporter.PrintCurrentForm(grid);
		}

		public static void DesignCurrentForm(DBGrid grid)
		{
			if (_reporter != null)
				_reporter.DesignCurrentForm(grid);
		}

		public static void Design(DBGrid grid)
		{
			if (_reporter != null)
				_reporter.Design(grid);
		}

		public static void ExportCurrentForm(DBGrid grid, ReportExportFormat format)
		{
			if (_reporter != null)
				_reporter.ExportCurrentForm(grid, format);
		}

		public static string ConvertReport(Control grid, string source)
		{
			if (_reporter != null)
				return _reporter.ConvertReport(source);
			return null;
		}

		public static void OutputReport(Control grid, string reportName, System.Data.DataTable dataSource)
		{
			OutputReport(grid, reportName, dataSource, ReportDestinations.Screen, null);
		}

		public static void OutputReport(Control grid, string reportName, System.Data.DataTable dataSource, ReportDestinations outputTarget)
		{
			OutputReport(grid, reportName, dataSource, outputTarget, null);
		}

		public static void OutputReport(Control grid, string reportName, System.Data.DataTable dataSource,
			ReportDestinations outputTarget, ActionParameters reportParameters)
		{
			if (_reporter != null)
				_reporter.OutputReport(reportName, dataSource, outputTarget, reportParameters);
		}

		public static void OutputReport(Control grid, string reportName, System.Data.DataTable dataSource,
			ReportExportFormat format, ActionParameters reportParameters)
		{
			if (_reporter != null)
				_reporter.OutputReport(reportName, dataSource, format, reportParameters);
		}

		static IReporter loadReporter()
		{

			Assembly assembly;
			try
			{
				assembly = Assembly.Load(assemblyID);
				if (assembly == null)
				{
					MessageBox.Show("Не установлен файл интерфейса отчетов: " + assemblyID + ".dll",
						"Newbank.Forms.ReportManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return null;
				}
			}
			catch (ArgumentNullException eNull)
			{
				MessageBox.Show(eNull.Message, "Newbank.Forms.ReportManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			catch (FileLoadException eLoad)
			{
				MessageBox.Show(eLoad.Message, "Newbank.Forms.ReportManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			catch (BadImageFormatException eBad)
			{
				MessageBox.Show(eBad.Message, "Newbank.Forms.ReportManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			Type reporterType = assembly.GetExportedTypes().FirstOrDefault<Type>(type => type.IsClass && typeof(IReporter).IsAssignableFrom(type));
			if (reporterType != null)
				return (IReporter)Activator.CreateInstance(reporterType);

			return null;
		}

		public static ReportExportFormat GetExportReportFormat(string fileName, string exportFormatID)
		{
			using (SaveFileDialog dlg = new SaveFileDialog())
			{
				dlg.FileName = fileName;
				string filesMask = string.Empty;
				List<string> ids = new List<string>();
				foreach (KeyValuePair<string, ReportExportFormat> kvp in ReportExportFormats.Formats)
				{
					filesMask += kvp.Value.Description + kvp.Value.FileMaskDescription + "|" + kvp.Value.FileMask + "|";
					ids.Add(kvp.Key);
					if (kvp.Value.ID == exportFormatID)
						dlg.FilterIndex = ids.Count;

				}
				dlg.Filter = filesMask.Substring(0, filesMask.Length - 1);
				dlg.OverwritePrompt = false;
				dlg.AddExtension = true;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string newFileName = dlg.FileName;
					bool exists = System.IO.File.Exists(newFileName);
					if (!exists || (MessageBox.Show("Файл " + newFileName + " уже существует. Перезаписать?","Экспорт в файл",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
					{
						ReportExportFormat format = ReportExportFormats.GetFormat(ids[dlg.FilterIndex - 1]);
						format.CreateParam.ExportFileName = newFileName;
						return format;
					}
				}
			}
			return null;
		}
	}
}
