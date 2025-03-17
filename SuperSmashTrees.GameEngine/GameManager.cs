using System;
using System.Collections.Generic;
using System.Linq;
using SuperSmashTrees.Core.Models;
using SuperSmashTrees.Core.DataStructures;

namespace SuperSmashTrees.GameEngine
{
    public class GameManager
    {
        public List<Player> Players { get; private set; }
        public TokenManager TokenManager { get; private set; }
        public TreeChallenge CurrentChallenge { get; private set; }
        public float GameTime { get; private set; }
        public float GameDuration { get; private set; }
        public bool IsGameActive { get; private set; }

        // Para la generación de plataformas
        public List<Platform> Platforms { get; private set; }

        private readonly Random random;
        private float challengeTimer;
        private float challengeDuration;
        private float collisionCheckDistance = 1.5f;
        private float tokenCollectionDistance = 2.0f;
        private float screenBoundsX;
        private float screenBoundsY;

        public event Action<Player, TreeChallenge> OnChallengeCompleted;
        public event Action<Player, Power> OnPowerAcquired;
        public event Action<Player, int> OnScoreChanged;
        public event Action<Player> OnPlayerFell;
        public event Action OnGameEnded;

        public GameManager(int numberOfPlayers, float gameAreaWidth, float gameAreaHeight, float gameDuration = 180.0f)
        {
            Players = new List<Player>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                Players.Add(new Player(i, $"Player {i + 1}"));

                // Posiciones iniciales
                Players[i].PositionX = gameAreaWidth / (numberOfPlayers + 1) * (i + 1);
                Players[i].PositionY = gameAreaHeight * 0.7f;
            }

            TokenManager = new TokenManager(gameAreaWidth);

            random = new Random();
            GameTime = 0;
            GameDuration = gameDuration;
            IsGameActive = false;

            screenBoundsX = gameAreaWidth;
            screenBoundsY = gameAreaHeight;

            // Generar plataformas aleatorias
            Platforms = GenerateRandomPlatforms(5, 10, gameAreaWidth, gameAreaHeight);

            // Generar primer reto
            GenerateNewChallenge();
            challengeTimer = 0;
            challengeDuration = 30.0f; // 30 segundos por reto
        }

        private List<Platform> GenerateRandomPlatforms(int minPlatforms, int maxPlatforms, float width, float height)
        {
            List<Platform> platforms = new List<Platform>();

            int numPlatforms = random.Next(minPlatforms, maxPlatforms + 1);

            for (int i = 0; i < numPlatforms; i++)
            {
                float platformWidth = random.Next(50, 200);
                float platformHeight = 20;
                float posX = random.Next(0, (int)(width - platformWidth));
                float posY = random.Next((int)(height * 0.2f), (int)(height * 0.8f));

                platforms.Add(new Platform(posX, posY, platformWidth, platformHeight));
            }

            // Asegurar una plataforma en la parte inferior central como punto de inicio
            platforms.Add(new Platform(width / 2 - 100, height * 0.9f, 200, 20));

            return platforms;
        }

        private void GenerateNewChallenge()
        {
            CurrentChallenge = TreeChallenge.GenerateRandomChallenge();

            // Asignar a cada jugador el tipo de árbol correspondiente al reto
            foreach (var player in Players)
            {
                player.SetCurrentTreeType(CurrentChallenge.TreeType);
            }
        }

        public void StartGame()
        {
            IsGameActive = true;
            GameTime = 0;
        }

        public void Update(float deltaTime)
        {
            if (!IsGameActive)
                return;

            GameTime += deltaTime;

            if (GameTime >= GameDuration)
            {
                EndGame();
                return;
            }

            // Actualizar tokens
            TokenManager.Update(deltaTime);

            // Comprobar colisiones con tokens
            foreach (var player in Players)
            {
                var token = TokenManager.CheckCollision(player, tokenCollectionDistance);
                if (token != null)
                {
                    player.AddToken(token.Value);
                    CheckChallengeCompletion(player);
                }
            }

            // Comprobar colisiones entre jugadores y aplicar poderes
            CheckPlayerCollisions();

            // Comprobar caídas fuera de pantalla
            CheckPlayerFalls();

            // Actualizar reto
            challengeTimer += deltaTime;
            if (challengeTimer >= challengeDuration)
            {
                GenerateNewChallenge();
                challengeTimer = 0;
            }

            // Actualizar estado de poderes
            UpdatePowers();
        }

