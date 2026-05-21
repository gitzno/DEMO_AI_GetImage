namespace DEMOShoesSizeTag.Interfaces
{
    public interface IGeminiVisionService
    {
        Task<string> ExtractShoeSizeAsync(IFormFile image);
    }
}
