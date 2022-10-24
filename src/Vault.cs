internal class Path
{
	private readonly string[,] _maze;

	public Path(string[,] maze)
	{
		_maze = maze;
	}

	public override string ToString() =>
		string.Join(' ', Rooms.Select(r => _maze[r.Y, r.X]));

	internal List<(int X, int Y)> Rooms { get; } = new();

	internal int Evaluate()
	{
		var result = 0;
		var lastOperation = "+";
		foreach (var (X, Y) in Rooms)
		{
			var s = _maze[Y, X];
			switch (s)
			{
				case "*":
				case "+":
				case "-":
					lastOperation = s;
					break;

				default:
					var v = int.Parse(s);
					result = lastOperation switch
					{
						"*" => result * v,
						"+" => result + v,
						"-" => result - v,
						_ => throw new InvalidOperationException()
					};
					break;
					
			}
		}
		return result;
	}

	internal List<Path> GetAdjacents()
	{
		void Test(int x, int y, List<Path> adjacents)
		{
			if (x < 0 || y < 0 || x > 3 || y > 3)
			{
				return;
			}
			if (x == 0 && y == 3)
			{
				return;
			}
			var path = new Path(_maze);
			foreach (var r in Rooms)
			{
				path.Rooms.Add(r);
			}
			path.Rooms.Add((x, y));
			adjacents.Add(path);
		}

		var (x, y) = Rooms.Last();
		var adjacents = new List<Path>();
		Test(x - 1, y, adjacents);	
		Test(x + 1, y, adjacents);	
		Test(x, y - 1, adjacents);	
		Test(x, y + 1, adjacents);	
		return adjacents;
	}
}

internal static class Vault
{
	private static readonly string[,] _maze = new string[,]
	{
		{  "*", "8",  "-",  "1" },
		{  "4", "*", "11",  "*" },
		{  "+", "4",  "-", "18" },
		{ "22", "-",  "9",  "*" },
	};
	private static readonly PriorityQueue<Path, int> _paths = new(); 

	internal static void Solve()
	{
		var start = new Path(_maze);
		start.Rooms.Add((0, 3));
		_paths.Enqueue(start, start.Rooms.Count);

		while (_paths.Count > 0)
		{
			var p = _paths.Dequeue();
			if (p.Rooms.Last().X == 3 && p.Rooms.Last().Y == 0)
			{
				var result = p.Evaluate();
				if (result == 30)
				{
					Console.WriteLine($"{p} = 30");
					return;
				}
				
			}
			else
			{
				foreach (var a in p.GetAdjacents())
				{
					_paths.Enqueue(a, a.Rooms.Count);
				}
			}
		}
	}
}