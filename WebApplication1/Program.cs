using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApplication1.ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskService>(new InMemoryTaskService());

var app = builder.Build();

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


List<ToDo> ToDoList  = new List<ToDo>(); //ToDo list

/// <summary>
/// Returnere ToDoList
/// </summary>

// app.MapGet("/ToDoList", () => ToDoList);
app.MapGet("/ToDoList", (ITaskService service) => service.GetToDoList());

///<summary>
///Returnere en ToDo ud fra id givet i parameteren. Hvis ingen findes returneres error 404.
///hvis flere med samme Id findes retuneres DuplicateId.
///</summary>
app.MapGet("/ToDoList/{Id}", Results<Ok<ToDo>, NotFound> (int id, ITaskService service) => {
    try {
    // var targetToDo = ToDoList.SingleOrDefault(t => id == t.Id);
    var targetToDo = service.GetToDoById(id);
    return targetToDo is null
        ? TypedResults.NotFound() //hvis id ikke passer til en ToDo
        : TypedResults.Ok(targetToDo);  //hvis id passer til en ToDo
    }
    //i tilfælde der flere med samme id, udnødvendigt men havde lyst til at lave det :)
    catch (InvalidOperationException) {
        throw new DuplicateId(id);
    }
});

/// <summary>
/// Fjerner alle ToDo fra ToDoList med givet id
/// </summary>
app.MapDelete("/ToDoList/{Id}", (int id, ITaskService service) => {
    service.DeleteToDoById(id);
    return TypedResults.NoContent();
});


///<summary>
///Skaber en Todo ud fra paramerteren samt tilføjer den til ToDoList
///</summary>
///<param>
/// Selve den ToDo der bliver skabt
///</param>
app.MapPost("/ToDoList", (ToDo task, ITaskService service) => {
    service.AddToDO(task);
    return TypedResults.Created("/ToDoList/{Id}", task);

}) //endpoint filter
    //giver error feedback
.AddEndpointFilter(async (context, next) => {
    var taskArgument = context.GetArgument<ToDo>(0);
    var errorDic = new Dictionary<string, string[]>();
    if (taskArgument.DueDate < DateTime.UtcNow) {
        errorDic.Add(nameof(ToDo.DueDate), ["Cannot have due date in a past date"]);
    }
    if (taskArgument.IsComplete) {
        errorDic.Add(nameof(ToDo.IsComplete), ["Cannot add an allready completed task"]);
    }
    if (errorDic.Count > 0) {
        return Results.ValidationProblem(errorDic);
    }
    return await next(context);

});



app.Run();

///<summary>
/// record af hvad en ToDo indeholder
///</summary>
public record ToDo(int Id, string Name, DateTime DueDate, bool IsComplete); //record for ToDo type

