using System.Collections.Generic;
using System.Linq;

using Forms = System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Illuminated.Client
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;

		private Texture2D temp;
		private SpriteFont debugFont;

		private Model model;
		private IllClient client;

		private Input input;

		private string[] msgLog = new string[5];

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.IsMouseVisible = true;

			var form = (Forms.Form)Forms.Form.FromHandle(Window.Handle);
			form.Closing += this.Quit;
		}

		protected override void Initialize()
		{
			base.Initialize();

			this.model = new Model();
			this.client = new IllClient(this.model);

			this.input = new Input();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			temp = Content.Load<Texture2D>("Textures/32x-temp");
			debugFont = Content.Load<SpriteFont>("Fonts/Debug");
		}

		private void Quit(
			object sender = null,
			System.ComponentModel.CancelEventArgs e = null)
		{
			this.client.Dispose();
			this.Exit();
		}

		protected override void Update(GameTime gameTime)
		{
			this.input.Update();

			if (this.input.Released(Keys.Escape))
			{
				this.Quit();
			}

			var dt = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

			var mappings = new Dictionary<Keys, Vector2>() {
				{ Keys.A, new Vector2(-1,  0) },
				{ Keys.F, new Vector2(+1,  0) },
				{ Keys.D, new Vector2( 0, -1) },
				{ Keys.S, new Vector2( 0, +1) } };

			foreach (var key in mappings.Keys)
			{
				if (this.input.Down(key))
				{
					this.model.Player.MoveLocal(mappings[key] * dt);
				}
			}

			if (this.input.Pressed(Keys.L))
			{
				this.client.SendMessage(
					Net.Message.Create(Net.Message.MessageType.Login)
						.SetField("username", "foo")
						.Secure());
			}

			this.client.Receive();

			foreach (var player in this.model.Players)
			{
				player.Curves.Tick(
					gameTime.ElapsedGameTime.Milliseconds);
			}

			base.Update(gameTime);
		}

		private void DrawShadowedString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color
		) {
			var u = new Vector2(1, 1);
			var ux = new Vector2(1, 0);
			spriteBatch.DrawString(spriteFont, text, position + u + ux, Color.Black);
			spriteBatch.DrawString(spriteFont, text, position, color);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();

			foreach (var player in this.model.Players)
			{
				var position =
					(player.Position + player.Curves.Current)
					.Round();

				spriteBatch.Draw(
					this.temp,
					position,
					player == this.model.Player
						? Color.Red
						: Color.White);

				if (false)
					this.DrawShadowedString(
						this.debugFont,
						$"{position.X}, {position.Y}",
						player.Position.Round(),
						Color.White);
			}

			var first = System.Math.Max(0, this.client.History.Count - 5);
			var last = System.Math.Min(this.client.History.Count - first, 5);
			var history = this.client.History.GetRange(first, last);

			for (var i = 0; i < last - first; i++)
			{
				this.spriteBatch.DrawString(
					this.debugFont,
					history[i].ToString(),
					new Vector2(
						0,
						this.graphics.PreferredBackBufferHeight - 20 * (i + 1)),
					Color.White);
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
