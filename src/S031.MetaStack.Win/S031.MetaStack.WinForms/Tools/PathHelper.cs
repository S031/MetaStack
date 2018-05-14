using System;

namespace S031.MetaStack.WinForms
{
	public static class PathHelper
	{
		/// <summary>
		/// Путь к текущему исполняемому файлу программы включая имя файла
		/// </summary>
		public static string ExecutablePath { get { return System.Windows.Forms.Application.ExecutablePath; } }
		
		/// <summary>
		/// Имя исполняемого файла программы включая расширение
		/// </summary>
		public static string ExecutableName { get { return System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath); } }
		
		/// <summary>
		/// Имя исполняемого файла программы без расширения
		/// </summary>
		public static string ApplicationName { get { return System.IO.Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath); } }

		/// <summary>
		/// Путь к каталогу запуска программы
		/// </summary>
		public static string ApplicationPath { get { return System.Windows.Forms.Application.StartupPath; } }

		/// <summary>
		/// Тукущий каталог
		/// </summary>
		public static string CurrentPath { get { return System.IO.Directory.GetCurrentDirectory(); } }

		/// <summary>
		/// Путь поиска dll при загрузке в домен приложения
		/// </summary>
		public static string BaseDllPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }

		/// <summary>
		/// Каталог с данными приложения
		/// </summary>
		public static string ApplicationDataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); } }
		
		/// <summary>
		/// Каталог с данными пользователя
		/// </summary>
		public static string UserDataPath { get { return System.Windows.Forms.Application.UserAppDataPath; } }

		/// <summary>
		/// Путь к рабочему столу
		/// </summary>
		public static string DesktopPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop); } }

		/// <summary>
		/// Путь к каталогу Мои Документы
		/// </summary>
		public static string MyDocumentsPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); } }

		/// <summary>
		/// Путь к папке System(32,64 и т.д.)
		/// </summary>
		public static string SystemPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.System); } }

		/// <summary>
		/// Путь к папке  Program Files
		/// </summary>
		public static string ProgramFilesPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); } }

		/// <summary>
		/// Путь к папке Templates
		/// </summary>
		public static string TemplatesPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Templates); } }

		public static string UserTempPath { get { return System.IO.Path.GetTempPath(); } }
	}
}
