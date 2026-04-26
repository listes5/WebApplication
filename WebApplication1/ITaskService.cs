namespace WebApplication1;

public interface ITaskService {
    public ToDo? GetToDoById(int id);
    public List<ToDo> GetToDoList();
    public void DeleteToDoById(int id);
    public ToDo AddToDO(ToDo task);

}