using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var todos = new List<Todo>();

//Middlewares
app.Use(async (context, next) => {
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.Now}] Start");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.Now}] Finished");
});

//Routes
app.MapGet("/todos", () => todos);
app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => 
{
    var targetTodo = todos.SingleOrDefault(t => id == t.Id);
    return targetTodo is null ? TypedResults.NotFound() : TypedResults.Ok(targetTodo);
});
app.MapPost("/todos", (Todo task) => 
{
    todos.Add(task);
    return TypedResults.Created("/todos/{id}", task);
});
app.MapPut("/todos/{id}", Results<Ok<Todo>, NotFound> (int id, Todo task) => 
{
    int index = todos.FindIndex(t => id == t.Id);
    if(index >= 0) {
        todos[index] = task;
        return TypedResults.Ok(task);
    }else{
        return TypedResults.NotFound();
    }
});
app.MapDelete("/todos/{id}", (int id) => 
{
    todos.RemoveAll(t => id == t.Id);
    return TypedResults.NoContent();
});

app.Run();

public record Todo(int Id, string Name, DateTime DueDate, bool IsCompleted);