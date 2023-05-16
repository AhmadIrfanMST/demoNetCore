using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebApplication3.Authentication;
using WebApplication3.ViewModels;
using WebApplication3.helpers;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [HttpGet]
        [Route("get-role-permissions")]
        public async Task<ActionResult> Index(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            var claims = await _roleManager.GetClaimsAsync(role);
            var roleClaimValues = claims.Select(a => a.Value).ToList();
            return Ok(new Response { Status = "Success", Message = "Role Permissions", data = roleClaimValues });
            //return View(model);
        }
        [HttpPost]
        [Route("add-permissions-to-role")]
        public async Task<IActionResult> Update(PermissionViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
            var selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _roleManager.AddPermissionClaim(role, claim.Value);
            }
            return RedirectToAction("Index", new { roleId = model.RoleId });
        }
    }
}
