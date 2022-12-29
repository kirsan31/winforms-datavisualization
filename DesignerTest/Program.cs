namespace DesignerTest;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
#pragma warning disable CA2000 // Dispose objects before losing scope
        Application.Run(new Form1());
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}