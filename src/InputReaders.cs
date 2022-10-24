interface IInputReader
{
	ushort GetNextKey();
}

internal class ConsoleInputReader : IInputReader
{
	private int _pos = 0;
	private string? _input = null;

	public ushort GetNextKey()
	{
		if (_pos == _input?.Length)
		{
			_input = null;
		}
		if (_input == null)
		{
			_input = Console.ReadLine() + '\n';
			_pos = 0;
		}
		return _input[_pos++];
	}
}

internal class RecordedInputReader : IInputReader
{
	public RecordedInputReader(string answers, IInputReader fallbackReader)
	{
		_answers = answers;
		_fallbackReader = fallbackReader;		
	}

	internal EventHandler? Depleted;
	private int _pos;
	private readonly string _answers;
	internal static string Answers1 = string.Join('\n', new[]
	{
		"doorway",
		"north",
		"north",
		"bridge",
		"continue",
		"down",
		"east",
		"take empty lantern",
		"west",
		"west",
		"passage",
		"ladder",
		"west",
		"south",
		"north",
		"take can",
		"use can",
		"use lantern",
		"west",
		"ladder",
		"darkness",
		"continue",
		"west",
		"west",
		"west",
		"west",
		"north",
		"take red coin",
		"north",
		"west",
		"take blue coin",
		"up",
		"take shiny coin",
		"down",
		"east",
		"east",
		"take concave coin",
		"down",
		"take corroded coin",
		"up",
		"west",
		"use blue coin",
		"use red coin",
		"use shiny coin",
		"use concave coin",
		"use corroded coin",
		"north",
		"take teleporter",
		"use teleporter",
	}) + '\n';
	internal static string Answers2 = string.Join('\n', new[]
	{
		"north",
		"north",
		"north",
		"north",
		"north",
		"north",
		"north",
		"north",
		"north",
		"take orb",
		"north",
		"east",
		"east",
		"north",
		"west",
		"south",
		"east",
		"east",
		"west",
		"north",
		"north",
		"east",
		"vault",
		"take mirror",
		"use mirror",
}) + '\n';
	private readonly IInputReader _fallbackReader;

	public ushort GetNextKey()
	{
		if (_pos == _answers.Length - 1)
		{
			Depleted?.Invoke(this, EventArgs.Empty);
		}
		if (_pos == _answers.Length)
		{
			return _fallbackReader.GetNextKey();
		}
		return _answers[_pos++];
	}
}