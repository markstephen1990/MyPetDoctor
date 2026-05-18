namespace FernGames
{
    public interface IInteractableZoneWithTrigger : IInteractableZone
    {
        public void OnZoneTriggerActivated(PlayerBehavior playerBehavior);
    }
}