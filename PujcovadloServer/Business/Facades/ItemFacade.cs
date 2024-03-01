using AutoMapper;
using NuGet.Packaging;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Helpers;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ItemFacade
{
    private readonly ImageFacade _imageFacade;
    private readonly ItemService _itemService;
    private readonly LoanService _loanService;
    private readonly ItemCategoryService _itemCategoryService;
    private readonly ItemTagService _itemTagService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ItemFacade(ImageFacade imageFacade, ItemService itemService, LoanService loanService,
        ItemCategoryService itemCategoryService,
        ItemTagService itemTagService, IAuthenticateService authenticateService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _imageFacade = imageFacade;
        _itemService = itemService;
        _loanService = loanService;
        _itemCategoryService = itemCategoryService;
        _itemTagService = itemTagService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Returns all items by given filter.
    /// </summary>
    /// <param name="filter">Filter data</param>
    /// <returns>Paginated and filtered items</returns>
    public Task<PaginatedList<Item>> GetAll(ItemFilter filter)
    {
        var userId = _authenticateService.TryGetCurrentUserId();

        // If user is not authenticated and filters only is items
        if (userId != null && filter.OwnerId == userId)
            return _itemService.GetAll(filter);

        // User not or not filtering his own items
        return _itemService.GetAllPublic(filter);
    }

    /// <summary>
    /// Fill request data to the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="request">Item request data.</param>
    private async Task FillItemRequest(Item item, ItemRequest request)
    {
        // Map request to item
        item.Name = request.Name;
        item.Alias = UrlHelper.CreateUrlStub(item.Name);
        item.Description = request.Description;
        item.Parameters = request.Parameters;

        // update prices
        item.PricePerDay = request.PricePerDay.Value;
        item.RefundableDeposit = request.RefundableDeposit;
        item.PurchasePrice = request.PurchasePrice;
        item.SellingPrice = request.SellingPrice;

        // New categories
        var categories = await _itemCategoryService.GetByIds(request.Categories);

        // Update categories
        item.Categories.Clear();
        item.Categories.AddRange(categories);

        // new tags
        var tags = await _itemTagService.GetOrCreate(request.Tags);

        // Update tags
        item.Tags.Clear();
        item.Tags.AddRange(tags);
    }

    /// <summary>
    /// Creates a new item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="request"></param>
    public async Task<Item> CreateItem(ItemRequest request)
    {
        var user = await _authenticateService.GetCurrentUser();

        // Fill request data
        var item = new Item();
        await FillItemRequest(item, request);

        // Set initial status
        item.Status = ItemStatus.Public;

        // Set owner
        item.Owner = user;

        // Create the item  
        await _itemService.Create(item);

        return item;
    }

    /// <summary>
    /// Updates item using <see cref="ItemRequest"/>
    /// </summary>
    /// <param name="item">Item to be updated.</param>
    /// <param name="request">Request with new data.</param>
    public async Task UpdateItem(Item item, ItemRequest request)
    {
        await FillItemRequest(item, request);

        await UpdateMainImage(item, request);

        // Update the item
        await _itemService.Update(item);
    }

    private async Task UpdateMainImage(Item item, ItemRequest request)
    {
        // Main image is defined
        if (request.MainImageId != null)
        {
            // Get the image by id using the image facade
            var mainImage = await _imageFacade.GetImageOrNull(request.MainImageId.Value);

            // Throw exception if the image does not exist
            if (mainImage == null)
                throw new ArgumentException("Main image id is invalid.");

            // Throw exception if the main image does not belong to the item
            if (mainImage.Item == null || mainImage.Item.Id != item.Id)
            {
                throw new ArgumentException("Main image does not belong to the item");
            }

            // Set the main image
            item.MainImage = mainImage;
        }
        // Main image is not defined
        else
        {
            item.MainImage = null;
            item.MainImageId = null;
        }
    }

    /// <summary>
    /// Deletes the item if it is allowed.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="OperationNotAllowedException"></exception>
    public async Task DeleteItem(Item item)
    {
        // Check if the item can be deleted
        if (!await CanDelete(item))
            throw new OperationNotAllowedException("Item cannot be deleted because there are running loans.");

        // Delete the item
        await _itemService.Delete(item);
    }

    /// <summary>
    /// Can the item be deleted?
    /// </summary>
    /// <param name="item">Item to be deleted</param>
    /// <returns>True - if the item can be deleted</returns>
    public async Task<bool> CanDelete(Item item)
    {
        // Check if the item has any running loans
        var runningLoans = await _loanService.GetRunningLoansCountByItem(item);

        // Return true if there are no running loans
        return runningLoans == 0;
    }

    /// <summary>
    /// Returns item with given id.
    /// </summary>
    /// <param name="id">Item's id</param>
    /// <returns>Item</returns>
    /// <exception cref="EntityNotFoundException">Thrown when item id is invalid.</exception>
    public async Task<Item> GetItem(int id)
    {
        var item = await _itemService.Get(id);

        // Check if item exists
        if (item == null) throw new EntityNotFoundException($"Item with id {id} not found.");

        // Return item
        return item;
    }

    /// <summary>
    /// Adds a new image to the item
    /// </summary>
    /// <param name="item">Item</param>
    /// <param name="image">New image to be added.</param>
    /// <param name="filePath">Filepath to the image file.</param>
    /// <exception cref="ArgumentException">Thrown when maximum amount of images per item was exceeded.</exception>
    public async Task AddImage(Item item, Image image, string filePath)
    {
        // Check that the item has not reached the maximum number of images
        if (item.Images.Count >= _configuration.MaxImagesPerItem)
            throw new ArgumentException("Max images per item exceeded.");

        // set image item
        image.Item = item;

        // Create using image facade
        await _imageFacade.CreateImage(image, filePath);

        // Update item
        await _itemService.Update(item);
    }

    /// <summary>
    /// Returns image of the item.
    /// </summary>
    /// <param name="itemId">Item Id</param>
    /// <param name="imageId">Image id</param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException">Thrown when image with given imageId and itemId does not exist.</exception>
    public async Task<Image> GetImage(int itemId, int imageId)
    {
        // Get the image by id using the image facade
        var image = await _imageFacade.GetImage(imageId);

        // Check that the image belongs to the item
        if (image.Item == null || image.Item.Id != itemId)
        {
            throw new EntityNotFoundException("Image not found");
        }

        return image;
    }

    /// <summary>
    /// Deletes an image which belongs to an item.
    /// </summary>
    /// <param name="image">The image to be deleted.</param>
    /// <returns></returns>
    /// <exception cref="OperationNotAllowedException">thrown when the image does not belong to an item.</exception>
    public async Task DeleteImage(Image image)
    {
        // Check that the image belongs to an item
        if (image.Item == null)
        {
            throw new OperationNotAllowedException("Image does not belong to any item");
        }

        // Make main image null if the image is the main image
        if (image.Item.MainImageId == image.Id)
            image.Item.MainImage = null;

        // Delete the image
        await _imageFacade.DeleteImage(image);

        // Update item
        await _itemService.Update(image.Item);
    }
}