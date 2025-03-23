using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SuperSmashTrees.Core.Models;
using SuperSmashTrees.GameEngine;
using SuperSmashTrees.Core.DataStructures;

namespace SuperSmashTrees.UI
{
    public partial class MainWindow : Window
    {
        private GameManager gameManager;
        private DispatcherTimer gameTimer;
        private Dictionary<int, Dictionary<Key, bool>> playerKeyStates;
        private Dictionary<int, UIElement> playerSprites;
        private Dictionary<int, TreeVisualizer> treeVisualizers;
        private Dictionary<UIElement, Token> tokenElements;
        private Dictionary<UIElement, Platform> platformElements;
        private Dictionary<int, Color> playerColors;

        // Opciones de juego
        private int numberOfPlayers = 2; // Por defecto 2 jugadores, ampliable hasta 4
        private bool useExternalControllers = false;
        private ExternalControllerManager externalControllerManager;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Inicializar diccionarios
            playerKeyStates = new Dictionary<int, Dictionary<Key, bool>>();
            playerSprites = new Dictionary<int, UIElement>();
            treeVisualizers = new Dictionary<int, TreeVisualizer>();
            tokenElements = new Dictionary<UIElement, Token>();
            platformElements = new Dictionary<UIElement, Platform>();

            // Definir colores para jugadores
            playerColors = new Dictionary<int, Color>
            {
                { 0, Colors.Red },
                { 1, Colors.Blue },
                { 2, Colors.Green },
                { 3, Colors.Yellow }
            };

            // Configurar teclas para cada jugador
            SetupPlayerControls();

            // Inicializar GameManager con el tamaño del canvas
            gameManager = new GameManager(numberOfPlayers, (float)gameCanvas.ActualWidth, (float)gameCanvas.ActualHeight);

            // Configurar temporizador del juego
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            gameTimer.Tick += GameTimer_Tick;

            // Suscribirse a eventos del GameManager
            gameManager.OnChallengeCompleted += GameManager_OnChallengeCompleted;
            gameManager.OnPowerAcquired += GameManager_OnPowerAcquired;
            gameManager.OnScoreChanged += GameManager_OnScoreChanged;
            gameManager.OnPlayerFell += GameManager_OnPlayerFell;
            gameManager.OnGameEnded += GameManager_OnGameEnded;

            // Inicializar soporte para controles externos (opcional)
            if (useExternalControllers)
            {
                externalControllerManager = new ExternalControllerManager();
                externalControllerManager.ControllerInput += ExternalControllerManager_ControllerInput;
                externalControllerManager.Initialize(numberOfPlayers);
            }

            // Configurar la interfaz inicial
            CreateScoreboard();
            UpdateChallengeText();
            UpdateTimeText();
        }

