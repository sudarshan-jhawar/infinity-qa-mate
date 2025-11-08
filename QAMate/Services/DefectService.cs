using QAMate.Data;
using QAMate.Models;
using QAMate.Repositories;

namespace QAMate.Services
{
    // Implements business logic & mapping between entity and DTO.
    public class DefectService : IDefectService
    {
        private readonly IDefectRepository _repository;
        public DefectService(IDefectRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DefectDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<DefectDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<DefectDto> CreateAsync(DefectDto dto)
        {
            var now = DateTime.UtcNow;
            var entity = new Defect
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status ?? "Open",
                Severity = dto.Severity,
                Priority = dto.Priority,
                CreatedAt = now,
                UpdatedAt = now,
                LastModifiedAt = now
            };
            entity = await _repository.AddAsync(entity);
            return MapToDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, DefectDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            existing.Title = dto.Title ?? existing.Title;
            existing.Description = dto.Description ?? existing.Description;
            existing.Status = dto.Status ?? existing.Status;
            existing.Severity = dto.Severity == 0 ? existing.Severity : dto.Severity;
            existing.Priority = dto.Priority == 0 ? existing.Priority : dto.Priority;
            existing.LastModifiedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static DefectDto MapToDto(Defect entity) => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Severity = entity.Severity,
            Priority = entity.Priority,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            LastModifiedAt = entity.LastModifiedAt
        };
    }
}
