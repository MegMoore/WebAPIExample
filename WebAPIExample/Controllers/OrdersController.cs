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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            return await _context.Orders.Include(x => x.Customer).Include(x => x.OrderLines).ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            var order = await _context.Orders.Include(x => x.Customer)
                                .Include(x => x.OrderLines)!
                                .ThenInclude(x => x.Item)
                                .SingleOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders/{id}/ok
        [HttpGet("ok")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersIfOk()
        {
          if(_context.Orders == null)
            {
                return NotFound();
            }
          return await _context.Orders
                        .Where(x => x.Status == "OK")
                        .Include(x => x.Customer)
                        .ToListAsync();
        }


        //Put: api/Orders/{id}/ok
        //Updating Order Status to OK

        [HttpPut("{id}/ok")]
        public async Task<IActionResult> SetOrderStatusToOk(int id, Order order)
        {
            order.Status = "OK";
            return await PutOrder(id, order);   
        }

        //PUT: api/Orders/{id}/backorder

        [HttpPut("{id}/backorder")]
        public async Task<IActionResult> SetOrderStatusToBackOrder(int id, Order order)
        {
            order.Status = "Backorder";
            return await PutOrder(id, order);
        }

        //PUT: api/Orders/{id}/closed

        [HttpPut("{id}/closed")]
        public async Task<IActionResult> SetOrderStatusToClosed(int id, Order order)
        {
            order.Status = "Closed";
            return await PutOrder(id, order);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
          if (_context.Orders == null)
          {
              return Problem("Entity set 'AppDbContext.Orders'  is null.");
          }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
