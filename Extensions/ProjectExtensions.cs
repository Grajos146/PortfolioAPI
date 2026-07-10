using PortfolioAPI.Dtos;
using PortfolioAPI.Model.Entities;

namespace PortfolioAPI.Extensions;

public static class ProjectExtensions
{
    public static ProjectResponseDto ToResponseDto(this Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            TechStack = project.TechStack,
            GithubUrl = project.GithubUrl,
            LiveUrl = project.LiveUrl,
            ImageUrl = project.ImageUrl,
        };
    }

    public static Project ToEntity(this CreateProjectDto createDto)
    {
        return new Project
        {
            Title = createDto.Title,
            Description = createDto.Description,
            TechStack = createDto.TechStack,
            GithubUrl = createDto.GithubUrl,
            LiveUrl = createDto.LiveUrl,
            ImageUrl = createDto.ImageUrl,
        };
    }

    public static void UpdateFromDto(this Project project, CreateProjectDto updateDto)
    {
        project.Title = updateDto.Title;
        project.Description = updateDto.Description;
        project.TechStack = updateDto.TechStack;
        project.GithubUrl = updateDto.GithubUrl;
        project.LiveUrl = updateDto.LiveUrl;
        project.ImageUrl = updateDto.ImageUrl;
    }
}
