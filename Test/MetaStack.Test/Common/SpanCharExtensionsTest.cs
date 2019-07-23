﻿using Xunit;
using S031.MetaStack.Common;
using System;
using S031.MetaStack.Common.Logging;

namespace MetaStack.Test.Common
{
	public class SpanExtensionsTest
	{
		public SpanExtensionsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		void SpanCharTest()
		{
			using (FileLog l = new FileLog("SpanCharExtensionsTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime start = DateTime.Now;
				Type t = this.GetType();
				string s = string.Empty;
				string source = Environment.StackTrace;
				int i = 0;
				for (i = 0; i < 100_000; i++)
				{
					s = source.GetToken(5, "\r\n");
				}
				DateTime stop = DateTime.Now;
				l.Debug($"String.GetToken Return for {i} runs look {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (i = 0; i < 100_000; i++)
				{
					s = source.AsSpan()
						.GetToken(5, "\r\n")
						.ToString();
				}
				stop = DateTime.Now;
				l.Debug($"Sapn.GetToken Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (i = 0; i < 100_000; i++)
				{
					s = source.Split("\r\n".ToCharArray())[5];
				}
				stop = DateTime.Now;
				l.Debug($"String.Split.GetToken Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (i = 0; i < 1_000_000; i++)
				{
					s = source.Left(100);
				}
				stop = DateTime.Now;
				l.Debug($"String.Left Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (i = 0; i < 1_000_000; i++)
				{
					s = source.AsSpan()
						.Left(100)
						.ToString();
				}
				stop = DateTime.Now;
				l.Debug($"Span.Left Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				//source = s;
				for (i = 0; i < 1_000_000; i++)
				{
					s = source.Qt();
				}
				stop = DateTime.Now;
				l.Debug($"String.Qt Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");

				start = DateTime.Now;
				for (i = 0; i < 1_000_000; i++)
				{
					s = string.Concat('"', source, '"');
				}
				stop = DateTime.Now;
				l.Debug($"String.Concat Return for {i} runs took value = {s} {(stop - start).TotalMilliseconds} ms");
			}
		}
	}
}

