using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Configuration;
using Stimulsoft.Base;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Components.TextFormats;
using S031.MetaStack.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Common;
using S031.MetaStack.WinForms.ORM;
using System.Windows.Forms;

namespace MetApp
{
	public sealed class Reporter: IReporter
	{
		internal const string ReportFileExtension = ".mrt";
		internal const string TableAliasDefault = "rpt";

		internal static readonly string[] ParentAssemblies = { "S031.MetaStack.Common", "S031.MetaStack.WinForms"};
		internal static readonly string[] ParentUsings = { "S031.MetaStack.Common", "S031.MetaStack.WinForms" };

		public Reporter()
		{
			List<string> assms = new List<string>(StiOptions.Engine.ReferencedAssemblies);
			assms.AddRange(Reporter.ParentAssemblies);
			StiOptions.Engine.ReferencedAssemblies = assms.ToArray();
		}

		#region IReporter Members

		void IReporter.PrintCurrentForm(IObjectHost objectHost)
		{
			DBGrid grid = (objectHost as DBGrid);
			if (grid == null)
				return;
			JObject setup = JObject.Parse(ConfigurationManager.AppSettings["ReportingSettings"]);
			ReportCreateParam rcp = new ReportCreateParam()
			{
				FontSize = (float)setup["FontSize"],
				TitleOnPage = (bool)setup["ColumnTitleOnPage"],
				Period =  (bool)setup["Period"],
				Border = (int)setup["Grid"],
				MultiLine = (bool)setup["MultiLine"],
				Sign = (bool)setup["Sign"]
			};

			StiReport report = makeReport(grid, rcp);
			PrepareReport(report);
			report.Render(true);
			report.Show(false);
		}

		void IReporter.DesignCurrentForm(IObjectHost objectHost)
		{
			DBGrid grid = (objectHost as DBGrid);
			if (grid == null)
				return;
			JObject setup = JObject.Parse(ConfigurationManager.AppSettings["ReportingSettings"]);
			ReportCreateParam rcp = new ReportCreateParam()
			{
				FontSize = (float)setup["FontSize"],
				TitleOnPage = (bool)setup["ColumnTitleOnPage"],
				Period = (bool)setup["Period"],
				Border = (int)setup["Grid"],
				MultiLine = (bool)setup["MultiLine"],
				Sign = (bool)setup["Sign"]
			};

			StiReport report = makeReport(grid, rcp);
			PrepareReport(report);
			report.Design(false);
		}

		void IReporter.Design(IObjectHost objectHost)
		{
			StiReport report = new StiReport();
			PrepareReport(report);
			report.Design(false); 
		}

		internal static void PrepareReport(StiReport report)
		{
			string usings = string.Empty;
			foreach (string item in Reporter.ParentUsings)
			{
				usings += ("using " + item + ";\n");
			}
			report.Script = usings + report.Script;
		}

		void IReporter.ExportCurrentForm(IObjectHost objectHost, ReportExportFormat format)
		{
			DBGrid grid = (objectHost as DBGrid);
			if (grid == null)
				return;

			StiReport report = makeReport(grid, format.CreateParam);
			report.Render(true);
			report.ExportDocument(getExportFormat(format.ID), format.CreateParam.ExportFileName);
		}

		static StiExportFormat getExportFormat(string ID)
		{
			StiExportFormat stiFormat = StiExportFormat.Text;
			switch (ID)
			{
				case "html":
					stiFormat = StiExportFormat.Html;
					break;
				case "xls":
					stiFormat = StiExportFormat.Excel;
					break;
				case "pdf":
					stiFormat = StiExportFormat.Pdf;
					break;
			}
			return stiFormat;
		}

		string IReporter.ConvertReport(string source)
		{
			throw new NotImplementedException();
		}

		void IReporter.OutputReport(string reportName, DataTable dataSource)
		{
			_outputReport(reportName, dataSource, ReportDestinations.Screen, null, null);
		}

