using System.ComponentModel.DataAnnotations;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.UploadMedicalStudy.Commands;
using ExtractHUContext.WriteSide.Domain.Ports;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Domain;

namespace QuantumHUClassification.Api.Controllers;

/// <summary>
/// Manages Medical Studies using CQRS pattern
/// </summary>
/// <remarks>
/// Note: File upload operations bypass Wolverine message bus since Stream objects
/// cannot be serialized. The handler is called directly for these operations.
/// </remarks>
[ApiController]
[Route("api/medical-studies")]
[Produces("application/json")]
public class MedicalStudiesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMedicalStudyRepository _medicalStudyRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<MedicalStudiesController> _logger;

    public MedicalStudiesController(
        IFileStorageService fileStorageService,
        IMedicalStudyRepository medicalStudyRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<MedicalStudiesController> logger)
    {
        _fileStorageService = fileStorageService;
        _medicalStudyRepository = medicalStudyRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a new medical study with a ZIP file
    /// </summary>
    /// <param name="request">The medical study upload request</param>
    /// <returns>The created study with its unique identifier</returns>
    /// <response code="200">Returns the newly created study ID and confirmation message</response>
    /// <response code="400">If the request is invalid or file is missing</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadMedicalStudyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(500_000_000)] // 500 MB limit
    public async Task<IActionResult> UploadStudy([FromForm] UploadMedicalStudyRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("File is required");

        Stream? fileStream = null;

        try
        {
            fileStream = request.File.OpenReadStream();

            var command = new UploadMedicalStudyCommand(
                Guid.NewGuid(),
                request.File.FileName,
                request.File.ContentType ?? "application/zip",
                request.File.Length,
                fileStream
            );

            // Call handler directly - streams cannot be serialized through message bus
            var result = await UploadMedicalStudyHandler.Handle(
                command,
                _fileStorageService,
                _medicalStudyRepository,
                _dateTimeProvider
            );

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new UploadMedicalStudyResponse(
                command.StudyId,
                "Medical study uploaded successfully"
            ));
        }
        finally
        {
            if (fileStream != null)
                await fileStream.DisposeAsync();
        }
    }
}

/// <summary>
/// Request model for uploading a medical study
/// </summary>
public record UploadMedicalStudyRequest
{
    /// <summary>
    /// The study ZIP file
    /// </summary>
    [Required(ErrorMessage = "File is required")]
    public required IFormFile File { get; init; }
}

/// <summary>
/// Response model for a successfully uploaded medical study
/// </summary>
/// <param name="StudyId">The unique identifier of the uploaded study</param>
/// <param name="Message">Confirmation message</param>
public record UploadMedicalStudyResponse(Guid StudyId, string Message);
