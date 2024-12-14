using ECommerceAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinePaymentSystem.Models;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts(
        [FromQuery] string category = null,
        [FromQuery] string search = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        return await query.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}