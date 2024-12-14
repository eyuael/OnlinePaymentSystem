using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;
using OnlinePaymentSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlinePaymentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(List<OrderItem> items)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                Items = items,
                TotalAmount = items.Sum(i => i.Quantity * i.PriceAtTime)
            };

            // Update stock quantities
            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for product {product?.Name}");

                product.StockQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        return order == null ? NotFound() : Ok(order);
    }
}
