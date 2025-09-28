using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Modelo.Dto;
using Restaurante.Aplicacion.Services; // IReservaService
using System.Net.Mime;

namespace Restaurante.Api.Controllers
{
    /// <summary>
    /// Controller for managing reservations (Reservas).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(IReservaService reservaService, ILogger<ReservasController> logger)
        {
            _reservaService = reservaService ?? throw new ArgumentNullException(nameof(reservaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a paginated list of all reservations with filtering (e.g., by date, estado) and sorting.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Items per page.</param>
        /// <param name="filter">Filter by estado or fecha (e.g., "Pendiente" or "2023-01-01").</param>
        /// <param name="sortBy">Sort by field (e.g., "FechaInicio desc").</param>
        /// <returns>Paginated list of ReservaDto.</returns>
        /// <response code="200">Returns the list.</response>
        /// <response code="400">Invalid parameters.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReservaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "filter", "sortBy" })]
        public async Task<IActionResult> GetAllReservas(
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
                var reservas = await _reservaService.GetAllAsync(page, pageSize, filter, sortBy);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservas.");
                return StatusCode(500, "An error occurred while retrieving reservations.");
            }
        }

        /// <summary>
        /// Retrieves a specific reservation by ID, including related mesa and cliente.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        /// <returns>The ReservaDto.</returns>
        /// <response code="200">Returns the reservation.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetReservaById(Guid id)
        {
            var reserva = await _reservaService.GetByIdAsync(id);
            if (reserva == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            return Ok(reserva);
        }

        /// <summary>
        /// Creates a new reservation, checking for availability.
        /// </summary>
        /// <param name="createDto">Reservation details.</param>
        /// <returns>Created ReservaDto.</returns>
        /// <response code="201">Created successfully.</response>
        /// <response code="400">Invalid input or conflict (e.g., table not available).</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReserva([FromBody] CreateReservaDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdReserva = await _reservaService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetReservaById), new { id = createdReserva.Id }, createdReserva);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // e.g., "Table not available"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reserva.");
                return StatusCode(500, "An error occurred while creating the reservation.");
            }
        }

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
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
        public async Task<IActionResult> UpdateReserva(Guid id, [FromBody] UpdateReservaDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingReserva = await _reservaService.GetByIdAsync(id);
            if (existingReserva == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            try
            {
                await _reservaService.UpdateAsync(id, updateDto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reserva.");
                return StatusCode(500, "An error occurred while updating the reservation.");
            }
        }

        /// <summary>
        /// Partially updates a reservation using JSON Patch.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
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
        [Consumes("application/json-patch+json")]
        public async Task<IActionResult> PatchReserva(Guid id, [FromBody] JsonPatchDocument<UpdateReservaDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest("Invalid patch document.");
            }

            var existingReserva = await _reservaService.GetByIdAsync(id);
            if (existingReserva == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            try
            {
                await _reservaService.PatchAsync(id, patchDocument);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching reserva.");
                return StatusCode(500, "An error occurred while patching the reservation.");
            }
        }

        /// <summary>
        /// Deletes a reservation by ID.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="404">Not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReserva(Guid id)
        {
            var existingReserva = await _reservaService.GetByIdAsync(id);
            if (existingReserva == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            try
            {
                await _reservaService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reserva.");
                return StatusCode(500, "An error occurred while deleting the reservation.");
            }
        }
    }
}