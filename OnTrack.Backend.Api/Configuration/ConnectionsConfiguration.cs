namespace OnTrack.Backend.Api.Configuration;

public sealed class ConnectionsConfiguration : IOptionsSection
{
	public static string SectionKey => "ConnectionStrings";

	public string SqlDatabase { get; set; }
}