		void IReporter.OutputReport(string reportName, DataTable dataSource, ActionParameters reportParameters)
		{
			_outputReport(reportName, dataSource, ReportDestinations.Screen, null, reportParameters);
		}


		void IReporter.OutputReport(string reportName, DataTable dataSource, ReportDestinations outputTarget)
		{
			_outputReport(reportName, dataSource, outputTarget, null, null);
			
		}

		void IReporter.OutputReport(string reportName, DataTable dataSource, ReportDestinations outputTarget, ActionParameters reportParameters)
		{
			_outputReport(reportName, dataSource, outputTarget, null, reportParameters);
		}

		void IReporter.OutputReport(string reportName, DataTable dataSource, ReportExportFormat format, ActionParameters reportParameters)
		{
			_outputReport(reportName, dataSource, ReportDestinations.File, format, reportParameters);
		}

		//Для возможного использования
		//foreach (Stimulsoft.Report.Dictionary.StiVariable variable in report.Dictionary.Variables)
		//{
		//    variable.Value = dbs.GetSetting(variable.Name.Replace('_', ';'));
		//}
		//StiReport r = new StiReport();
		//r.Render();
		//r.RenderedPages.Remove(r.RenderedPages[0]);
		//r.RenderedPages.AddRange(report.RenderedPages);
		//r.RenderedPages.Add(report.RenderedPages[0]);
		//r.Show(true);
		static void _outputReport(string reportName, DataTable dataSource, ReportDestinations outputTarget, ReportExportFormat format, ActionParameters reportParameters)
		{
			string reportsPath = System.IO.Path.Combine(PathHelper.ApplicationPath, "Reports");
			string reportFilePath = System.IO.Path.Combine(reportsPath,
				reportName.EndsWith(ReportFileExtension) ? reportName : reportName + ReportFileExtension);

			StiReport report = new StiReport();
			bool loaded = false;
			if (System.IO.File.Exists(reportFilePath))
			{
				report.Load(reportFilePath);
				loaded = true;
			}
			else
			{
				throw new System.IO.FileNotFoundException($"Report file {reportsPath} not fount");
			}
			if (loaded)
			{
				string tableName = TableAliasDefault; ;
				StiDataBand db = report.GetComponents().OfType<StiDataBand>().FirstOrDefault();
				if (db != null)
				{
					if (!string.IsNullOrEmpty(db.DataSourceName))
						tableName = db.DataSourceName;
					else
						db.DataSourceName = tableName;
				}
				dataSource.TableName = tableName;
				report.RegData(dataSource.TableName, dataSource);
				report.Dictionary.Synchronize();
				report.Dictionary.DataSources[0].Name = tableName;
				report.Dictionary.DataSources[0].Alias = tableName;


				if (reportParameters != null)
				{
					foreach (KeyValuePair<string, object> kvp in reportParameters)
					{
						if (!report.Dictionary.Variables.Contains(kvp.Key))
							report.Dictionary.Variables.Add("Parameters", kvp.Key, kvp.Value);
						else
							report.Dictionary.Variables[kvp.Key].ValueObject = kvp.Value;
					}
				}
				if (outputTarget == ReportDestinations.Designer)
					report.Design(false);
				else if (outputTarget == ReportDestinations.Printer)
				{
					report.Render(true);
					report.Print(true);
				}
				else if (outputTarget == ReportDestinations.File)
				{
					if (format == null)
					{
						format = ReportManager.GetExportReportFormat(
							reportName.EndsWith(ReportFileExtension) ? reportName.Left(reportName.Length - ReportFileExtension.Length) : reportName,
							ReportExportFormats.Formats.First().Value.ID);
					}
					if (format != null)
					{
						report.Render(true);
						if (format.CreateParam.Charset == ReportExportCharsets.Windows_1251)
							report.ExportDocument(getExportFormat(format.ID), format.CreateParam.ExportFileName, 
								new Stimulsoft.Report.Export.StiHtmlExportSettings() { Encoding = System.Text.Encoding.Default });
						else
							report.ExportDocument(getExportFormat(format.ID), format.CreateParam.ExportFileName);
					}
				}
				else
				{
					report.Render(true);
					report.Show();
				}
			}
		}

