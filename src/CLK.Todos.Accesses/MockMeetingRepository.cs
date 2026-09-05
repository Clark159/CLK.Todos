using CLK.Todos;

namespace CLK.Todos.Accesses;

public class MockMeetingRepository : IMeetingRepository
{
    // Fields
    private readonly object _lock = new();

    private readonly List<Meeting> _meetings = new();


    // Methods
    public void Add(Meeting meeting)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(meeting);

        // Lock
        lock (_lock)
        {
            // Execute
            meeting.CreateTime = DateTime.UtcNow;
            meeting.UpdateTime = DateTime.UtcNow;
            _meetings.Add(meeting);
        }
    }

    public void Update(Meeting meeting)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(meeting);

        // Lock
        lock (_lock)
        {
            // Search
            var entity = _meetings.FirstOrDefault(m => m.MeetingId == meeting.MeetingId);
            if (entity is null) throw new KeyNotFoundException($"Meeting not found: {meeting.MeetingId}");

            // Execute
            entity.Title = meeting.Title;
            entity.StartTime = meeting.StartTime;
            entity.EndTime = meeting.EndTime;
            entity.Location = meeting.Location;
            entity.IsCancelled = meeting.IsCancelled;
            entity.UpdateTime = DateTime.UtcNow;
        }
    }

    public void Remove(Guid meetingId)
    {
        // Lock
        lock (_lock)
        {
            // Search
            var entity = _meetings.FirstOrDefault(m => m.MeetingId == meetingId);
            if (entity is null) throw new KeyNotFoundException($"Meeting not found: {meetingId}");

            // Execute
            _meetings.Remove(entity);
        }
    }

    public Meeting? FindById(Guid meetingId)
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _meetings.FirstOrDefault(m => m.MeetingId == meetingId);
        }
    }

    public IReadOnlyList<Meeting> FindAll()
    {
        // Lock
        lock (_lock)
        {
            // Return
            return _meetings
                .OrderBy(m => m.IsCancelled)
                .ThenBy(m => m.StartTime)
                .ToList();
        }
    }
}
