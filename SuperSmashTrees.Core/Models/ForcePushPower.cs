namespace SuperSmashTrees.Core.Models
{
    public class ForcePushPower : Power
    {
        public float PushForce { get; private set; }

        public ForcePushPower() : base(
            PowerType.ForcePush,
            "Force Push",
            "Empuja a otros jugadores con gran fuerza",
            3)
        {
            PushForce = 10.0f;
        }

        public override void ApplyEffect(Player player)
        {
            // Esta implementación es básica, tendrás que adaptarla según tu sistema de juego
            if (IsActive && !IsExpired())
            {
                // El jugador objetivo recibe la fuerza
                // En un contexto real, necesitarías especificar la posición del jugador que usa el poder
                player.ApplyForceFrom(0, 0, PushForce);
            }
        }
    }
}