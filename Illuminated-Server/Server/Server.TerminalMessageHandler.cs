using System.Collections.Generic;

using Lidgren.Network;

namespace Illuminated.Net
{
	public partial class Server
	{
		private class TerminalMessageHandler : MessageHandler.ISubHandler
		{
			public string Key => "terminal";
			public MessageHandler.HandlerDelegate Handler => this.Handle;
			public MessageQueue MessageQueue { get; private set; } = new MessageQueue();

			private Server server;

			public TerminalMessageHandler(Server server)
			{
				this.server = server;
			}

			public void Handle(NetIncomingMessage incoming)
			{
				var command = incoming.ReadString();
				var argumentCount = incoming.ReadInt32();
				var arguments = new List<string>();

				for (var i = 0; i < argumentCount; i++)
				{
					arguments.Add(incoming.ReadString());
				}

				switch (command)
				{
					case "add-user":
						this.server.SqlDo(
							""
						);
						break;
				}
			}
		}
	}
}
