// CSharp Editor Example with Code Completion
// Copyright (c) 2006, Daniel Grunwald
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other materials
//   provided with the distribution.
// 
// - Neither the name of the ICSharpCode team nor the names of its contributors may be used to
//   endorse or promote products derived from this software without specific prior written
//   permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System;
using System.Linq;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public sealed class CodeCompletionProvider : ICompletionDataProvider
	{
		static readonly Regex _tester = new Regex(@"\((.*?)\)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		static readonly ImageList _il = getImageList();

		static readonly List<string> _langs = new List<string>() { "VBNET", "SQL" };
		static readonly List<char> _quoteChar = new List<char>() { '"', '\'' };

		bool _CtrlEnter;
		bool _letterStart;
		string _lang;

		static ImageList getImageList()
		{
			ImageList iList = new ImageList();
			iList.Images.Add(ResourceManager.GetImage("Statement")); //Statement
			iList.Images.Add(ResourceManager.GetImage("Eval"));	//Function
			iList.Images.Add(ResourceManager.GetImage("Flag"));	//Constant
			iList.Images.Add(ResourceManager.GetImage("Func"));	//Method
			iList.Images.Add(ResourceManager.GetImage("Prop"));	//Property
			iList.Images.Add(ResourceManager.GetImage("Method"));	//Variable
			iList.Images.Add(ResourceManager.GetImage("Table"));	//Table
			return iList;
		}

		public bool LetterStart
		{
			get { return _letterStart; }
			set { _letterStart = value; }
		}

		public bool CtrlEnter
		{
			get { return _CtrlEnter; }
			set { _CtrlEnter = value; }
		}

		public string LanguageID { get { return _lang; } }

		public CodeCompletionProvider(string languageID)
		{
			_lang = languageID;
		}
		
		public ImageList ImageList {
			get { return _il; }
		}
		
		public string PreSelection {
			get 
			{
				if (LetterStart)
					return string.Empty;
				return null;
			}
		}
		
		public int DefaultIndex {
			get {
				return -1;
			}
		}

		public CompletionDataProviderKeyResult ProcessKey(char key)
		{
			if (char.IsLetterOrDigit(key) || key == '_')
			{
				return CompletionDataProviderKeyResult.NormalKey;
			}
			else
			{
				// key triggers insertion of selected items
				return CompletionDataProviderKeyResult.InsertionKey;
			}
		}
		
		/// <summary>
		/// Called when entry should be inserted. Forward to the insertion action of the completion data.
		/// </summary>
		public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
		{
			textArea.Caret.Position = textArea.Document.OffsetToPosition(CtrlEnter ? insertionOffset - 1 : insertionOffset);
			return data.InsertAction(textArea, key);
		}
		
		public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			List<ICompletionData> list = new List<ICompletionData>(512);
			if (charTyped != '.')
			{
				if (_lang.Equals("VBNET", System.StringComparison.CurrentCultureIgnoreCase))
				{
					list.AddRange(CodeCompletionData.VBSStatements);
					list.AddRange(CodeCompletionData.VBSConstants);
					list.AddRange(CodeCompletionData.VBSFunctions);
					list.AddRange(CodeCompletionData.NewbankStatements);
					list.AddRange(CodeCompletionData.NewbankConstants);
					list.AddRange(CodeCompletionData.NewbankFunctions);
					list.AddRange(MethodList(textArea));
					list.AddRange(VarList(textArea));
					list.AddRange(ModuleVarList(textArea));
				}
				else if (_lang.Equals("SQL", System.StringComparison.CurrentCultureIgnoreCase))
				{
					list.AddRange(CodeCompletionData.SQLStatements);
					list.AddRange(CodeCompletionData.SQLDataTypes);
					list.AddRange(CodeCompletionData.SQLFunctions);
					list.AddRange(CodeCompletionData.SQLTables);
					list.AddRange(GetTableAliasList(textArea.Document.TextContent).Select<KeyValuePair<string, string>, ICompletionData>((kvp, i) =>
						new DefaultCompletionData(kvp.Key, kvp.Value, 6)).ToArray());
				}
			}
			else
			{
				if (_lang.Equals("VBNET", System.StringComparison.CurrentCultureIgnoreCase))
				{
					list.AddRange(CodeCompletionData.VBSMethods);
					list.AddRange(CodeCompletionData.VBSProperties);
					list.AddRange(CodeCompletionData.NewbankMethods);
					list.AddRange(CodeCompletionData.NewbankProperties);
				}
				else if (_lang.Equals("SQL", System.StringComparison.CurrentCultureIgnoreCase))
				{
					list.AddRange(SQLFieldList(textArea));
				}
			}
			return list.ToArray();
		}

		static ICompletionData[] VarList(TextArea textArea)
		{
			List<ICompletionData> list = new List<ICompletionData>(8);
			IDocument doc = textArea.Document;
			for (int i = doc.GetLineNumberForOffset(textArea.Caret.Offset); i >= 0; i--)
			{
				LineSegment line = doc.GetLineSegment(i);
				string lineText = doc.GetText(line.Offset, line.Length);
				if (!string.IsNullOrEmpty(procName(lineText)))
				{
					list.AddRange(paramNames(lineText));
					break;
				}
				list.AddRange(varNames(lineText));
			}

			return list.ToArray();
		}

		static ICompletionData[] ModuleVarList(TextArea textArea)
		{
			List<ICompletionData> list = new List<ICompletionData>(8);
			IDocument doc = textArea.Document;
			for (int i = 0; i < doc.GetLineNumberForOffset(textArea.Caret.Offset); i++)
			{
				LineSegment line = doc.GetLineSegment(i);
				string lineText = doc.GetText(line.Offset, line.Length);
				if (!string.IsNullOrEmpty(procName(lineText)))
					break;
				
				list.AddRange(varNames(lineText));

				SourceCodeCompletionData constanta = constName(lineText);
				if (constanta != null)
					list.Add(constanta);
			}

			return list.ToArray();
		}
		
		static ICompletionData[] paramNames(string line)
		{
			List<ICompletionData> tokens = new List<ICompletionData>();
			line = line.TruncDub(" ").TrimStart();
			line = line.Replace(vbo.vbTab, "");
			MatchCollection mc = _tester.Matches(line);
			
			if (mc.Count>0)
			{
				string description = line;
				line = mc[0].Value.Substring(1, mc[0].Value.Length - 2);

				foreach(string item in line.Split(','))
				{
					string text = item.Trim();
					int sepIndex = text.IndexOf('(');
					if (sepIndex > -1)
						tokens.Add(new SourceCodeCompletionData(text.Substring(0, sepIndex), description, CodeCompletionData._variable));
					else
						tokens.Add(new SourceCodeCompletionData(text, description, CodeCompletionData._variable));
				}
			}
			return tokens.ToArray();
		}

		public static ICompletionData[] MethodList(TextArea textArea)
		{
			List<ICompletionData> list = new List<ICompletionData>(16);
			IDocument doc = textArea.Document;
			int count = doc.TotalNumberOfLines;
			for (int i = 0; i < count; i++)
			{
				LineSegment line = doc.GetLineSegment(i);
				string lineText = doc.GetText(line.Offset, line.Length);
				string text = procName(lineText);
				if (!string.IsNullOrEmpty(text))
					list.Add(new SourceCodeCompletionData(text, lineText, CodeCompletionData._function) { LineNumber = i });
			}
			return list.ToArray();
		}

		static ICompletionData[] varNames(string line)
		{
			List<ICompletionData> tokens = new List<ICompletionData>();
			line = line.TruncDub(" ").TrimStart();
			line = line.Replace(vbo.vbTab, "");
			if (line.Length > 4 && line.Substring(0, 4).ToLower() == "dim ")
			{
				string description = line;
				int i = line.IndexOf(':');
				if (i > 4)
					line = line.Substring(4, i - 4);
				else
					line = line.Substring(4);

				foreach(string text in line.Split(','))
				{
					int sepIndex = text.IndexOf('(');
					if (sepIndex > -1)
						tokens.Add(new SourceCodeCompletionData(text.Substring(0, sepIndex), description, CodeCompletionData._variable));
					else
						tokens.Add(new SourceCodeCompletionData(text, description, CodeCompletionData._variable));
				}
			}
			return tokens.ToArray();
		}

		static SourceCodeCompletionData constName(string line)
		{
			line = line.TruncDub(" ").TrimStart();
			line = line.Replace(vbo.vbTab, "");

			int pos = line.ToLower().IndexOf("const ");
			if (pos > -1)
			{
				int i = line.IndexOf("=", pos+5);
				if (i > -1)
				{
					string cName = line.Substring(pos+6, i - pos - 6).Trim();
					return new SourceCodeCompletionData(cName, line, CodeCompletionData._constant);
				}
			}
			return null;
		}

		static string procName(string line)
		{
			line = line.TruncDub(" ").TrimStart();
			line = line.Replace(vbo.vbTab, "");

			string[] buff = line.Split(' ');
			if (buff.Length < 2)
				return string.Empty;
			else if (buff[0].ToLower() == "sub" || buff[0].ToLower() == "function")
				line = buff[1];
			else if ((buff[0].ToLower() == "private" || buff[0].ToLower() == "public") &&
				(buff[1].ToLower() == "sub" || buff[1].ToLower() == "function"))
				line = buff[2];
			else if (buff[0].ToLower() == "property" &&
				(buff[1].ToLower() == "get" || buff[1].ToLower() == "let" || buff[1].ToLower() == "let"))
				line = buff[2];
			else if (buff.Length < 4)
				return string.Empty;
			else if ((buff[0].ToLower() == "private" || buff[0].ToLower() == "public") &&
				(buff[1].ToLower() == "property" &&
				(buff[2].ToLower() == "get" || buff[2].ToLower() == "let" || buff[2].ToLower() == "let")))
				line = buff[2] + " " + buff[3];
			else
				return string.Empty;

			int i = line.IndexOf('(');
			if (i > -1)
				line = line.Substring(0, i);

			return line;
		}

		public static bool IsLanguageSupported(string langugeID)
		{
			return _langs.Contains(langugeID);
		}

		public static char QuoteChar(string langID)
		{
			int i = _langs.IndexOf(langID);

			if (i >= 0) return _quoteChar[i];
			return '"';
		}
	
		static ICompletionData[] SQLFieldList(TextArea textArea)
		{
			string table = getTableName(textArea);
			Dictionary<string, string> tableAliasList = GetTableAliasList(textArea.Document.TextContent);
			if (tableAliasList.ContainsKey(table))
				return CodeCompletionData.SQLFields(tableAliasList[table]);
			else if (!string.IsNullOrEmpty(table))
				return CodeCompletionData.SQLFields(table);
			return new List<ICompletionData>().ToArray();
		}

		static string getTableName(TextArea textArea)
		{
			IDocument doc = textArea.Document;
			LineSegment line = doc.GetLineSegment(doc.GetLineNumberForOffset(textArea.Caret.Offset));
			string str = doc.GetText(line.Offset, textArea.Caret.Position.X);
			string table = "";
			int pos = 0;
			for (int i = str.Length - 1; i >= 0; i--)
			{
				if (IsLegalNameSymbol(str[i]))
					pos++;
				else
					break;
			}
			if (pos > 0) table = str.Substring(str.Length - pos);
			return table;
		}

		private static bool IsLegalNameSymbol(char key)
		{
			return (char.IsLetter(key) && ((key >= '\x0041' && key < '\x005A') || (key >= '\x0061' && key < '\x007A'))) ||
				char.IsNumber(key);
		}

		static Dictionary<string, string> GetTableAliasList(string sql)
		{
			sql = sql.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').TruncDub(" ");
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
			ICompletionData[] tableList = CodeCompletionData.SQLTables;
			foreach (ICompletionData item in tableList)
			{
				string table = item.Text;
				int pos = sql.IndexOf(' ' + table + ' ', StringComparison.CurrentCultureIgnoreCase);
				for (; pos > -1; )
				{
					string alias = table;
					int start = pos + table.Length + 2;
					int i;
					for (i = start; i < sql.Length && IsLegalNameSymbol(sql[i]); i++)
					{
					}
					if (i > start) alias = sql.Substring(start, i - start);
					if (!result.ContainsKey(alias)) result.Add(alias, table); 
					pos = sql.IndexOf(' ' + table + ' ', start, StringComparison.CurrentCultureIgnoreCase);
				}
			}
			return result;
		}
	}

	public sealed class SourceCodeCompletionData : DefaultCompletionData
	{
		public SourceCodeCompletionData(string text, string description, int imageIndex) : base(text, description, imageIndex) { }
		public int LineNumber { get; set; }
	}
}
