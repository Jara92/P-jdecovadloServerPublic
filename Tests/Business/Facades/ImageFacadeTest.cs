using AutoMapper;
using Moq;
using PujcovadloServer;
using PujcovadloServer.Authentication;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;

namespace Tests.Business.Facades;

public class ImageFacadeTest
{
    private ImageFacade _itemFacade;

    private Mock<ImageService> _imageService;
    private Mock<IFileStorage> _fileStorage;
    private Mock<IAuthenticateService> _authenticateService;
    private Mock<IMapper> _mapper;
    private Mock<PujcovadloServerConfiguration> _configuration;

    private ApplicationUser _user;
    private ApplicationUser _owner;

    private Image _image;
    private string _filePath;

    private string _imagesPath = "images";

    [SetUp]
    public void Setup()
    {
        _imageService = new Mock<ImageService>(null);
        _fileStorage = new Mock<IFileStorage>();
        _authenticateService = new Mock<IAuthenticateService>();
        _mapper = new Mock<IMapper>();
        _configuration = new Mock<PujcovadloServerConfiguration>(null);

        _itemFacade = new ImageFacade(_imageService.Object, _authenticateService.Object, _mapper.Object,
            _configuration.Object, _fileStorage.Object);

        _user = new ApplicationUser { Id = "1" };
        _owner = new ApplicationUser { Id = "2" };

        _image = new Image
        {
            Name = "Testovaci soubor.png",
            Extension = ".png",
            MimeType = "image/png"
        };

        _filePath = "tmp/random_name.tmp";

        // Mock configuration
        _configuration.Setup(o => o.ImagesPath).Returns(_imagesPath);
    }

    #region CreateImage

    [Test]
    public void CreateImage_UserNotAuthenticated_ThrowsException()
    {
        // User is not authenticated - must 
        _authenticateService
            .Setup(o => o.GetCurrentUser())
            .ThrowsAsync(new NotAuthenticatedException());

        // Must throw UserNotAuthenticatedException because the user is not authenticated
        Assert.ThrowsAsync<NotAuthenticatedException>(async () => await _itemFacade.CreateImage(_image, _filePath));

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Create was not called
        _imageService.Verify(o => o.Create(_image), Times.Never);

        // verify that Save was not called
        _fileStorage.Verify(o => o.Save(_filePath, _imagesPath, _image.MimeType, _image.Extension), Times.Never);
    }

    [Test]
    public async Task CreateImage_UserAuthenticated_CreatesItem()
    {
        var expectedFileName = "storage_file_name.png";

        // Builde expected item
        var expectedImage = new Image
        {
            Name = "Testovaci soubor.png",
            Extension = ".png",
            MimeType = "image/png",
            Path = expectedFileName,
            Owner = _user
        };

        // User is authenticated
        _authenticateService
            .Setup(o => o.GetCurrentUser())
            .ReturnsAsync(_user);

        // Mock storage
        _fileStorage
            .Setup(o => o.Save(_filePath, It.IsAny<string>(), _image.MimeType, _image.Extension))
            .ReturnsAsync(expectedFileName);

        // Must return the created item
        var result = await _itemFacade.CreateImage(_image, _filePath);

        // Asserations
        AssertImage(expectedImage, result);

        // Verify that GetCurrentUser was called
        _authenticateService.Verify(o => o.GetCurrentUser(), Times.Once);

        // Verify that Create was called
        _imageService.Verify(o => o.Create(result), Times.Once);

        // verify that Save was called
        _fileStorage.Verify(o => o.Save(_filePath, _imagesPath, _image.MimeType, _image.Extension), Times.Once);
    }

    #endregion

    #region DeleteImage

    [Test]
    public async Task DeleteItem_ThereAreNoRunningLoans_DeletesItem()
    {
        await _itemFacade.DeleteImage(_image);

        // Verify that Delete was called
        _imageService.Verify(o => o.Delete(_image), Times.Once);

        // Verify that storage Delete was called
        _fileStorage.Verify(o => o.Delete(_imagesPath, _image.Path), Times.Once);
    }

    #endregion

    private void AssertImage(Image expected, Image actual)
    {
        Assert.That(actual.Name, Is.EqualTo(expected.Name));
        Assert.That(actual.Extension, Is.EqualTo(expected.Extension));
        Assert.That(actual.MimeType, Is.EqualTo(expected.MimeType));
        Assert.That(actual.Path, Is.EqualTo(expected.Path));
        Assert.That(actual.Owner.Id, Is.EqualTo(expected.Owner.Id));
    }
}