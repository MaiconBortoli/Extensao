using EasyStock.Data;
using EasyStock.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyStock.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Adicionar()
        {
            var produto = new Produto();
            return View(produto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(Produto produto)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(); // Ou redirecione pra tela de login
                }

                var userId = int.Parse(userIdClaim);

                // Atribuir o UserId ao produto
                produto.UserId = userId;

                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Lista));
            }
            return View(produto);
        }


        public async Task<IActionResult> Lista()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // Ou redirecione pra tela de login
            }

            var userId = int.Parse(userIdClaim);
            var produtos = await _context.Produtos
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(produtos);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // Ou redirecione pra tela de login
            }

            var userId = int.Parse(userIdClaim);
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (produto == null)
            {
                return NotFound(); // Caso não encontre o produto ou o produto não pertença ao usuário
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }



        public async Task<IActionResult> Editar(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // Ou redirecione pra tela de login
            }

            var userId = int.Parse(userIdClaim);
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (produto == null)
            {
                return NotFound();
            }

            return View("Adicionar", produto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // Ou redirecione pra tela de login
            }

            var userId = int.Parse(userIdClaim);

            // Verifica se o produto realmente pertence ao usuário logado
            var produtoExistente = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (produtoExistente == null)
            {
                return NotFound();
            }

            // Garante que o UserId não seja alterado
            produto.UserId = userId;

            // Atualiza diretamente o produto rastreado
            _context.Entry(produtoExistente).CurrentValues.SetValues(produto);

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Lista));
            }

            return View("Adicionar", produto);
        }




        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }

    }
}
