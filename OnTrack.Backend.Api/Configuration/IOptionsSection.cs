using System.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Configuration;

public interface IOptionsSection
{
	[JsonIgnore]
	static abstract string SectionKey { get; }
}
