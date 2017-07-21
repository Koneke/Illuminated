using Lidgren.Network;

namespace Illuminated.Net
{
	public class Client : IIllClient
	{
		protected static int IDCounter = 0;
		private int id = IDCounter++;
		public int ID => this.id;

		public Server Server;
		public NetConnection Connection { get; set; }

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
