try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    // Tworzymy folder wwwroot fizycznie ZANIM wywołamy UseStaticFiles()
    var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    Directory.CreateDirectory(wwwrootPath);

    app.UseStaticFiles();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("KRYTYCZNY BŁĄD STARTU:");
    Console.WriteLine(ex.ToString());
    Console.ResetColor();
    Console.ReadLine();
}