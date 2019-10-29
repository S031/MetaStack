﻿using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MetaStack.Test.Common
{
	public class MapTableTest
	{
		private const string fixed_key_part = "For specified conditions (PasswordGeneratorOptions), the password length can not be less than characters = ";
		public MapTableTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		private void MapTableTests()
		{
			using (FileLog l = new FileLog("MapTableTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				const int loop_count = 1_000_010;

				DateTime start = DateTime.Now;
				MapTable<string, int> d = new MapTable<string, int>();
				d.Add(fixed_key_part + "0", 0);
				//for (int i = 1; i < loop_count; i++)
				//	d.Add(fixed_key_part + i.ToString(), i);
				System.Threading.Tasks.Parallel.For(1, loop_count, i =>
					d.Add(fixed_key_part + i.ToString(), i));
				DateTime stop = DateTime.Now;
				l.Debug($"Create MapTable with {loop_count} elements. Time = {(stop - start).TotalMilliseconds} ms");

				Dictionary<string, int> d1 = new Dictionary<string, int>();
				object o = new object();
				start = DateTime.Now;
				System.Threading.Tasks.Parallel.For(0, loop_count, i =>
				{
					lock (o)
						d1.Add(fixed_key_part + i.ToString(), i);
				});
				stop = DateTime.Now;
				l.Debug($"Create Dictionary with {loop_count} elements. Time = {(stop - start).TotalMilliseconds} ms");


				start = DateTime.Now;
				int k = 0;
				for (int i = 0; i < loop_count; i++)
				{
					k = d[fixed_key_part + "500011"];
				}
				stop = DateTime.Now;
				l.Debug($"MapTable TryGetValue with {loop_count} runs index of = {k} whith fragmentation = {d.Fragmentation} total time = {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (int i = 0; i < loop_count; i++)
				{
					lock (o)
						k = d1[fixed_key_part + "500011"];
				}
				stop = DateTime.Now;
				int fragmentation = (d1.GetType().GetField("_buckets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(d1) as int[]).Count(p => p == -1);
				l.Debug($"Dictionary TryGetValue with {loop_count} runs index of = {k} whith fragmentation = {fragmentation} total time = {(stop - start).TotalMilliseconds} ms");

				//ConcurrentDictionary<string, int> d2 = new ConcurrentDictionary<string, int>();
				//start = DateTime.Now;
				//System.Threading.Tasks.Parallel.For(0, loop_count, i =>
				//	d2.TryAdd(fixed_key_part + i.ToString(), i));
				//stop = DateTime.Now;
				//l.Debug($"Create ConcurrentDictionary  with {loop_count} elements. Time = {(stop - start).TotalMilliseconds} ms");

				//start = DateTime.Now;
				//for (int i = 0; i < loop_count; i++)
				//	d2.TryGetValue(fixed_key_part + "500011", out k);
				//stop = DateTime.Now;
				//l.Debug($"ConcurrentDictionary TryGetValue with {loop_count} runs index of = {k} whith fragmentation = {0} total time = {(stop - start).TotalMilliseconds} ms");
			}
		}
	}
}

