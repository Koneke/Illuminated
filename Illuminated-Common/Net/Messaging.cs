using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Message
	{
		public static Message Create(string type)
		{
			// generic
			switch (type)
			{
				case Types.Vector:
					return new Message(Types.Vector)
						.AddField<float>("x")
						.AddField<float>("y");
			}

			// from server
			switch (type)
			{
				case Types.Spawn:
					return new Message(Types.Spawn)
						.AddField<int>("player-id")
						.AddInherited(Create(Types.Vector));

				case Types.OtherSpawn:
					return new Net.Message(Types.OtherSpawn)
						.AddField<int>("player-id")
						.AddInherited(Create(Types.Vector));

				case Types.OtherDisconnected:
					return new Net.Message(Types.OtherDisconnected)
						.AddField<int>("player-id");

				case Types.PlayerPosition:
					return new Message(Types.PlayerPosition)
						.AddField<int>("player-id")
						.AddInherited(Create(Types.Vector));
			}

			// from client
			switch (type)
			{
				case Types.Run:
					return new Net.Message(Types.Run)
						.AddField<float>("dx")
						.AddField<float>("dy");
			}

			throw new Exception($"Unknown message type {type}.");
		}

		public string Type;
		public Dictionary<string, NetValue> Fields;

		private List<string> keys;
		private List<Message> inheritedMessages;

		public Message(string type)
		{
			this.Type = type;
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
					catch (Exception e)
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

			foreach (var key in this.keys)
			{
				var nv = this.Fields[key];

				var ts = new TypeSwitch()
					.Case((int x) => this.SetField(key, message.ReadInt32()))
					.Case((float x) => this.SetField(key, message.ReadSingle()));

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
				message.Write(this.Type);
			}

			foreach (var key in this.keys)
			{
				var nv = this.Fields[key];

				var ts = new TypeSwitch()
					.Case((int x) => message.Write(x))
					.Case((float x) => message.Write(x));

				ts.Switch(nv.Value);
			}

			foreach (var inherited in this.inheritedMessages)
			{
				inherited.Write(message, true);
			}

			return this;
		}
	}
}
