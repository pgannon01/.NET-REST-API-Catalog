using Moq;
using Xunit;
using Catalog.Api.Repositories;
using System;
using Catalog.Api.Entities;
using Microsoft.Extensions.Logging;
using Catalog.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Catalog.Api.Dtos;
using System.Collections.Generic;

namespace Catalog.UnitTests
{
    public class ItemsControllerTests
    {
        // Naming convention to follow with test methods: UnitOfWork_StateUnderTest_ExpectedBehavior
            // UnitOfWork: What is it that you're testing, what function is going to be tested by this test
            // StateUnderTest: Under which conditions are you testing this method
            // ExpectedBehavior: What do we expect from this unit after we execute the action parts of this test

        private readonly Mock<IItemsRepositories> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();
        private readonly Random rand = new();

        [Fact] // Declares a method as a test method, need to add it to any test methods
        public async void GetItemAsync_WithUnexistingItem_ReturnsNotFound()
        {
            // Other good convetions: Write it like this

            // Arrange (First, set up everything to be ready to execute the test)
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act (Actually execute the test)
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert (Verify whatever needs to be verified about the execution of the action)
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void GetItemAsync_WithExistingItem_ReturnsExpectedItem()
        {
            // Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            //Assert
            result.Value.Should().BeEquivalentTo(expectedItem); 
            // Should compare the values of the properties of the resulting dto, with the expected items values
            // This method helps us to not have to go through every property and do that for us
            // ComparingByMembers tells it to not compare the dto directly to the item, and to just focus on the properties of both of them
                // As long as the properties match, have the same names and values, then it should go ahead and tell us the objects are the same
        }

        [Fact]
        public async void GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            // Arrange
            // Get a set of items from the repo
            var expectedItems = new[]{CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var actualItems = await controller.GetItemsAsync();

            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public async void GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
        {
            // Arrange
            // Get a set of items from the repo
            var allItems = new[] 
            { 
                new Item() { Name = "Potion" },
                new Item() { Name = "Antidote" },
                new Item() { Name = "Hi-Potion" },
            };

            var nameToMatch = "Potion";

            repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

            // Assert
            // Assert we only found items where the name matches the nameToMatch
            foundItems.Should().OnlyContain(
                item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
            );
        }

        [Fact]
        public async void CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            // Arrange
            // Get a set of items from the repo
            var itemToCreate = new CreateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), rand.Next(1000));

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.CreateItemAsync(itemToCreate);

            // Assert
            // Try to retrieve item dto
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
            itemToCreate.Should().BeEquivalentTo(createdItem, options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());
            createdItem.Id.Should().NotBeEmpty();
            createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, new TimeSpan(0, 0, 1));
        }

        [Fact]
        public async void UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // Arrange
            // Get a set of items from the repo
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(existingItem);

            var itemId = existingItem.Id;
            var itemToUpdate = new UpdateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), existingItem.Price + 3);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async void DeleteItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // Arrange
            // Get a set of items from the repo
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>())).ReturnsAsync(existingItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

            // Act
            var result = await controller.DeleteItemAsync(existingItem.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        private Item CreateRandomItem()
        {
            return new() 
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}