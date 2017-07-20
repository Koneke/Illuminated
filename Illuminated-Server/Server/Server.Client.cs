using Lidgren.Network;

namespace Illuminated.Net
{
	public class Client
	{
		protected static int IDCounter = 0;
		public int ID = IDCounter++;

		public Server Server;
		public NetConnection Connection;

		public MessageRecipient Recipient;

		public Player Player;

		public Client(Server server, NetConnection connection, Player player)
		{
			this.Server = server;
			this.Connection = connection;
			this.Player = player;

			this.Recipient = new MessageRecipient(this.Connection);
		}
	}
}
