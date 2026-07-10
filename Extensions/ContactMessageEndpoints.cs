using FluentValidation;
using PortfolioAPI.Data;
using PortfolioAPI.Dtos;
using PortfolioAPI.Model.Entities;
using PortfolioAPI.Model.Services;
using PortfolioAPI.Validation;

namespace PortfolioAPI.Extensions;

public static class ContactMessageEndpoints
{
    public static void MapContactMessageEndpoints(this WebApplication app)
    {
        _ = app.MapPost(
                "/api/contact",
                async (
                    ContactMessageDto dto,
                    IValidator<ContactMessageDto> validator,
                    ApplicationDbContext db,
                    EmailService emailService
                ) =>
                {
                    var validationResult = await validator.ValidateAsync(dto);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(
                            validationResult.Errors.Select(e => e.ErrorMessage)
                        );
                    }

                    var contactMessage = new ContactMessage
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        Message = dto.Message,
                    };

                    db.ContactMessages.Add(contactMessage);
                    await db.SaveChangesAsync();

                    try
                    {
                        await emailService.SendContactEmailAsync(dto.Name, dto.Email, dto.Message);
                        return Results.Ok(new { message = "Message sent successfully." });
                    }
                    catch (Exception ex)
                    {
                        // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                        Console.WriteLine($"Error sending email: {ex.Message}");
                        return Results.Problem(
                            detail: "Message saved but failed to send email.",
                            statusCode: 500
                        );
                    }
                }
            )
            .RequireRateLimiting("ContactFormPolicy");
    }
}
