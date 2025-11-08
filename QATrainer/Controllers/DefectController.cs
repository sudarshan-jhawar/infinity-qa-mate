using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QATrainer.Models;
namespace QATrainer.Controllers;

[ApiController]
[Route("[controller]")]
public class DefectsController : ControllerBase
{
    private readonly InfinityQaContext _context;

    public DefectsController(InfinityQaContext context)
    {
        _context = context;
    }

    // GET: /Defects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Defect>>> GetAll()
    {
        var defects = await _context.Defects.ToListAsync();
        return Ok(defects);
    }

    
    // POST: api/Defects
    [HttpPost]
    public async Task<IActionResult> CreateDefect([FromBody] Defect defect)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        defect.DefectId = Guid.NewGuid().ToString();
        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDefectById), new { id = defect.DefectId }, defect);
    }

    
    // Optional: GET method to support CreatedAtAction
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDefectById(int id)
    {
        var defect = await _context.Defects.FindAsync(id);
        if (defect == null)
        {
            return NotFound();
        }
        return Ok(defect);
    }

}