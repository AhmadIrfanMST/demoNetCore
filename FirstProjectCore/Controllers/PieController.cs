using FirstProjectCore.Models;
using FirstProjectCore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FirstProjectCore.Controllers
{
    public class PieController : Controller
    {
        private readonly IPieRepository _pieRepository;
        private readonly ICategoryRepository _categoryRepository;
        public PieController(IPieRepository pie, ICategoryRepository categoryRepository)
        {
            _pieRepository = pie;
            _categoryRepository = categoryRepository;   
        }

        public IActionResult List()
        {
            PieListViewModel piesListViewModel = new PieListViewModel(_pieRepository.AllPies,"Cheese cakes");
            return View(piesListViewModel);
        }
    }
}
