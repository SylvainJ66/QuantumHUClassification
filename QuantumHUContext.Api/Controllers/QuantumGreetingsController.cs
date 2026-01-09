using System.ComponentModel.DataAnnotations;
using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllQuantumGreetings.Queries;
using ExtractHUContext.ReadSide.Domain.ReadModels;
using ExtractHUContext.WriteSide.Domain.CommandHandlers.CreateQuantumGreeting.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace QuantumHUContext.Api.Controllers;

/// <summary>
/// Manages Quantum Greetings using CQRS pattern
/// </summary>
/// <remarks>
/// This controller provides endpoints to create and retrieve quantum greetings.
/// All operations are handled through Wolverine message bus using command and query patterns.
/// </remarks>
[ApiController]
[Route("api/quantum-greetings")]
[Produces("application/json")]
public class QuantumGreetingsController(IMessageBus messageBus) : ControllerBase
{
    /// <summary>
    /// Creates a new quantum greeting
    /// </summary>
    /// <param name="request">The greeting creation request containing the message</param>
    /// <returns>The created greeting with its unique identifier</returns>
    /// <response code="200">Returns the newly created greeting ID and confirmation message</response>
    /// <response code="400">If the request is invalid or message is empty</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/quantum-greetings
    ///     {
    ///        "message": "Hello from the quantum realm!"
    ///     }
    ///
    /// The greeting will be persisted using the write-side model and will be
    /// available through the read-side query endpoints.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGreetingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGreeting([FromBody] CreateGreetingRequest request)
    {
        var command = new CreateQuantumGreetingCommand(
            Guid.NewGuid(),
            request.Message
        );

        await messageBus.InvokeAsync(command);

        return Ok(new CreateGreetingResponse(command.GreetingId, "Quantum greeting created!"));
    }

    /// <summary>
    /// Retrieves all quantum greetings
    /// </summary>
    /// <returns>A collection of all quantum greetings</returns>
    /// <response code="200">Returns the list of all greetings</response>
    /// <remarks>
    /// Sample response:
    ///
    ///     [
    ///         {
    ///             "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///             "message": "Hello from the quantum realm!",
    ///             "createdAt": "2026-01-09T10:30:00Z"
    ///         }
    ///     ]
    ///
    /// This endpoint queries the read-side model for optimal performance.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuantumGreetingReadModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllGreetings()
    {
        var query = new GetAllQuantumGreetingsQuery();
        var result = await messageBus.InvokeAsync<IEnumerable<QuantumGreetingReadModel>>(query);

        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a new quantum greeting
/// </summary>
public record CreateGreetingRequest
{
    /// <summary>
    /// The greeting message content
    /// </summary>
    /// <example>Hello from the quantum realm!</example>
    [Required(ErrorMessage = "Message is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters")]
    public required string Message { get; init; }
}

/// <summary>
/// Response model for a successfully created greeting
/// </summary>
/// <param name="Id">The unique identifier of the created greeting</param>
/// <param name="Message">Confirmation message</param>
public record CreateGreetingResponse(Guid Id, string Message);
