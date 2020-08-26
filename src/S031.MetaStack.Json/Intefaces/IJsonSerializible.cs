namespace S031.MetaStack.Json
{
	public interface IJsonSerializible
	{
		void FromJson(JsonValue source);
		void ToJson(JsonWriter writer);
		string ToString(Formatting formatting);
	}
}