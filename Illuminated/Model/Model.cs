using System.Collections.Generic;

namespace Illuminated.Client
{
	public partial class Model
	{
		public IllLocalPlayer Player;
		public List<IllPlayer> Players;
		public Dictionary<int, IllPlayer> PlayersByID;

		public Model()
		{
			this.Player = null;
			this.Players = new List<IllPlayer>();
			this.PlayersByID = new Dictionary<int, IllPlayer>();
		}

		public IllPlayer PlayerByID(int id)
		{
			return this.PlayersByID[id];
		}

		public Model AddPlayer(IllPlayer player)
		{
			this.Players.Add(player);
			this.PlayersByID.Add(player.ID, player);

			return this;
		}

		public Model RemovePlayer(IllPlayer player)
		{
			this.Players.Remove(player);
			this.PlayersByID.Remove(Player.ID);

			return this;
		}
	}
}
