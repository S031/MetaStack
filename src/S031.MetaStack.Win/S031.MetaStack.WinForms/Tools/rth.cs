using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms
{
	public static class rth
	{
		public static DateTime DateStart
		{
			get => LocalSettings.Default.DateStart;
			set => LocalSettings.Default.DateStart = value;
		}
		public static DateTime DateFinish
		{
			get => LocalSettings.Default.DateFinish;
			set => LocalSettings.Default.DateFinish = value;
		}
		public static string Za()
		{
			return Za(rth.DateStart, rth.DateFinish);
		}
		public static string Za(DateTime date)
		{
			return Za(date, date);
		}
		public static string Za(DateTime dateStart, DateTime dateFinish)
		{
			if (!dateStart.IsEmpty()) dateStart = rth.DateStart;
			if (!dateFinish.IsEmpty()) dateFinish = rth.DateFinish;
			if (dateStart == dateFinish)
				return dateFinish.ToString("dd MMMM yyyy г.");
			return "период с " + dateStart.ToString(vbo.DateFormat + " г.") + " по " + dateFinish.ToString(vbo.DateFormat + " г.");
		}
	}
}
