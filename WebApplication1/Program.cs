using Microsoft.AspNetCore.Http.HttpResults;
using WebApplication1.ExceptionHandling;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.TaskService;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskService>(new ToDoTaskService());
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

/// <summary>
/// diregere at hvis man skriver task/(int) så viderfører det den til ToDoList/int
/// </summary>
/// <param name="RewriteOptions().AddRedirect("tasks/(.*)""></param>
app.UseRewriter(new RewriteOptions().AddRedirect("tasks/(.*)", "ToDoList/$1"));

///<summary>
/// Giver oplysninger hver gang noget går i gang og når det slutter
/// </summary>
app.Use(async (context, next) => //gør dette seperat, parrallel programming
{
    //giv information om hvad der sker nu
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context); //vent til informationen er færdig

    //giv information om hvad der sker nu når det færdigt
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

/// <summary>
/// Returnere ToDoList
/// </summary>
app.MapGet("/ToDoList", (ITaskService service) => service.GetToDoList());

///<summary>
///Returnere en ToDo ud fra id givet i parameteren. Hvis ingen findes returneres error 404 med ProblemDetails.
///hvis flere med samme Id findes returneres 409 Conflict via GlobalExceptionHandler.
///</summary>
app.MapGet("/ToDoList/{Id}", Results<Ok<ToDo>, NotFound<ProblemDetails>> (int id, ITaskService service) => {
    var targetToDo = service.GetToDoById(id); 
    
    //smider selv DuplicateId hvis flere matcher
    return targetToDo is null
        ? TypedResults.NotFound(new ProblemDetails {
            Status = StatusCodes.Status404NotFound,
            Title = "ToDo not found",
            Detail = $"Der findes ingen ToDo med id {id}"
        })
        : TypedResults.Ok(targetToDo);
});

/// <summary>
/// Fjerner alle ToDo fra ToDoList med givet id
/// </summary>
app.MapDelete("/ToDoList/{Id}", (int id, ITaskService service) => {
    service.DeleteToDoById(id);
    return TypedResults.NoContent();
});


///<summary>
///Skaber en Todo ud fra en ToDoTask og tilføjer den til ToDoList
///</summary>
app.MapPost("/ToDoList", (ToDoTask task, ITaskService service) => {
    ToDo toDoTask = service.AddToDO(task);
    return TypedResults.Created($"/ToDoList/{toDoTask.Id}", toDoTask);

}) //endpoint filter
    //giver error feedback
    .AddEndpointFilter(async (context, next) => {
        var taskArgument = context.GetArgument<ToDoTask>(0);
        var errorDic = new Dictionary<string, string[]>();
        if (taskArgument.DueDate < DateTime.UtcNow) {
            errorDic.Add(nameof(ToDoTask.DueDate), ["Cannot have due date in a past date"]);
        }
        if (errorDic.Count > 0) {
            return Results.ValidationProblem(errorDic);
        }
        return await next(context);

});

///<summary>
///Ændre navn og DueDate på en eksiterende ToDo udfra id
///</summary>
app.MapPut("/ToDoList/{Id}", (int id, ToDoTask task, ITaskService service) => {
    var targetToDo = service.GetToDoById(id);
    if (targetToDo is null) {
        return Results.NotFound(new ProblemDetails {
            Status = StatusCodes.Status404NotFound,
            Title = "ToDo not found",
            Detail = $"Der findes ingen ToDo med id {id}"
        });
    }
    else if (task.DueDate < DateTime.UtcNow)
    {
        return Results.Problem(new ProblemDetails {
            Status = StatusCodes.Status400BadRequest,
            Title = "DueDate invalid",
            Detail = "Cannot have due date in a past date"
        });
    }
    var updated = service.UpdateToDo(id, task); // skal implementeres i service
    return Results.Ok(updated);
});




app.Run();

///<summary>
/// record af hvad en ToDo indeholder
///</summary>
public record ToDo(int Id, string Name, DateTime DueDate, bool IsComplete); //record for ToDo type
public record ToDoTask(string Name, DateTime DueDate); 
