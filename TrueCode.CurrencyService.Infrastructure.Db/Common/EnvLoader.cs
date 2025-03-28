namespace TrueCode.CurrencyService.Infrastructure.Common;

public static class EnvLoader
{
    public static void Load(string path, string file = ".env.dev")
    {
        var envPath = Path.Combine(AppContext.BaseDirectory, file);
        envPath = Path.Combine(path, file);
        if (!File.Exists(envPath)) return;
        
        var lines = File.ReadAllLines(envPath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length != 2) continue;
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}