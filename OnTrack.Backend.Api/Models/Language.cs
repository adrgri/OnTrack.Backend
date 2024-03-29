﻿using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<LanguageId, Language>, Language>]
public sealed record class Language : Entity<LanguageId>
{
	public string Code { get; set; }
	public string Name { get; set; }
}
