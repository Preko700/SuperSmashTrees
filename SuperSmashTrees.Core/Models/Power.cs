using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.Models
{
    public abstract class Power
    {
        public PowerType Type { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public abstract void Use(Player player, GameState gameState);
    }

    public class ForcePushPower : Power
    {
        public ForcePushPower()
        {
            Type = PowerType.ForcePush;
            Name = "Force Push";
            Description = "Push other players away with force";
        }

        public override void Use(Player player, GameState gameState)
        {
            // Encontrar jugadores cercanos y empujarlos
            var nearbyPlayers = gameState.Players.Where(p => p.Id != player.Id &&
                                                       Math.Abs(p.Position.X - player.Position.X) < 100 &&
                                                       Math.Abs(p.Position.Y - player.Position.Y) < 100);

            foreach (var nearbyPlayer in nearbyPlayers)
            {
                double dx = nearbyPlayer.Position.X - player.Position.X;
                double dy = nearbyPlayer.Position.Y - player.Position.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                if (length > 0)
                {
                    double pushForce = 200;
                    nearbyPlayer.Velocity.X += (dx / length) * pushForce;
                    nearbyPlayer.Velocity.Y += (dy / length) * pushForce * 0.5;
                }
            }
        }
    }

    public class ShieldPower : Power
    {
        public ShieldPower()
        {
            Type = PowerType.Shield;
            Name = "Shield";
            Description = "Protects from attacks and pushes";
        }

        public override void Use(Player player, GameState gameState)
        {
            // Implementar lógica para activar el escudo
            // Por ejemplo, agregar un estado de inmunidad temporal
            // Esta es una implementación simplificada
            player.Health += 25;
        }
    }

    public class AirJumpPower : Power
    {
        public AirJumpPower()
        {
            Type = PowerType.AirJump;
            Name = "Air Jump";
            Description = "Jump in mid-air to avoid falling";
        }

        public override void Use(Player player, GameState gameState)
        {
            // Permitir salto en el aire para evitar caídas
            if (player.IsFalling)
            {
                player.Velocity.Y = -15; // Fuerza de salto en el aire
                player.IsFalling = false;
            }
        }
    }
}
