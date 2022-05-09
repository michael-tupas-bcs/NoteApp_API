using Microsoft.EntityFrameworkCore;
using NoteApp_API.NoteModel;
using NoteApp_API.UserModel;

namespace NoteApp_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<UserData> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}
