using System;
using Eto.Forms;
using Eto.Drawing;

namespace Chatterino.Eto
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			// run application with our main form
			new Application().Run(new MainForm());
		}
	}
}
