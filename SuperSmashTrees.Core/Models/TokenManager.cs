using System;
using System.Collections.Generic;

namespace SuperSmashTrees.Core.Models
{
    public class TokenManager
    {
        private readonly Random random;
        private readonly List<Token> tokens;
        private readonly float spawnWidth;
        private readonly float fallSpeed;
        public int MaxActiveTokens { get; set; }
        public float SpawnRate { get; set; } // Tokens por segundo

        public TokenManager(float spawnWidth, float fallSpeed = 2.0f)
        {
            random = new Random();
            tokens = new List<Token>();
            this.spawnWidth = spawnWidth;
            this.fallSpeed = fallSpeed;
            MaxActiveTokens = 10;
            SpawnRate = 1.0f;
        }

        public List<Token> GetActiveTokens()
        {
            return tokens.FindAll(t => !t.IsCollected);
        }

        public void Update(float deltaTime)
        {
            // Eliminamos tokens recolectados
            tokens.RemoveAll(t => t.IsCollected);

            // Movemos tokens hacia abajo
            foreach (var token in tokens)
            {
                token.PositionY -= fallSpeed * deltaTime;
            }

            // Generamos nuevos tokens si es necesario
            if (tokens.Count < MaxActiveTokens && random.NextDouble() < SpawnRate * deltaTime)
            {
                SpawnToken();
            }
        }

        private void SpawnToken()
        {
            int value = random.Next(1, 100);
            float posX = (float)random.NextDouble() * spawnWidth;
            float posY = 0; // Aparecen arriba de la pantalla

            tokens.Add(new Token(value, posX, posY));
        }

        public Token CheckCollision(Player player, float collisionDistance)
        {
            foreach (var token in tokens)
            {
                if (!token.IsCollected)
                {
                    float distance = (float)Math.Sqrt(
                        Math.Pow(player.PositionX - token.PositionX, 2) +
                        Math.Pow(player.PositionY - token.PositionY, 2));

                    if (distance < collisionDistance)
                    {
                        token.Collect();
                        return token;
                    }
                }
            }
            return null;
        }
    }
}