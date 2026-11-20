using System.Globalization;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Rendering;

var dir = Path.Combine(Path.GetTempPath(), "zerohour");
var file = Path.Combine(dir, "event.json");

string? code = null, name = null;
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--set": code = i + 1 < args.Length ? args[++i] : ""; break;
        case "--name": name = i + 1 < args.Length ? args[++i] : ""; break;
        case "-h": case "--help":
            AnsiConsole.MarkupLine("[bold cyan]zerohour[/] [grey]--set YYMMDDHHmm --name \"Event name\"[/]  save an event");
            AnsiConsole.MarkupLine("[bold cyan]zerohour[/]                                     run the countdown");
            return;
    }
}

Event? ev = File.Exists(file) ? JsonSerializer.Deserialize<Event>(File.ReadAllText(file)) : null;

if (code != null || name != null)
{
    DateTime target = ev?.Target ?? default;
    if (code != null)
    {
        if (code.Length != 10 || !code.All(char.IsDigit) ||
            !DateTime.TryParseExact("20" + code, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out target))
        {
            AnsiConsole.MarkupLine("[red]Bad code.[/] Use YYMMDDHHmm, e.g. 2612171300 = Dec 17 2026 1:00 PM");
            return;
        }
    }
    if (target == default) { AnsiConsole.MarkupLine("[red]No date saved yet. Use --set first.[/]"); return; }
    ev = new Event(target, string.IsNullOrWhiteSpace(name) ? ev?.Name ?? "Zero Hour" : name,
                   code != null ? DateTime.Now : ev?.Start ?? DateTime.Now);
    Directory.CreateDirectory(dir);
    File.WriteAllText(file, JsonSerializer.Serialize(ev));
    AnsiConsole.MarkupLine($"[green]Saved:[/] {Markup.Escape(ev.Name)} -> {ev.Target:f}");
    return;
}

if (ev == null)
{
    AnsiConsole.MarkupLine("[yellow]Nothing saved.[/] Try: zerohour --set 2612171300 --name \"My Event\"");
    return;
}

var rng = new Random();
var clock = System.Diagnostics.Stopwatch.StartNew();
AnsiConsole.Clear();

AnsiConsole.Live(new Markup("")).Start(ctx =>
{
    while (true)
    {
        double t = clock.Elapsed.TotalSeconds;
        var left = ev.Target - DateTime.Now;
        if (left <= TimeSpan.Zero) break;
        ctx.UpdateTarget(Countdown(ev, left, t));
        Thread.Sleep(80);
    }
    for (int f = 0; f < 400; f++)
    {
        double t = clock.Elapsed.TotalSeconds;
        ctx.UpdateTarget(Celebrate(ev, t));
        Thread.Sleep(80);
    }
});

IRenderable Countdown(Event e, TimeSpan left, double t)
{
    var parts = new (string Label, int Val)[]
        { ("DAYS", (int)left.TotalDays), ("HOURS", left.Hours), ("MINUTES", left.Minutes), ("SECONDS", left.Seconds) };

    var grid = new Grid();
    foreach (var _ in parts) grid.AddColumn(new GridColumn().Centered());
    grid.AddRow(parts.Select((p, i) => (IRenderable)new FigletText($"{p.Val:00}").Centered().Color(Hue(t * 60 + i * 70))).ToArray());
    grid.AddRow(parts.Select((p, i) => (IRenderable)new Markup($"[bold {Hue(t * 60 + i * 70).ToMarkup()}]{p.Label}[/]")).ToArray());

    double total = (e.Target - e.Start).TotalSeconds;
    double frac = total <= 0 ? 1 : Math.Clamp(1 - left.TotalSeconds / total, 0, 1);
    int width = Math.Max(10, Math.Min(AnsiConsole.Profile.Width - 10, 80));
    int filled = (int)(frac * width);
    var bar = string.Concat(Enumerable.Range(0, width).Select(i =>
        i < filled ? $"[{Hue(t * 90 + i * 6).ToMarkup()}]█[/]" : "[grey23]░[/]"));

    return new Panel(new Rows(
            new Align(new Markup(Rainbow(e.Name, t)), HorizontalAlignment.Center),
            new Text(""),
            grid,
            new Text(""),
            new Align(new Markup($"{bar} [bold]{frac:P1}[/]"), HorizontalAlignment.Center),
            new Align(new Markup($"[grey]{e.Target:dddd, MMMM d yyyy 'at' h:mm tt}[/]"), HorizontalAlignment.Center)))
        .Border(BoxBorder.Double).BorderColor(Hue(t * 80)).Expand();
}

IRenderable Celebrate(Event e, double t)
{
    int w = Math.Max(10, Math.Min(AnsiConsole.Profile.Width - 6, 100));
    string chars = "*+.o~^#@";
    var lines = Enumerable.Range(0, 8).Select(_ => (IRenderable)new Markup(string.Concat(Enumerable.Range(0, w).Select(_ =>
        rng.Next(4) == 0 ? $"[{Hue(rng.Next(360)).ToMarkup()}]{chars[rng.Next(chars.Length)]}[/]" : " "))));
    return new Panel(new Rows(
            new Rows(lines),
            new Align(new FigletText("ZERO HOUR").Centered().Color(Hue(t * 200)), HorizontalAlignment.Center),
            new Align(new Markup(Rainbow(e.Name, t * 3)), HorizontalAlignment.Center),
            new Rows(lines.Reverse())))
        .Border(BoxBorder.Heavy).BorderColor(Hue(t * 300)).Expand();
}

string Rainbow(string s, double t) =>
    string.Concat(s.Select((c, i) => $"[bold {Hue(t * 120 + i * 14).ToMarkup()}]{Markup.Escape(c.ToString())}[/]"));

static Color Hue(double h)
{
    h %= 360; if (h < 0) h += 360;
    double x = 1 - Math.Abs(h / 60 % 2 - 1);
    var (r, g, b) = h < 60 ? (1.0, x, 0.0) : h < 120 ? (x, 1.0, 0.0) : h < 180 ? (0.0, 1.0, x)
                  : h < 240 ? (0.0, x, 1.0) : h < 300 ? (x, 0.0, 1.0) : (1.0, 0.0, x);
    return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
}

record Event(DateTime Target, string Name, DateTime Start);
