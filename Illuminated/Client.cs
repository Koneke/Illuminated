﻿using System;

using Microsoft.Xna.Framework;

using Lidgren.Network;

namespace Illuminated.Client
{
	public interface IConnected
	{
		void Disconnect();
	}

	public class IllClient : IConnected
	{
		private Model model;
		private NetClient client;

		private Common.Async.PeriodicalTask commitMovement;

		public IllClient(Model model)
		{
			this.model = model;

			client = new NetClient(new NetPeerConfiguration("Illuminated a-0.1"));
			client.Start();
			client.Connect(
				host: "127.0.0.1",
				port: 3333);
		}

		private void CommitLocalPlayerMovement()
		{
			var ds = this.model.Player.UncommittedMovement.Round();

			this.SendMessage(Net.Message.Create(Net.Message.Types.Run)
				.SetField("dx", ds.X)
				.SetField("dy", ds.Y));

			this.model.Player.CommitMovement(ds);
		}

		public void SendMessage(Net.Message message)
		{
			var outgoing = this.client.CreateMessage();

			message.Write(outgoing);

			this.client.SendMessage(
				outgoing,
				NetDeliveryMethod.ReliableOrdered);
		}

		public void SpawnLocalPlayer(Model.IllLocalPlayer player)
		{
			this.SpawnPlayer(player);

			this.model.Player = player;

			this.commitMovement = Common.Async.Periodical(
				this.CommitLocalPlayerMovement,
				TimeSpan.FromMilliseconds(13.0)).Run();
		}

		public void SpawnPlayer(Model.IllPlayer player)
		{
			this.model.AddPlayer(player);
		}

		public void DisconnectPlayer(Model.IllPlayer player)
		{
			this.model.RemovePlayer(player);
		}

		public void Receive()
		{
			NetIncomingMessage incoming;

			while ((incoming = client.ReadMessage()) != null)
			{
				switch (incoming.MessageType)
				{
					case NetIncomingMessageType.Data:
						var message = Net.Message
							.Create(incoming.ReadString())
							.Read(incoming);

						switch (message.Type)
						{
							case Net.Message.MessageType.Spawn:
								this.SpawnLocalPlayer(new Model.IllLocalPlayer(
										message.GetField<int>("player-id"))
									.SetPosition(new Vector2(
										message.GetField<float>("x"),
										message.GetField<float>("y")))
									as Model.IllLocalPlayer);
								break;

							case Net.Message.MessageType.OtherSpawn:
								this.SpawnPlayer(new Model.IllPlayer(
										message.GetField<int>("player-id"))
									.SetPosition(new Vector2(
										message.GetField<float>("x"),
										message.GetField<float>("y")))
									as Model.IllPlayer);
								break;

							case Net.Message.MessageType.OtherDisconnected:
								this.DisconnectPlayer(this.model.PlayerByID(
									message.GetField<int>("player-id")));
								break;

							case Net.Message.MessageType.PlayerPosition:
								var player = this.model.PlayerByID(
									message.GetField<int>("player-id"));

								player.SetPosition(new Vector2(
									message.GetField<float>("x"),
									message.GetField<float>("y")));
								break;
						}
						break;

					case NetIncomingMessageType.StatusChanged:
						switch (incoming.SenderConnection.Status)
						{
							default:
								break;
						}
						break;

					default:
						break;
				}
			}
		}

		public void Disconnect()
		{
			this?.commitMovement?.Deschedule();
			this.client.Disconnect("Leaving.");
		}
	}
}
