using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using Shared;

namespace RabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext appDbContext,
            RabbitMQPublisher rabbitMQPublisher)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            // User property comes with Controller class.
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await _appDbContext.UserFiles.AddAsync(userFile);

            await _appDbContext.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new CreateExcelMessage
            {
                FileId = userFile.Id,
                UserId = user.Id
            });

            TempData["StartCreatingExcel"] = true;

            return RedirectToAction("Files","Product");
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _appDbContext.UserFiles.Where(u => u.UserId == user.Id).ToListAsync());
        }
    }
}
