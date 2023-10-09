using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIExample.Data;
using WebAPIExample.Models;

namespace WebAPIExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderLinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderLinesController(AppDbContext context)
        {
            _context = context;
        }
        // Get the total amount of orders
        private async Task RecalculateOrderTotal(int id)
        {
            var total = (from o in _context.Orders
                         join ol in _context.OrderLines
                             on o.Id equals ol.OrderId
                         join i in _context.Items
                             on ol.ItemId equals i.Id
                         where o.Id == id
                         select new
                         {
                             LineTotal = ol.Quantity * i.Price
                         }).Sum(x => x.LineTotal);
            var order = await _context.Orders.FindAsync(id);
            order!.Total = total;
            await _context.SaveChangesAsync();


        }

        // GET: api/OrderLines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderLine>>> GetOrderLine()
        {
          if (_context.OrderLines == null)
          {
              return NotFound();
          }
            return await _context.OrderLines.Include(x => x.Order).Include(x => x.Item).ToListAsync();
        }

        // GET: api/OrderLines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderLine>> GetOrderLine(int id)
        {
          if (_context.OrderLines == null)
          {
              return NotFound();
          }
            var orderLine = await _context.OrderLines
                            .Include(x => x.Order)
                            .Include(x => x.Item)
                            .SingleOrDefaultAsync(x => x.Id == id);

            if (orderLine == null)
            {
                return NotFound();
            }

            return orderLine;
        }

        // PUT: api/OrderLines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderLine(int id, OrderLine orderLine)
        {
            if (id != orderLine.Id)
            {
                return BadRequest();
            }

            _context.Entry(orderLine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await RecalculateOrderTotal(orderLine.OrderId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderLineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/OrderLines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OrderLine>> PostOrderLine(OrderLine orderLine)
        {
          if (_context.OrderLines == null)
          {
              return Problem("Entity set 'AppDbContext.OrderLine'  is null.");
          }
            _context.OrderLines.Add(orderLine);
            await _context.SaveChangesAsync();
            await RecalculateOrderTotal(orderLine.OrderId);

            return CreatedAtAction("GetOrderLine", new { id = orderLine.Id }, orderLine);
        }

        // DELETE: api/OrderLines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderLine(int id)
        {
            if (_context.OrderLines == null)
            {
                return NotFound();
            }
            var orderLine = await _context.OrderLines.FindAsync(id);
            if (orderLine == null)
            {
                return NotFound();
            }
            var orderId = orderLine.OrderId;

            _context.OrderLines.Remove(orderLine);
            await _context.SaveChangesAsync();
            await RecalculateOrderTotal(orderId);

            return NoContent();
        }

        private bool OrderLineExists(int id)
        {
            return (_context.OrderLines?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
