using AutoMapper;
using Restaurante.Aplicacion.Services;
using Restaurante.Infraestructura.Entities;
using Restaurante.Infraestructura.Repository;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq.Expressions;

namespace Restaurante.Tests.Aplicación
{
    public class ReservaServiceTests
    {
        private readonly Mock<IReservaRepository> _reservaRepoMock;
        private readonly Mock<IMesaRepository> _mesaRepoMock;
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ReservaService _reservaService;

        public ReservaServiceTests()
        {
            _reservaRepoMock = new Mock<IReservaRepository>();
            _mesaRepoMock = new Mock<IMesaRepository>();
            _clienteRepoMock = new Mock<IClienteRepository>();
            _mapperMock = new Mock<IMapper>();
            _reservaService = new ReservaService(_reservaRepoMock.Object, _mesaRepoMock.Object, _clienteRepoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ReservationEndsExactlyWhenAnotherBegins_ShouldAllow()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var existingReserva = new Reserva
            {
                Id = Guid.NewGuid(),
                MesaId = mesaId,
                FechaInicio = DateTime.UtcNow.AddHours(1),
                Duracion = TimeSpan.FromHours(1),
                Estado = "Pendiente"
            };

            var newDto = new CreateReservaDto
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                FechaInicio = existingReserva.FechaInicio + existingReserva.Duracion, // Starts exactly when existing ends
                Duracion = TimeSpan.FromHours(1),
                NumeroPersonas = 4
            };

            var mesa = new Mesa { Id = mesaId, Capacidad = 5 };
            var cliente = new Cliente { Id = clienteId };

            _mesaRepoMock.Setup(r => r.GetByIdAsync(mesaId, null)).ReturnsAsync(mesa);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(clienteId, null)).ReturnsAsync(cliente);
            _reservaRepoMock.Setup(r => r.HasConflictAsync(mesaId, newDto.FechaInicio, newDto.FechaInicio + newDto.Duracion, null)).ReturnsAsync(false); // No conflict

            var newReserva = new Reserva { Id = Guid.NewGuid() };
            _mapperMock.Setup(m => m.Map<Reserva>(newDto)).Returns(newReserva);
            _mapperMock.Setup(m => m.Map<ReservaDto>(newReserva)).Returns(new ReservaDto { Id = newReserva.Id });

            // Act
            var result = await _reservaService.CreateAsync(newDto);

            // Assert
            Assert.NotNull(result);
            _reservaRepoMock.Verify(r => r.AddAsync(It.IsAny<Reserva>()), Times.Once);
            _clienteRepoMock.Verify(c => c.UpdateAsync(It.IsAny<Cliente>()), Times.Once); // Points updated
        }

        [Fact]
        public async Task CreateAsync_ReservationOverlapsAnother_ShouldReject()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var existingReserva = new Reserva
            {
                Id = Guid.NewGuid(),
                MesaId = mesaId,
                FechaInicio = DateTime.UtcNow.AddHours(1),
                Duracion = TimeSpan.FromHours(2),
                Estado = "Pendiente"
            };

            var newDto = new CreateReservaDto
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                FechaInicio = existingReserva.FechaInicio.AddHours(0.5), // Overlaps
                Duracion = TimeSpan.FromHours(1),
                NumeroPersonas = 4
            };

            var mesa = new Mesa { Id = mesaId, Capacidad = 5 };
            var cliente = new Cliente { Id = clienteId };

            _mesaRepoMock.Setup(r => r.GetByIdAsync(mesaId, null)).ReturnsAsync(mesa);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(clienteId, null)).ReturnsAsync(cliente);
            _reservaRepoMock.Setup(r => r.HasConflictAsync(mesaId, newDto.FechaInicio, newDto.FechaInicio + newDto.Duracion, null)).ReturnsAsync(true); // Conflict

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _reservaService.CreateAsync(newDto));
            Assert.Equal("Time slot conflicts with an existing reservation.", ex.Message);
            _reservaRepoMock.Verify(r => r.AddAsync(It.IsAny<Reserva>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ReservationExceedsCapacity_ShouldReject()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var newDto = new CreateReservaDto
            {
                MesaId = mesaId,
                ClienteId = clienteId,
                FechaInicio = DateTime.UtcNow.AddHours(1),
                Duracion = TimeSpan.FromHours(1),
                NumeroPersonas = 6 // Exceeds capacity
            };

            var mesa = new Mesa { Id = mesaId, Capacidad = 4 };
            var cliente = new Cliente { Id = clienteId };

            _mesaRepoMock.Setup(r => r.GetByIdAsync(mesaId, null)).ReturnsAsync(mesa);
            _clienteRepoMock.Setup(r => r.GetByIdAsync(clienteId, null)).ReturnsAsync(cliente);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _reservaService.CreateAsync(newDto));
            Assert.Equal("Reservation exceeds table capacity.", ex.Message);
            _reservaRepoMock.Verify(r => r.AddAsync(It.IsAny<Reserva>()), Times.Never);
        }

        [Fact]
        public async Task GetAvailableTablesAsync_ReturnsOnlyAvailableTables_WithoutConflicts()
        {
            // Arrange
            var partySize = 4;
            var start = DateTime.UtcNow.AddHours(1);
            var duration = TimeSpan.FromHours(1);
            var end = start + duration;

            var mesa1 = new Mesa { Id = Guid.NewGuid(), Capacidad = 5, Estado = "Disponible" }; // Available
            var mesa2 = new Mesa { Id = Guid.NewGuid(), Capacidad = 3, Estado = "Disponible" }; // Too small
            var mesa3 = new Mesa { Id = Guid.NewGuid(), Capacidad = 6, Estado = "Disponible" }; // Has conflict
            var mesas = new List<Mesa> { mesa1, mesa2, mesa3 };

            _mesaRepoMock.Setup(r => r.GetAllAsync(
                It.Is<Expression<Func<Mesa, bool>>>(expr => true), // Filter check in service
                It.IsAny<Func<IQueryable<Mesa>, IOrderedQueryable<Mesa>>>(),
                null, null, null))
                .ReturnsAsync(mesas);

            _reservaRepoMock.Setup(r => r.HasConflictAsync(mesa1.Id, start, end, null)).ReturnsAsync(false);
            _reservaRepoMock.Setup(r => r.HasConflictAsync(mesa2.Id, start, end, null)).ReturnsAsync(false);
            _reservaRepoMock.Setup(r => r.HasConflictAsync(mesa3.Id, start, end, null)).ReturnsAsync(true);

            _mapperMock.Setup(m => m.Map<IEnumerable<MesaDto>>(It.IsAny<IEnumerable<Mesa>>()))
                .Returns(new List<MesaDto> { new MesaDto { Id = mesa1.Id } });

            // Act
            var result = await _reservaService.GetAvailableTablesAsync(partySize, start, duration);

            // Assert
            Assert.Single(result); // Only mesa1
            Assert.Equal(mesa1.Id, result.First().Id);
        }
    }
}