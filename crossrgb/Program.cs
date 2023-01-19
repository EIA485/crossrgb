using PrimS.Telnet;
using System.ComponentModel;

Console.CursorVisible = false;

var options = new counter[2] { new("delay", 0, 20), new("multiplier", 1) };
int selected = 0;
options[0].isSelected = true;
BackgroundWorker bw = new BackgroundWorker();
bw.DoWork += (a, b) =>
{
	switch (Console.ReadKey(true).Key)
	{
		case ConsoleKey.RightArrow:
			options[selected].right();
			break;
		case ConsoleKey.LeftArrow:
			options[selected].left();
			break;
		case ConsoleKey.UpArrow:
			if (selected > 0)
			{
				options[selected].isSelected = false;
				selected--;
				options[selected].isSelected = true;
			}
			break;
		case ConsoleKey.DownArrow:
			if (selected < options.Length - 1)
			{
				options[selected].isSelected = false;
				selected++;
				options[selected].isSelected = true;
			}
			break;
	}
};
bw.RunWorkerCompleted += (a, b) => { if (!bw.IsBusy) bw.RunWorkerAsync(); };
bw.RunWorkerAsync();

bool connected = false;
int index = 0;
UpdateConnectedState(false);
restart: //( ͡° ͜ʖ ͡°)
try
{
	using (Client tel = new("127.0.0.1", 2121, new()))
	{
		if (connected == false) UpdateConnectedState(true);

		while (true)
		{
			Thread.Sleep(options[0].val);
			index = (index + options[1].val) % 1530;
			string cmd = GetComand(index);
			await tel.WriteLineAsync(cmd);
			Console.SetCursorPosition(0, options.Length + 1);
			Console.Write(cmd + "  ");
		}
	}
}
catch
{
	if (connected) UpdateConnectedState(false);
	goto restart;
}

void UpdateConnectedState(bool state)
{
	connected = state;
	Console.SetCursorPosition(0, options.Length);
	if (state)
	{
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("Connected   ");
		Console.ForegroundColor = ConsoleColor.White;
	}
	else
	{
		Console.BackgroundColor = ConsoleColor.DarkRed;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("Disconnected");
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
	}
	Console.Write(DateTime.Now);
}

string GetComand(int index) => $"cl_crosshaircolor_r {Math.Clamp((index <= 255) ? index : (1020 - index), 0, 255)}; cl_crosshaircolor_g {Math.Clamp((index <= 1020) ? index - 510 : 1530 - index, 0, 255)}; cl_crosshaircolor_b {Math.Clamp((index <= 1020) ? 510 - index : index - 1020, 0, 255)};";

class counter
{
	public counter(string name, int index, int? def = null, int min = 1)
	{
		this.index = index;
		this.min = min;
		val = def ?? min;
		valPos = name.Length + 2;
		Console.SetCursorPosition(0, index);
		Console.Write(name + ":");
		draw();
	}
	int valPos;
	int index;
	int min;
	int lastLen = 0;
	public bool isSelected
	{
		set
		{
			_selected = value;
			draw();
		}
	}
	bool _selected = false;
	public void draw()
	{
		Console.SetCursorPosition(valPos, index);
		var valstr = val.ToString();
		var len = valstr.Length;
		if (_selected)
		{
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write(valstr);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
		}
		else Console.Write(valstr);

		//clear console
		if (len < lastLen)
		{
			Console.SetCursorPosition(valPos + len, index);
			Console.Write(new string(' ', lastLen - len));
		}
		lastLen = len;
	}
	public int val { private set; get; }
	public void right() { val++; draw(); }
	public void left() { if (val > min) { val--; draw(); } }
}
