namespace WebApplication1;

public class InMemoryTaskService : ITaskService {

    private readonly List<ToDo> toDoList = [];

    public ToDo AddToDO(ToDo task) {
        toDoList.Add(task);
        return task;
    }

    public void DeleteToDoById(int id) {
        toDoList.RemoveAll(task => id == task.Id);
    }

    public ToDo? GetToDoById(int id) {
        return toDoList.SingleOrDefault(task => id == task.Id);
    }

    public List<ToDo> GetToDoList() {
        return toDoList;
    }
}