        private void SetupPlayerControls()
        {
            // Configurar controles para el Jugador 1 (WASD + espacio)
            var player1Keys = new Dictionary<Key, bool>
            {
                { Key.W, false },   // Arriba
                { Key.S, false },   // Abajo
                { Key.A, false },   // Izquierda
                { Key.D, false },   // Derecha
                { Key.Space, false} // Acción/Poder
            };
            playerKeyStates.Add(0, player1Keys);

            // Configurar controles para el Jugador 2 (Flechas + Enter)
            var player2Keys = new Dictionary<Key, bool>
            {
                { Key.Up, false },     // Arriba
                { Key.Down, false },   // Abajo
                { Key.Left, false },   // Izquierda
                { Key.Right, false },  // Derecha
                { Key.Return, false }  // Acción/Poder
            };
            playerKeyStates.Add(1, player2Keys);

            // Si hay más de 2 jugadores, configurar controles adicionales
            if (numberOfPlayers > 2)
            {
                // Jugador 3 (IJKL + U)
                var player3Keys = new Dictionary<Key, bool>
                {
                    { Key.I, false },  // Arriba
                    { Key.K, false },  // Abajo
                    { Key.J, false },  // Izquierda
                    { Key.L, false },  // Derecha
                    { Key.U, false }   // Acción/Poder
                };
                playerKeyStates.Add(2, player3Keys);

                // Jugador 4 (8456 + 7 en NumPad)
                var player4Keys = new Dictionary<Key, bool>
                {
                    { Key.NumPad8, false },  // Arriba
                    { Key.NumPad5, false },  // Abajo
                    { Key.NumPad4, false },  // Izquierda
                    { Key.NumPad6, false },  // Derecha
                    { Key.NumPad7, false }   // Acción/Poder
                };
                playerKeyStates.Add(3, player4Keys);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            float deltaTime = 0.016f; // ~16ms

            // Procesar entrada de teclado para cada jugador
            ProcessKeyboardInput(deltaTime);

            // Actualizar lógica del juego
            gameManager.Update(deltaTime);

            // Actualizar representación visual
            UpdateGameVisuals();

            // Actualizar textos de UI
            UpdateTimeText();
            UpdateChallengeText();
        }

        private void ProcessKeyboardInput(float deltaTime)
        {
            // Procesar entrada de teclado para cada jugador
            foreach (var playerEntry in playerKeyStates)
            {
                int playerId = playerEntry.Key;
                var keyStates = playerEntry.Value;

                if (playerId >= gameManager.Players.Count)
                    continue;

                Player player = gameManager.Players[playerId];

                // Movimiento horizontal
                float moveX = 0;
                if (keyStates[GetMoveLeftKey(playerId)])
                    moveX -= 5.0f * deltaTime;
                if (keyStates[GetMoveRightKey(playerId)])
                    moveX += 5.0f * deltaTime;

                player.PositionX += moveX;

                // Salto
                if (keyStates[GetJumpKey(playerId)] && !player.IsFalling)
                {
                    // Implementar lógica de salto
                    // ...
                }

                // Usar poder
                if (keyStates[GetActionKey(playerId)])
                {
                    // Buscar poder disponible (simplificado para este ejemplo)
                    var forcePush = player.GetPower(PowerType.ForcePush);
                    if (forcePush != null && !forcePush.IsActive)
                    {
                        gameManager.ActivatePower(player, PowerType.ForcePush);
                    }
                }
            }
        }

        private Key GetMoveLeftKey(int playerId)
        {
            switch (playerId)
            {
                case 0: return Key.A;
                case 1: return Key.Left;
                case 2: return Key.J;
                case 3: return Key.NumPad4;
                default: return Key.A;
            }
        }

        private Key GetMoveRightKey(int playerId)
        {
            switch (playerId)
            {
                case 0: return Key.D;
                case 1: return Key.Right;
                case 2: return Key.L;
                case 3: return Key.NumPad6;
                default: return Key.D;
            }
        }

        private Key GetJumpKey(int playerId)
        {
            switch (playerId)
            {
                case 0: return Key.W;
                case 1: return Key.Up;
                case 2: return Key.I;
                case 3: return Key.NumPad8;
                default: return Key.W;
            }
        }

        private Key GetActionKey(int playerId)
        {
            switch (playerId)
            {
                case 0: return Key.Space;
                case 1: return Key.Return;
                case 2: return Key.U;
                case 3: return Key.NumPad7;
                default: return Key.Space;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Actualizar estado de teclas para todos los jugadores
            foreach (var playerEntry in playerKeyStates)
            {
                foreach (var key in playerEntry.Value.Keys.ToList())
                {
                    if (e.Key == key)
                    {
                        playerEntry.Value[key] = true;
                    }
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // Actualizar estado de teclas para todos los jugadores
            foreach (var playerEntry in playerKeyStates)
            {
                foreach (var key in playerEntry.Value.Keys.ToList())
                {
                    if (e.Key == key)
                    {
                        playerEntry.Value[key] = false;
                    }
                }
            }
        }

        private void ExternalControllerManager_ControllerInput(object sender, ControllerInputEventArgs e)
        {
            // Procesar entrada de controlador externo
            if (e.PlayerId >= gameManager.Players.Count)
                return;

            Player player = gameManager.Players[e.PlayerId];

            // Movimiento horizontal
            player.PositionX += e.AxisX * 5.0f;

            // Salto
            if (e.ButtonJump && !player.IsFalling)
            {
                // Implementar lógica de salto
                // ...
            }

            // Usar poder
            if (e.ButtonAction)
            {
                // Activar poder si está disponible
                var forcePush = player.GetPower(PowerType.ForcePush);
                if (forcePush != null && !forcePush.IsActive)
                {
                    gameManager.ActivatePower(player, PowerType.ForcePush);
                }
            }
        }

        private void UpdateGameVisuals()
        {
            // Actualizar posiciones de jugadores
            UpdatePlayerSprites();

            // Actualizar tokens
            UpdateTokens();

            // Actualizar visualización de árboles si está visible
            if (treePanel.Visibility == Visibility.Visible)
            {
                UpdateTreeVisualizations();
            }
        }

        private void UpdatePlayerSprites()
        {
            foreach (var player in gameManager.Players)
            {
                // Obtener o crear sprite del jugador
                if (!playerSprites.ContainsKey(player.Id))
                {
                    // Crear sprite nuevo para el jugador
                    Ellipse playerEllipse = new Ellipse
                    {
                        Width = 30,
                        Height = 30,
                        Fill = new SolidColorBrush(playerColors[player.Id]),
                        Stroke = Brushes.White,
                        StrokeThickness = 2
                    };

                    // Añadir texto con el nombre del jugador
                    TextBlock playerName = new TextBlock
                    {
                        Text = player.Name,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    // Contenedor para el sprite y su nombre
                    Grid playerContainer = new Grid();
                    playerContainer.Children.Add(playerEllipse);

                    // Añadir al canvas
                    gameCanvas.Children.Add(playerContainer);
                    playerSprites[player.Id] = playerContainer;
                }

                // Actualizar posición
                var sprite = playerSprites[player.Id];
                Canvas.SetLeft(sprite, player.PositionX - 15); // Centrar horizontalmente
                Canvas.SetTop(sprite, player.PositionY - 15);  // Centrar verticalmente

                // Efecto visual para el escudo si está activo
                if (player.IsShielded)
                {
                    if (!(sprite is Grid grid && grid.Children.OfType<Ellipse>().Any(e => e.Name == "ShieldEffect")))
                    {
                        Ellipse shield = new Ellipse
                        {
                            Name = "ShieldEffect",
                            Width = 40,
                            Height = 40,
                            Fill = new SolidColorBrush(Color.FromArgb(100, 200, 200, 255)),
                            Stroke = Brushes.LightBlue,
                            StrokeThickness = 2
                        };

                        ((Grid)sprite).Children.Add(shield);
                        Panel.SetZIndex(shield, -1);
                    }
                }
                else
                {
                    // Eliminar efecto de escudo si existe
                    if (sprite is Grid grid)
                    {
                        var shield = grid.Children.OfType<Ellipse>().FirstOrDefault(e => e.Name == "ShieldEffect");
                        if (shield != null)
                        {
                            grid.Children.Remove(shield);
                        }
                    }
                }
            }
        }

        private void UpdateTokens()
        {
            // Eliminar tokens antiguos
            foreach (var token in tokenElements.ToList())
            {
                if (token.Value.IsCollected)
                {
                    gameCanvas.Children.Remove(token.Key);
                    tokenElements.Remove(token.Key);
                }
            }

            // Añadir nuevos tokens y actualizar los existentes
            foreach (var token in gameManager.TokenManager.GetActiveTokens())
            {
                bool exists = tokenElements.Values.Contains(token);

                if (!exists)
                {
                    // Crear nuevo elemento visual para el token
                    TextBlock tokenText = new TextBlock
                    {
                        Text = token.Value.ToString(),
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Ellipse tokenEllipse = new Ellipse
                    {
                        Width = 25,
                        Height = 25,
                        Fill = Brushes.Gold,
                        Stroke = Brushes.DarkGoldenrod,
                        StrokeThickness = 2
                    };

                    Grid tokenContainer = new Grid();
                    tokenContainer.Children.Add(tokenEllipse);
                    tokenContainer.Children.Add(tokenText);

                    gameCanvas.Children.Add(tokenContainer);
                    tokenElements[tokenContainer] = token;
                }

                // Actualizar posición de todos los tokens
                foreach (var entry in tokenElements.ToList())
                {
                    UIElement element = entry.Key;
                    Token tokenData = entry.Value;

                    Canvas.SetLeft(element, tokenData.PositionX - 12.5);
                    Canvas.SetTop(element, tokenData.PositionY - 12.5);
                }
            }
        }

        private void CreateInitialPlatforms()
        {
            // Limpiar plataformas actuales
            foreach (var platform in platformElements.Keys.ToList())
            {
                gameCanvas.Children.Remove(platform);
            }
            platformElements.Clear();

            // Crear plataformas según el GameManager
            foreach (var platform in gameManager.Platforms)
            {
                Rectangle platformRect = new Rectangle
                {
                    Width = platform.Width,
                    Height = platform.Height,
                    Fill = new LinearGradientBrush(
                        Colors.DarkGreen,
                        Colors.ForestGreen,
                        new Point(0, 0),
                        new Point(0, 1)
                    ),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    RadiusX = 5,
                    RadiusY = 5
                };

                gameCanvas.Children.Add(platformRect);
                platformElements[platformRect] = platform;

                Canvas.SetLeft(platformRect, platform.PositionX);
                Canvas.SetTop(platformRect, platform.PositionY);
            }
        }

        private void UpdateTreeVisualizations()
        {
            // Actualizar visualizaciones de árboles para cada jugador
            treeCanvas.Children.Clear();

            // Seleccionar el jugador con el foco actual (por simplicidad, mostramos solo un árbol a la vez)
            var focusPlayer = gameManager.Players.FirstOrDefault();
            if (focusPlayer != null)
            {
                // Crear o actualizar visualizador de árbol
                if (!treeVisualizers.ContainsKey(focusPlayer.Id))
                {
                    treeVisualizers[focusPlayer.Id] = new TreeVisualizer(treeCanvas);
                }

                // Renderizar el árbol actual
                switch (focusPlayer.CurrentTreeType)
                {
                    case TreeType.BST:
                        treeVisualizers[focusPlayer.Id].RenderBinaryTree(focusPlayer.BSTree.Root);
                        break;
                    case TreeType.AVL:
                        treeVisualizers[focusPlayer.Id].RenderBinaryTree(focusPlayer.AVLTree.Root);
                        break;
                    case TreeType.BTree:
                        treeVisualizers[focusPlayer.Id].RenderBTree(focusPlayer.BTree);
                        break;
                }
            }
        }

        private void CreateScoreboard()
        {
            // Limpiar el panel de puntuación
            scorePanel.Children.Clear();

            // Crear un panel para cada jugador
            foreach (var player in gameManager.Players)
            {
                StackPanel playerScorePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10)
                };

                // Círculo con el color del jugador
                Ellipse playerColorEllipse = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = new SolidColorBrush(playerColors[player.Id]),
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Texto con nombre y puntuación
                TextBlock scoreText = new TextBlock
                {
                    Text = $"{player.Name}: 0",
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = player.Id  // Guardamos el ID para actualizaciones
                };

                playerScorePanel.Children.Add(playerColorEllipse);
                playerScorePanel.Children.Add(scoreText);

                scorePanel.Children.Add(playerScorePanel);
            }
        }

        private void UpdateTimeText()
        {
            // Actualizar texto del temporizador
            int minutes = (int)(gameManager.GameTime / 60);
            int seconds = (int)(gameManager.GameTime % 60);
            txtTimer.Text = $"Tiempo: {minutes:00}:{seconds:00}";

            // Alerta visual cuando queda poco tiempo
            if (gameManager.GameDuration - gameManager.GameTime <= 30)
            {
                if (txtTimer.Foreground != Brushes.Red)
                {
                    txtTimer.Foreground = Brushes.Red;

                    // Añadir animación de parpadeo
                    DoubleAnimation blinkAnimation = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(0.5),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    txtTimer.BeginAnimation(OpacityProperty, blinkAnimation);
                }
            }
            else
            {
                txtTimer.Foreground = Brushes.White;
                txtTimer.BeginAnimation(OpacityProperty, null);
            }
        }

        private void UpdateChallengeText()
        {
            // Actualizar texto del reto actual
            txtChallenge.Text = gameManager.CurrentChallenge.Description;
        }

        private void UpdateScoreText(Player player, int newScore)
        {
            // Buscar y actualizar el texto de puntuación para el jugador
            foreach (var panel in scorePanel.Children.OfType<StackPanel>())
            {
                var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock != null && (int)textBlock.Tag == player.Id)
                {
                    textBlock.Text = $"{player.Name}: {newScore}";

                    // Animar cambio de puntuación
                    ScaleTransform scale = new ScaleTransform(1.0, 1.0);
                    textBlock.RenderTransform = scale;

                    DoubleAnimation scaleAnimation = new DoubleAnimation
                    {
                        From = 1.5,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(0.3)
                    };

                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                }
            }
        }

        private void ShowMessage(string message, TimeSpan duration)
        {
            // Mostrar mensaje superpuesto
            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                FontSize = 24,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 500
            };

            messagePanel.Children.Clear();
            messagePanel.Children.Add(messageBlock);
            overlayPanel.Visibility = Visibility.Visible;

            // Ocultar después de la duración especificada
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = duration
            };

            timer.Tick += (s, e) =>
            {
                overlayPanel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };

            timer.Start();
        }

        private void ShowPlayerMessage(Player player, string message)
        {
            // Mostrar mensaje flotante sobre el jugador
            if (!playerSprites.ContainsKey(player.Id))
                return;

            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                Padding = new Thickness(5),
                FontSize = 14
            };

            gameCanvas.Children.Add(messageBlock);

            // Posicionar encima del jugador
            Canvas.SetLeft(messageBlock, player.PositionX - messageBlock.ActualWidth / 2);
            Canvas.SetTop(messageBlock, player.PositionY - 40);

            // Animación de desvanecimiento
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (s, e) =>
            {
                gameCanvas.Children.Remove(messageBlock);
                timer.Stop();
            };

            timer.Start();
        }

