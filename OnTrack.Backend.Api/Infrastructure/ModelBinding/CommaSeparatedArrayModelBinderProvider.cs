using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnTrack.Backend.Api.Infrastructure.ModelBinding;

public sealed class CommaSeparatedArrayModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		return (context.Metadata.IsCollectionType && context.Metadata.ModelType.IsArray)
			? new CommaSeparatedArrayModelBinder()
			: null;
	}
}
