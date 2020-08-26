using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace S031.MetaStack.Integral.Security.Users
{
	/// <summary>
	/// !!! not completed (add json serialization)
	/// </summary>
	public class UserInfo : ClaimsPrincipal, IJsonSerializible
	{
		public UserInfo(IIdentity identity) : base(identity)
		{
		}

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
			writer.WritePropertyName("Identity");
			writer.WriteStartObject();
			WriteIdentityRaw(writer, this.Identity as ClaimsIdentity);
			writer.WriteEndObject();

			writer.WritePropertyName("Claims");
			writer.WriteStartArray();
			foreach (var claim in Claims)
			{
				writer.WriteStartObject();
				WriteClaimRaw(writer, claim);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}
		private static void WriteIdentityRaw(JsonWriter writer, ClaimsIdentity idn)
		{
			if (idn != null)
			{
				writer.WriteProperty("Name", idn.Name);
				writer.WriteProperty("AuthenticationType", idn.AuthenticationType);
				writer.WriteProperty("Label", idn.Label);
				writer.WriteProperty("RoleClaimType", idn.RoleClaimType);
				writer.WriteProperty("NameClaimType", idn.NameClaimType);
			}
		}
		private static void WriteClaimRaw(JsonWriter writer, Claim claim)
		{
			writer.WriteProperty("Issuer", claim.Issuer);
			writer.WriteProperty("Type", claim.Type);
			writer.WriteProperty("Value", claim.Value);
			writer.WriteProperty("ValueType", claim.ValueType);
		}
		
		/// <summary>
		/// Deserialize content only <see cref="UserInfo"/> from json string
		/// !!! not completed
		/// </summary>
		/// <returns>json string</returns>
		public static UserInfo ReadFrom(string serializedJsonString)
		{
			JsonObject j = (JsonObject)(new JsonReader(serializedJsonString).Read());
			ClaimsIdentity idn = new ClaimsIdentity();
			return new UserInfo(idn, j);
		}
		protected internal UserInfo(IIdentity identity, JsonValue source)
			: this(identity)
		{
			if (source != null)
				FromJson(source);
		}
		public void FromJson(JsonValue source)
		{
			throw new NotImplementedException();
		}


	}
}
