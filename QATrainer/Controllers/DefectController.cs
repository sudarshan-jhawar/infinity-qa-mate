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


    // GET: /Defects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Defect>>> GetAll()
    {
        var defects = await _context.Defects.ToListAsync();
        return Ok(defects);
    }


}