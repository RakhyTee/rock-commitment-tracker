using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models.Responses;

namespace RockCommitmentTracker.Api.Controllers;

[ApiController]
[Route("members/{memberId}/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IRockService _service;
    private readonly IMapper _mapper;

    public ProfileController(IRockService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("enriched")]
    public async Task<IActionResult> GetEnrichedProfile(string memberId)
    {
        var profile = await _service.GetEnrichedProfileAsync(memberId);
        return Ok(_mapper.Map<EnrichedProfileResponse>(profile));
    }
}
