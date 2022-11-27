using AutoMapper;
using DB.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Models.Requests;
using IntervalLearningApi.Models.UsersGroups;

namespace IntervalLearningApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Courses

        CreateMap<CreateCourseRequest, CreateOrPatchCourse>();
        CreateMap<PatchCourseRequest, CreateOrPatchCourse>();
        
        CreateMap<CourseEntity, Course>();
        CreateMap<TopicEntity, Topic>();
        CreateMap<UsersGroupEntity, UsersGroup>();
        CreateMap<UserEntity, UserInfo>();
    }
}