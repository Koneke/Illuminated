using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public interface IIllClient
	{
		int ID { get; }
		NetConnection Connection { get; set; }
	}

	public class ClientCollection<Client> where Client : IIllClient
	{
		public Client this[NetConnection c] => this.ByConnection[c];
		public Client this[int i] => this.ByID[i];

		public List<Client> All;
		public Dictionary<NetConnection, Client> ByConnection;
		public Dictionary<int, Client> ByID;

		public ClientCollection()
		{
			this.All = new List<Client>();
			this.ByConnection = new Dictionary<NetConnection, Client>();
			this.ByID = new Dictionary<int, Client>();
		}

		public ClientCollection<Client> AddClient(
			NetConnection connection,
			Client client
		) {
			this.All.Add(client);
			this.ByConnection.Add(connection, client);
			this.ByID.Add(client.ID, client);

			return this;
		}

		public ClientCollection<Client> RemoveClient(Client client)
		{
			this.All.Remove(client);
			this.ByConnection.Remove(client.Connection);
			this.ByID.Remove(client.ID);

			return this;
		}
	}
}
