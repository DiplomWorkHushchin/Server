using API.DTOs.CourseDTOs;
using API.DTOs.CourseDTOs.Tasks;
using API.DTOs.UserDTOs;
using API.Entities;
using API.Entities.Courses;
using AutoMapper;
using System.Globalization;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.Photos.Any() ? src.Photos.First().Url : null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src =>
                src.DateOfBirth.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)))
            .ForMember(dest => dest.Group, opt => opt.MapFrom(src =>
                src.Group != null ? src.Group.Name : null));

        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                src.StartDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                src.EndDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)))
            .ForMember(dest => dest.CourseSchedule, opt => opt.MapFrom(src => src.CourseSchedule))
            .ForMember(dest => dest.Instructors, opt => opt.MapFrom(src => src.Instructors))
            .ForMember(dest => dest.CoverBanner, opt => opt.MapFrom(src =>
                src.CoverBanner != null ? src.CoverBanner : null))
            .ForMember(dest => dest.EnrolledStudents, opt => opt.MapFrom(src => src.EnrolledStudents.Select(cs => cs.Student)));

        CreateMap<CourseInstructor, CourseInstructorDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Instructor.UserName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Instructor.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Instructor.LastName))
            .ForMember(dest => dest.FatherName, opt => opt.MapFrom(src => src.Instructor.FatherName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Instructor.Email))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.Instructor.Photos.Any() ? src.Instructor.Photos.First().Url : null))
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
            .ForMember(dest => dest.CanCreateAssignments, opt => opt.MapFrom(src => src.Permissions.CanCreateAssignments))
            .ForMember(dest => dest.CanModifyAssignments, opt => opt.MapFrom(src => src.Permissions.CanModifyAssignments))
            .ForMember(dest => dest.CanGradeStudents, opt => opt.MapFrom(src => src.Permissions.CanGradeStudents))
            .ForMember(dest => dest.CanManageUsers, opt => opt.MapFrom(src => src.Permissions.CanManageUsers));

        CreateMap<User, CourseStudentDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FatherName, opt => opt.MapFrom(src => src.FatherName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.Photos.Any() ? src.Photos.First().Url : null));

        CreateMap<CourseMaterialsFiles, CourseMaterialFilesDto>();

        CreateMap<CourseMaterial, CourseMaterialDto>()
            .ForMember(dest => dest.MaterialsFiles, opt => opt.MapFrom(src => src.MaterialsFiles));

        CreateMap<CourseSchedule, CourseScheduleDto>();

        CreateMap<SubmissionsFiles, SubmissionFileDto>();

        CreateMap<Submissions, SubmissionDto>()
            .ForMember(dest => dest.SubmissionFiles, opt => opt.MapFrom(src => src.SubmissionsFiles))
            .ReverseMap();
    }
}
