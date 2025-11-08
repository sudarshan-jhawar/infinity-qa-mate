using Microsoft.AspNetCore.Mvc;
using QAMate.Models;
using QAMate.Services;

namespace QAMate.Api.Controllers
{
    // Controller exposes CRUD endpoints for Defect via service layer.
    [ApiController]
    [Route("api")] // Explicit base path to match required endpoint patterns.
    public class DefectController : ControllerBase
    {
        private readonly IDefectService _service;
        public DefectController(IDefectService service)
        {
            _service = service;
        }

        // GET /api/defect (list all)
        [HttpGet("defect")] // As required (singular in spec)
        public async Task<ActionResult<IEnumerable<DefectDto>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        // GET /api/defects/{id}
        [HttpGet("defects/{id:int}")]
        public async Task<ActionResult<DefectDto>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /* Example POST JSON body:
        {
          "name": "UI Alignment Issue",
          "price": 12.5
        }
        */
        // POST /api/defects
        [HttpPost("defects")]
        public async Task<ActionResult<DefectDto>> Create([FromBody] DefectDto dto)
        {
            var created = await _service.CreateAsync(dto);
            // Return CreatedAt for RESTful pattern.
            return CreatedAtAction(nameof(GetById), new { id = created.Defect_ID }, created);
        }

        /* Example PUT JSON body:
        {
          "id": 1,
          "name": "Updated Defect",
          "price": 20.0
        }
        */
        // PUT /api/defects/{id}
        [HttpPut("defects/{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] DefectDto dto)
        {
            if (dto.Defect_ID != 0 && dto.Defect_ID != id) return BadRequest("ID mismatch.");
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        // DELETE /api/defects/{id}
        [HttpDelete("defects/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
