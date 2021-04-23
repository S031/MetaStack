using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace S031.MetaStack.Integral.Security
{
	public class UserInfo : ClaimsPrincipal, IJsonSerializible
	{
		public UserInfo(IIdentity identity) : base(identity)
		{
		}

		public int ID { get; internal set; }
		public int StructuralUnitID { get; set; }
		public string UserName
			=> Identity.Name;
		public string DomainName { get; set; }
		public string Name { get; set; }
		public int PersonID { get; set; }
		public int AccessLevelID { get; set; }
		public string PasswordHash { get; set; }
		public List<string> Roles { get; private set; } = new List<string>();
		public List<UserLogin> UserLogins { get; private set; } = new List<UserLogin>();
		public List<Permission> UserPermissions { get; private set; } = new List<Permission>();
		public string SessionToken { get; set; }

		/// <summary>
		/// Serialize <see cref="ActionInfo"/> to json string
		/// </summary>
		/// <returns>json string</returns>
		public override string ToString()
			=> ToString(Formatting.None);
		public string ToString(Formatting formatting)
		{
			JsonWriter writer = new JsonWriter(formatting);
			ToJson(writer);
			return writer.ToString();
		}
		public void ToJson(JsonWriter writer)
		{
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
		}
		public void ToJsonRaw(JsonWriter writer)
		{
			writer.WriteProperty(nameof(ID), ID)
				.WriteProperty(nameof(StructuralUnitID), StructuralUnitID)
				.WriteProperty(nameof(DomainName), DomainName)
				.WriteProperty(nameof(Name), Name)
				.WriteProperty(nameof(PersonID), PersonID)
				.WriteProperty(nameof(AccessLevelID), AccessLevelID)
				.WriteProperty(nameof(PasswordHash), PasswordHash)
				.WriteProperty(nameof(SessionToken), SessionToken);
			
			writer.WritePropertyName("Identity");
			writer.WriteStartObject();
			WriteIdentityRaw(writer, this.Identity as ClaimsIdentity);
			writer.WriteEndObject();

			writer.WritePropertyName(nameof(Roles));
			writer.WriteStartArray();
			foreach (var role in Roles)
				writer.WriteValue(role);
			writer.WriteEndArray();

			writer.WritePropertyName(nameof(UserLogins));
			writer.WriteStartArray();
			foreach (var login in UserLogins)
				login.ToJson(writer);
			writer.WriteEndArray();

			writer.WritePropertyName(nameof(UserPermissions));
			writer.WriteStartArray();
			foreach (var permission in UserPermissions)
				permission.ToJson(writer);
			writer.WriteEndArray();
		}
		private static void WriteIdentityRaw(JsonWriter writer, ClaimsIdentity idn)
		{
			if (idn != null)
			{
				writer.WriteProperty("Name", idn.Name)
					.WriteProperty("AuthenticationType", idn.AuthenticationType)
					.WriteProperty("Label", idn.Label)
					.WriteProperty("RoleClaimType", ClaimsTypes.GetKey(idn.RoleClaimType))
					.WriteProperty("NameClaimType", ClaimsTypes.GetKey(idn.NameClaimType))
					.WritePropertyName("Claims");
				writer.WriteStartArray();
				foreach (var claim in idn.Claims)
				{
					writer.WriteStartObject();
					WriteClaimRaw(writer, claim);
					writer.WriteEndObject();
				}
				writer.WriteEndArray();
			}
		}
		private static void WriteClaimRaw(JsonWriter writer, Claim claim)
		{
			writer.WriteProperty("Issuer", claim.Issuer)
				.WriteProperty("Type",  ClaimsTypes.GetKey(claim.Type))
				.WriteProperty("Value",  claim.Value)
				.WriteProperty("ValueType",  ClaimsTypes.GetTypeKey(claim.ValueType));
		}
		
		public static UserInfo Parse(string serializedJsonString)
			=>new UserInfo(new JsonReader(serializedJsonString).Read());
		public UserInfo(JsonValue source)
			: this(CreateIdentityFromJson((JsonObject)source))
		{
			FromJson(source);
		}
		private static ClaimsIdentity CreateIdentityFromJson(JsonObject j)
		{
			j.NullTest(nameof(j));
			if (j.TryGetValue("Identity", out JsonObject o))
			{
				string authenticationType = o.GetStringOrDefault("AuthenticationType");
				string claimTypesForName = authenticationType.Equals("windows", StringComparison.OrdinalIgnoreCase)
					? ClaimTypes.Name
					: ClaimTypes.Email;
				ClaimsIdentity idn = new ClaimsIdentity(authenticationType, claimTypesForName, ClaimTypes.Role);
				if (o.TryGetValue("Claims", out JsonArray a))
					foreach (JsonObject obj in a)
						idn.AddClaim(new Claim(
							ClaimsTypes.GetValue(obj.GetStringOrDefault("Type")),
							obj.GetStringOrDefault("Value"),
							ClaimsTypes.GetValue(obj.GetStringOrDefault("ValueType"))));

				return idn;
			}
			throw new KeyNotFoundException("'Identity' property not fount in source json");
		}
		public void FromJson(JsonValue source)
		{
			var j = source as JsonObject;
			ID = j.GetIntOrDefault(nameof(ID));
			StructuralUnitID = j.GetIntOrDefault(nameof(StructuralUnitID));
			PersonID = j.GetIntOrDefault(nameof(PersonID));
			AccessLevelID = j.GetIntOrDefault(nameof(AccessLevelID));
			DomainName = j.GetStringOrDefault(nameof(DomainName));
			Name = j.GetStringOrDefault(nameof(Name));
			PasswordHash = j.GetStringOrDefault(nameof(PasswordHash));
			SessionToken = j.GetStringOrDefault(nameof(SessionToken));

			if (j.TryGetValue("UserLogins", out JsonArray a))
				foreach (JsonObject obj in a)
					UserLogins.Add(new UserLogin(obj));

			if (j.TryGetValue("Roles", out a))
				foreach (string role in a)
					Roles.Add(role);
			
			if (j.TryGetValue("UserPermissions", out a))
				foreach (JsonObject obj in a)
					UserPermissions.Add(new Permission (obj));
		}
	}
}
