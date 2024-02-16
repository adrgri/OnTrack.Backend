using System.ComponentModel;
using System.Text.Json.Serialization;

using OnTrack.Backend.Api.ComponentModel;
using OnTrack.Backend.Api.Text.Json.Serialization;

namespace OnTrack.Backend.Api.Models;

[TypeConverter(typeof(StronglyTypedIdTypeConverter<MilestoneId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<MilestoneId>))]
public sealed record class MilestoneId : StronglyTypedId;
