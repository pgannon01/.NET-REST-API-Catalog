using System;

namespace Catalog.Api.Entities
{
    // Instead of the normal class declaration, in .NET 5 we can do record types
    // Record types are classes, but used for immutable objects, convenient for objects we recieve from the web
    // Also have with-expressions support
    // And have value-based equality support
    public class Item
    {
        public Guid Id { get; set; } // init instead of set, makes it so that we can create a new object of this class as normal, but can't modify it afterwards
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedDate { get; set; } // The date and time where the item got created in the system
    }
}