using System.Text;

internal enum ReturnCode
{
	None,
	Halt,
	Hang,
}
internal class VM
{	
	internal VM(ushort registerValue)
	{
		_registerValue = registerValue;
		ParseInstructionsToMemory();
	}

	private readonly ushort[] _memory = new ushort[ushort.MaxValue];
	private readonly ushort[] _registers = new ushort[8];
	private readonly Stack<ushort> _stack = new();
	private int _pos = 0;

	// TODO: refactor into I/O
	private readonly StringBuilder _outputRecorder = new();
	private int _currentInputPos = 0;
	private string _answers = string.Join('\n', new[]
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
		"take teleporter"
	}) + '\n';
	private readonly ushort _registerValue;

	internal ReturnCode Run()
	{
		while (true)
		{
			var i = _memory[_pos];
			switch (i)
			{
				case 0: // halt
					return ReturnCode.Halt;

				case 1: // set
					{
						var a = GetRegister();
						var b = GetValue();
						_registers[a] = b;
						Next();
						break;
					}

				case 2: // push
					{
						var a = GetValue();
						_stack.Push(a);
						Next();
						break;
					}

				case 3: // pop
					{
						var a = Next();
						SetValue(a, _stack.Pop());
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
					_pos = GetValue();
					break;

				case 7: // jt
					{
						var a = GetValue();
						var b = GetValue();
						if (a != 0)
						{
							_pos = b;
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
							_pos = b;
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
						SetValue(a, _memory[b]);
						Next();
						break;
					}

				case 16: // wmem
					{
						var a = GetValue();
						var b = GetValue();
						_memory[a] = b;
						Next();
						break;
					}

				case 17: // call
					{
						var a = GetValue();
						_stack.Push((ushort)(_pos + 1));
						_pos = a;
						break;
					}

				case 18: // ret
					{
						_pos = _stack.Pop();
						break;
					}

				case 19: // out
					{
						var c = (char)GetValue();
						Console.Write(c);
						
						_outputRecorder.Append(c);
						if (_outputRecorder.ToString().EndsWith("A strange, electronic voice"))
						{
							return ReturnCode.Hang;
						}
						Next();
						break;
					}

				case 20: // in
					{
						var a = Next();
						SetValue(a, GetNextKey());
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
	}

	private void ParseInstructionsToMemory()
	{
		var bytes = File.ReadAllBytes("challenge.bin");
		var pos = 0;
		for (var i = 0; i < bytes.Length - 1; i += 2)
		{
			_memory[pos++] = BitConverter.ToUInt16(bytes, i);
		}
	}

	ushort GetNextKey()
	{
		if (_currentInputPos == _answers.Length)
		{
			_registers[7] = _registerValue;
			_currentInputPos = 0;
			_answers = "use teleporter\n";
		}
		return _answers[_currentInputPos++];
	}


	private ushort Next() => _memory[++_pos];

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
			v = _registers[v - 32768];
		}
		return v;
	}

	private void SetValue(ushort to, ushort v)
	{
		if (to > 32767)
		{
			_registers[to - 32768] = v;
		}
		else
		{
			_memory[to] = v;
		}
	}

	private static ushort Mod(int v) => (ushort)(v % 32768);
}