using System;

namespace TopJellyfin.Models;

public class SectionItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int? ProductionYear { get; set; }

    public DateTime? PremiereDate { get; set; }

    public float? CommunityRating { get; set; }

    public bool HasPrimaryImage { get; set; }

    public bool HasBackdropImage { get; set; }
}
