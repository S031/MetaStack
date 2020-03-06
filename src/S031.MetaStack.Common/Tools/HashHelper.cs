using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Common
{
	public unsafe static class HashHelper
	{
		const uint FNV_prime = 16777619;
		const uint FNV_basis = 2166136261U;

		/// <summary>
		/// For internal usage for getting hash code from const string used in switch/case statement 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static uint ComputeHash(this string s)
		{
			uint num = FNV_basis;
			fixed (char* source = s)
			{
				char* p = source;
				for (int index = 0; index < s.Length; index++)
				{
					num = (uint)(((int)*p ^ (int)num) * FNV_prime);
					p++;
				}
			}
			return num;
		}
	}
}
