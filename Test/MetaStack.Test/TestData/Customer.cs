using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Buffers;
using S031.MetaStack.Common;

namespace MetaStack.Test
{
	[Serializable]
	public class Customer
	{
		static Customer()
		{
			BinaryDataFormatterService.Register<CustomerFormatter, Customer>();
		}

		public virtual int Id { get; set; }
		public virtual string Title { get; set; }
		public virtual DateTime RegisterDate { get; set; }
		public virtual IList<Address> AddressList { get; set; } = new List<Address>();
		public virtual IDictionary<string, object> StatistcsCodes { get; set; } = new MapTable<string, object>();

		public static Customer CreateTestCustomer()
		{
			Customer c = new Customer()
			{
				Id = 1,
				RegisterDate = DateTime.Now,
				Title = "ПАО Металлинвестбанк"
			};
			c.StatistcsCodes["INN"] = "99999977777";
			c.StatistcsCodes["OKPO"] = "1234-12345";

			c.AddressList.Add(new Address()
			{
				Zip = "101000",
				Region = "77",
				City = "Москва",
				Street = "Большая полянка",
				House = 47,
				RegisterDate = DateTime.Now

			});
			c.AddressList.Add(new Address()
			{
				Zip = "119634",
				Region = "77",
				City = "Москва",
				Street = "Китай-город",
				House = 10,
				RegisterDate = DateTime.Now
			});
			return c;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Id:{Id}, Title:{Title}, RegisterDate:{RegisterDate}");
			sb.AppendLine($"AddressList: [");
			foreach (var a in AddressList)
				sb.AppendLine($"\t{{ Zip:{a.Zip}, Region:{a.Region}, City:{a.City}, Street:{a.Street}, House:{a.House}, Flat:{a.Flat} }}");
			sb.AppendLine($"]");
			sb.AppendLine($"StatistcsCodes: [");
			foreach (var s in StatistcsCodes)
				sb.AppendLine($"\t{{ {s.Key}:{s.Value} }}");
			sb.AppendLine($"]");

			return sb.ToString();
		}
	}

	[Serializable]
	public class Address
	{
		public virtual string Zip { get; set; }
		public virtual string Region { get; set; }
		public virtual string City { get; set; }
		public virtual string Street { get; set; }
		public virtual int House { get; set; }
		public virtual int Flat { get; set; }
		public virtual DateTime RegisterDate { get; set; } = DateTime.Now;
	}

	public class CustomerFormatter : IBinaryDataFormatter
	{
		public static CustomerFormatter Instance = new CustomerFormatter();

		public object Read(Type type, BinaryDataReader reader)
		{
			Customer c = new Customer()
			{
				Id = reader.Read<int>(),
				Title = reader.Read<string>(),
				RegisterDate = reader.Read<DateTime>()
			};
			int count = reader.Read<int>();
			for (int i = 0; i < count; i++)
			{
				c.AddressList.Add(
					new Address()
					{
						Zip = reader.Read<string>(),
						Region = reader.Read<string>(),
						City = reader.Read<string>(),
						Street = reader.Read<string>(),
						House = reader.Read<int>(),
						Flat = reader.Read<int>(),
						RegisterDate = reader.Read<DateTime>()
					}
					);
			}
			return c;
		}

		public void Write(BinaryDataWriter writer, object value)
		{
			Customer c = (Customer)value;
			writer.Write(c.Id);
			writer.Write(c.Title);
			writer.Write(c.RegisterDate);
			writer.Write(c.AddressList.Count);
			foreach (var a in c.AddressList)
			{
				writer.Write(a.Zip);
				writer.Write(a.Region);
				writer.Write(a.City);
				writer.Write(a.Street);
				writer.Write(a.House);
				writer.Write(a.Flat);
				writer.Write(a.RegisterDate);
			}
		}
	}
}
