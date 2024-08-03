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
}).AddEndpointFilter(async (context, next) => {
    var task = context.GetArgument<Todo>(0);
    var errors = new Dictionary<string, string[]>();

    if(task.IsCompleted)
    {
        errors.Add(nameof(Todo.IsCompleted), ["Não pode ser adicionado uma tarefa já concluída"]);
    }

    if(errors.Count > 0)
        return Results.ValidationProblem(errors);

    return await next(context);
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
app.MapPut("/todos/{id}/complete", Results<Ok<Todo>, NotFound> (int id) => {
    int index = todos.FindIndex(t => id == t.Id);
    if(index >= 0) {
        Todo completed = new Todo(todos[index].Id, todos[index].Name, todos[index].DueDate, true);
        todos[index] = completed;
        return TypedResults.Ok(completed);
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