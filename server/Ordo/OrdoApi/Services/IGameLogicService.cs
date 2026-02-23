using OrdoApi.Models;

namespace OrdoApi.Interfaces;

public interface IGameLogicService
{
    bool IsMoveValid(Game game, GuestPlayer player, List<TilePlacement> placements);

    int CalculateScore(Game game, List<TilePlacement> placements);

    void ExecuteMove(Game game, GuestPlayer player, List<TilePlacement> placements);
}