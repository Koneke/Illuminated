using System;
using System.Collections.Generic;
using System.Linq;

using Lidgren.Network;
using Microsoft.Xna.Framework;

using Illuminated.Common;

namespace Illuminated.Net
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("-- Starting Illuminated (Server) a-0.1 --");
			var server = new Server();
		}
	}

	public class Player
	{
		public Vector2 Position;
	}

	public class Model
	{
		public List<Server.Client> Clients;
		public Dictionary<NetConnection, Server.Client> ClientsByConnection;
		public Dictionary<int, Server.Client> ClientsByID;

		public Model()
		{
			this.Clients = new List<Server.Client>();
			this.ClientsByConnection = new Dictionary<NetConnection, Server.Client>();
			this.ClientsByID = new Dictionary<int, Server.Client>();
		}

		public Model AddClient(NetConnection connection, Server.Client client)
		{
			this.Clients.Add(client);
			this.ClientsByConnection.Add(connection, client);
			this.ClientsByID.Add(client.ID, client);

			return this;
		}

		public Model DisconnectClient(Server.Client client)
		{
			this.Clients.Remove(client);
			this.ClientsByConnection.Remove(client.Connection);
			this.ClientsByID.Remove(client.ID);

			return this;
		}
	}

	public class Server
	{
		public class Client // : IConnected
		{
			protected static int IDCounter = 0;
			public int ID = IDCounter++;

			public Server Server;
			public NetConnection Connection;
			public Player Player;

			public Client(Server server, NetConnection connection, Player player)
			{
				this.Server = server;
				this.Connection = connection;
				this.Player = player;
			}

			public void CatchUp()
			{
				foreach (var existing in this.Server.model.Clients)
				{
					if (existing == this) continue;

					this.Server.SendMessage(
						this,
						Message.Create(Message.MessageType.OtherSpawn)
							.SetField("player-id", existing.ID)
							.SetField("x", existing.Player.Position.X)
							.SetField("y", existing.Player.Position.Y));
				}
			}
		}

		private bool shutdown;
		private NetServer server;

		private Model model;

		public Server()
		{
			this.server = new NetServer(
				new NetPeerConfiguration("Illuminated a-0.1") {
					Port = 3333
				});

			this.Run();
		}

		public void SendMessage(Client client, Message message)
		{
			var outgoing = this.server.CreateMessage();
			message.Write(outgoing);

			this.server.SendMessage(
				outgoing,
				client.Connection,
				NetDeliveryMethod.ReliableOrdered);
		}

		public void SendEveryoneMessage(Client excludedClient, Message message)
		{
			foreach (var client in this.model.Clients)
			{
				var outgoing = this.server.CreateMessage();
				message.Write(outgoing);

				this.SendMessage(client, message);
			}
		}

		public void SendInvertedMessage(Client excludedClient, Message message)
		{
			foreach (var client in this.model.Clients)
			{
				if (client == excludedClient)
				{
					continue;
				}

				var outgoing = this.server.CreateMessage();
				message.Write(outgoing);

				this.SendMessage(client, message);
			}
		}

		private void Run()
		{
			this.model = new Model();

			this.server.Start();

			Async.Do(() => {
				Console.Read();
				Console.WriteLine("Shutting down.");
				shutdown = true;
			});

			while (!shutdown)
			{
				this.Receive();
			}
		}

		private Client Spawn(NetConnection connection)
		{
			var player = new Player {
				Position = new Vector2(100.0f, 100.0f)
			};

			var client = new Client(
				this,
				connection,
				player);

			this.model.AddClient(client.Connection, client);

			this.SendMessage(
				client,
				Message.Create(Message.MessageType.Spawn)
					.SetField("player-id", client.ID)
					.SetField("x", player.Position.X)
					.SetField("y", player.Position.Y));

			client.CatchUp();

			return client;
		}

		private Client ClientFromMessage(NetIncomingMessage message) =>
			this.model.ClientsByConnection[message.SenderConnection];

		private void Receive()
		{
			NetIncomingMessage incoming;
			Client client;

			while ((incoming = server.ReadMessage()) != null)
			{
				switch (incoming.MessageType)
				{
					case NetIncomingMessageType.Data:
						client = this.ClientFromMessage(incoming);

						var message = Message
							.Create(incoming.ReadString())
							.Read(incoming);

						switch (message.Type)
						{
							case Net.Message.MessageType.Run:
								client.Player.Position += new Vector2(
									message.GetField<float>("dx"),
									message.GetField<float>("dy"));

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.PlayerPosition)
										.SetField("player-id", client.ID)
										.SetField("x", client.Player.Position.X)
										.SetField("y", client.Player.Position.Y));

								break;
						}
						break;

					case NetIncomingMessageType.StatusChanged:
						switch (incoming.SenderConnection.Status)
						{
							case NetConnectionStatus.Connected:
								Console.WriteLine("New client.");

								client = this.Spawn(incoming.SenderConnection);

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.OtherSpawn)
										.SetField("player-id", client.ID)
										.SetField("x", client.Player.Position.X)
										.SetField("y", client.Player.Position.Y));

								break;

							case NetConnectionStatus.Disconnected:
								Console.WriteLine("Client left.");

								client = this.model.ClientsByConnection
									[incoming.SenderConnection];

								this.SendInvertedMessage(
									client,
									Message.Create(Message.MessageType.OtherDisconnected)
										.SetField("player-id", client.ID));

								this.model.DisconnectClient(client);

								break;
						}
						break;

					default:
						Console.WriteLine($"Unknown message type: {incoming.MessageType}.");
						break;
				}
			}
		}
	}
}
