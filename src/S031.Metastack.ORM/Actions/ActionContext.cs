namespace S031.MetaStack.ORM.Actions
{
	public class ActionContext
	{
		public string UserName { get; set; }
		public string ConnectionName { get; set; }
		public string SessionID { get; set; }
		public override string ToString() => $"{UserName}.{SessionID}.{ConnectionName}";
		public override int GetHashCode() => new { UserName, SessionID, ConnectionName }.GetHashCode();
		public override bool Equals(object obj) => obj == null ? false : this.GetHashCode() == obj.GetHashCode();
	}
}
