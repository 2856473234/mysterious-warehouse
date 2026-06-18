using Microsoft.EntityFrameworkCore;
using FullStackTodo.Models;

namespace FullStackTodo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
