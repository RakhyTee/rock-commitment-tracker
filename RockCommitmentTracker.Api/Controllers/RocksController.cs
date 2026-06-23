using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RockCommitmentTracker.Api.Models;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models.Responses;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Api.Controllers;

[ApiController]
[Route("members/{memberId}/rocks")]
[Authorize]
public class RocksController : ControllerBase
{
    private readonly IRockService _service;
    private readonly IMapper _mapper;

    public RocksController(IRockService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRock(string memberId, [FromBody] CreateRockRequest request)
    {
        var rock = await _service.CreateAsync(
            memberId, request.Title, request.Category, request.DueDate, request.Note);

        return CreatedAtAction(nameof(GetAll), new { memberId }, _mapper.Map<RockResponse>(rock));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string memberId, [FromQuery] RockStatus? status = null)
    {
        var rocks = await _service.GetAllAsync(memberId, status);
        return Ok(rocks.Select(_mapper.Map<RockResponse>));
    }

    [HttpPatch("{rockId}")]
    public async Task<IActionResult> UpdateStatus(string memberId, string rockId, [FromBody] UpdateRockStatusRequest request)
    {
        var rock = await _service.UpdateStatusAsync(memberId, rockId, request.NewStatus);
        return Ok(_mapper.Map<RockResponse>(rock));
    }
}
