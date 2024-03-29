﻿using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Infrastructure.DataAccess;

namespace OnTrack.Backend.Api.Models;

[EntityTypeConfiguration<StronglyTypedIdEntityConfiguration<StatusId, Status>, Status>]
public sealed record class Status : Entity<StatusId>
{
	public string Name { get; set; }
	public int Order { get; set; }
}
