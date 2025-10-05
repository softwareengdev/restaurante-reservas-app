// MesasController.cs in Restaurante.Api.Controllers
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
    /// Controller for managing restaurant tables (Mesas).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require JWT authentication for all endpoints; adjust per need
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class MesasController : ControllerBase
    {
        private readonly IMesaService _mesaService;
        private readonly ILogger<MesasController> _logger;

        public MesasController(IMesaService mesaService, ILogger<MesasController> logger)
        {
            _mesaService = mesaService ?? throw new ArgumentNullException(nameof(mesaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a paginated list of all tables with optional filtering and sorting.
        /// </summary>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="filter">Filter by estado (e.g., "Disponible").</param>
        /// <param name="sortBy">Sort by field (e.g., "Capacidad asc").</param>
        /// <returns>A paginated list of MesaDto.</returns>
        /// <response code="200">Returns the list of tables.</response>
        /// <response code="400">Invalid pagination or filter parameters.</response>
        /// <response code="401">Unauthorized access.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MesaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "filter", "sortBy" })]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> GetAllMesas(
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
                var mesas = await _mesaService.GetAllAsync(page, pageSize, filter, sortBy);
                return Ok(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mesas.");
                return StatusCode(500, "An error occurred while retrieving tables.");
            }
        }

        /// <summary>
        /// Retrieves a specific table by ID, including related reservations if requested.
        /// </summary>
        /// <param name="id">The unique ID of the table.</param>
        /// <param name="includeReservas">Whether to include related reservations.</param>
        /// <returns>The requested MesaDto.</returns>
        /// <response code="200">Returns the table details.</response>
        /// <response code="404">Table not found.</response>
        /// <response code="401">Unauthorized access.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MesaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "includeReservas" })]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> GetMesaById(Guid id, [FromQuery] bool includeReservas = false)
        {
            var mesa = await _mesaService.GetByIdAsync(id, includeReservas);
            if (mesa == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }
            return Ok(mesa);
        }

        /// <summary>
        /// Creates a new table.
        /// </summary>
        /// <param name="createDto">The details of the table to create.</param>
        /// <returns>The created MesaDto with location header.</returns>
        /// <response code="201">Table created successfully.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="401">Unauthorized access.</response>
        /// <response code="403">Forbidden (e.g., non-admin).</response>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Restrict to admins
        [ProducesResponseType(typeof(MesaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> CreateMesa([FromBody] CreateMesaDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdMesa = await _mesaService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetMesaById), new { id = createdMesa.Id }, createdMesa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mesa.");
                return StatusCode(500, "An error occurred while creating the table.");
            }
        }

        /// <summary>
        /// Updates an existing table.
        /// </summary>
        /// <param name="id">The ID of the table to update.</param>
        /// <param name="updateDto">The updated details.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Table updated successfully.</response>
        /// <response code="400">Invalid input or ID mismatch.</response>
        /// <response code="404">Table not found.</response>
        /// <response code="401">Unauthorized access.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> UpdateMesa(Guid id, [FromBody] UpdateMesaDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _mesaService.UpdateAsync(id, updateDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Table with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mesa.");
                return StatusCode(500, "An error occurred while updating the table.");
            }
        }

        /// <summary>
        /// Partially updates a table using JSON Patch.
        /// </summary>
        /// <param name="id">The ID of the table to patch.</param>
        /// <param name="patchDocument">The JSON Patch document.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Table patched successfully.</response>
        /// <response code="400">Invalid patch document.</response>
        /// <response code="404">Table not found.</response>
        /// <response code="401">Unauthorized access.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPatch("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Consumes("application/json-patch+json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> PatchMesa(Guid id, [FromBody] JsonPatchDocument<UpdateMesaDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest("Invalid patch document.");
            }

            try
            {
                await _mesaService.PatchAsync(id, patchDocument);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Table with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching mesa.");
                return StatusCode(500, "An error occurred while patching the table.");
            }
        }

        /// <summary>
        /// Deletes a table by ID.
        /// </summary>
        /// <param name="id">The ID of the table to delete.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Table deleted successfully.</response>
        /// <response code="404">Table not found.</response>
        /// <response code="401">Unauthorized access.</response>
        /// <response code="403">Forbidden.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "UserOrAdmin")]
        public async Task<IActionResult> DeleteMesa(Guid id)
        {
            try
            {
                await _mesaService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Table with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mesa.");
                return StatusCode(500, "An error occurred while deleting the table.");
            }
        }
    }
}