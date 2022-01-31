﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.MaxDepth;

public class MaxDepthWithCollections : AutoMapperSpecBase, IAsyncLifetime
{
    TrainingCourseDto _course;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        //cfg.AllowNullDestinationValues = false;
        cfg.CreateProjection<TrainingCourse, TrainingCourseDto>().MaxDepth(1);
        cfg.CreateProjection<TrainingContent, TrainingContentDto>();
    });

    [Fact]
    public void Should_project_with_MaxDepth()
    {
        _course.CourseName.ShouldBe("Course 1");
        var content = _course.Content[0];
        content.ContentName.ShouldBe("Content 1");
        content.Course.ShouldBeNull();
    }

    class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var course = new TrainingCourse { CourseName = "Course 1" };
            context.TrainingCourses.Add(course);
            var content = new TrainingContent { ContentName = "Content 1", Course = course };
            context.TrainingContents.Add(content);
            course.Content.Add(content);
        }
    }

    class ClientContext : LocalDbContext
    {
        public DbSet<TrainingCourse> TrainingCourses { get; set; }
        public DbSet<TrainingContent> TrainingContents { get; set; }
    }

    public class TrainingCourse
    {
        [Key]
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public virtual IList<TrainingContent> Content { get; set; } = new List<TrainingContent>();
    }

    public class TrainingContent
    {
        [Key]
        public int ContentId { get; set; }

        public string ContentName { get; set; }

        public virtual TrainingCourse Course { get; set; }
    }

    public class TrainingCourseDto
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public virtual IList<TrainingContentDto> Content { get; set; }
    }

    public class TrainingContentDto
    {
        public int ContentId { get; set; }

        public string ContentName { get; set; }

        public TrainingCourseDto Course { get; set; }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();

        using (var context = new ClientContext())
        {
            _course = ProjectTo<TrainingCourseDto>(context.TrainingCourses).FirstOrDefault(n => n.CourseName == "Course 1");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}