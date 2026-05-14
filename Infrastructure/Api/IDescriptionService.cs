public interface IDescriptionService
{
    public Task<string?> GetDescription(string imdbId, CancellationToken ct = default);

}