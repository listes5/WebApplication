using Microsoft.VisualBasic;
using WebApplication1.ExceptionHandling;

namespace WebApplication1.TaskService;


public class ToDoTaskService : ITaskService {

    private readonly List<ToDo> toDoList = [];

    private int _id = 0;
    public int Id()
    {
        _id++;
        return _id;
    }
    public ToDo AddToDO(ToDoTask task) {
        ToDo ToDoWithId = new ToDo(Id(), task.Name, task.DueDate, false);
        toDoList.Add(ToDoWithId);
        return ToDoWithId;
    }

    public void DeleteToDoById(int id) {
        toDoList.RemoveAll(task => id == task.Id);
    }

    public ToDo? GetToDoById(int id) {
        try {
            return toDoList.SingleOrDefault(task => id == task.Id);
        }
        catch (InvalidOperationException) {
            throw new DuplicateId(id); //excepection
        }
    }

    public List<ToDo> GetToDoList() {
        return toDoList;
    }

    public ToDo UpdateToDo(int id, ToDoTask task)
    {
        if (GetToDoById(id) != null) {
            toDoList.Remove(GetToDoById(id)!);
            DeleteToDoById(id);
            ToDo updated = new ToDo(id, task.Name, task.DueDate, false);
            toDoList.Add(updated);
            return updated;
        }
        else throw new Exception();
    }
}