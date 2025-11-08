using QAMate.Models;

namespace QAMate.Services
{
    // Business logic abstraction for Defect operations using DTOs.
    public interface IDefectService
    {
        Task<IEnumerable<DefectDto>> GetAllAsync();
        Task<DefectDto?> GetByIdAsync(int id);
        Task<DefectDto> CreateAsync(DefectDto dto);
        Task<bool> UpdateAsync(int id, DefectDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
