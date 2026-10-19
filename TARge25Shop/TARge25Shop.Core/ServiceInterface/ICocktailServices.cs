using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Teenuse leping: kontroller kasutab seda liidest, mitte konkreetset teenuseklassi.
    public interface ICocktailServices
    {
        // Otsib kokteile terve nime või nimeosa järgi (s-parameeter).
        Task<CocktailRootDto?> CocktailResultHttpClient(string search);
        // Toob kokteilid valitud algustähe järgi (f-parameeter).
        Task<CocktailRootDto?> CocktailByLetterHttpClient(string letter);
    }
}
