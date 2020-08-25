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
	public class UserInfo : ClaimsPrincipal
	{
		public UserInfo(IIdentity identity) : base(identity)
		{
		}

		/// <summary>
		/// Serialize content only <see cref="UserInfo"/> to json string
		/// !!! not completed
		/// </summary>
		/// <returns>json string</returns>
		public void ToStringRaw(JsonWriter writer)
		{
			writer.WritePropertyName("Identity");
			writer.WriteStartObject();
			WriteIdentityRaw(writer);
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

		private void WriteIdentityRaw(JsonWriter writer)
		{
			ClaimsIdentity idn = (this.Identity as ClaimsIdentity);
			if (idn!= null)
			{
				writer.WriteProperty("AuthenticationType", idn.AuthenticationType);
				writer.WriteProperty("Label", idn.Label);
				writer.WriteProperty("Name", idn.Name);
				writer.WriteProperty("RoleClaimType", idn.RoleClaimType);
			}
		}

		private void WriteClaimRaw(JsonWriter writer, Claim claim)
		{
			writer.WriteProperty("Type", claim.Type);
			writer.WriteProperty("Value", claim.Value);
			writer.WriteProperty("Issuer", claim.Issuer);
		}

		/// <summary>
		/// Serialize <see cref="ActionInfo"/> to json string
		/// </summary>
		/// <returns>json string</returns>
		public override string ToString()
		{
			JsonWriter w = new JsonWriter(Formatting.None);
			w.WriteStartObject();
			ToStringRaw(w);
			w.WriteEndObject();
			return w.ToString();
		}

		public static UserInfo Create(string serializedJsonString)
		{
			JsonObject j = (JsonObject)(new JsonReader(serializedJsonString).Read());
			ClaimsIdentity idn = new ClaimsIdentity();
			UserInfo u = new UserInfo(idn)
			{
			};
			return u;
		}
	}
}
