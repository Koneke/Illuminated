using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public class ClientCollection
	{
		public List<Client> All;
		public Dictionary<NetConnection, Client> ByConnection;
		public Dictionary<int, Client> ByID;

		public ClientCollection()
		{
			this.All = new List<Client>();
			this.ByConnection = new Dictionary<NetConnection, Client>();
			this.ByID = new Dictionary<int, Client>();
		}

		public ClientCollection AddClient(NetConnection connection, Client client)
		{
			this.All.Add(client);
			this.ByConnection.Add(connection, client);
			this.ByID.Add(client.ID, client);

			return this;
		}

		public ClientCollection RemoveClient(Client client)
		{
			this.All.Remove(client);
			this.ByConnection.Remove(client.Connection);
			this.ByID.Remove(client.ID);

			return this;
		}
	}
}
