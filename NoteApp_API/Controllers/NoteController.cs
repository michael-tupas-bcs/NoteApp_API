using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteApp_API.Data;
using NoteApp_API.NoteModel;
using NoteApp_API.UserModel;
using NoteApp_API.Services.UserService;

namespace NoteApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        public readonly DataContext _context;
        
        public IUserService _userService;
        public NoteController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<List<Note>>> Get()
        {
            //var note = await _context.Notes.ToArrayAsync();
            var notes = await _context.Notes.Where(x => x.userid == Convert.ToInt32(_userService.GetMyName())).ToListAsync();
            
            return Ok(notes);
        }

        [HttpGet("{id}"), Authorize]
        public async Task<ActionResult<List<Note>>> Get(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) 
            {
                return BadRequest("note not found");
            }
            return Ok(note);
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<List<Note>>> AddNote(Note note) 
        {
            note.userid = Convert.ToInt32(_userService.GetMyName());
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var notes = await _context.Notes.Where(x => x.userid == Convert.ToInt32(_userService.GetMyName())).ToListAsync();
            return Ok(notes);
        }

        [HttpPut, Authorize]
        public async Task<ActionResult<List<Note>>> UpdateNote(Note request)
        {
            var dbNote = await _context.Notes.FindAsync(request.id);
            if (dbNote == null)
            {
                return BadRequest("note not found");
            }

            dbNote.title = request.title;
            dbNote.body = request.body;
            dbNote.img = request.img;

            await _context.SaveChangesAsync();

            var notes = await _context.Notes.Where(x => x.userid == Convert.ToInt32(_userService.GetMyName())).ToListAsync();
            return Ok(notes);
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<ActionResult<List<Note>>> Delete(int id)
        {
            var dbNote = await _context.Notes.FindAsync(id);
            if (dbNote == null)
            {
                return BadRequest("note not found");
            }

            _context.Notes.Remove(dbNote);
            await _context.SaveChangesAsync();

            var notes = await _context.Notes.Where(x => x.userid == Convert.ToInt32(_userService.GetMyName())).ToListAsync();
            return Ok(notes);
        }

    }
}
