namespace SuperSmashTrees.Core.Models
{
    public class AirJumpPower : Power
    {
        public float JumpForce { get; private set; }

        public AirJumpPower() : base(
            PowerType.AirJump,
            "Air Jump",
            "Te permite realizar un salto en el aire para evitar caer",
            1) // Duración corta, es más un efecto instantáneo
        {
            JumpForce = 15.0f;
        }

        public override void ApplyEffect(Player player)
        {
            if (IsActive && !IsExpired() && player.IsFalling)
            {
                // Implementación básica, se activaría en el GameEngine cuando el jugador
                // está cayendo y decide usar este poder
                player.IsFalling = false;
                player.CanAirJump = false;

                // En un contexto real de juego, aquí aplicarías una fuerza vertical
                // para simular un salto en el aire
                player.PositionY += JumpForce;

                // Auto-desactivar después de un uso
                Deactivate();
            }
        }
    }
}