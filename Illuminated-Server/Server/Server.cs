using System;

using Lidgren.Network;

using Illuminated.Common;

namespace Illuminated.Net
{
	public partial class Server
	{
		private bool shutdown;
		private NetServer server;

		private Messenger messenger;
		private MessageHandler messageHandler;
		private DataMessageHandler dataMessageHandler;
		private ConnectionMessageHandler connectionMessageHandler;

		private Model model;

		public Server()
		{
			this.server = new NetServer(
				new NetPeerConfiguration("Illuminated a-0.1") {
					Port = 3333
				});

			this.model = new Model(this);

			var dmh = new DataMessageHandler(this.model);
			var cmh = new ConnectionMessageHandler(this.model);

			this.messageHandler = new MessageHandler(
				this.model.Clients,
				dmh.HandleData,
				cmh.HandleConnection);

			this.messenger = new Messenger(this.server)
				.Poll(this.model.MessageQueue)
				.Poll(dmh.MessageQueue)
				.Poll(cmh.MessageQueue);

			this.Run();
		}

		private void Run()
		{
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

		private void Receive()
		{
			NetIncomingMessage incoming;

			while ((incoming = server.ReadMessage()) != null)
			{
				this.messageHandler.Handle(incoming);
			}

			this.messenger.Flush();
		}
	}
}
