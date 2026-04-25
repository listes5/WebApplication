namespace WebApplication1.ExceptionHandling;

public class DuplicateId : Exception {
    public int Id {get;} = -1; //hvis -1 er den ikke blevet assignet endu

    public DuplicateId (int id)
    : base($"Der findes to eller flere ToDo med samme Id: ${id}") {
        Id = id;
    }


}