		#endregion

		static StiReport makeReport(DBGrid grid, ReportCreateParam param)
		{
			JMXSchema schema = grid.Schema;
			DataTable dt = grid.BaseTable;
			double top = 0;
			float fontSize = param.FontSize;

			string tableName = TableAliasDefault; ;
			StiReport report = new StiReport();
			report.ScriptLanguage = StiReportLanguageType.CSharp;
			report.RegData(tableName, dt);
			report.Dictionary.Synchronize();
			report.Dictionary.DataSources[0].Name = tableName;
			report.Dictionary.DataSources[0].Alias = tableName;
			report.ReportName = schema.Name;
			
			StiPage page = report.Pages.Items[0];
			page.Margins.Left = 1;
			page.Margins.Right = 1;
			page.AlignToGrid();


			//ReportTitle
			bool titleOnPage = param.TitleOnPage;
			StiReportTitleBand titleBand = new StiReportTitleBand();
			titleBand.Height = 0.5f;
			titleBand.CanGrow = true;
			titleBand.Name = "TitleBand";
			page.Components.Add(titleBand);

			//Period
			bool period = param.Period;
			if (period)
			{
				StiText periodText = new StiText(new RectangleD(0, 0, page.Width, 0.5f));
				periodText.Text.Value = rth.Za();
				periodText.HorAlignment = StiTextHorAlignment.Right;
				periodText.Name = "periodText";
				periodText.Border.Side = StiBorderSides.None;
				//periodText.DockStyle = StiDockStyle.Right;
				periodText.Font = new Font(grid.Font.FontFamily.Name, fontSize, FontStyle.Bold);
				titleBand.Components.Add(periodText);
				top += periodText.Height;
			}

			//Title
			StiText titleText = new StiText(new RectangleD(0, period ? 0.5f : 0, page.Width, 1f));
			titleText.Text.Value = grid.Schema.Name;
			titleText.HorAlignment = StiTextHorAlignment.Center;
			titleText.Name = "titleText";
			titleText.Border.Side = StiBorderSides.None;
			titleText.Font = new Font(grid.Font.FontFamily.Name, 12f, FontStyle.Bold);
			titleText.WordWrap = true;
			titleText.CanGrow = true;
			titleBand.Components.Add(titleText);
			top += titleText.Height;

			//Create HeaderBand
			StiBand headBand;
			StiHeaderBand headerBand = new StiHeaderBand();
			headerBand.Height = 0.5f;
			headerBand.Name = "HeaderBand";
			if (titleOnPage)
			{
				page.Components.Add(headerBand);
				headBand = headerBand;
			}
			else
				headBand = titleBand;


			//Create Databand
			StiDataBand dataBand = new StiDataBand();
			dataBand.DataSourceName = tableName;
			dataBand.Height = 0.5f;
			dataBand.Name = "DataBand";
			page.Components.Add(dataBand);

			//Create FooterBand
			StiFooterBand footerBand = new StiFooterBand();
			footerBand.Height = 0.5f;
			footerBand.CanGrow = true;
			footerBand.Name = "FooterBand";
			footerBand.CanBreak = false;
			page.Components.Add(footerBand);

			Double pos = 0;
			int nameIndex = 1;
			int i = 0;
			bool multiLine = param.MultiLine;
			int border = param.Border;
			bool totals = false;
			List<StiText> footerList = new List<StiText>();
			foreach (DataGridViewColumn column in grid.Columns)
			{
				bool visible = (column.Visible && column.Width > 1);

				if (visible)
				{
					Double columnWidth = StiAlignValue.AlignToMinGrid(column.Width/33, 0.03, true);
					
					//Create text on header
					StiText headerText = new StiText(new RectangleD(pos, titleOnPage ?  0: top, columnWidth, 0.5f));
					headerText.Text.Value = column.HeaderText;
					headerText.HorAlignment = StiTextHorAlignment.Center;
					headerText.Name = "HeaderText" + nameIndex.ToString();
					if (border == 0)
						headerText.Border.Side = StiBorderSides.Top | StiBorderSides.Bottom;
					else
						headerText.Border.Side = StiBorderSides.All;
					headerText.CanGrow = true;
					headerText.GrowToHeight = true;
					headerText.WordWrap = multiLine;
					headerText.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily.Name, fontSize);
					headBand.Components.Add(headerText);

					MacroType tt = DBGridBase.GetMacroType(schema.Attributes[i].DataType);
					string format = column.DefaultCellStyle.Format;
					if (string.IsNullOrEmpty(format) && tt == MacroType.date)
						format = vbo.DateFormat;

					StiTextHorAlignment horAlign;
					switch (column.DefaultCellStyle.Alignment)
					{
						case DataGridViewContentAlignment.MiddleRight:
							horAlign = StiTextHorAlignment.Right;
							break;
						case DataGridViewContentAlignment.MiddleLeft:
							horAlign = StiTextHorAlignment.Left;
							break;
						default:
							horAlign = StiTextHorAlignment.Center;
							break;
					}

					//Create text on Data Band
					StiText dataText = new StiText(new RectangleD(pos, 0, columnWidth, 0.5f));
					string field = tableName + "." + Stimulsoft.Report.CodeDom.StiCodeDomSerializator.ReplaceSymbols(column.DataPropertyName);
					if (AccntFormat.ValidFormat(format))
					{
						dataText.Text.Value = "{Substring(" + field + ",0,5)+" + "-".Qt() +
							"+Substring(" + field + ",5,3)+" + "-".Qt() +
							"+Substring(" + field + ",8,1)+" + "-".Qt() +
							"+Substring(" + field + ",9,4)+" + "-".Qt() +
							"+Substring(" + field + ",13,7)" + "}";
					}
					else
						dataText.Text.Value = "{" + field + "}";
					dataText.Name = "DataText" + nameIndex.ToString();
					dataText.CanGrow = true;
					dataText.GrowToHeight = true;
					dataText.WordWrap = multiLine;
					dataText.HorAlignment = horAlign;
					//dataText.VertAlignment = StiVertAlignment.Center;
					if (border == 0)
						dataText.Border.Side = StiBorderSides.None;
					else if (border == 1)
						dataText.Border.Side = StiBorderSides.Bottom;
					else
						dataText.Border.Side = StiBorderSides.All;
					if (grid.RowsDefaultCellStyle.Font == null)
						dataText.Font = new Font(grid.Font.FontFamily.Name, fontSize);
					else
						dataText.Font = new Font(grid.RowsDefaultCellStyle.Font.FontFamily.Name, fontSize);

					dataText.TextFormat = parseFormat(format);
					if (tt == MacroType.num)
						dataText.ExcelValue.Value = "{" + field + "}";
					dataBand.Components.Add(dataText);

					//Create text on footer
					string summary = schema.Attributes[i].Agregate.ToLower();
					if (summary == "sum")
						summary = "Sum(" + field + ")";
					else if (summary == "rcnt")
						summary = "Count()";
					else if (summary == "ave")
						summary = "Avg(" + field + ")";
					else if (summary == "min")
						summary = "Min(" + field + ")";
					else if (summary == "max")
						summary = "Max(" + field + ")";
					else
						summary = string.Empty;
					bool summaryEmpty = string.IsNullOrEmpty(summary);

					int footerIndex = i;
					//if (!string.IsNullOrEmpty(grid.FooterText(footerIndex)))
					if (!summaryEmpty)
						totals = true;

					StiText footerText = new StiText(new RectangleD(pos, 0, columnWidth, 0.5f));
					if (footerIndex == 0 && summaryEmpty)
						footerText.Text.Value = grid.FooterCaption;
					else if (footerIndex == 0)
						footerText.Text.Value = "{" + (grid.FooterCaption + vbo.vbTab).Qt() + "+" + summary + "}";
					else if (!summaryEmpty)
					{
						footerText.Text.Value = "{" + summary + "}";
						footerText.ExcelValue.Value = "{" + summary + "}";
					}
					else
						footerText.Text.Value = grid.FooterText(footerIndex);

					footerText.Name = "FooterText" + nameIndex.ToString();
					if (border == 0)
						footerText.Border.Side = StiBorderSides.Top | StiBorderSides.Bottom;
					else
						footerText.Border.Side = StiBorderSides.All;
					
					if (!summaryEmpty && summary != "Count()")
						footerText.TextFormat = (StiFormatService)dataText.TextFormat.Clone();

					footerText.HorAlignment = horAlign;
					//footerText.VertAlignment = StiVertAlignment.Center;
					footerText.Font = new Font(grid.Font.FontFamily.Name, fontSize);
					footerList.Add(footerText);

					pos += columnWidth;

					nameIndex++;
				}
				i++;
			}
			if (totals)
				footerBand.Components.AddRange(footerList.ToArray());

