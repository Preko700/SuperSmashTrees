namespace SuperSmashTrees.Core.Models
{
    public class ShieldPower : Power
    {
        public ShieldPower() : base(
            PowerType.Shield,
            "Shield",
            "Te protege contra cualquier ataque o empuje",
            5)
        {
        }

        public override void ApplyEffect(Player player)
        {
            if (IsActive && !IsExpired())
            {
                player.IsShielded = true;
            }
            else
            {
                player.IsShielded = false;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            // Asegúrate de que este código se ejecute cuando el escudo expire
        }
    }
}