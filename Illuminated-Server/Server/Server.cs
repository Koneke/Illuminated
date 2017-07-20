using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Data.SQLite;
using System.IO;

using Lidgren.Network;

using Illuminated.Common;

namespace Illuminated.Net
{
	public partial class Server
	{
		private SQLiteConnection DB;

		private bool shutdown;
		private NetServer netServer;

		private Messenger messenger;
		private MessageHandler messageHandler;

		private Model model;

		private void foo()
		{
		}

		public void SqlDo(string s)
		{
			var cmd = new SQLiteCommand(s, this.DB);
		}

		public Server()
		{
			this.netServer = new NetServer(
				new NetPeerConfiguration("Illuminated a-0.1") {
					Port = 3333
				});

			if (!Directory.Exists("Data"))
			{
				Directory.CreateDirectory("Data");
			}

			if (!File.Exists("Data/master.db"))
			{
				SQLiteConnection.CreateFile("Data/master.db");
				this.DB = new SQLiteConnection("Data Source=Data/master.db;Version=3");

				var cmd = new SQLiteCommand(
					String.Join("\n", (new List<string> {
						"create table (",
						"	username varchar(30),",
						"	hash binary("
					})),
					this.DB);
			}
			else
			{
				this.DB = new SQLiteConnection("Data Source=Data/master.db;Version=3");
			}

			this.model = new Model(this);

			var dmh = new DataMessageHandler(this.model);
			var cmh = new ConnectionMessageHandler(this.model);
			var tmh = new TerminalMessageHandler(this);
			var smh = new SecurityMessageHandler(this.model.Clients);

			this.messageHandler = new MessageHandler(
				this.model.Clients,
				dmh.HandleData,
				cmh.HandleConnection,
				tmh.HandleTerminal,
				smh.HandleSecurity);

			this.messenger = new Messenger(this.netServer)
				.Poll(this.model.MessageQueue)
				.Poll(dmh.MessageQueue)
				.Poll(cmh.MessageQueue)
				.Poll(tmh.MessageQueue)
				.Poll(smh.MessageQueue);

			this.Run();
		}

		private void Run()
		{
			this.netServer.Start();

			Async.Do(() => {
				Console.Read();
				Console.WriteLine("Shutting down.");
				shutdown = true;
			});

			while (!shutdown)
			{
				this.Receive();
			}

			this.DB.Close();
		}

		private void Receive()
		{
			NetIncomingMessage incoming;

			while ((incoming = netServer.ReadMessage()) != null)
			{
				this.messageHandler.Handle(incoming);
			}

			this.messenger.Flush();
		}
	}
}
