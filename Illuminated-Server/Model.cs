
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Illuminated.Net
{
	public class Model
	{
		public ClientCollection Clients =
			new ClientCollection();

		public MessageQueue MessageQueue =
			new MessageQueue();

		private Server server;

		public Model(Server server)
		{
			this.server = server;
		}

		private void CatchUp(Client client)
		{
			foreach (var existing in this.Clients.All)
			{
				if (existing == client) continue;

				this.MessageQueue.SendSingle(
					client.Recipient,
					Message.Create(Message.MessageType.OtherSpawn)
						.SetField("player-id", existing.ID)
						.SetField("x", existing.Player.Position.X)
						.SetField("y", existing.Player.Position.Y));
			}
		}

		public Client Spawn(NetConnection connection)
		{
			var client = new Client(
				this.server,
				connection,
				new Player {
					Position = new Vector2(100.0f, 100.0f)
				} );

			this.Clients.AddClient(client.Connection, client);

			this.MessageQueue.SendSingle(
				new MessageRecipient(client.Connection),
				Message.Create(Message.MessageType.Spawn)
					.SetField("player-id", client.ID)
					.SetField("x", client.Player.Position.X)
					.SetField("y", client.Player.Position.Y));

			this.CatchUp(client);

			return client;
		}
	}
}
