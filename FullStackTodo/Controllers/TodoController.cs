using Microsoft.AspNetCore.Mvc;
using FullStackTodo.Data;
using FullStackTodo.Models;

namespace FullStackTodo.Controllers;

public class TodoController : Controller
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Todo
    public IActionResult Index()
    {
        var items = _db.TodoItems.OrderBy(t => t.IsDone).ThenByDescending(t => t.CreatedAt).ToList();
        ViewBag.Total = items.Count;
        ViewBag.Done = items.Count(t => t.IsDone);
        return View(items);
    }

    // POST /Todo/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TodoItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            TempData["Error"] = "标题不能为空";
            return RedirectToAction("Index");
        }

        _db.TodoItems.Add(item);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    // POST /Todo/Toggle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int id)
    {
        var item = _db.TodoItems.Find(id);
        if (item != null)
        {
            item.IsDone = !item.IsDone;
            _db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // POST /Todo/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var item = _db.TodoItems.Find(id);
        if (item != null)
        {
            _db.TodoItems.Remove(item);
            _db.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
