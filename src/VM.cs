internal class VM
{	
	internal VM(IInputReader inputReader, IOutputWriter outputWriter)
	{		
		_inputReader = inputReader;
		_outputWriter = outputWriter;
		ParseInstructionsToMemory();
	}

	internal int PC { get; set; } = 0;
	internal ushort[] Memory { get; } = new ushort[ushort.MaxValue];
	internal ushort[] Registers { get; } = new ushort[8];
	internal Stack<ushort> Stack { get; } = new();

	private readonly IInputReader _inputReader;
	private readonly IOutputWriter _outputWriter;

	internal bool IsEnabled { get; set; } = true;
	internal bool IsTracingEnabled { get; set; }

	internal void Disassemble(int startAddress, int count = 1, bool printState = false)
	{
		for (var i = 0; i < count; i++)
		{
			var offset = 1;
			string DecodeValue(int pcOffset)
			{
				var v = Memory[startAddress + pcOffset];
				offset++;
				return v > 32767 ? $"[{v - 32768}]" : v.ToString();
			}
			var decoded = Memory[startAddress] switch
			{
				00 => $"halt \t\t",
				01 => $"set  {DecodeValue(1)} {DecodeValue(2)}\t",
				02 => $"push {DecodeValue(1)}\t\t",
				03 => $"pop  {DecodeValue(1)}\t\t",
				04 => $"eq   {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				05 => $"gt   {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				06 => $"jmp  {DecodeValue(1)}\t\t",
				07 => $"jt   {DecodeValue(1)} {DecodeValue(2)}\t",
				08 => $"jf   {DecodeValue(1)} {DecodeValue(2)}\t",
				09 => $"add  {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				10 => $"mult {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				11 => $"mod  {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				12 => $"and  {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				13 => $"or   {DecodeValue(1)} {DecodeValue(2)} {DecodeValue(3)}",
				14 => $"not  {DecodeValue(1)} {DecodeValue(2)}\t",
				15 => $"rmem {DecodeValue(1)} {DecodeValue(2)}\t",
				16 => $"wmem {DecodeValue(1)} {DecodeValue(2)}\t",
				17 => $"call {DecodeValue(1)}\t\t",
				18 => $"ret  \t\t",
				19 => $"out  {DecodeValue(1)}\t\t",
				20 => $"in   {DecodeValue(1)}\t\t",
				21 => $"nop  \t\t",
				> 21 => "??",
			};
			var line = $"{startAddress}: {decoded}\t";
			if (printState)
			{
				line += $"[{Registers[0]}], [{Registers[1]}], .., [{Registers[7]}], stack: {Stack.Count} ({Stack.Peek()})";
			}
			Console.WriteLine(line);

			startAddress += offset;
		}
	}
	
	internal void Trace()
	{
		if (IsTracingEnabled)
		{
			Disassemble(PC, printState: true);
		}
	}
	internal bool Run()
	{
		while (IsEnabled)
		{
			Trace();

			var i = Memory[PC];
			switch (i)
			{
				case 0: // halt
					return true;

				case 1: // set
					{
						var a = GetRegister();
						var b = GetValue();
						Registers[a] = b;
						Next();
						break;
					}

				case 2: // push
					{
						var a = GetValue();
						Stack.Push(a);
						Next();
						break;
					}

				case 3: // pop
					{
						var a = Next();
						SetValue(a, Stack.Pop());
						Next();
						break;
					}

				case 4: // eq
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, Mod(b == c ? 1 : 0));
						Next();
						break;
					}

				case 5: // gt
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, Mod(b > c ? 1 : 0));
						Next();
						break;
					}

				case 6: // jmp
					PC = GetValue();
					break;

				case 7: // jt
					{
						var a = GetValue();
						var b = GetValue();
						if (a != 0)
						{
							PC = b;
						}
						else
						{
							Next();
						}
						break;
					}

				case 8: // jf
					{
						var a = GetValue();
						var b = GetValue();
						if (a == 0)
						{
							PC = b;
						}
						else
						{
							Next();
						}
						break;
					}

				case 9: // add
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, Mod(b + c));
						Next();
						break;
					}

				case 10: // mult
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, Mod(b * c));
						Next();
						break;
					}

				case 11: // mod
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, Mod(b % c));
						Next();
						break;
					}

				case 12: // and
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, (ushort)(b & c));
						Next();
						break;
					}

				case 13: // or
					{
						var a = Next();
						var b = GetValue();
						var c = GetValue();
						SetValue(a, (ushort)(b | c));
						Next();
						break;
					}

				case 14: // not
					{
						var a = Next();
						var b = GetValue();
						b = (ushort)~b;
						b <<= 1;
						b >>= 1;
						SetValue(a, b);
						Next();
						break;
					}

				case 15: // rmem
					{
						var a = Next();
						var b = GetValue();
						SetValue(a, Memory[b]);
						Next();
						break;
					}

				case 16: // wmem
					{
						var a = GetValue();
						var b = GetValue();
						Memory[a] = b;
						Next();
						break;
					}

				case 17: // call
					{
						var a = GetValue();
						Stack.Push((ushort)(PC + 1));
						PC = a;
						break;
					}

				case 18: // ret
					{
						PC = Stack.Pop();
						break;
					}

				case 19: // out
					{
						var c = (char)GetValue();
						_outputWriter.Write(c);
						Next();
						break;
					}

				case 20: // in
					{
						var a = Next();
						SetValue(a, _inputReader.GetNextKey());
						Next();
						break;
					}

				case 21: // noop
					Next();
					break;

				default:
					throw new NotImplementedException($"opcode {i} is not implemented yet.");
			}
		}
		return false;
	}

	private void ParseInstructionsToMemory()
	{
		var bytes = File.ReadAllBytes("challenge.bin");
		var pos = 0;
		for (var i = 0; i < bytes.Length - 1; i += 2)
		{
			Memory[pos++] = BitConverter.ToUInt16(bytes, i);
		}
	}
	private ushort Next() => Memory[++PC];

	private ushort GetRegister()
	{
		var v = Next();
		return (ushort)(v - 32768);
	}

	private ushort GetValue()
	{
		var v = Next();
		if (v > 32767)
		{
			v = Registers[v - 32768];
		}
		return v;
	}

	private void SetValue(ushort to, ushort v)
	{
		if (to > 32767)
		{
			Registers[to - 32768] = v;
		}
		else
		{
			Memory[to] = v;
		}
	}

	private static ushort Mod(int v) => (ushort)(v % 32768);
}