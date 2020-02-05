using System;
using System.Collections;
using System.Collections.Generic;

namespace TypeKitchen.Tests.Serialization
{
	public class ValueTypes : IEnumerable<object[]>
	{
		private static object[] Single(object instance) => new[] { instance };

		public IEnumerator<object[]> GetEnumerator()
		{
			yield return Single((byte) 255);
			yield return Single((sbyte) -128);
			yield return Single(true);
			yield return Single(false);
			yield return Single(123);
			yield return Single((short) 123);
			yield return Single((ushort) 123);
			yield return Single(12345678L);
			yield return Single(12345678UL);
			yield return Single(123.456f);
			yield return Single(123.456m);
			yield return Single(123.456d);

			yield return Single(default(TimeSpan));
			yield return Single(default(DateTime));
			yield return Single(default(DateTimeOffset));

			yield return Single(default(TimeSpan?));
			yield return Single(default(DateTime?));
			yield return Single(default(DateTimeOffset?));
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}