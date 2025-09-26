using IdStrategiesDemo.Data;
using IdStrategiesDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var cs = ctx.Configuration.GetConnectionString("Default")!;
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(cs, s => s.EnableRetryOnFailure()));
        services.AddSingleton(ctx.Configuration);
    });

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await ctx.Database.EnsureCreatedAsync();

var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
int insertCount = config.GetValue<int>("Benchmark:InsertCount");
int batchSize = config.GetValue<int>("Benchmark:BatchSize");
int pageSkip = config.GetValue<int>("Benchmark:PageSkip");
int pageTake = config.GetValue<int>("Benchmark:PageTake");

Console.WriteLine($"InsertCount={insertCount}, BatchSize={batchSize}");

ctx.ChangeTracker.AutoDetectChangesEnabled = false;

// Sonuçları toplayacağımız liste
var results = new List<(string Name, string Type, double Seconds)>();

// Helper: Insert benchmark
async Task<(string name, TimeSpan elapsed)> InsertBenchmark<T>(
    string name, Func<T> factory, DbSet<T> set) where T : class
{
    var sw = Stopwatch.StartNew();
    int left = insertCount;

    while (left > 0)
    {
        int cur = Math.Min(batchSize, left);
        var batch = Enumerable.Range(0, cur).Select(_ => factory()).ToList();
        await set.AddRangeAsync(batch);
        await ctx.SaveChangesAsync();
        left -= cur;
    }

    sw.Stop();
    Console.WriteLine($"{name} INSERT => {sw.Elapsed.TotalSeconds:F2}s");
    results.Add((name, "Insert", sw.Elapsed.TotalSeconds));
    return (name, sw.Elapsed);
}

// Helper: Query benchmark
async Task<(string name, TimeSpan elapsed)> QueryBenchmark(string name, IQueryable<object> query)
{
    var sw = Stopwatch.StartNew();
    _ = await query.Skip(pageSkip).Take(pageTake).ToListAsync();
    sw.Stop();
    Console.WriteLine($"{name} QUERY => {sw.Elapsed.TotalSeconds:F2}s");
    results.Add((name, "Query", sw.Elapsed.TotalSeconds));
    return (name, sw.Elapsed);
}

// CSV yazma helper
void WriteCsv(string filePath)
{
    using var writer = new System.IO.StreamWriter(filePath);
    writer.WriteLine("Name,Type,Seconds");
    foreach (var r in results)
    {
        writer.WriteLine($"{r.Name},{r.Type},{r.Seconds:F2}");
    }
    Console.WriteLine($"CSV written: {filePath}");
}

// INSERT benchmarkları
await InsertBenchmark("INT", () => new RecInt(), ctx.RecsInt);
await InsertBenchmark("GUIDv4", () => new RecGuidV4(), ctx.RecsGuidV4);
await InsertBenchmark("ULID_STR", () => new RecUlidString(), ctx.RecsUlidString);
await InsertBenchmark("ULID_BIN", () => new RecUlidBinary(), ctx.RecsUlidBinary);
await InsertBenchmark("GUIDv4+ClusterOnDate", () => new RecGuidV4_ClusterOnDate(), ctx.RecsGuidV4_ClusterOnDate);
await InsertBenchmark("GUIDv7", () => new RecGuidV7(), ctx.RecsGuidV7);

// QUERY benchmarkları
await QueryBenchmark("INT", ctx.RecsInt.OrderBy(x => x.Id).Select(x => (object)x));
await QueryBenchmark("GUIDv4", ctx.RecsGuidV4.OrderBy(x => x.Id).Select(x => (object)x));
await QueryBenchmark("ULID_STR", ctx.RecsUlidString.OrderBy(x => x.Id).Select(x => (object)x));
await QueryBenchmark("ULID_BIN", ctx.RecsUlidBinary.OrderBy(x => x.Id).Select(x => (object)x));
await QueryBenchmark("GUIDv4+ClusterOnDate", ctx.RecsGuidV4_ClusterOnDate.OrderBy(x => x.CreatedOn).Select(x => (object)x));
await QueryBenchmark("GUIDv7", ctx.RecsGuidV7.OrderBy(x => x.Id).Select(x => (object)x));

// CSV'ye yaz
WriteCsv("benchmark_results.csv");


Console.WriteLine("Done.");
