namespace CLK.Todos;

public interface IMeetingRepository
{
    // Methods
    void Add(Meeting meeting);

    void Update(Meeting meeting);

    void Remove(Guid meetingId);

    Meeting? FindById(Guid meetingId);

    IReadOnlyList<Meeting> FindAll();
}
