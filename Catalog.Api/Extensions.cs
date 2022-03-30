using Catalog.Api.Dtos;
using Catalog.Api.Entities;

namespace Catalog.Api
{
    // Extension method is meant to builds extensive definition of one type by adding some method that can be executed by that type
    public static class Extensions 
    {
        public static ItemDto AsDto(this Item item) 
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
}