using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApplication1.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

/// <summary>
/// Liste over alle ToDo
/// </summary>
/// <returns></returns>
List<ToDo> ToDoList  = new List<ToDo>(); //ToDo list

/// <summary>
/// Returnere ToDoList
/// </summary>
app.MapGet("/ToDoList/{ToDoList}", () => ToDoList);

///<summary>
///Returnere en ToDo ud fra id givet i parameteren. Hvis ingen findes returneres error 404.
///hvis flere med samme Id findes retuneres DuplicateId.
///</summary>
app.MapGet("/ToDo/{Id}", Results<Ok<ToDo>, NotFound> (int id) => {
    try {
    var targetToDo = ToDoList.SingleOrDefault(t => id == t.Id); //
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
app.MapDelete("/ToDoList/{Id}", (int id) => {
    ToDoList.RemoveAll(t => id == t.Id);
    return TypedResults.NoContent();
});


///<summary>
///Skaber en Todo ud fra paramerteren samt tilføjer den til ToDoList
///</summary>
///<param>
/// Selve den ToDo der bliver skabt
///</param>
app.MapPost("/ToDoList", (ToDo task) => {
    ToDoList.Add(task);
    return TypedResults.Created("/ToDoList/{Id}", task);

});

app.Run();

///<summary>
/// record af hvad en ToDo indeholder
///</summary>
public record ToDo(int Id, string Name, DateTime DueDate, bool IsComplete); //record for ToDo type