        private void ShowGameResults()
        {
            // Crear panel de resultados
            Border resultsPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)),
                BorderBrush = Brushes.Gold,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(20),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 400
            };

            StackPanel resultsContent = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            TextBlock titleBlock = new TextBlock
            {
                Text = "Resultados finales",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            resultsContent.Children.Add(titleBlock);

            // Lista de jugadores ordenados por puntuación
            var orderedPlayers = gameManager.Players.OrderByDescending(p => p.Score).ToList();

            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                var player = orderedPlayers[i];

                // Fila de resultado
                Grid resultRow = new Grid
                {
                    Margin = new Thickness(0, 5, 0, 5)
                };

                resultRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                resultRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                resultRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Posición
                // Posición
                TextBlock positionBlock = new TextBlock
                {
                    Text = $"{i + 1}.",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = i == 0 ? Brushes.Gold : Brushes.White,
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Nombre del jugador con color
                StackPanel playerInfo = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                Ellipse playerColorDot = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(playerColors[player.Id]),
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock playerNameBlock = new TextBlock
                {
                    Text = player.Name,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                playerInfo.Children.Add(playerColorDot);
                playerInfo.Children.Add(playerNameBlock);

                // Puntuación
                TextBlock scoreBlock = new TextBlock
                {
                    Text = player.Score.ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Añadir a la fila
                Grid.SetColumn(positionBlock, 0);
                Grid.SetColumn(playerInfo, 1);
                Grid.SetColumn(scoreBlock, 2);

                resultRow.Children.Add(positionBlock);
                resultRow.Children.Add(playerInfo);
                resultRow.Children.Add(scoreBlock);

                resultsContent.Children.Add(resultRow);

                // Añadir corona al ganador
                if (i == 0)
                {
                    TextBlock crownBlock = new TextBlock
                    {
                        Text = "👑",
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    resultsContent.Children.Insert(1, crownBlock);
                }
            }

            // Botones
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            Button playAgainButton = new Button
            {
                Content = "Jugar de nuevo",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.Green)
            };

            Button mainMenuButton = new Button
            {
                Content = "Menú principal",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(5)
            };

            playAgainButton.Click += (s, e) => { RestartGame(); };
            mainMenuButton.Click += (s, e) => { ShowMainMenu(); };

            buttonPanel.Children.Add(playAgainButton);
            buttonPanel.Children.Add(mainMenuButton);
            resultsContent.Children.Add(buttonPanel);

            resultsPanel.Child = resultsContent;

            // Mostrar en la superposición
            messagePanel.Children.Clear();
            messagePanel.Children.Add(resultsPanel);
            overlayPanel.Visibility = Visibility.Visible;
        }

        private void RestartGame()
        {
            // Reiniciar juego
            gameManager = new GameManager(numberOfPlayers, (float)gameCanvas.ActualWidth, (float)gameCanvas.ActualHeight);

            // Limpiar y reiniciar visuales
            gameCanvas.Children.Clear();
            playerSprites.Clear();
            tokenElements.Clear();
            platformElements.Clear();

            // Suscribirse a eventos nuevamente
            gameManager.OnChallengeCompleted += GameManager_OnChallengeCompleted;
            gameManager.OnPowerAcquired += GameManager_OnPowerAcquired;
            gameManager.OnScoreChanged += GameManager_OnScoreChanged;
            gameManager.OnPlayerFell += GameManager_OnPlayerFell;
            gameManager.OnGameEnded += GameManager_OnGameEnded;

            // Reiniciar UI
            CreateScoreboard();
            CreateInitialPlatforms();
            UpdateChallengeText();
            UpdateTimeText();

            // Ocultar superposiciones
            overlayPanel.Visibility = Visibility.Collapsed;

            // Iniciar el juego
            gameManager.StartGame();
            gameTimer.Start();
        }

        private void ShowMainMenu()
        {
            // En una implementación real, esto podría mostrar un menú principal
            // Por ahora, simplemente reinicia el juego después de ocultar los resultados
            overlayPanel.Visibility = Visibility.Collapsed;
            ShowSettingsMenu();
        }

        private void ShowSettingsMenu()
        {
            // Mostrar panel de configuración
            overlayPanel.Visibility = Visibility.Visible;
            messagePanel.Children.Clear();

            Border settingsPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)),
                BorderBrush = Brushes.SkyBlue,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(20),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 400
            };

            StackPanel settingsContent = new StackPanel
            {
                Margin = new Thickness(10)
            };

            // Título
            TextBlock titleBlock = new TextBlock
            {
                Text = "Configuración",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.SkyBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            settingsContent.Children.Add(titleBlock);

            // Número de jugadores
            TextBlock playersLabel = new TextBlock
            {
                Text = "Número de jugadores:",
                FontSize = 16,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };

            StackPanel playerButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 15)
            };

            for (int i = 2; i <= 4; i++)
            {
                int playerNum = i;
                Button playerButton = new Button
                {
                    Content = $"{i} Jugadores",
                    Width = 100,
                    Margin = new Thickness(5),
                    Background = numberOfPlayers == i ?
                        new SolidColorBrush(Colors.Green) :
                        new SolidColorBrush(Colors.Gray)
                };

                playerButton.Click += (s, e) =>
                {
                    // Actualizar selección
                    numberOfPlayers = playerNum;

                    // Cambiar colores de todos los botones
                    foreach (var btn in playerButtons.Children.OfType<Button>())
                    {
                        btn.Background = new SolidColorBrush(Colors.Gray);
                    }

                    // Resaltar el seleccionado
                    playerButton.Background = new SolidColorBrush(Colors.Green);
                };

                playerButtons.Children.Add(playerButton);
            }

            settingsContent.Children.Add(playersLabel);
            settingsContent.Children.Add(playerButtons);

            // Opción para usar controles externos
            CheckBox externalControllersCheckbox = new CheckBox
            {
                Content = "Usar controles externos (gamepads)",
                Foreground = Brushes.White,
                IsChecked = useExternalControllers,
                Margin = new Thickness(0, 0, 0, 15)
            };

            externalControllersCheckbox.Checked += (s, e) => { useExternalControllers = true; };
            externalControllersCheckbox.Unchecked += (s, e) => { useExternalControllers = false; };

            settingsContent.Children.Add(externalControllersCheckbox);

            // Botones
            StackPanel actionButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            Button startGameButton = new Button
            {
                Content = "Iniciar juego",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.Green)
            };

            startGameButton.Click += (s, e) =>
            {
                overlayPanel.Visibility = Visibility.Collapsed;
                RestartGame();
            };

            actionButtons.Children.Add(startGameButton);
            settingsContent.Children.Add(actionButtons);

            settingsPanel.Child = settingsContent;
            messagePanel.Children.Add(settingsPanel);
        }

        #region Event Handlers

        private void GameManager_OnChallengeCompleted(Player player, TreeChallenge challenge)
        {
            // Mostrar mensaje de reto completado
            string message = $"{player.Name} ha completado el reto: {challenge.Description}";
            ShowPlayerMessage(player, "¡Reto completado!");

            // Actualizar pantalla de reto
            UpdateChallengeText();
        }

        private void GameManager_OnPowerAcquired(Player player, Power power)
        {
            // Mostrar mensaje de poder adquirido
            ShowPlayerMessage(player, $"¡{power.Name}!");
        }

        private void GameManager_OnScoreChanged(Player player, int newScore)
        {
            // Actualizar puntuación
            UpdateScoreText(player, newScore);
        }

        private void GameManager_OnPlayerFell(Player player)
        {
            // Mostrar mensaje de caída
            ShowPlayerMessage(player, "¡Has caído!");
        }

        private void GameManager_OnGameEnded()
        {
            // Detener el temporizador del juego
            gameTimer.Stop();

            // Mostrar resultados
            ShowGameResults();
        }

        #endregion

        #region Button Event Handlers

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Iniciar el juego
            if (!gameTimer.IsEnabled)
            {
                gameManager.StartGame();
                gameTimer.Start();

                // Crear plataformas iniciales
                CreateInitialPlatforms();

                // Cambiar texto del botón
                btnStart.Content = "Pausar";
            }
            else
            {
                // Pausar el juego
                gameTimer.Stop();
                btnStart.Content = "Continuar";
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            // Si el juego está en marcha, pausarlo
            if (gameTimer.IsEnabled)
            {
                gameTimer.Stop();
                btnStart.Content = "Continuar";
            }

            // Mostrar menú de configuración
            ShowSettingsMenu();
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            // Confirmar salida
            MessageBoxResult result = MessageBox.Show(
                "¿Estás seguro de que quieres salir del juego?",
                "Confirmar salida",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }

    #region Helper Classes

    // Clase para visualizar árboles
    public class TreeVisualizer
    {
        private Canvas canvas;
        private const int NodeRadius = 20;
        private const int HorizontalSpacing = 50;
        private const int VerticalSpacing = 50;

        public TreeVisualizer(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void RenderBinaryTree<T>(TreeNode<T> root) where T : IComparable<T>
        {
            if (root == null)
                return;

            canvas.Children.Clear();
            int maxDepth = CalculateDepth(root);
            int totalWidth = (int)Math.Pow(2, maxDepth) * HorizontalSpacing;

            DrawNode(root, totalWidth / 2, 30, totalWidth / 4);
        }

        public void RenderBTree<T>(BTree<T> tree) where T : IComparable<T>
        {
            // Esta implementación sería más compleja y requeriría acceso a detalles internos del BTree
            // Para una demostración, mostraremos solo un mensaje

            TextBlock message = new TextBlock
            {
                Text = $"B-Tree (Altura: {tree.GetHeight()})",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 16
            };

            canvas.Children.Add(message);
            Canvas.SetLeft(message, 10);
            Canvas.SetTop(message, 10);
        }

        private void DrawNode<T>(TreeNode<T> node, int x, int y, int offset) where T : IComparable<T>
        {
            // Dibujar círculo
            Ellipse nodeCircle = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2
            };

            canvas.Children.Add(nodeCircle);
            Canvas.SetLeft(nodeCircle, x - NodeRadius);
            Canvas.SetTop(nodeCircle, y - NodeRadius);

            // Dibujar valor
            TextBlock valueText = new TextBlock
            {
                Text = node.Value.ToString(),
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            canvas.Children.Add(valueText);

            // Centrar texto en el nodo
            double textX = x - valueText.ActualWidth / 2;
            double textY = y - valueText.ActualHeight / 2;
            Canvas.SetLeft(valueText, textX);
            Canvas.SetTop(valueText, textY);

            // Dibujar hijos y conexiones
            if (node.Left != null)
            {
                int childX = x - offset;
                int childY = y + VerticalSpacing;

                // Línea de conexión
                Line leftLine = new Line
                {
                    X1 = x,
                    Y1 = y + NodeRadius,
                    X2 = childX,
                    Y2 = childY - NodeRadius,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2
                };

                canvas.Children.Add(leftLine);

                // Dibujar hijo izquierdo
                DrawNode(node.Left, childX, childY, offset / 2);
            }

            if (node.Right != null)
            {
                int childX = x + offset;
                int childY = y + VerticalSpacing;

                // Línea de conexión
                Line rightLine = new Line
                {
                    X1 = x,
                    Y1 = y + NodeRadius,
                    X2 = childX,
                    Y2 = childY - NodeRadius,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2
                };

                canvas.Children.Add(rightLine);

                // Dibujar hijo derecho
                DrawNode(node.Right, childX, childY, offset / 2);
            }
        }

        private int CalculateDepth<T>(TreeNode<T> node) where T : IComparable<T>
        {
            if (node == null)
                return 0;

            int leftDepth = CalculateDepth(node.Left);
            int rightDepth = CalculateDepth(node.Right);

            return Math.Max(leftDepth, rightDepth) + 1;
        }
    }

    // Clase para gestionar controles externos
    public class ExternalControllerManager
    {
        public event EventHandler<ControllerInputEventArgs> ControllerInput;
        private List<IExternalController> controllers;

        public ExternalControllerManager()
        {
            controllers = new List<IExternalController>();
        }

        public void Initialize(int numberOfControllers)
        {
            // En una implementación real, aquí se detectarían y configurarían los controladores conectados
            // Este es un ejemplo simplificado que simularía la conexión
            for (int i = 0; i < numberOfControllers; i++)
            {
                try
                {
                    // Se intentaría crear una conexión real con un controlador
                    IExternalController controller = new GamepadController(i);
                    controller.InputReceived += Controller_InputReceived;
                    controllers.Add(controller);
                }
                catch (Exception ex)
                {
                    // Registrar error de conexión
                    System.Diagnostics.Debug.WriteLine($"No se pudo inicializar el controlador {i}: {ex.Message}");
                }
            }
        }

        private void Controller_InputReceived(object sender, ControllerInputEventArgs e)
        {
            // Reenviar el evento a quien esté suscrito
            ControllerInput?.Invoke(this, e);
        }

        public void Shutdown()
        {
            // Liberar recursos de los controladores
            foreach (var controller in controllers)
            {
                controller.Dispose();
            }
            controllers.Clear();
        }
    }

    // Interfaces y clases para la abstracción de controles externos
    public interface IExternalController : IDisposable
    {
        event EventHandler<ControllerInputEventArgs> InputReceived;
        int ControllerId { get; }
        void Update();  // Método que se llamaría en cada frame para procesar entradas
    }

    public class ControllerInputEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public float AxisX { get; set; }  // -1.0 a 1.0
        public float AxisY { get; set; }  // -1.0 a 1.0
        public bool ButtonJump { get; set; }
        public bool ButtonAction { get; set; }
        public bool ButtonSpecial { get; set; }
    }

    public class GamepadController : IExternalController
    {
        public event EventHandler<ControllerInputEventArgs> InputReceived;
        public int ControllerId { get; private set; }
        private bool isRunning;
        private System.Threading.Thread pollingThread;

        public GamepadController(int id)
        {
            ControllerId = id;

            // En una implementación real, aquí se inicializaría la conexión con el gamepad
            // Usando una biblioteca como SharpDX.XInput, SlimDX, o similar

            // Iniciar hilo de polling (para simulación)
            isRunning = true;
            pollingThread = new System.Threading.Thread(PollingLoop);
            pollingThread.IsBackground = true;
            pollingThread.Start();
        }

        private void PollingLoop()
        {
            Random random = new Random();
            while (isRunning)
            {
                // En una implementación real, aquí se leerían los datos del controlador
                // Esta es una simulación para testing que crea entradas aleatorias ocasionales
                if (random.Next(100) < 5)  // 5% de probabilidad de generar input
                {
                    var args = new ControllerInputEventArgs
                    {
                        PlayerId = ControllerId,
                        AxisX = (float)random.NextDouble() * 2 - 1,  // -1 a 1
                        AxisY = (float)random.NextDouble() * 2 - 1,  // -1 a 1
                        ButtonJump = random.Next(100) < 10,          // 10% de probabilidad
                        ButtonAction = random.Next(100) < 5,         // 5% de probabilidad
                        ButtonSpecial = random.Next(100) < 3         // 3% de probabilidad
                    };

                    // Disparar evento en el hilo de UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InputReceived?.Invoke(this, args);
                    });
                }

                System.Threading.Thread.Sleep(16);  // ~60 Hz
            }
        }

        public void Update()
        {
            // En una implementación real con polling sincrónico, aquí se actualizarían los datos
        }

        public void Dispose()
        {
            isRunning = false;
            if (pollingThread != null && pollingThread.IsAlive)
            {
                pollingThread.Join(1000);  // Esperar hasta 1 segundo para que termine
            }

            // Liberar recursos del controlador en una implementación real
        }
    }

    // Clase que representa un reto de árbol
    public class TreeChallenge
    {
        public string Description { get; private set; }
        public TreeType TreeType { get; private set; }
        private Func<object, bool> validationFunction;

        public TreeChallenge(string description, TreeType treeType, Func<object, bool> validationFunction)
        {
            Description = description;
            TreeType = treeType;
            this.validationFunction = validationFunction;
        }

        public bool CheckCompletion<T>(object tree)
        {
            return validationFunction(tree);
        }

        public static TreeChallenge GenerateRandomChallenge()
        {
            Random random = new Random();
            int challengeType = random.Next(0, 6);  // 6 tipos de desafíos

            switch (challengeType)
            {
                case 0:
                    return new TreeChallenge(
                        "Construye un BST con al menos 5 nodos",
                        TreeType.BST,
                        tree => {
                            var bst = tree as BinarySearchTree<int>;
                            return bst != null && CountNodes(bst.Root) >= 5;
                        });
                case 1:
                    return new TreeChallenge(
                        "Construye un BST con altura menor a 4",
                        TreeType.BST,
                        tree => {
                            var bst = tree as BinarySearchTree<int>;
                            return bst != null && bst.GetHeight() < 4;
                        });
                case 2:
                    return new TreeChallenge(
                        "Construye un árbol AVL con al menos 7 nodos",
                        TreeType.AVL,
                        tree => {
                            var avl = tree as AVLTree<int>;
                            return avl != null && CountNodes(avl.Root) >= 7;
                        });
                case 3:
                    return new TreeChallenge(
                        "Construye un árbol AVL balanceado con altura exacta de 3",
                        TreeType.AVL,
                        tree => {
                            var avl = tree as AVLTree<int>;
                            return avl != null && avl.GetHeight() == 3;
                        });
                case 4:
                    return new TreeChallenge(
                        "Construye un B-Tree con al menos 3 nodos",
                        TreeType.BTree,
                        tree => {
                            var btree = tree as BTree<int>;
                            return btree != null && btree.GetSize() >= 3;
                        });
                default:
                    return new TreeChallenge(
                        "Construye un B-Tree con altura de al menos 2",
                        TreeType.BTree,
                        tree => {
                            var btree = tree as BTree<int>;
                            return btree != null && btree.GetHeight() >= 2;
                        });
            }
        }

        private static int CountNodes<T>(TreeNode<T> root) where T : IComparable<T>
        {
            if (root == null)
                return 0;
            return 1 + CountNodes(root.Left) + CountNodes(root.Right);
        }
    }

    #endregion
}
