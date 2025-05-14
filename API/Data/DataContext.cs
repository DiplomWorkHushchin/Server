using API.Entities;
using API.Entities.Courses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<User, Role, int, IdentityUserClaim<int>,
    UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    public DbSet<RefreshToken> RefreshToken { get; set; }
    public DbSet<UserPhoto> UserPhotos { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseSchedule> CourseSchedules { get; set; }
    public DbSet<CourseMaterial> CourseMaterials { get; set; }
    public DbSet<CourseInstructor> CourseInstructors { get; set; }
    public DbSet<CourseStudent> CourseStudents { get; set; }
    public DbSet<CourseMaterialsFiles> CourseMaterialsFiles { get; set; }
    public DbSet<Submissions> Submissions { get; set; }
    public DbSet<SubmissionsFiles> SubmissionsFiles { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User and Role configuration
        builder.Entity<User>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<Role>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired();

        builder.Entity<Group>()
            .HasMany(g => g.Students)
            .WithOne(u => u.Group)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.Cascade);


        // Course Instructor configuration
        builder.Entity<CourseInstructor>()
                .HasKey(ci => new { ci.CourseId, ci.InstructorId });

        builder.Entity<CourseInstructor>()
            .HasOne(ci => ci.Course)
            .WithMany(c => c.Instructors)
            .HasForeignKey(ci => ci.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseInstructor>()
            .HasOne(ci => ci.Instructor)
            .WithMany()
            .HasForeignKey(ci => ci.InstructorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseInstructor>()
            .OwnsOne(ci => ci.Permissions, perms =>
            {
                perms.Property(p => p.CanCreateAssignments).HasDefaultValue(false);
                perms.Property(p => p.CanModifyAssignments).HasDefaultValue(false);
                perms.Property(p => p.CanGradeStudents).HasDefaultValue(false);
                perms.Property(p => p.CanManageUsers).HasDefaultValue(false);
            });


        // Course Student configuration
        builder.Entity<CourseStudent>()
            .HasKey(sc => new { sc.CourseId, sc.StudentId });

        builder.Entity<CourseStudent>()
            .HasOne(sc => sc.Course)
            .WithMany(c => c.EnrolledStudents)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseStudent>()
            .HasOne(sc => sc.Student)
            .WithMany()
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course Schedule configuration
        builder.Entity<CourseSchedule>()
            .HasKey(cs => cs.Id);

        builder.Entity<CourseSchedule>()
            .HasOne(cs => cs.Course)
            .WithMany(c => c.CourseSchedule)
            .HasForeignKey(cs => cs.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course Materials configuration
        builder.Entity<CourseMaterial>()
            .HasKey(m => m.Id);

        builder.Entity<CourseMaterial>()
            .HasOne(m => m.Course)
            .WithMany(c => c.Materials)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course Material files
        builder.Entity<CourseMaterialsFiles>()
            .HasOne(f => f.CourseMaterial)
            .WithMany(m => m.MaterialsFiles)
            .HasForeignKey(f => f.CourseMaterialId);

        builder.Entity<CourseMaterialsFiles>()
            .HasKey(f => f.Id);

        builder.Entity<CourseMaterialsFiles>()
            .Property(f => f.FilePath)
            .IsRequired();

        builder.Entity<CourseMaterialsFiles>()
            .Property(f => f.Name);

        // Course Submissions configuration
        builder.Entity<Submissions>()
            .HasKey(s => s.Id);

        builder.Entity<Submissions>()
            .HasOne(s => s.Course)
            .WithMany(c => c.Submissions)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Submissions>()
            .HasOne(s => s.User)
            .WithMany() 
            .HasForeignKey(s => s.UserId);

        builder.Entity<Submissions>()
            .HasOne(s => s.Task)
            .WithMany() 
            .HasForeignKey(s => s.TaskId);

        builder.Entity<SubmissionsFiles>()
            .HasOne(f => f.Submissions)
            .WithMany(s => s.SubmissionsFiles)
            .HasForeignKey(f => f.SubmissionId);
    }
}
