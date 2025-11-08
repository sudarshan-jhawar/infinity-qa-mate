using Microsoft.EntityFrameworkCore;
using QAMate.Data;

namespace QAMate.Repositories
{
    // Concrete implementation of data access for Defect entity using EF Core.
    public class DefectRepository : IDefectRepository
    {
        private readonly AppDbContext _context;
        public DefectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Defect>> GetAllAsync()
        {
            return await _context.Defect.AsNoTracking().ToListAsync();
        }

        public async Task<Defect?> GetByIdAsync(int id)
        {
            return await _context.Defect.FindAsync(id);
        }

        public async Task<Defect> AddAsync(Defect defect)
        {
            _context.Defect.Add(defect);
            await _context.SaveChangesAsync();
            return defect;
        }

        public async Task<bool> UpdateAsync(Defect defect)
        {
            _context.Defect.Update(defect);
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Defect.FindAsync(id);
            if (entity == null) return false;
            _context.Defect.Remove(entity);
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }
    }
}
