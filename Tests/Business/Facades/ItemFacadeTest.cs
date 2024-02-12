using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Requests;

namespace Tests.Business.Facades;

public class ItemFacadeTest
{
    private ItemFacade _itemFacade;

    private Mock<ImageFacade> _imageFacade;
    private Mock<ItemService> _itemService;
    private Mock<LoanService> _loanService;
    private Mock<ItemCategoryService> _itemCategoryService;
    private Mock<ItemTagService> _itemTagService;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;
    private ApplicationUser _owner;

    private Item _item;

    private const double _tolerance = 0.0001;

    [SetUp]
    public void Setup()
    {
        _imageFacade = new Mock<ImageFacade>(null, null, null, null);
        _itemService = new Mock<ItemService>(null);
        _loanService = new Mock<LoanService>(null, null);
        _itemCategoryService = new Mock<ItemCategoryService>(null);
        _itemTagService = new Mock<ItemTagService>(null);
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _itemFacade = new ItemFacade(_imageFacade.Object, _itemService.Object, _loanService.Object,
            _itemCategoryService.Object, _itemTagService.Object, _authenticateService.Object, _mapper.Object,
            _configuration.Object);

        _user = new ApplicationUser { Id = "1" };
        _owner = new ApplicationUser { Id = "2" };

        _item = new Item
        {
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user
        };
    }

    #region CreateItem

    [Test]
    public void CreateItem_UserNotAuthenticated_ThrowsException()
    {
        // Arrange
        var request = new ItemRequest();

        // User is not authenticated - must 
        _authenticateService
            .Setup(o => o.GetCurrentUser())
            .ThrowsAsync(new NotAuthenticatedException());

        // Must throw UserNotAuthenticatedException because the user is not authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () => await _itemFacade.CreateItem(request));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Create was not called
        _itemService.Verify(o => o.Create(It.IsAny<Item>()), Times.Never);
    }

    [Test]
    public async Task CreateItem_UserAuthenticated_CreatesItem()
    {
        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Categories = new List<ItemCategoryRequest>()
            {
                new() { Id = 1 }
            },
            Tags = new List<ItemTagRequest>()
            {
                new() { Name = "Tag" }
            }
        };

        // Builde expected item
        var expectedItem = new Item
        {
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user
        };
        expectedItem.Categories.Add(new ItemCategory() { Id = 1 });
        expectedItem.Tags.Add(new ItemTag() { Name = "Tag" });

        // User is authenticated
        _authenticateService
            .Setup(o => o.GetCurrentUser())
            .ReturnsAsync(_user);

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory> { new() { Id = 1 } });

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag> { new() { Name = "Tag" } });

        // Must return the created item
        var result = await _itemFacade.CreateItem(request);

        // Asserations
        AssertItem(expectedItem, result);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Create was called
        _itemService.Verify(o => o.Create(result), Times.Once);
    }

    #endregion

    #region UpdateItem

    [Test]
    public async Task UpdateItem_UserAuthenticated_UpdatesItem()
    {
        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000,
            Categories = new List<ItemCategoryRequest>()
            {
                new() { Id = 1 },
                new() { Id = 2 },
            },
            Tags = new List<ItemTagRequest>()
            {
                new() { Name = "Tag" },
                new() { Name = "Tag2" },
            }
        };

        // Old item
        var oldItem = new Item
        {
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user
        };

        // old item categories
        oldItem.Categories.Add(new ItemCategory() { Id = 1 });

        // old item tags
        oldItem.Tags.Add(new ItemTag() { Name = "Tag" });

        // Builde expected item
        var expectedItem = new Item
        {
            Name = "Nový item zmena",
            Alias = "nov-item-zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000,
            Status = ItemStatus.Public,
            Owner = _user
        };

        // expected categories
        expectedItem.Categories.Add(new ItemCategory() { Id = 1 });
        expectedItem.Categories.Add(new ItemCategory() { Id = 2 });

        // expected tags
        expectedItem.Tags.Add(new ItemTag() { Name = "Tag" });
        expectedItem.Tags.Add(new ItemTag() { Name = "Tag2" });

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory> { new() { Id = 1 }, new() { Id = 2 } });

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag> { new() { Name = "Tag" }, new() { Name = "Tag2" } });

        await _itemFacade.UpdateItem(oldItem, request);

        // Asserations
        AssertItem(expectedItem, oldItem);

        // Verify that Update was called
        _itemService.Verify(o => o.Update(oldItem), Times.Once);
    }

    [Test]
    public async Task UpdateItem_UserAuthenticatedAndItemWasDenied_ItemIsBeingApproved()
    {
        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000
        };

        var oldItem = new Item
        {
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Denied,
            Owner = _user
        };

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory>());

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag>());

        await _itemFacade.UpdateItem(oldItem, request);

        // Asserations
        Assert.That(oldItem.Status, Is.EqualTo(ItemStatus.Approving));
    }

    #endregion

    #region DeleteItem

    // TODO: Add tests for DeleteItem

    #endregion

    /// <summary>
    /// Helper method to assert that two items are equal.
    /// </summary>
    /// <param name="expected">Expected item object</param>
    /// <param name="actual">Actual item object</param>
    private void AssertItem(Item? expected, Item? actual)
    {
        if (expected == null && actual == null) return;
        if (expected == null || actual == null) Assert.Fail("One of the items is null");

        Assert.That(actual.Name, Is.EqualTo(expected.Name));
        Assert.That(actual.Alias, Is.EqualTo(expected.Alias));
        Assert.That(actual.Description, Is.EqualTo(expected.Description));
        Assert.That(actual.PricePerDay, Is.EqualTo(expected.PricePerDay).Within(_tolerance));
        Assert.That(actual.PurchasePrice, Is.EqualTo(expected.PurchasePrice).Within(_tolerance));
        Assert.That(actual.SellingPrice, Is.EqualTo(expected.SellingPrice).Within(_tolerance));
        Assert.That(actual.RefundableDeposit, Is.EqualTo(expected.RefundableDeposit).Within(_tolerance));
        Assert.That(actual.Status, Is.EqualTo(expected.Status));
        Assert.That(actual.Owner.Id, Is.EqualTo(expected.Owner.Id));

        // assert categories
        Assert.That(actual.Categories.Count, Is.EqualTo(expected.Categories.Count));
        for (var i = 0; i < actual.Categories.Count; i++)
        {
            Assert.That(actual.Categories.ElementAt(i).Id, Is.EqualTo(expected.Categories.ElementAt(i).Id));
        }

        // assert tags
        Assert.That(actual.Tags.Count, Is.EqualTo(expected.Tags.Count));
        for (var i = 0; i < actual.Tags.Count; i++)
        {
            Assert.That(actual.Tags.ElementAt(i).Name, Is.EqualTo(expected.Tags.ElementAt(i).Name));
        }
    }
}