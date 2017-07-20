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

				// security
				case MessageType.SafeHandshake:
					return new Message(MessageType.SafeHandshake)
						.AddField<string>("guid")
						.AddField<string>("salt");
			}

			return null;
		}

		private static Message createClientMessage(MessageType type)
		{
			// from client
			switch (type)
			{
				case MessageType.Login:
					return new Net.Message(MessageType.Login)
						.AddField<string>("username")
						.AddField<byte[]>("password");

				case MessageType.Run:
					return new Net.Message(MessageType.Run)
						.AddField<float>("dx")
						.AddField<float>("dy");

				// security
				case MessageType.RequestSafeConversation:
					return new Net.Message(MessageType.RequestSafeConversation);
			}

			return null;
		}

		private static Message createBoth(MessageType type)
		{
			// from both
			switch (type)
			{
				case MessageType.SafeHandshake:
					return new Net.Message(MessageType.SafeMessage)
						.AddField<string>("guid")
						.AddField<string>("data");
			}

			return null;
		}

		public static Message Create(MessageType type) =>
			createGeneric(type) ??
			createServerMessage(type) ??
			createClientMessage(type) ??
			createBoth(type) ??
			throw new Exception($"Unknown message type {type}.");
	}
}
