namespace MSH.Infrastructure.Entities;

/// <summary>
/// Interface for entities that need user tracking (who created/updated the entity)
/// </summary>
public interface IUserTrackable
{
    string CreatedById { get; set; }
    string? UpdatedById { get; set; }
}
