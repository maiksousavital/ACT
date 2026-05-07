using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACT.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _companyService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null) return NotFound();
        return Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request)
    {
        var created = await _companyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> Update(int id, [FromBody] UpdateCompanyRequest request)
    {
        var updated = await _companyService.UpdateAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
