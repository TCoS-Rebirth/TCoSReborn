using System;


public static class Debug
{
    public static void Log(string msg)
    {
        Console.WriteLine(msg);
    }

    public static void Log(string msg, ConsoleColor color)
    {
        ConsoleColor currentColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ForegroundColor = currentColor;
    }
}
