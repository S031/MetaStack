using S031.MetaStack.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSSubjects: CKSOperationResult, IDictionary<long, CKSSubject>
	{
		private readonly MapTable<long, CKSSubject> _subjects = new MapTable<long, CKSSubject>();
		
		public CKSSubjects(string cksResponse) : base(cksResponse)
		{
			if (Status == CKSOperationStatus.ok)
			{
				var xsubjs = Source.Element("clients");

				foreach (var client in xsubjs.Elements())
					_subjects.Add(client.Attribute("id").Value.ToLongOrDefault(), new CKSSubject(client));
			}
		}

		public CKSSubject this[long key] { get => _subjects[key]; set => _subjects[key] = value; }

		public ICollection<long> Keys => _subjects.Keys;

		public ICollection<CKSSubject> Values => _subjects.Values;

		public int Count => _subjects.Count;

		public bool IsReadOnly => true;

		public void Add(long key, CKSSubject value)
			=>	throw new NotImplementedException();

		public void Add(KeyValuePair<long, CKSSubject> item)
			=> throw new NotImplementedException();

		public void Clear()
			=> throw new NotImplementedException();

		public bool Contains(KeyValuePair<long, CKSSubject> item)
			=> _subjects.Contains(item);

		public bool ContainsKey(long key)
			=> _subjects.ContainsKey(key);

		public void CopyTo(KeyValuePair<long, CKSSubject>[] array, int arrayIndex)
			=> throw new NotImplementedException();

		public IEnumerator<KeyValuePair<long, CKSSubject>> GetEnumerator()
			=> _subjects.GetEnumerator();

		public bool Remove(long key)
			=> throw new NotImplementedException();

		public bool Remove(KeyValuePair<long, CKSSubject> item)
			=> throw new NotImplementedException();

		public bool TryGetValue(long key, [MaybeNullWhen(false)] out CKSSubject value)
			=> _subjects.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator()
			=> _subjects.GetEnumerator();

		public override string ToString()
			=> Source.ToString();

	}


}
