using S031.MetaStack.Common;
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
				const int loop_count = 1_000_000;
				int[] test_data = new int[] { 32, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 65536, 131072, 524288, 1048576/*, 2097152, 4194304*/ };
				foreach (int item_size in test_data)
				{
					string key = fixed_key_part + (item_size / 2).ToString();
					DateTime start = DateTime.Now;
					MapTable<string, int> d = new MapTable<string, int>();
					//for (int i = 1; i < item_size; i++)
					//	d.Add(fixed_key_part + i.ToString(), i);
					System.Threading.Tasks.Parallel.For(0, item_size, i =>
						d.Add(fixed_key_part + i.ToString(), i));
					DateTime stop = DateTime.Now;
					l.Debug($"Create MapTable with {item_size} with colissions = {d.Collisions} elements. Time = {(stop - start).TotalMilliseconds} ms");

					Dictionary<string, int> d1 = new Dictionary<string, int>();
					object o = new object();
					start = DateTime.Now;
					System.Threading.Tasks.Parallel.For(0, item_size, i =>
					{
						lock (o)
							d1.Add(fixed_key_part + i.ToString(), i);
					});
					stop = DateTime.Now;
					l.Debug($"Create Dictionary with {item_size} elements. Time = {(stop - start).TotalMilliseconds} ms");
					
					start = DateTime.Now;
					ReadOnlyCache<string, int> d2 = new ReadOnlyCache<string, int>(
						Enumerable.Range(0, item_size)
						//.Select(i => new KeyValuePair<string, int>(fixed_key_part + i.ToString(), i))
						.Select(i => (fixed_key_part + i.ToString(), i))
						.ToArray());
					stop = DateTime.Now;
					l.Debug($"Create ReadOnlyCache with {item_size} elements. Time = {(stop - start).TotalMilliseconds} ms");


					start = DateTime.Now;
					int k = 0;
					for (int i = 0; i < loop_count; i++)
					{
						k = d[key];
					}
					stop = DateTime.Now;
					l.Debug($"MapTable TryGetValue with {loop_count} runs index of = {k} whith fragmentation = {d.Fragmentation} total time = {(stop - start).TotalMilliseconds} ms");

					start = DateTime.Now;
					for (int i = 0; i < loop_count; i++)
					{
						lock (o)
							k = d1[key];
					}
					stop = DateTime.Now;
					int fragmentation = (d1.GetType().GetField("_buckets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(d1) as int[]).Count(p => p == -1);
					l.Debug($"Dictionary TryGetValue with {loop_count} runs index of = {k} whith fragmentation = {fragmentation} total time = {(stop - start).TotalMilliseconds} ms");

					start = DateTime.Now;
					for (int i = 0; i < loop_count; i++)
					{
						lock (o)
							k = d2[key];
					}
					stop = DateTime.Now;
					l.Debug($"ReadOnlyCache TryGetValue with {loop_count} runs index of = {k} total time = {(stop - start).TotalMilliseconds} ms");
				}

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

