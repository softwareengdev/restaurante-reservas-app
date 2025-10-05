// ClientesController.cs in Restaurante.Api.Controllers
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Modelo.Model;
using Restaurante.Aplicacion.Repository;
using Restaurante.Modelo.Dto;
using System.Net.Mime;

namespace Restaurante.Api.Controllers
{
    /// <summary>
    /// Controller for managing clients (Clientes).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService ?? throw new ArgumentNullException(nameof(clienteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a paginated list of all clients with filtering (e.g., by email, VIP) and sorting.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Items per page.</param>
        /// <param name="filter">Filter by email or EsVip (e.g., "true").</param>
        /// <param name="sortBy">Sort by field (e.g., "NumeroVisitas desc").</param>
        /// <returns>Paginated list of ClienteDto.</returns>
        /// <response code="200">Returns the list.</response>
        /// <response code="400">Invalid parameters.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "filter", "sortBy" })]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> GetAllClientes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = null)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Invalid pagination parameters.");
            }

            try
            {
                var clientes = await _clienteService.GetAllAsync(page, pageSize, filter, sortBy);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clientes.");
                return StatusCode(500, "An error occurred while retrieving clients.");
            }
        }

        /// <summary>
        /// Retrieves a specific client by ID, including related reservations if requested.
        /// </summary>
        /// <param name="id">Client ID.</param>
        /// <param name="includeReservas">Include reservations.</param>
        /// <returns>The ClienteDto.</returns>
        /// <response code="200">Returns the client.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "includeReservas" })]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> GetClienteById(Guid id, [FromQuery] bool includeReservas = false)
        {
            var cliente = await _clienteService.GetByIdAsync(id, includeReservas);
            if (cliente == null)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            return Ok(cliente);
        }

        /// <summary>
        /// Creates a new client.
        /// </summary>
        /// <param name="createDto">Client details.</param>
        /// <returns>Created ClienteDto.</returns>
        /// <response code="201">Created successfully.</response>
        /// <response code="400">Invalid input (e.g., duplicate email).</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCliente = await _clienteService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetClienteById), new { id = createdCliente.Id }, createdCliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // e.g., "Email already exists"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cliente.");
                return StatusCode(500, "An error occurred while creating the client.");
            }
        }

        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="id">Client ID.</param>
        /// <param name="updateDto">Updated details.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Updated successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> UpdateCliente(Guid id, [FromBody] UpdateClienteDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _clienteService.UpdateAsync(id, updateDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cliente.");
                return StatusCode(500, "An error occurred while updating the client.");
            }
        }

        /// <summary>
        /// Partially updates a client using JSON Patch.
        /// </summary>
        /// <param name="id">Client ID.</param>
        /// <param name="patchDocument">Patch document.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Patched successfully.</response>
        /// <response code="400">Invalid patch.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("application/json-patch+json")]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> PatchCliente(Guid id, [FromBody] JsonPatchDocument<UpdateClienteDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest("Invalid patch document.");
            }

            try
            {
                await _clienteService.PatchAsync(id, patchDocument);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching cliente.");
                return StatusCode(500, "An error occurred while patching the client.");
            }
        }

        /// <summary>
        /// Deletes a client by ID.
        /// </summary>
        /// <param name="id">Client ID.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")] // Restrict deletion to admins
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> DeleteCliente(Guid id)
        {
            try
            {
                await _clienteService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Client with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cliente.");
                return StatusCode(500, "An error occurred while deleting the client.");
            }
        }
    }
}