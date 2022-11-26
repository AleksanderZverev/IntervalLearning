using AutoMapper;
using DB.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Models.UsersGroups;

namespace IntervalLearningApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CourseEntity, Course>();
        CreateMap<TopicEntity, Topic>();
        CreateMap<UsersGroupEntity, UsersGroup>();
        CreateMap<UserEntity, UserInfo>();
    }
}