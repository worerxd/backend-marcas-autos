
using MarcasAutos.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasAutosController : ControllerBase
{
    private readonly MarcasAutosDbContext _context;

    public MarcasAutosController(MarcasAutosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var marcas = await _context.MarcasAutos
            .AsNoTracking()
            .ToListAsync();

        return Ok(marcas);
    }
}
