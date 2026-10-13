using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    public interface IChuckNorrisServices
    {
        Task<ChuckNorrisRootDto?> ChuckNorrisResultHttpClient();
    }
}
