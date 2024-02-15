using System.ComponentModel;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<LanguageId, Language>, Language>()]
public sealed record class Language : IEntity<LanguageId>
{
	public LanguageId Id { get; init; }
	public string Code { get; set; }
	public string Name { get; set; }
}

[TypeConverter(typeof(StronglyTypedIdTypeConverter<LanguageId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<LanguageId>))]
public sealed record class LanguageId : StronglyTypedId;
