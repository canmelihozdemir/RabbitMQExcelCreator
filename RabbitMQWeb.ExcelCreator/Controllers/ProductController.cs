using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreator.Models;
using RabbitMQWeb.ExcelCreator.Services;

namespace RabbitMQWeb.ExcelCreator.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppIdentityDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
        public ProductController(UserManager<IdentityUser> userManager, AppIdentityDbContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _userManager = userManager;
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new UserFile()
            {
                UserId=user.Id,
                FileName = fileName,
                FileStatus=FileStatus.Creating,
                FilePath="-"
            };

            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();


            _rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId=userFile.Id});



            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(Files));
        }


        public async Task<IActionResult> Files()
        {
            var user= await _userManager.FindByNameAsync(User.Identity.Name);


            return View(await _context.UserFiles.Where(x=>x.UserId==user.Id).OrderByDescending(x=>x.Id).ToListAsync());
        }
    }
}
