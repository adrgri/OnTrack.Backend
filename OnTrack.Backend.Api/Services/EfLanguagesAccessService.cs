using Microsoft.EntityFrameworkCore;

using OnTrack.Backend.Api.Models;

namespace OnTrack.Backend.Api.Services;

public sealed class EfLanguagesAccessService<TDbContext>(TDbContext context)
	: EfEntityAccessService<TDbContext, Language, LanguageId>(context)
	where TDbContext : DbContext;
