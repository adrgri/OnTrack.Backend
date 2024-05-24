using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Configuration;

public sealed class ConnectionsConfiguration : IOptionsSection
{
	public static string SectionKey => "Connections";

	[Required]
	public SqlServerType SqlServerType { get; set; }

	[Required]
	[JsonIgnore]
	public string SqlDatabase { get; set; }
}
