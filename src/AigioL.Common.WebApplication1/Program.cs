using AigioL.Common.AspNetCore.Helpers.ProgramMain;
using System.Text.Json.Serialization;

namespace AigioL.Common.WebApplication1;

static class Program
{
    public static unsafe void Main(string[] args)
    {
        ProgramHelper.M("AigioL.Common.WebApplication1", args, &ConfigureServices, &Configure);
    }

    static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });
    }

    static void Configure(WebApplication app)
    {
        var sampleTodos = new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
            new(6, "中文测试", DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
        };

        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", () => sampleTodos);
        todosApi.MapGet("/{id}", (int id) =>
            sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());
    }
}

public sealed record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
