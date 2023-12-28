using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember.ValueObjects;

namespace Application.Services.Study.Remember;

public class RememberService
{
    private readonly IStudyRepository studyRepository;

    public RememberService(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Domain.Schedule.Entities.Remember.Remember Create(RepeatsSchedule schedule, Card card, RememberWeight weight, int phaseIndex, DateTime date)
    {
        var rememberId = studyRepository.CardRemembers.GetUniqueId(new(schedule, card)).Value;
        
        return new Domain.Schedule.Entities.Remember.Remember(
            schedule.ParentUserId, 
            schedule.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            rememberId,
            weight, 
            (short)phaseIndex,
            date);
    }
    
    public Domain.Schedule.Entities.Remember.Remember CreateLearnedRemember(RepeatsSchedule schedule, Card card, RememberWeight weight, DateTime date)
    {
        var rememberId = studyRepository.CardRemembers.GetUniqueId(new(schedule, card)).Value;
        
        var remember =  new Domain.Schedule.Entities.Remember.Remember(
            schedule.ParentUserId, 
            schedule.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            rememberId,
            weight, 
            0,
            date);
        
        remember.MakeLearnedRemember();
        return remember;
    }
}