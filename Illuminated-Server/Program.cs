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

}
