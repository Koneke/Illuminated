using System;

namespace Illuminated.Net
{
	public class NetValue
	{
		public Type Type;
		public bool Defined;
		public object Value;

		public NetValue(Type type)
		{
			this.Type = type;
			this.Defined = false;
		}

		public NetValue(Type type, object value)
		{
			this.Type = type;
			this.Defined = true;
			this.Value = value;
		}

		public void SetValue<T>(object o)
		{
			this.Type = typeof(T);
			this.Defined = true;
			this.Value = o;
		}

		public override string ToString() =>
			this.Value.ToString();
	}
}
