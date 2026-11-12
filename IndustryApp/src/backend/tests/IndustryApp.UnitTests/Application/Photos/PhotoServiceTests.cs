using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Reports.Photos;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Common;
using IndustryApp.UnitTests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Photos;

/// <summary>FR-02.1 ve UC-02 2a: imza ve boyut kontrolü, geçici anahtar, yetkili okuma.</summary>
public sealed class PhotoServiceTests
{
    private readonly IImageProcessor _processor = Substitute.For<IImageProcessor>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IAccidentReportRepository _reports = Substitute.For<IAccidentReportRepository>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly PhotoService _service;

    public PhotoServiceTests()
    {
        _user.UserId.Returns(_userId);
        _service = new PhotoService(_processor, _storage, _reports, _user,
            Options.Create(new PhotosOptions { MaxFileSizeMb = 1, MaxWidthPx = 1920, JpegQuality = 80, MaxCountPerReport = 8 }),
            NullLogger<PhotoService>.Instance);
    }

    // TC-02.1.3
    [Fact]
    public async Task UploadPhoto_WrongSignature_ReturnsUnsupportedImage()
    {
        // Uzantı ve Content-Type JPEG diyor ama içerik bir PDF.
        var result = await _service.UploadTemporaryAsync(Upload("%PDF-1.7 sahte fotoğraf"u8.ToArray(), "kaza.jpg"), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.UnsupportedImage);
        await _processor.DidNotReceiveWithAnyArgs().ProcessAsync(default!, default);
        await _storage.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task UploadPhoto_LargerThanLimit_ReturnsFileTooLarge()
    {
        var bytes = new byte[1024 * 1024 + 1];
        bytes[0] = 0xFF;
        bytes[1] = 0xD8;
        bytes[2] = 0xFF;

        var result = await _service.UploadTemporaryAsync(Upload(bytes, "buyuk.jpg", declaredLength: 10), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.FileTooLarge);
    }

    [Fact]
    public async Task UploadPhoto_Valid_StoresJpegUnderUsersTemporaryFolder()
    {
        var processed = Substitute.For<IProcessedImage>();
        processed.Width.Returns(1920);
        processed.Height.Returns(1080);
        processed.EncodeJpegAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Stream>().WriteAsync(new byte[500]).AsTask());
        _processor.ProcessAsync(Arg.Any<IImageInput>(), Arg.Any<CancellationToken>()).Returns(processed);

        var result = await _service.UploadTemporaryAsync(Upload(TestImages.Png(10, 10), "on.png"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TempKey.Should().StartWith($"temp/{_userId}/").And.EndWith(".jpg");
        PhotoKeys.IsTemporaryOf(result.Value.TempKey, _userId).Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new { Width = 1920, Height = 1080, SizeBytes = 500L });
        await _storage.Received(1).SaveAsync(Arg.Any<Stream>(), result.Value.TempKey, "image/jpeg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadPhoto_UndecodableContent_ReturnsUnsupportedImage()
    {
        _processor.ProcessAsync(Arg.Any<IImageInput>(), Arg.Any<CancellationToken>())
            .Returns<IProcessedImage>(_ => throw new UnsupportedImageException("bozuk"));

        var result = await _service.UploadTemporaryAsync(Upload([0xFF, 0xD8, 0xFF, 0x00], "bozuk.jpg"), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.UnsupportedImage);
    }

    [Fact]
    public async Task OpenPhoto_UnmatchedMechanic_ReturnsNotMatched()
    {
        var photoId = Guid.NewGuid();
        _reports.GetPhotoAccessAsync(photoId, Arg.Any<CancellationToken>())
            .Returns(new PhotoAccessInfo(photoId, "reports/x.jpg", "image/jpeg", Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()]));
        _user.IsInRole(RoleNames.Mechanic).Returns(true);

        var result = await _service.OpenAsync(photoId, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotMatched);
        await _storage.DidNotReceiveWithAnyArgs().OpenReadAsync(default!, default);
    }

    [Fact]
    public async Task OpenPhoto_MatchedMechanic_GetsStream()
    {
        var photoId = Guid.NewGuid();
        _reports.GetPhotoAccessAsync(photoId, Arg.Any<CancellationToken>())
            .Returns(new PhotoAccessInfo(photoId, "reports/x.jpg", "image/jpeg", Guid.NewGuid(), Guid.NewGuid(), [_userId]));
        _storage.OpenReadAsync("reports/x.jpg", Arg.Any<CancellationToken>()).Returns(new MemoryStream([1, 2]));

        var result = await _service.OpenAsync(photoId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Theory]
    [InlineData("temp/{user}/0f8fad5b-d9cb-469f-a165-70867728950e.jpg", true)]
    [InlineData("temp/{other}/0f8fad5b-d9cb-469f-a165-70867728950e.jpg", false)]
    [InlineData("temp/{user}/../../reports/x.jpg", false)]
    [InlineData("reports/2026/11/{user}/0f8fad5b-d9cb-469f-a165-70867728950e.jpg", false)]
    public void PhotoKeys_IsTemporaryOf_AcceptsOnlyOwnTemporaryKeys(string pattern, bool expected)
    {
        var key = pattern.Replace("{user}", _userId.ToString()).Replace("{other}", Guid.NewGuid().ToString());

        PhotoKeys.IsTemporaryOf(key, _userId).Should().Be(expected);
    }

    private static IUploadPhotoRequest Upload(byte[] bytes, string fileName, long? declaredLength = null)
    {
        var request = Substitute.For<IUploadPhotoRequest>();
        request.Content.Returns(new MemoryStream(bytes));
        request.FileName.Returns(fileName);
        request.ContentType.Returns("image/jpeg");
        request.Length.Returns(declaredLength ?? bytes.Length);
        return request;
    }
}
