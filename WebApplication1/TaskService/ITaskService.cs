namespace WebApplication1.TaskService;


public interface ITaskService {
    public ToDo? GetToDoById(int id);
    public List<ToDo> GetToDoList();
    public void DeleteToDoById(int id);
    public ToDo AddToDO(ToDoTask task);
    public ToDo UpdateToDo(int id, ToDoTask task);

}