using System;
using System.Threading;
using System.Threading.Tasks;

namespace Illuminated.Common
{
	public static class Async
	{
		public static void Do(Action action)
		{
			(new Task(action)).Start();
		}

		public class PeriodicalTask
		{
			private Action action;
			private bool die;
			private TimeSpan interval;

			public PeriodicalTask(Action action, TimeSpan interval)
			{
				this.action = action;
				this.interval = interval;
			}

			public PeriodicalTask Run()
			{
				this.die = false;
				Do(this.start);

				return this;
			}

			public void Deschedule()
			{
				this.die = true;
			}

			private void start()
			{
				while (!this.die)
				{
					var pre = DateTime.Now;
					this.action();
					var post = DateTime.Now;

					var sleep = Math.Max(
						0,
						(interval - (post - pre)).Milliseconds);

					Thread.Sleep(sleep);
				}
			}
		}

		public static PeriodicalTask Periodical(Action action, TimeSpan interval)
		{
			return new PeriodicalTask(
				action,
				interval);
		}
	}
}
