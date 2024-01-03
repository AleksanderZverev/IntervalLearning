using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember.ValueObjects;
using DomainServices.DB.Repositories.Study;

namespace DomainServices.BoundedContext.Study.RememberService;

public class RememberService
{
    private readonly IStudyRepository studyRepository;

    public RememberService(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Remember Create(RepeatsSchedule schedule, Card card, RememberWeight weight,
        int phaseIndex, DateTime date, MediumSingleLineString? comment)
    {
        var rememberId = studyRepository.CardRemembers.GetUniqueId(new(schedule, card)).Value;
        
        return new Remember(
            schedule.ParentUserId, 
            schedule.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            rememberId,
            weight, 
            (short)phaseIndex,
            date)
        {
            Comment = comment,
        };
    }
    
    public Remember CreateLearnedRemember(RepeatsSchedule schedule, Card card, RememberWeight weight, DateTime date)
    {
        var rememberId = studyRepository.CardRemembers.GetUniqueId(new(schedule, card)).Value;
        
        var remember =  new Remember(
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