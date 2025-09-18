using Microsoft.AspNetCore.Http;
using Moq;
using SearchBugs.Application.BugTracking.Attachments;
using SearchBugs.Application.Common.Interfaces;
using SearchBugs.Domain;
using SearchBugs.Domain.Bugs;
using SearchBugs.Domain.Projects;
using SearchBugs.Domain.Users;
using Shared.Errors;
using Shared.Results;

namespace SearchBugs.Application.UnitTests.BugTrackingTest;

public class AddAttachmentCommandHandlerTest
{
    private readonly Mock<IBugRepository> _bugRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly AddAttachmentCommandHandler _sut;

    public AddAttachmentCommandHandlerTest()
    {
        _bugRepository = new();
        _currentUserService = new();
        _unitOfWork = new();
        _sut = new AddAttachmentCommandHandler(_bugRepository.Object, _currentUserService.Object, _unitOfWork.Object);
    }

    private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var mockFile = new Mock<IFormFile>();
        var stream = new MemoryStream(content);

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        return mockFile;
    }

    [Fact]
    public async Task Handle_WhenBugNotFound_ShouldReturnFailure()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var fileContent = new byte[] { 1, 2, 3, 4 };
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", fileContent);
        var command = new AddAttachmentCommand(bugId, mockFile.Object);

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Bug>(new NotFoundError("Bug.NotFound", "Bug not found")));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Bug.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenBugExists_ShouldAddAttachmentSuccessfully()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileName = "screenshot.png";
        var fileContent = new byte[] { 10, 20, 30, 40, 50 };
        var contentType = "image/png";
        var mockFile = CreateMockFormFile(fileName, contentType, fileContent);
        var command = new AddAttachmentCommand(bugId, mockFile.Object);

        var bug = Bug.Create(
            "Bug with Attachment",
            "This bug needs a screenshot",
            BugStatus.Open.Id,
            BugPriority.Medium.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(fileName, result.Value.FileName);
        Assert.Equal(contentType, result.Value.ContentType);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddingAttachment_ShouldCallAllRequiredMethods()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockFile = CreateMockFormFile("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new byte[100]);
        var command = new AddAttachmentCommand(bugId, mockFile.Object);

        var bug = Bug.Create(
            "Document Bug",
            "Needs documentation",
            BugStatus.InProgress.Id,
            BugPriority.High.Id,
            BugSeverity.High.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _bugRepository.Verify(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleAttachmentsAdded_ShouldMaintainAttachmentList()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bug = Bug.Create(
            "Multi Attachment Bug",
            "Multiple files needed",
            BugStatus.Open.Id,
            BugPriority.Low.Id,
            BugSeverity.Low.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        var mockFile1 = CreateMockFormFile("file1.txt", "text/plain", new byte[] { 1, 2, 3 });
        var mockFile2 = CreateMockFormFile("file2.jpg", "image/jpeg", new byte[] { 4, 5, 6 });
        var firstAttachment = new AddAttachmentCommand(bugId, mockFile1.Object);
        var secondAttachment = new AddAttachmentCommand(bugId, mockFile2.Object);

        // Act
        var firstResult = await _sut.Handle(firstAttachment, CancellationToken.None);
        var secondResult = await _sut.Handle(secondAttachment, CancellationToken.None);

        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(2, bug.Attachments.Count);
        Assert.Contains(bug.Attachments, a => a.FileName == "file1.txt");
        Assert.Contains(bug.Attachments, a => a.FileName == "file2.jpg");
    }

    [Fact]
    public async Task Handle_WhenAttachmentHasCorrectProperties_ShouldSetAllFields()
    {
        // Arrange
        var bugId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedFileName = "requirements.pdf";
        var expectedContent = new byte[] { 25, 50, 75, 100 };
        var expectedContentType = "application/pdf";
        var mockFile = CreateMockFormFile(expectedFileName, expectedContentType, expectedContent);
        var command = new AddAttachmentCommand(bugId, mockFile.Object);

        var bug = Bug.Create(
            "Requirements Bug",
            "Missing requirements document",
            BugStatus.Open.Id,
            BugPriority.High.Id,
            BugSeverity.Medium.Name,
            new ProjectId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid())).Value;

        _bugRepository.Setup(x => x.GetByIdAsync(new BugId(bugId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(bug));

        _currentUserService.Setup(x => x.UserId)
            .Returns(new UserId(userId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFileName, result.Value.FileName);
        Assert.Equal(expectedContentType, result.Value.ContentType);
        Assert.Equal(expectedContent.Length, result.Value.Content.Length);
        Assert.Single(bug.Attachments);

        var addedAttachment = bug.Attachments.First();
        Assert.Equal(expectedFileName, addedAttachment.FileName);
        Assert.Equal(expectedContentType, addedAttachment.ContentType);
        Assert.Equal(bug.Id, addedAttachment.BugId);
    }
}
