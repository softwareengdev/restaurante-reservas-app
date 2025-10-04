using AutoMapper;
using Restaurante.Infraestructura.Entities;
using Restaurante.Modelo.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// RestauranteProfile.cs
// Place this in a Profiles folder within Restaurante.Api or a shared project like Restaurante.Aplicacion
// This class defines all mappings between Entities and DTOs using AutoMapper
// Ensure AutoMapper is registered in Program.cs: builder.Services.AddAutoMapper(typeof(RestauranteProfile));
namespace Restaurante.Aplicacion.Profiles // Adjust namespace as per your project structure
{
    public class RestauranteProfile : Profile
    {
        public RestauranteProfile()
        {
            // Mesa mappings
            // Entity to DTO
            CreateMap<Mesa, MesaDto>()
                .ForMember(dest => dest.Reservas, opt => opt.MapFrom(src => src.Reservas)); // Map navigation if included

            // DTO to Entity (for Create/Update)
            CreateMap<CreateMesaDto, Mesa>();
            CreateMap<UpdateMesaDto, Mesa>();

            // For patching or partial updates, map back to DTO if needed
            CreateMap<Mesa, UpdateMesaDto>();

            // Reserva mappings
            CreateMap<Reserva, ReservaDto>()
                .ForMember(dest => dest.Mesa, opt => opt.MapFrom(src => src.Mesa))
                .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente));

            CreateMap<CreateReservaDto, Reserva>();
            CreateMap<UpdateReservaDto, Reserva>();
            CreateMap<Reserva, UpdateReservaDto>();

            // Cliente mappings
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.Reservas, opt => opt.MapFrom(src => src.Reservas));

            CreateMap<CreateClienteDto, Cliente>();
            CreateMap<UpdateClienteDto, Cliente>();
            CreateMap<Cliente, UpdateClienteDto>();

            // Additional mappings if needed (e.g., for nested objects)
            // Reverse mappings are automatic if needed, but explicit for clarity
            CreateMap<MesaDto, Mesa>().ReverseMap();
            CreateMap<ReservaDto, Reserva>().ReverseMap();
            CreateMap<ClienteDto, Cliente>().ReverseMap();

            // Handle TimeSpan or any custom conversions if required
            // For example, if Duration needs special handling (though TimeSpan maps directly)
        }
    }
}