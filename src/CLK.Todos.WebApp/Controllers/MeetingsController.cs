using CLK.Todos;
using Microsoft.AspNetCore.Mvc;

namespace CLK.Todos.WebApp;

public class MeetingsController : Controller
{
    // Fields
    private readonly TodoContext _todoContext;


    // Constructors
    public MeetingsController(TodoContext todoContext)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(todoContext);

        // Default
        _todoContext = todoContext;
    }


    // Methods
    // GET: /Meetings
    public IActionResult Index()
    {
        // Search
        var meetings = _todoContext.MeetingRepository.FindAll();

        // Return
        return View(meetings);
    }

    // GET: /Meetings/Create
    public IActionResult Create()
    {
        // Return
        return View();
    }

    // POST: /Meetings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Title,StartTime,EndTime,Location")] Meeting? meeting = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(meeting);
        if (!ModelState.IsValid) return View(meeting);

        // Execute
        _todoContext.MeetingRepository.Add(meeting);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Meetings/Edit/{meetingId}
    public IActionResult Edit(Guid meetingId)
    {
        // Search
        var meeting = _todoContext.MeetingRepository.FindById(meetingId);
        if (meeting is null) return NotFound();

        // Return
        return View(meeting);
    }

    // POST: /Meetings/Edit/{meetingId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Guid meetingId, [Bind("MeetingId,Title,StartTime,EndTime,Location,IsCancelled")] Meeting? meeting = null)
    {
        // Contracts
        ArgumentNullException.ThrowIfNull(meeting);
        if (meetingId != meeting.MeetingId) return View(meeting);
        if (!ModelState.IsValid) return View(meeting);

        // Execute
        _todoContext.MeetingRepository.Update(meeting);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // GET: /Meetings/Delete/{meetingId}
    public IActionResult Delete(Guid meetingId)
    {
        // Search
        var meeting = _todoContext.MeetingRepository.FindById(meetingId);
        if (meeting is null) return NotFound();

        // Return
        return View(meeting);
    }

    // POST: /Meetings/Delete/{meetingId}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(Guid meetingId)
    {
        // Execute
        _todoContext.MeetingRepository.Remove(meetingId);

        // Return
        return RedirectToAction(nameof(Index));
    }

    // POST: /Meetings/Toggle/{meetingId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(Guid meetingId)
    {
        // Search
        var meeting = _todoContext.MeetingRepository.FindById(meetingId);
        if (meeting is null) return NotFound();

        // Execute
        meeting.ToggleCancelled();
        _todoContext.MeetingRepository.Update(meeting);

        // Return
        return RedirectToAction(nameof(Index));
    }
}
