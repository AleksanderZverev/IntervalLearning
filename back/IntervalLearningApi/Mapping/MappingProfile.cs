using AutoMapper;
using DB.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Models.Requests;
using IntervalLearningApi.Models.Topics;
using IntervalLearningApi.Models.Topics.TopicCollections;
using IntervalLearningApi.Models.UsersGroups;

namespace IntervalLearningApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Courses

        CreateMap<CreateCourseRequest, CreateCourseParameters>();
        CreateMap<PatchCourseRequest, PatchCourseParameters>();
        CreateMap<CourseEntity, Course>();

        // Topics

        CreateMap<CreateTopicRequest, CreateTopicParameters>();
        CreateMap<PatchTopicRequest, PatchTopicParameters>();
        CreateMap<TopicEntity, Topic>();

        // TopicsCollections
        CreateMap<CreateTopicCollectionRequest, CreateTopicCollectionParameters>();
        CreateMap<PatchTopicCollectionRequest, PatchTopicCollectionParameters>();
        CreateMap<TopicCollectionEntity, TopicCollection>();

        // UsersGroups

        CreateMap<UsersGroupEntity, UsersGroup>();
        CreateMap<UserEntity, UserInfo>();
    }
}