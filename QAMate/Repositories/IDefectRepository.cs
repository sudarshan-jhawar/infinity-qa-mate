using QAMate.Data;

namespace QAMate.Repositories
{
    // Data access abstraction for Defect entity.
    public interface IDefectRepository
    {
        Task<IEnumerable<Defect>> GetAllAsync();
        Task<Defect?> GetByIdAsync(int id);
        Task<Defect> AddAsync(Defect defect);
        Task<bool> UpdateAsync(Defect defect);
        Task<bool> DeleteAsync(int id);
    }
}
