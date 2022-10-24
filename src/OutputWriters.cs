using System.Text;

interface IOutputWriter
{
	void Write(char c);
}

internal class ConsoleOutputWriter : IOutputWriter
{
	public void Write(char c)
	{
		Console.Write(c);
	}
}

internal class RecordingOutputWriter : IOutputWriter
{
	private readonly StringBuilder _sb = new();

	public void Write(char c)
	{
		_sb.Append(c);
	}
}

