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
            var entity = new Defect
            {
                Title = dto.Title,
                Description = dto.Description,
                Module = dto.Module,
                Severity = dto.Severity,
                Priority = dto.Priority,
                CreatedDate = dto.CreatedDate == default ? DateTime.UtcNow : dto.CreatedDate,
                Environment = dto.Environment,
                Embedding = dto.Embedding
            };
            entity = await _repository.AddAsync(entity);
            return MapToDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, DefectDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            // Map all updatable properties from DTO to entity
            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Module = dto.Module;
            existing.Severity = dto.Severity;
            existing.Priority = dto.Priority;
            // Preserve existing CreatedDate when DTO doesn't provide one
            existing.CreatedDate = dto.CreatedDate == default ? existing.CreatedDate : dto.CreatedDate;
            existing.Environment = dto.Environment;
            existing.Embedding = dto.Embedding;
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
            Module = entity.Module,
            Severity = entity.Severity,
            Priority = entity.Priority,
            CreatedDate = entity.CreatedDate,
            Environment = entity.Environment,
            Embedding = entity.Embedding
        };
    }
}
