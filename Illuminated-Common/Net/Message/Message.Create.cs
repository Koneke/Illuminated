using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Message
	{
		private static Message createGeneric(MessageType type)
		{
			switch (type)
			{
				case MessageType.Vector:
					return new Message(MessageType.Vector)
						.AddField<float>("x")
						.AddField<float>("y");
			}

			return null;
		}

		private static Message createServerMessage(MessageType type)
		{
			switch (type)
			{
				case MessageType.Spawn:
					return new Message(MessageType.Spawn)
						.AddField<int>("player-id")
						.AddInherited(Create(MessageType.Vector));

				case MessageType.OtherSpawn:
					return new Net.Message(MessageType.OtherSpawn)
						.AddField<int>("player-id")
						.AddInherited(Create(MessageType.Vector));

				case MessageType.OtherDisconnected:
					return new Net.Message(MessageType.OtherDisconnected)
						.AddField<int>("player-id");

				case MessageType.PlayerPosition:
					return new Message(MessageType.PlayerPosition)
						.AddField<int>("player-id")
						.AddInherited(Create(MessageType.Vector));
			}

			return null;
		}

		private static Message createClientMessage(MessageType type)
		{
			// from client
			switch (type)
			{
				case MessageType.Run:
					return new Net.Message(MessageType.Run)
						.AddField<float>("dx")
						.AddField<float>("dy");
			}

			return null;
		}

		public static Message Create(MessageType type) =>
			createGeneric(type) ??
			createServerMessage(type) ??
			createClientMessage(type) ??
			throw new Exception($"Unknown message type {type}.");
	}
}
