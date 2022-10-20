Console.WriteLine("START");

var returnCode = ReturnCode.None;
ushort registerValue = 2;
//while (returnCode != ReturnCode.Halt)
{
	Console.WriteLine($"register value: {registerValue}");
	var vm = new VM(registerValue++);
	returnCode = vm.Run();
}

