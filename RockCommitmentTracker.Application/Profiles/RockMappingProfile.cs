using AutoMapper;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Application.Models.Responses;

namespace RockCommitmentTracker.Application.Profiles;

public class RockMappingProfile : Profile
{
    public RockMappingProfile()
    {
        CreateMap<Rock, RockResponse>();
        CreateMap<EnrichedProfile, EnrichedProfileResponse>();
    }
}
