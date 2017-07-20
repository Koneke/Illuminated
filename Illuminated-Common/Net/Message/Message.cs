using System;
using System.Collections.Generic;
using System.Linq;

using Lidgren.Network;

namespace Illuminated.Net
{
	public class SenderInfo
	{
		public NetConnection Connection;
	}

	public partial class Message
	{
		public MessageType Type;
		public bool IsSecure;
		public Dictionary<string, NetValue> Fields;

		private List<string> keys;
		private List<Message> inheritedMessages;

		public SenderInfo SenderInfo;

		public Message(MessageType type)
		{
			this.Type = type;
			this.IsSecure = false;
			this.Fields = new Dictionary<string, NetValue>();
			this.keys = new List<string>();
			this.inheritedMessages = new List<Message>();
		}

		public Message AddField<T>(string fieldName)
		{
			this.Fields.Add(fieldName, new NetValue(typeof(T)));
			this.keys.Add(fieldName);

			return this;
		}

		public Message AddInherited(Message message)
		{
			this.inheritedMessages.Add(message);

			return this;
		}

		public Type GetFieldType(string fieldName)
		{
			return this.Fields[fieldName].Type;
		}

		public T GetField<T>(string fieldName)
		{
			if (!this.Fields.ContainsKey(fieldName))
			{
				foreach (var inherited in this.inheritedMessages)
				{
					T field;

					try
					{
						field = inherited.GetField<T>(fieldName);
					}
					catch (Exception)
					{
						continue;
					}

					return field;
				}

				throw new Exception($"Non-existant field {fieldName}.");
			}

			if (!this.Fields[fieldName].Defined)
			{
				throw new Exception($"Undefined field {fieldName}.");
			}

			return (T)this.Fields[fieldName].Value;
		}

		public object GetField(string fieldName)
		{
			return this.GetField<object>(fieldName);
		}

		public Message SetField<T>(string fieldName, T fieldValue)
		{
			if (!this.Fields.ContainsKey(fieldName))
			{
				foreach (var inherited in this.inheritedMessages)
				{
					try
					{
						inherited.SetField(fieldName, fieldValue);
						return this;
					}
					catch (Exception)
					{
					}
				}

				throw new Exception($"Non-existant field {fieldName}.");
			}

			if (typeof(T) != this.Fields[fieldName].Type)
			{
				throw new Exception("Invalid value-type.");
			}

			this.Fields[fieldName] = new NetValue(typeof(T), fieldValue);

			return this;
		}

		public Message Read(NetIncomingMessage message)
		{
			// why in the name of actual fuck do I need to reset the
			// position of an incoming message stream manually.....?
			//message.Position = 0;
			this.SenderInfo = new SenderInfo
			{
				Connection = message.SenderConnection
			};

			foreach (var key in this.keys)
			{
				var nv = this.Fields[key];

				var ts = new TypeSwitch()
					.Case((int x) => this.SetField(key, message.ReadInt32()))
					.Case((float x) => this.SetField(key, message.ReadSingle()))
					.Case((byte x) => this.SetField(key, message.ReadByte()))
					.Case((string x) => this.SetField(key, message.ReadString()));

				ts.Switch(nv.Type);
			}

			foreach (var inherited in this.inheritedMessages)
			{
				inherited.Read(message);
			}

			return this;
		}

		public Message Write(NetOutgoingMessage message, bool isInherited = false)
		{
			if (!isInherited)
			{
				message.Write(this.Type.ToString());
			}

			foreach (var key in this.keys)
			{
				var nv = this.Fields[key];

				var ts = new TypeSwitch()
					.Case((int x) => message.Write(x))
					.Case((float x) => message.Write(x))
					.Case((byte x) => message.Write(x))
					.Case((string x) => message.Write(x));

				ts.Switch(nv.Value);
			}

			foreach (var inherited in this.inheritedMessages)
			{
				inherited.Write(message, true);
			}

			return this;
		}

		public Message Secure(bool status = true)
		{
			this.IsSecure = status;

			return this;
		}

		private IEnumerable<(string k, NetValue v)> GetFields()
		{
			var fs = new List<(string k, NetValue v)>(this.Fields.Keys
				.Zip(this.Fields.Values, (k, v) => (k: k, v: v)));

			foreach (var im in this.inheritedMessages)
			{
				fs.AddRange(im.GetFields());
			}

			return fs;
		}

		public override string ToString()
		{
			return
				$"{this.Type} " + "{ " +
				string.Join(
					", ",
					this.GetFields()
						.Select((kv) => $"{kv.k}:{kv.v.ToString()}")) +
				" }";
		}
	}
}
