using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.DTO;
using SimpleInventorySystem.Model.Entities;

namespace SimpleInventorySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        // TODO: replace ApplicationDbContext with the actual name of your DbContext class
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _context.Customers
                .Select(c => ToDto(c))
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(ToDto(customer));
        }

        // POST: api/customer
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Address = dto.Address
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var resultDto = ToDto(customer);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, resultDto);
        }

        // PUT: api/customer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDto dto)
        {
            if (id != dto.CustomerId)
            {
                return BadRequest();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.Name = dto.Name;
            customer.Address = dto.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.Customers.AnyAsync(c => c.CustomerId == id);
                if (!exists)
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/customer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static CustomerDto ToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Address = customer.Address
            };
        }
    }
}