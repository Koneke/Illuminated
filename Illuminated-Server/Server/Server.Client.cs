using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		public class Client
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
	}
}
