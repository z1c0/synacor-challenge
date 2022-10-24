var reg7 = Teleporter.Solve();

Console.WriteLine("Solving vault puzzle ...");
Vault.Solve();

var inputReader = new RecordedInputReader(
	RecordedInputReader.Answers1,
	new RecordedInputReader(RecordedInputReader.Answers2, new ConsoleInputReader()));
var vm = new VM(inputReader, new ConsoleOutputWriter());

inputReader.Depleted += (_, _) =>
{
	//vm.IsTracingEnabled = true;

	vm.Registers[7] = reg7;

	vm.Memory[5489] = 21; // nop
	vm.Memory[5490] = 21; // nop
	vm.Memory[5495] = 7; // jt
};

//vm.Disassemble(0, 7000);
vm.Run();

