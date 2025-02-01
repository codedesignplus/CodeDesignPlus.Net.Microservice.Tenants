namespace CodeDesignPlus.Net.Microservice.Tenants.Rest.Controllers;

/// <summary>
/// Controller for managing the Tenants.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands and queries.</param>
/// <param name="mapper">Mapper instance for mapping between DTOs and commands/queries.</param>
[Route("api/[controller]")]
[ApiController]
public class TenantController(IMediator mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Get all Tenants.
    /// </summary>
    /// <param name="criteria">Criteria for filtering and sorting the Tenants.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of Tenants.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTenants([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTenantQuery(criteria), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a Tenant by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the Tenant.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Tenant.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenantById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(id), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new Tenant.
    /// </summary>
    /// <param name="data">Data for creating the Tenant.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP status code 204 (No Content).</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<CreateTenantCommand>(data), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Update an existing Tenant.
    /// </summary>
    /// <param name="id">The unique identifier of the Tenant.</param>
    /// <param name="data">Data for updating the Tenant.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP status code 204 (No Content).</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto data, CancellationToken cancellationToken)
    {
        data.Id = id;

        await mediator.Send(mapper.Map<UpdateTenantCommand>(data), cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete an existing Tenant.
    /// </summary>
    /// <param name="id">The unique identifier of the Tenant.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP status code 204 (No Content).</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTenantCommand(id), cancellationToken);

        return NoContent();
    }
}