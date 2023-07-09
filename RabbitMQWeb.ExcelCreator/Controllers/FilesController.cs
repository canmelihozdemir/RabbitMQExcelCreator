using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreator.Hubs;
using RabbitMQWeb.ExcelCreator.Models;

namespace RabbitMQWeb.ExcelCreator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppIdentityDbContext _context;
        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppIdentityDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();

            var userFile = await _context.UserFiles.FirstAsync(x=>x.Id==fileId);
            var filePath=userFile.FileName+Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot/files",filePath);

            using FileStream stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            userFile.CreatedDate=DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed; 

            await _context.SaveChangesAsync();


            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");




            return Ok();

        }
    }
}
