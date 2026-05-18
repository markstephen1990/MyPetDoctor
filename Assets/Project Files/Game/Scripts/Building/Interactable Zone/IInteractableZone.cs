namespace FernGames
{
    public interface IInteractableZone
    {
        public void OnZoneEnter(PlayerBehavior playerBehavior);
        public void OnZoneExit(PlayerBehavior playerBehavior);
    }
}