			if ((pos - page.Width) < 1.6f)
			{
				page.Margins.Right = 0;
				page.Margins.Left /= 2;
			}
			else if (pos > page.Width)
			{
				page.Orientation = StiPageOrientation.Landscape;
				titleText.Width = page.Width;
				if (period)
					titleBand.Components[titleBand.Components.IndexOf("periodText")].Width = page.Width;
				if (pos > page.Width)
				{
					page.Margins.Right = 0;
					page.Margins.Left /= 2;
				}
				if (pos > page.Width)
				{
					page.Width = pos;
				}
			}

			if (param.Sign)
			{
				footerList.Clear();
				//StiText footerText = new StiText(new RectangleD(0, 0.5f, page.Width, 0.5f));
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(0, 1f, page.Width / 2, 0.5f));
				//footerText.Text.Value = "{CSetup.Setup.Properties(" + vbo.Qt("President") + ", " + vbo.Qt("") + ")}";
				//footerText.HorAlignment = StiTextHorAlignment.Right;
				//footerText.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily.Name, 10);
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(page.Width / 2, 1f, page.Width / 4, 0.5f));
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(page.Width * 3 / 4, 1f, page.Width / 4, 0.5f));
				//footerText.Text.Value = "{CSetup.Setup.Properties(" + vbo.Qt("PresidentName") + ", " + vbo.Qt("") + ")}";
				//footerText.HorAlignment = StiTextHorAlignment.Left;
				//footerText.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily.Name, 10);
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(0, 1.5f, page.Width, 0.5f));
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(0, 2f, page.Width / 2, 0.5f));
				//footerText.Text.Value = "{CSetup.Setup.Properties(" + vbo.Qt("ChifAccount") + ", " + vbo.Qt("") + ")}";
				//footerText.HorAlignment = StiTextHorAlignment.Right;
				//footerText.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily.Name, 10);
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(page.Width / 2, 2f, page.Width / 4, 0.5f));
				//footerList.Add(footerText);
				//footerText = new StiText(new RectangleD(page.Width * 3 / 4, 2f, page.Width / 4, 0.5f));
				//footerText.Text.Value = "{CSetup.Setup.Properties(" + vbo.Qt("ChifAccountName") + ", " + vbo.Qt("") + ")}";
				//footerText.HorAlignment = StiTextHorAlignment.Left;
				//footerText.Font = new Font(grid.ColumnHeadersDefaultCellStyle.Font.FontFamily.Name, 10);
				//footerList.Add(footerText);
				//footerBand.Height = 2.5f;
				//footerBand.Components.AddRange(footerList.ToArray());
			}
			return report;
		}

		static StiFormatService parseFormat(string format)
		{
			if (string.IsNullOrEmpty(format))
				return new StiGeneralFormatService();
			else
				return new StiCustomFormatService(format);
		}



	}
}
