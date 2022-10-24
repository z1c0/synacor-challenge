internal static class Teleporter
{
	private static readonly Dictionary<(ushort, ushort), ushort> _cache = new();
	private static ushort _reg7;

	internal static ushort Solve()
	{
		_reg7 = 25700;
		while (true)
		{
			_reg7++;
			Console.WriteLine($"Trying {_reg7} ... ");
			_cache.Clear();
			
			var x = Ackermann(4, 1);
			if (x == 6)
			{
				Console.WriteLine("OK!");
				return _reg7;
			}
			Console.WriteLine("no");
		}
	}

	internal static ushort Ackermann(ushort reg0, ushort reg1)
	{
		var key = (reg0, reg1);
		if (_cache.ContainsKey(key))
		{
			return _cache[key];
		}

		var ret = (reg0, reg1) switch
		{
			(0, _) => Add(reg1, 1),
			(_, 0) => Ackermann(Dec(reg0), _reg7),
			_ => Ackermann(Dec(reg0), Ackermann(reg0, Dec(reg1)))
		};

		_cache[key] = ret;
		return ret;
	}

	private static ushort Dec(ushort v) =>
		(ushort)(v - 1);

	private static ushort Add(ushort v1, ushort v2) =>
		(ushort)((v1 + v2) % 32768);
}