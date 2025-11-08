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
                Name = dto.Name,
                Price = dto.Price
            };
            entity = await _repository.AddAsync(entity);
            return MapToDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, DefectDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;
            existing.Name = dto.Name;
            existing.Price = dto.Price;
            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static DefectDto MapToDto(Defect entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price
        };
    }
}
