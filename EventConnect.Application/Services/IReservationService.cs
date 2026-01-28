using EventConnect.Domain.DTOs;

namespace EventConnect.Application.Services;

public interface IReservationService
{
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, int createdById);
    Task<IEnumerable<ReservationResponse>> GetMyReservationsAsync(int userId);
    Task<IEnumerable<ReservationResponse>> GetReservationsByEmpresaAsync(int empresaId, string? estado = null);
    Task<ReservationResponse?> GetReservationByIdAsync(int id);
    Task<bool> UpdateReservationStatusAsync(int id, UpdateReservationStatusRequest request, int userId);
    Task<bool> CancelReservationAsync(int id, string razon, int userId);
    Task<ReservationStatsDTO> GetReservationStatsAsync(int empresaId);
    Task<bool> VerificarDisponibilidadAsync(int empresaId, DateTime fechaEvento);
}
