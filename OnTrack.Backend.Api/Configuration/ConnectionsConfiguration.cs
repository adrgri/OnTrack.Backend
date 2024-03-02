using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Configuration;

public sealed class ConnectionsConfiguration : IOptionsSection
{
	public static string SectionKey => "ConnectionStrings";

	[Required]
	[JsonIgnore]
	public string SqlDatabase { get; set; }
}
