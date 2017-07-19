using System;
using System.Collections.Generic;

namespace Illuminated
{
	public static class Extensions
	{
		public static object GetDefault(this Type t)
		{
			return t.IsValueType
				? Activator.CreateInstance(t)
				: null;
		}

		public static object As(this object o, Type t)
		{
			return Convert.ChangeType(o, t);
		}
	}

	public class TypeSwitch
	{
		private Dictionary<Type, Action<object>> matches = new Dictionary<Type, Action<object>>();

		public TypeSwitch Case<T>(Action<T> action)
		{
			matches.Add(
				typeof(T),
				(x) => action((T)x));

			return this; 
		} 

		public void Switch(object x) { matches[x.GetType()](x); }

		public void Switch(Type t, object x) { matches[t](x); }
		
		public void Switch(Type t) { matches[t](t.GetDefault()); }

		public bool HasMatch(Type t) { return matches.ContainsKey(t); }
	}

	public class esw<T, U>
	{
		private Dictionary<T, U> cases = new Dictionary<T, U>();

		public esw<T, U> Case(T val, U action)
		{
			this.cases.Add(val, action);

			return this;
		}

		public U Switch(T val) => this.cases[val];
	}
}
