using Microsoft.AspNetCore.Mvc;
using UserProfile.Api.Services;
using UserProfile.Domain;

namespace UserProfile.Api.Controllers;

[ApiController]
[Route("profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IUserProfileService _service;

    public ProfilesController(IUserProfileService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Domain.UserProfile>> Create([FromBody] Domain.UserProfile profile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var created = await _service.CreateAsync(profile);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Domain.UserProfile>> GetById(int id)
    {
        var profile = await _service.GetByIdAsync(id);
        if (profile == null)
        {
            return NotFound();
        }
        return Ok(profile);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Domain.UserProfile>> Update(int id, [FromBody] Domain.UserProfile profile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        
        var updated = await _service.UpdateAsync(id, profile);
        return Ok(updated);
    }
}
