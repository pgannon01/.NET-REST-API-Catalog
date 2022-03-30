using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Catalog.Api.Repositories;
using Catalog.Api.Entities;
using System;
using System.Linq;
using Catalog.Api.Dtos;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Controllers
{
    // TODO: Redo the docker account stuff and ensure everything else is working through building.
    [ApiController] // Marks this class as an Api Controller, brings in a bunch of default behaviors to make our lives easier
    [Route("items")] // Which http route this controller will respond to
    public class ItemsController : ControllerBase
    {
        private readonly IItemsRepositories repository;
        private readonly ILogger<ItemsController> logger;

        public ItemsController(IItemsRepositories repository, ILogger<ItemsController> logger) 
        {
            this.repository = repository;
            this.logger = logger;
        }

        // Define our route to retrieve all our items
        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetItemsAsync(string nameToMatch = null)
        {
            // Setting param to null because we're not always expecting a user to want to find something with a specific name, so want to give that as an option
            var items = (await repository.GetItemsAsync()).Select( item => item.AsDto());

            // If there's a specific item a user wants, apply a filter to look for
            if (!string.IsNullOrWhiteSpace(nameToMatch))
            {
                // Apply a filter to find the nameToMatch item
                items = items.Where(item => item.Name.Contains(nameToMatch, StringComparison.OrdinalIgnoreCase));
                // OrdinalIgnoreCase means we'll ignore case, so can find no matter what casing the user uses
            }

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");
            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id) // ActionResult<> allows us to return more than one type from a method, so we can return NotFound() this way
        {
            var item = await repository.GetItemAsync(id);

            if (item is null) 
            {
                return NotFound();
            }

            return item.AsDto();
        }

        // POST /items
        [HttpPost]
        public async  Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto) 
        {
            Item item = new() 
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateItemAsync(item);

            // Return the item and a header that specified where you can go ahead and get information about that created item
            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto());
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public async  Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
        {
            var existingItem = await repository.GetItemAsync(id);

            // Verify the id exists
            if (existingItem is null) 
            {
                return NotFound();
            }

            existingItem.Name = itemDto.Name;
            existingItem.Price = itemDto.Price;

            await repository.UpdateItemAsync(existingItem);

            return NoContent(); // Convention is to return nothing if it works
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItemAsync(Guid id) 
        {
            var existingItem = repository.GetItemAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            await repository.DeleteItemAsync(id);

            return NoContent(); // Similar to Update
        }
    }
}