        private void CheckPlayerCollisions()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                for (int j = i + 1; j < Players.Count; j++)
                {
                    Player p1 = Players[i];
                    Player p2 = Players[j];

                    float distance = (float)Math.Sqrt(
                        Math.Pow(p1.PositionX - p2.PositionX, 2) +
                        Math.Pow(p1.PositionY - p2.PositionY, 2));

                    if (distance < collisionCheckDistance)
                    {
                        // Aplicar colisión básica
                        float pushForce = 1.0f;

                        // Si alguno tiene ForcePush activo, se aplica más fuerza
                        var forcePush1 = p1.AvailablePowers.Find(p => p.Type == PowerType.ForcePush && p.IsActive);
                        var forcePush2 = p2.AvailablePowers.Find(p => p.Type == PowerType.ForcePush && p.IsActive);

                        if (forcePush1 != null)
                        {
                            pushForce = 5.0f;
                            p2.ApplyForceFrom(p1.PositionX, p1.PositionY, pushForce);
                        }
                        else if (forcePush2 != null)
                        {
                            pushForce = 5.0f;
                            p1.ApplyForceFrom(p2.PositionX, p2.PositionY, pushForce);
                        }
                        else
                        {
                            // Colisión normal, ambos se empujan mutuamente
                            p1.ApplyForceFrom(p2.PositionX, p2.PositionY, pushForce);
                            p2.ApplyForceFrom(p1.PositionX, p1.PositionY, pushForce);
                        }
                    }
                }
            }
        }

        private void CheckPlayerFalls()
        {
            foreach (var player in Players)
            {
                bool onPlatform = IsPlayerOnPlatform(player);

                if (!onPlatform)
                {
                    player.IsFalling = true;

                    // Si el jugador está fuera de la pantalla
                    if (player.PositionY > screenBoundsY)
                    {
                        // Comprobar si puede usar AirJump
                        var airJump = player.AvailablePowers.Find(p => p.Type == PowerType.AirJump && !p.IsActive);

                        if (airJump != null && player.CanAirJump)
                        {
                            airJump.Activate();
                            airJump.ApplyEffect(player);
                        }
                        else
                        {
                            // Jugador cayó fuera de la pantalla
                            ResetPlayerPosition(player);
                            OnPlayerFell?.Invoke(player);

                            // Dar puntos a los otros jugadores
                            foreach (var otherPlayer in Players.Where(p => p.Id != player.Id))
                            {
                                otherPlayer.AddScore(10);
                                OnScoreChanged?.Invoke(otherPlayer, otherPlayer.Score);
                            }
                        }
                    }
                }
                else
                {
                    player.IsFalling = false;
                    player.CanAirJump = true;  // Reset air jump cuando está en plataforma
                }
            }
        }

        private bool IsPlayerOnPlatform(Player player)
        {
            foreach (var platform in Platforms)
            {
                if (player.PositionX >= platform.PositionX &&
                    player.PositionX <= platform.PositionX + platform.Width &&
                    Math.Abs(player.PositionY - platform.PositionY) < 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        private void ResetPlayerPosition(Player player)
        {
            // Colocar al jugador en una plataforma aleatoria
            if (Platforms.Count > 0)
            {
                int platformIndex = random.Next(Platforms.Count);
                var platform = Platforms[platformIndex];

                player.PositionX = platform.PositionX + platform.Width / 2;
                player.PositionY = platform.PositionY - 1.0f;  // Ligeramente por encima
            }
            else
            {
                // Si no hay plataformas, posición por defecto
                player.PositionX = screenBoundsX / 2;
                player.PositionY = screenBoundsY * 0.7f;
            }
        }

        private void CheckChallengeCompletion(Player player)
        {
            bool completed = false;

            switch (CurrentChallenge.TreeType)
            {
                case TreeType.BST:
                    completed = CurrentChallenge.CheckCompletion<int>(player.BSTree);
                    break;
                case TreeType.AVL:
                    completed = CurrentChallenge.CheckCompletion<int>(player.AVLTree);
                    break;
                case TreeType.BTree:
                    completed = CurrentChallenge.CheckCompletion<int>(player.BTree);
                    break;
            }

            if (completed)
            {
                // Otorgar puntos
                player.AddScore(25);
                OnScoreChanged?.Invoke(player, player.Score);

                // Otorgar poder aleatorio
                GiveRandomPower(player);

                // Generar nuevo reto
                GenerateNewChallenge();
                challengeTimer = 0;

                OnChallengeCompleted?.Invoke(player, CurrentChallenge);
            }
        }

        private void GiveRandomPower(Player player)
        {
            PowerType powerType = (PowerType)random.Next(Enum.GetValues(typeof(PowerType)).Length);
            Power newPower;

            switch (powerType)
            {
                case PowerType.ForcePush:
                    newPower = new ForcePushPower();
                    break;
                case PowerType.Shield:
                    newPower = new ShieldPower();
                    break;
                case PowerType.AirJump:
                default:
                    newPower = new AirJumpPower();
                    break;
            }

            player.AddPower(newPower);
            OnPowerAcquired?.Invoke(player, newPower);
        }

        private void UpdatePowers()
        {
            foreach (var player in Players)
            {
                foreach (var power in player.AvailablePowers)
                {
                    if (power.IsActive && power.IsExpired())
                    {
                        power.Deactivate();
                    }

                    if (power.IsActive)
                    {
                        power.ApplyEffect(player);
                    }
                }
            }
        }

        public void EndGame()
        {
            IsGameActive = false;

            // Ordenar jugadores por puntuación
            Players = Players.OrderByDescending(p => p.Score).ToList();

            OnGameEnded?.Invoke();
        }

        public void ActivatePower(Player player, PowerType powerType)
        {
            var power = player.GetPower(powerType);
            if (power != null)
            {
                power.Activate();
            }
        }

        public Player GetWinner()
        {
            if (!IsGameActive)
            {
                return Players.OrderByDescending(p => p.Score).First();
            }
            return null;
        }
    }
}