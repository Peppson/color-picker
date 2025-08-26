namespace ColorPicker.Services;

public static class DB
{
	public static void Print(string message)
	{
		#if !RELEASE
			Console.WriteLine(message);
		#endif
	}
}
