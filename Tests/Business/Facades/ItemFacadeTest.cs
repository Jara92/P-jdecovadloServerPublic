using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
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
        _imageFacade = new Mock<ImageFacade>(null, null, null, null, null);
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
            Owner = _owner
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
            Categories = new List<int> { 1 },
            Tags = new List<string> { "Tag" }
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
            Categories = new List<int> { 1, 2 },
            Tags = new List<string> { "Tag", "Tag2" },
            MainImageId = null
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
            Owner = _user,
            MainImageId = 20
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
            Owner = _user,
            MainImageId = 20 // Main image is not changed if request.MainImageId is null
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
    public async Task UpdateItem_UserAuthenticatedAndInvalidMainImageIdSet_UpdatesItem()
    {
        var newMainImageId = 2;

        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000,
            Categories = new List<int> { 1, 2 },
            Tags = new List<string> { "Tag", "Tag2" },
            MainImageId = newMainImageId
        };

        // Old item
        var oldItem = new Item
        {
            Id = 1,
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user,
            MainImageId = 1
        };

        // old item categories
        oldItem.Categories.Add(new ItemCategory() { Id = 1 });

        // old item tags
        oldItem.Tags.Add(new ItemTag() { Name = "Tag" });

        // Mock image
        _imageFacade
            .Setup(o => o.GetImage(newMainImageId))
            .ThrowsAsync(new EntityNotFoundException());

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory> { new() { Id = 1 }, new() { Id = 2 } });

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag> { new() { Name = "Tag" }, new() { Name = "Tag2" } });

        // exception is thrown because the image does not exist
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _itemFacade.UpdateItem(oldItem, request));

        // Verify that Update was not called
        _itemService.Verify(o => o.Update(oldItem), Times.Never);
    }

    [Test]
    public async Task UpdateItem_UserAuthenticatedAndNewImageWhichDoesNotBelongToTheItemIsSet_UpdatesItem()
    {
        var newMainImageId = 2;

        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000,
            Categories = new List<int> { 1, 2 },
            Tags = new List<string> { "Tag", "Tag2" },
            MainImageId = newMainImageId
        };

        // Old item
        var oldItem = new Item
        {
            Id = 1,
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user,
            MainImageId = 1
        };

        // old item categories
        oldItem.Categories.Add(new ItemCategory() { Id = 1 });

        // old item tags
        oldItem.Tags.Add(new ItemTag() { Name = "Tag" });

        // Mock image
        _imageFacade
            .Setup(o => o.GetImage(newMainImageId))
            .ReturnsAsync(new Image { Id = 2, Item = new Item { Id = 20 } });

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory> { new() { Id = 1 }, new() { Id = 2 } });

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag> { new() { Name = "Tag" }, new() { Name = "Tag2" } });

        // exception is thrown because the image does not belong to the item
        Assert.ThrowsAsync<ArgumentException>(async () => await _itemFacade.UpdateItem(oldItem, request));

        // Verify that Update was not called
        _itemService.Verify(o => o.Update(oldItem), Times.Never);
    }

    [Test]
    public async Task UpdateItem_UserAuthenticatedAndNewMainImageSet_UpdatesItem()
    {
        var newMainImageId = 2;

        // Arrange
        var request = new ItemRequest()
        {
            Name = "Nový item zmena",
            Description = "Description zmena",
            PricePerDay = 200,
            PurchasePrice = 2000,
            SellingPrice = 4000,
            RefundableDeposit = 2000,
            Categories = new List<int> { 1, 2 },
            Tags = new List<string> { "Tag", "Tag2" },
            MainImageId = newMainImageId
        };

        // Old item
        var oldItem = new Item
        {
            Id = 1,
            Name = "Nový item",
            Alias = "nov-item",
            Description = "Description",
            PricePerDay = 100,
            PurchasePrice = 1000,
            SellingPrice = 2000,
            RefundableDeposit = 1000,
            Status = ItemStatus.Public,
            Owner = _user,
            MainImageId = 1
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
            Owner = _user,
            MainImage = new Image { Id = newMainImageId, Item = oldItem }
        };

        // expected categories
        expectedItem.Categories.Add(new ItemCategory() { Id = 1 });
        expectedItem.Categories.Add(new ItemCategory() { Id = 2 });

        // expected tags
        expectedItem.Tags.Add(new ItemTag() { Name = "Tag" });
        expectedItem.Tags.Add(new ItemTag() { Name = "Tag2" });

        // Mock image
        _imageFacade
            .Setup(o => o.GetImage(newMainImageId))
            .ReturnsAsync(new Image { Id = newMainImageId, Item = oldItem });

        // Mock category service
        _itemCategoryService
            .Setup(o => o.GetByIds(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<ItemCategory> { new() { Id = 1 }, new() { Id = 2 } });

        // Mock tag service
        _itemTagService
            .Setup(o => o.GetOrCreate(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<ItemTag> { new() { Name = "Tag" }, new() { Name = "Tag2" } });

        // act
        await _itemFacade.UpdateItem(oldItem, request);

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
            RefundableDeposit = 2000,
            MainImageId = null
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

        // Asserations - status is the same
        Assert.That(oldItem.Status, Is.EqualTo(ItemStatus.Denied));
    }

    #endregion

    #region DeleteItem

    [Test]
    public void DeleteItem_ThereAreRunningLoans_ThrowsException()
    {
        // Arrange
        _loanService
            .Setup(o => o.GetRunningLoansCountByItem(_item))
            .ReturnsAsync(1 /* there is one running loan */);

        // Must throw ItemHasRunningLoansException because there are running loans
        Assert.ThrowsAsync<OperationNotAllowedException>(async () => await _itemFacade.DeleteItem(_item));

        // Verify that GetRunningLoansForItem was called
        _loanService.Verify(o => o.GetRunningLoansCountByItem(_item), Times.Once);

        // Verify that Delete was not called
        _itemService.Verify(o => o.Delete(_item), Times.Never);
    }

    [Test]
    public async Task DeleteItem_ThereAreNoRunningLoans_DeletesItem()
    {
        // Arrange
        _loanService
            .Setup(o => o.GetRunningLoansCountByItem(_item))
            .ReturnsAsync(0 /* there are no running loans */);

        await _itemFacade.DeleteItem(_item);

        // Verify that GetRunningLoansForItem was called
        _loanService.Verify(o => o.GetRunningLoansCountByItem(_item), Times.Once);

        // Verify that Delete was called
        _itemService.Verify(o => o.Delete(_item), Times.Once);
    }

    #endregion

    #region GetAll

    [Test]
    public async Task GetAll_UserAuthenticatedButNotTheFilteredOwner_ReturnsSelectedOwnersPublicItems()
    {
        // Arrange
        var filter = new ItemFilter()
        {
            CategoryId = 1, Page = 2, PageSize = 10, Status = ItemStatus.Approving, Search = "Search", Sortby = "Id",
            SortOrder = true, OwnerId = _owner.Id
        };
        var expectedFilter = new ItemFilter()
        {
            CategoryId = 1, Page = 2, PageSize = 10,
            Status = ItemStatus.Approving,
            Search = "Search", Sortby = "Id",
            SortOrder = true,
            OwnerId = _owner.Id
        };

        var expectedItems = new PaginatedList<Item>(new List<Item>()
        {
            new Item() { Id = 2 },
            new Item() { Id = 3 }
        }, 2, expectedFilter.Page, expectedFilter.PageSize);

        // User is authenticated
        _authenticateService
            .Setup(o => o.TryGetCurrentUserId())
            .Returns(_user.Id);

        // Mock item service
        _itemService
            // The GetAll method must be called with the expected filter
            .Setup(o => o.GetAllPublic(filter))
            .ReturnsAsync(expectedItems);

        // Must return the created item
        var result = await _itemFacade.GetAll(filter);

        // assert filter
        AssertItemFilter(expectedFilter, filter);

        // assert
        Assert.That(result.Count, Is.EqualTo(expectedItems.Count));
        Assert.That(result.PageIndex, Is.EqualTo(expectedItems.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(expectedItems.TotalPages));

        // assert items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(expectedItems[i].Id));
        }


        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.TryGetCurrentUserId(), Times.Once);

        // Verify that GetAllPublic was called because the user filters someone else's items
        _itemService.Verify(o => o.GetAllPublic(filter), Times.Once);

        // Verify that GetAll was not called because that would return private items of filtered user too
        _itemService.Verify(o => o.GetAll(filter), Times.Never);
    }

    [Test]
    public async Task GetAll_UserAuthenticatedAndIsTheFilteredOwner_ReturnsHisItems()
    {
        // Arrange
        var filter = new ItemFilter()
        {
            CategoryId = 1, Page = 2, PageSize = 10, Status = ItemStatus.Approving, Search = "Search", Sortby = "Id",
            SortOrder = true, OwnerId = _owner.Id
        };
        var expectedFilter = new ItemFilter()
        {
            CategoryId = 1, Page = 2, PageSize = 10,
            Status = ItemStatus.Approving,
            Search = "Search", Sortby = "Id",
            SortOrder = true,
            OwnerId = _owner.Id
        };

        var expectedItems = new PaginatedList<Item>(new List<Item>()
        {
            new Item() { Id = 2 },
            new Item() { Id = 3 }
        }, 2, expectedFilter.Page, expectedFilter.PageSize);

        // User is authenticated
        _authenticateService
            .Setup(o => o.TryGetCurrentUserId())
            .Returns(_owner.Id);

        // Mock item service
        _itemService
            // The GetAll method must be called with the expected filter
            .Setup(o => o.GetAll(filter))
            .ReturnsAsync(expectedItems);

        // Must return the created item
        var result = await _itemFacade.GetAll(filter);

        // assert filter
        AssertItemFilter(expectedFilter, filter);

        // assert
        Assert.That(result.Count, Is.EqualTo(expectedItems.Count));
        Assert.That(result.PageIndex, Is.EqualTo(expectedItems.PageIndex));
        Assert.That(result.TotalPages, Is.EqualTo(expectedItems.TotalPages));

        // assert items
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].Id, Is.EqualTo(expectedItems[i].Id));
        }


        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.TryGetCurrentUserId(), Times.Once);

        // Verify that GetAll was called
        _itemService.Verify(o => o.GetAll(filter), Times.Once);

        // Verify that GetAllPublic was not called because the user filters his own items
        _itemService.Verify(o => o.GetAllPublic(filter), Times.Never);
    }

    #endregion

    #region GetItem

    [Test]
    public async Task GetItem_ItemDoesNotExist_ThrowsException()
    {
        // Arrange
        var id = 1;
        _itemService
            .Setup(o => o.Get(id, true))
            .ThrowsAsync(new EntityNotFoundException());

        // Must throw ItemNotFoundException because the item does not exist
        Assert.ThrowsAsync<EntityNotFoundException>(async () => await _itemFacade.GetItem(id));

        // Verify that GetById was called
        _itemService.Verify(o => o.Get(id, true), Times.Once);
    }

    [Test]
    public async Task GetItem_ItemExists_ReturnsItem()
    {
        // Arrange
        var id = 1;
        _item.Id = id;

        _itemService
            .Setup(o => o.Get(id, true))
            .ReturnsAsync(_item);

        // Must return the created item
        var result = await _itemFacade.GetItem(id);

        // assert
        AssertItem(_item, result);

        // Verify that GetById was called
        _itemService.Verify(o => o.Get(id, true), Times.Once);
    }

    #endregion

    #region AddImage

    [Test]
    public async Task AddImage_ItemHasMaxImagesPerItem_ThrowsException()
    {
        // Arrange
        var item = new Item();
        // Add 5 images to the item
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());

        // new image
        var image = new Image();
        var path = "somePath/to/image.jpg";

        // Mock configuration - maximum is 5 images per item
        _configuration
            .Setup(o => o.MaxImagesPerItem)
            .Returns(5);

        // Must throw ArgumentException because the item has reached the maximum number of images
        Assert.ThrowsAsync<ArgumentException>(async () => await _itemFacade.AddImage(item, image, path));

        // Verify that the item was not updated
        _itemService.Verify(s => s.Update(item), Times.Never);

        // Verify that AddImage was not called
        _imageFacade.Verify(o => o.CreateImage(image, path), Times.Never);
    }

    [Test]
    public async Task AddImage_ItemHasAlreadyExceededMaxImagesPerItem_ThrowsException()
    {
        // Arrange
        var item = new Item();
        // Add 6 images to the item which is already more than the maximum
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());

        // new image
        var image = new Image();
        var path = "somePath/to/image.jpg";

        // Mock configuration - maximum is 5 images per item
        _configuration
            .Setup(o => o.MaxImagesPerItem)
            .Returns(5);

        // Must throw ArgumentException because the item has reached the maximum number of images
        Assert.ThrowsAsync<ArgumentException>(async () => await _itemFacade.AddImage(item, image, path));

        // Verify that the item was not updated
        _itemService.Verify(s => s.Update(item), Times.Never);

        // Verify that AddImage was not called
        _imageFacade.Verify(o => o.CreateImage(image, path), Times.Never);
    }

    [Test]
    public async Task AddImage_ItemHasNotReachedMaxImagesPerItem_AddsImage()
    {
        // Arrange
        var item = new Item();

        // Add 4 images to the item
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());
        item.Images.Add(new Image());

        var newImage = new Image();
        var newImagePath = "some/path/to/image.jpg";

        // Mock configuration - maximum is 5 images per item
        _configuration
            .Setup(o => o.MaxImagesPerItem)
            .Returns(5);

        // Must return the created item
        await _itemFacade.AddImage(item, newImage, newImagePath);

        // Verify that the item was updated
        _itemService.Verify(s => s.Update(item), Times.Once);

        // Verify that AddImage was called
        _imageFacade.Verify(o => o.CreateImage(newImage, newImagePath), Times.Once);
    }

    #endregion

    #region DeleteImage

    [Test]
    public void DeleteImage_ImageDoesNotBelongToAnItem_ThrowsException()
    {
        // Arrange
        var image = new Image { Id = 1, Item = null };

        // Must throw ArgumentException because the item has reached the maximum number of images
        Assert.ThrowsAsync<OperationNotAllowedException>(async () => await _itemFacade.DeleteImage(image));

        // Verify that the item was not updated
        _itemService.Verify(s => s.Update(It.IsAny<Item>()), Times.Never);

        // Verify that DeleteImage was not called
        _imageFacade.Verify(o => o.DeleteImage(image), Times.Never);
    }

    [Test]
    public async Task DeleteImage_ImageBelongsToAnItem_Success()
    {
        // Arrange
        var image = new Image { Id = 1, Item = _item };

        // Act - delete the image
        await _itemFacade.DeleteImage(image);

        // Verify that the item was updated
        _itemService.Verify(s => s.Update(_item), Times.Once);

        // Verify that DeleteImage was called
        _imageFacade.Verify(o => o.DeleteImage(image), Times.Once);
    }

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
        Assert.That(actual.MainImage?.Id, Is.EqualTo(expected.MainImage?.Id));

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

    public void AssertItemFilter(ItemFilter? expected, ItemFilter? actual)
    {
        if (expected == null && actual == null) return;
        if (expected == null || actual == null) Assert.Fail("One of the filters is null");

        Assert.That(actual.CategoryId, Is.EqualTo(expected.CategoryId));
        Assert.That(actual.Page, Is.EqualTo(expected.Page));
        Assert.That(actual.PageSize, Is.EqualTo(expected.PageSize));
        Assert.That(actual.Status, Is.EqualTo(expected.Status));
        Assert.That(actual.Search, Is.EqualTo(expected.Search));
        Assert.That(actual.Sortby, Is.EqualTo(expected.Sortby));
        Assert.That(actual.SortOrder, Is.EqualTo(expected.SortOrder));
        Assert.That(actual.OwnerId, Is.EqualTo(expected.OwnerId));
    }
}