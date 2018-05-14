using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using S031.MetaStack.Core.Data;


namespace S031.MetaStack.Core.ORM
{
	internal static class SysCatItems
	{
		static readonly List<JMXSchema> _sysCatItemsList = new List<JMXSchema>();
		public static List<JMXSchema> GetList() => _sysCatItemsList;

		static SysCatItems()
		{
			JMXSchema s = new JMXSchema("Order")
			{
				DbObjectName = new JMXObjectName("dbo", "Orders"),
				Name = "Orders",
				DirectAccess = DirectAccess.ForAll
			};
			s.Attributes.Add(new JMXAttribute("ID")
			{
				FieldName = "ID",
				DataType = MdbType.@int,
				Identity = new JMXIdentity(true),
				Required = true,
				Name = "ИД"
			});
			s.Attributes.Add(new JMXAttribute("MarketName")
			{
				FieldName = "MarketName",
				DataType = MdbType.@string,
				ServerDataType = "varchar",
				Width = 30,
				Required = true,
				Name = "Market Name"
			});
			s.Attributes.Add(new JMXAttribute("RequestTime")
			{
				FieldName = "RequestTime",
				DataType = MdbType.dateTime,
				ServerDataType = "datetime",
				DefaultConstraint = new JMXConstraint(JMXConstraintTypes.defaultConstraint, "GETDATE()"),
				Required = true,
				Name = "Время запроса"
			});
			s.Attributes.Add(new JMXAttribute("RequestResult")
			{
				FieldName = "RequestResult",
				DataType = MdbType.@string,
				ServerDataType = "varchar",
				Width = 256,
				Required = false,
				Name = "Результат запроса"
			});
			s.PrimaryKey = new JMXPrimaryKey("XPKOrders", "ID");
			//s.Indexes.Add(new JMXIndex("XIE1Orders", "RequestTime"));
			s.Indexes.Add(new JMXIndex("XIE2Orders", "MarketName"));
			_sysCatItemsList.Add(s);

			s = new JMXSchema("OrderDetail")
			{
				DbObjectName = new JMXObjectName("dbo", "OrderDetails"),
				Name = "Order detail",
				DirectAccess = DirectAccess.ForAll
			};
			s.Attributes.Add(new JMXAttribute("OrderID")
			{
				FieldName = "OrderID",
				DataType = MdbType.@int,
				Required = true,
				Name = "ИД запроса"
			});
			s.Attributes.Add(new JMXAttribute("IsBuy")
			{
				FieldName = "IsBuy",
				DataType = MdbType.@bool,
				Required = true,
				Name = "Тип buy или sell"
			});
			s.Attributes.Add(new JMXAttribute("Quantity")
			{
				FieldName = "Quantity",
				DataType = MdbType.@decimal,
				ServerDataType = "decimal",
				DataSize = new JMXDataSize(9, 8, 17),
				Required = true,
				Name = "Тип buy или sell"
			});
			s.Attributes.Add(new JMXAttribute("Rate")
			{
				FieldName = "Rate",
				DataType = MdbType.@decimal,
				ServerDataType = "decimal",
				DataSize = new JMXDataSize(9, 8, 17),
				Required = true,
				Name = "Тип buy или sell"
			});
			s.Indexes.Add(new JMXIndex("XIE1OrderDetails", "OrderID") { ClusteredOption = 1 });
			var fk = new JMXForeignKey("FK_OrderDetails_Orders")
			{
				RefDbObjectName = "dbo.Orders",
				RefObjectName = "dbo.Order"
			};
			fk.AddKeyMember("OrderID");
			fk.AddRefKeyMember("ID");
			s.ForeignKeys.Add(fk);
			_sysCatItemsList.Add(s);


			//s = new JMXSchema("SysDataTypesRow");
			//s.DbObjectName = new JMXObjectName("dbo", "SysDataTypesRows");
			//s.Name = "Типы данных";
			//s.DirectAccess = DirectAccess.None;
			//s.Attributes.Add(new JMXAttribute("ID")
			//{
			//	FieldName = "ID",
			//	DataType = Data.MdbType.@int,
			//	Name = "ИД",
			//	Identity = new JMXIdentity(true),
			//	Required = true,
			//	ReadOnly = true,
			//});
			//s.Attributes.Add(new JMXAttribute("c_bit")
			//{
			//	FieldName = "c_bit",
			//	DataType = Data.MdbType.@bool,
			//	IsNullable = false
			//});
			//s.Attributes.Add(new JMXAttribute("c_tinyint")
			//{
			//	FieldName = "c_tinyint",
			//	DataType = Data.MdbType.@byte,
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_smallint")
			//{
			//	FieldName = "c_smallint",
			//	DataType = Data.MdbType.@short,
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_int")
			//{
			//	FieldName = "c_int",
			//	DataType = Data.MdbType.@int,
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_bigint")
			//{
			//	FieldName = "c_bigint",
			//	DataType = Data.MdbType.@long,
			//	ServerDataType = "bigint",
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_real")
			//{
			//	FieldName = "c_real",
			//	DataType = Data.MdbType.@float,
			//	ServerDataType = "real",
			//	IsNullable = true,
			//	DataSize = new JMXDataSize(sizeof(float), 4, 10)
			//});
			//s.Attributes.Add(new JMXAttribute("c_float")
			//{
			//	FieldName = "c_float",
			//	DataType = Data.MdbType.@float,
			//	ServerDataType = "float",
			//	IsNullable = true,
			//	DataSize = new JMXDataSize(sizeof(float), 8, 15)
			//});
			//s.Attributes.Add(new JMXAttribute("c_decimal")
			//{
			//	FieldName = "c_decimal",
			//	DataType = Data.MdbType.@decimal,
			//	ServerDataType = "decimal",
			//	DataSize = new JMXDataSize(sizeof(decimal), 4, 20),
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_money")
			//{
			//	FieldName = "c_money",
			//	DataType = Data.MdbType.@decimal,
			//	ServerDataType = "money",
			//	IsNullable = true,
			//	DefaultConstraint = new JMXConstraint(JMXConstraintTypes.defaultConstraint, "10")
			//});
			//s.Attributes.Add(new JMXAttribute("c_datetime")
			//{
			//	FieldName = "c_datetime",
			//	DataType = Data.MdbType.dateTime,
			//	ServerDataType = "datetime",
			//	IsNullable = true,
			//	CheckConstraint = new JMXConstraint(JMXConstraintTypes.checkConstraint, "c_datetime >= getdate() - 3")
			//});
			//s.Attributes.Add(new JMXAttribute("c_time")
			//{
			//	FieldName = "c_time",
			//	DataType = Data.MdbType.dateTime,
			//	ServerDataType = "time",
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_varchar")
			//{
			//	FieldName = "c_varchar",
			//	DataType = Data.MdbType.@string,
			//	ServerDataType = "varchar",
			//	Width = 255,
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_binary")
			//{
			//	FieldName = "c_binary",
			//	DataType = Data.MdbType.byteArray,
			//	ServerDataType = "binary",
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_guid")
			//{
			//	FieldName = "c_guid",
			//	DataType =  Data.MdbType.guid,
			//	IsNullable = true
			//});
			//s.Attributes.Add(new JMXAttribute("c_object")
			//{
			//	DataType =  Data.MdbType.@object,
			//	IsNullable = true,
			//	IsArray = true,

			//});
			////s.PrimaryKey = new JMXPrimaryKey("PK_SysDataTypesRow", "ID");
			//_sysCatItemsList.Add(s);
			//var attObject = s.Attributes[s.Attributes.Count - 1];

			//s = new JMXSchema("SysDataTypesRowDetail");
			//s.DbObjectName = new JMXObjectName("dbo", "SysDataTypesRowDetails");
			//s.Name = "Дополнительные типы";
			//s.DirectAccess = DirectAccess.None;
			////s.Attributes.Add(new JMXAttribute("SysDataTypesRowID")
			////{
			////	FieldName = "SysDataTypesRowID",
			////	DataType = Data.MdbType.@int,
			////	Name = "ИД типов данных",
			////	Required = true,
			////	ReadOnly = true,
			////});
			//s.Attributes.Add(new JMXAttribute("ID")
			//{
			//	FieldName = "ID",
			//	DataType = Data.MdbType.@int,
			//	Name = "ИД",
			//	Identity = new JMXIdentity(true),
			//	Required = true,
			//	ReadOnly = true,
			//});
			//s.Attributes.Add(new JMXAttribute("PropertyName")
			//{
			//	FieldName = "PropertyName",
			//	DataType = Data.MdbType.@string,
			//	Name = "Наименование свойства",
			//	Width = 60,
			//	Required = true,
			//	ReadOnly = true,
			//});
			//s.Attributes.Add(new JMXAttribute("PropertyValue")
			//{
			//	FieldName = "PropertyValue",
			//	DataType = Data.MdbType.@string,
			//	Name = "Значение свойства",
			//	Width = 512,
			//	Required = true,
			//	ReadOnly = true,
			//});
			//s.PrimaryKey = new JMXPrimaryKey("PK_SysDataTypesRowDetails", "ID");
			////var fk = new JMXForeignKey("FK_SysDataTypesRowDetails_SysDataTypesRows");
			////fk.RefObject = new JMXObjectName("dbo.SysDataTypesRows");
			////fk.RefObjectName = "SysDataTypesRow";
			////fk.AddKeyMember("SysDataTypesRowID");
			////fk.AddRefKeyMember("ID");
			////s.ForeignKeys.Add(fk);

			//attObject.ObjectSchema = s;
		}


	}
}
