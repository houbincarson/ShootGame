using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShootGame
{
    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }
    
    public class Game
    {
        // 游戏属性
        public Rectangle Bounds { get; private set; }
        public GameState State { get; private set; }
        public List<Player> Players { get; private set; }
        public List<Bullet> Bullets { get; private set; }
        public List<Obstacle> Obstacles { get; private set; } // 障碍物列表
        public int WinnerPlayerNumber { get; private set; }
        
        // 背景元素
        private List<Star> stars;
        private Random random;
        
        // 构造函数
        public Game(Rectangle bounds)
        {
            Bounds = bounds;
            State = GameState.Start;
            Players = new List<Player>();
            Bullets = new List<Bullet>();
            Obstacles = new List<Obstacle>(); // 初始化障碍物列表
            stars = new List<Star>();
            random = new Random();
            
            // 初始化星星
            InitStars();
            
            // 初始化玩家
            InitPlayers();
        }
        
        // 初始化背景星星
        private void InitStars()
        {
            stars.Clear();
            int starCount = 100;
            
            for (int i = 0; i < starCount; i++)
            {
                float x = random.Next(Bounds.Width);
                float y = random.Next(Bounds.Height);
                float size = 1 + (float)random.NextDouble() * 3;
                
                // 随机星星颜色
                Color starColor;
                int colorType = random.Next(3);
                if (colorType == 0)
                    starColor = Color.FromArgb(255, 255, 255); // 白色
                else if (colorType == 1)
                    starColor = Color.FromArgb(200, 200, 255); // 蓝白色
                else
                    starColor = Color.FromArgb(255, 255, 200); // 黄白色
                
                stars.Add(new Star(new PointF(x, y), size, starColor));
            }
        }
        
        // 初始化玩家
        private void InitPlayers()
        {
            Players.Clear();
            
            // 玩家1（左侧）
            PointF player1Pos = new PointF(100, Bounds.Height / 2 - 20);
            Color player1Color = Color.FromArgb(255, 41, 128, 185); // 蓝色
            Player player1 = new Player(player1Pos, 5.0f, player1Color, 1, new Size(40, 40));
            Players.Add(player1);
            
            // 玩家2（右侧）
            PointF player2Pos = new PointF(Bounds.Width - 100 - 40, Bounds.Height / 2 - 20);
            Color player2Color = Color.FromArgb(255, 231, 76, 60); // 红色
            Player player2 = new Player(player2Pos, 5.0f, player2Color, 2, new Size(40, 40));
            Players.Add(player2);
        }
        
        // 初始化障碍物
        private void InitObstacles()
        {
            Obstacles.Clear();
            
            // 生成随机障碍物
            int obstacleCount = random.Next(8, 13); // 8-12个障碍物
            
            for (int i = 0; i < obstacleCount; i++)
            {
                // 随机位置（避开玩家初始位置）
                int x, y;
                Rectangle obstacleBounds;
                bool overlaps;
                
                do {
                    overlaps = false;
                    
                    // 随机位置
                    x = random.Next(50, Bounds.Width - 100);
                    y = random.Next(50, Bounds.Height - 100);
                    
                    // 随机大小（根据障碍物类型调整）
                    int width = random.Next(40, 100);
                    int height = random.Next(40, 100);
                    
                    obstacleBounds = new Rectangle(x, y, width, height);
                    
                    // 检查是否与玩家初始位置重叠
                    foreach (Player player in Players)
                    {
                        Rectangle playerBounds = new Rectangle(
                            (int)player.Position.X - 100, // 给玩家周围留出空间
                            (int)player.Position.Y - 100,
                            player.Size.Width + 200,
                            player.Size.Height + 200);
                            
                        if (obstacleBounds.IntersectsWith(playerBounds))
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    
                    // 检查是否与其他障碍物重叠
                    foreach (Obstacle obstacle in Obstacles)
                    {
                        if (obstacleBounds.IntersectsWith(obstacle.GetBounds()))
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    
                } while (overlaps);
                
                // 创建障碍物（随机类型）
                Obstacle newObstacle = null;
                int obstacleType = random.Next(5); // 0-4对应5种障碍物类型
                
                switch (obstacleType)
                {
                    case 0: // 假山
                        newObstacle = new Mountain(new PointF(x, y), new SizeF(obstacleBounds.Width, obstacleBounds.Height));
                        break;
                    case 1: // 河流
                        newObstacle = new River(new PointF(x, y), new SizeF(obstacleBounds.Width, obstacleBounds.Height));
                        break;
                    case 2: // 树木
                        newObstacle = new Tree(new PointF(x, y), new SizeF(obstacleBounds.Width, obstacleBounds.Height));
                        break;
                    case 3: // 沙场
                        newObstacle = new Sand(new PointF(x, y), new SizeF(obstacleBounds.Width, obstacleBounds.Height));
                        break;
                    case 4: // 草地
                        newObstacle = new Grass(new PointF(x, y), new SizeF(obstacleBounds.Width, obstacleBounds.Height));
                        break;
                }
                
                if (newObstacle != null)
                {
                    Obstacles.Add(newObstacle);
                }
            }
        }
        
        // 开始新游戏
        public void StartNewGame()
        {
            State = GameState.Playing;
            Bullets.Clear();
            
            // 重置玩家
            InitPlayers();
            
            // 初始化障碍物
            InitObstacles();
        }
        
        // 更新游戏状态
        public void Update()
        {
            // 更新星星
            foreach (Star star in stars)
            {
                star.Twinkle();
            }
            
            if (State == GameState.Playing)
            {
                // 更新玩家
                foreach (Player player in Players)
                {
                    // 重置速度修改器（每帧重置，由障碍物重新应用）
                    player.ResetSpeedModifier();
                    
                    // 移动玩家
                    player.Move(Bounds);
                    
                    // 检查玩家与障碍物的碰撞
                    foreach (Obstacle obstacle in Obstacles)
                    {
                        if (obstacle.CheckCollision(player))
                        {
                            // 应用障碍物效果
                            obstacle.ApplyEffect(player);
                            
                            // 如果是不可穿过的障碍物，阻止玩家移动
                            if (!obstacle.IsPassable)
                            {
                                // 简单的碰撞响应：将玩家推出障碍物
                                Rectangle playerBounds = player.GetBounds();
                                Rectangle obstacleBounds = obstacle.GetBounds();
                                
                                // 计算重叠区域
                                Rectangle overlap = Rectangle.Intersect(playerBounds, obstacleBounds);
                                
                                // 根据重叠区域的宽高比确定推动方向
                                if (overlap.Width < overlap.Height)
                                {
                                    // 水平方向推动
                                    if (playerBounds.X < obstacleBounds.X)
                                    {
                                        // 玩家在障碍物左侧
                                        player.Position = new PointF(
                                            obstacleBounds.X - playerBounds.Width,
                                            player.Position.Y
                                        );
                                    }
                                    else
                                    {
                                        // 玩家在障碍物右侧
                                        player.Position = new PointF(
                                            obstacleBounds.Right,
                                            player.Position.Y
                                        );
                                    }
                                }
                                else
                                {
                                    // 垂直方向推动
                                    if (playerBounds.Y < obstacleBounds.Y)
                                    {
                                        // 玩家在障碍物上方
                                        player.Position = new PointF(
                                            player.Position.X,
                                            obstacleBounds.Y - playerBounds.Height
                                        );
                                    }
                                    else
                                    {
                                        // 玩家在障碍物下方
                                        player.Position = new PointF(
                                            player.Position.X,
                                            obstacleBounds.Bottom
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 更新子弹
                List<Bullet> bulletsToRemove = new List<Bullet>();
                foreach (Bullet bullet in Bullets)
                {
                    bullet.Move();
                    
                    // 检查子弹是否超出边界
                    if (bullet.IsOutOfBounds(Bounds))
                    {
                        bulletsToRemove.Add(bullet);
                        continue;
                    }
                    
                    // 检查子弹与障碍物的碰撞
                    foreach (Obstacle obstacle in Obstacles)
                    {
                        if (obstacle.CheckCollision(bullet))
                        {
                            // 如果障碍物不可穿过，移除子弹
                            if (!obstacle.IsPassable)
                            {
                                bulletsToRemove.Add(bullet);
                                break;
                            }
                        }
                    }
                    
                    // 检查子弹与玩家的碰撞
                    foreach (Player player in Players)
                    {
                        // 子弹不能击中发射它的玩家
                        if (player.PlayerNumber == bullet.PlayerNumber)
                            continue;
                            
                        if (bullet.GetBounds().IntersectsWith(player.GetBounds()))
                        {
                            // 玩家被击中，减少生命值
                            player.Health -= 10;
                            
                            // 增加射击玩家的分数
                            Players[bullet.PlayerNumber - 1].Score += 10;
                            
                            // 移除子弹
                            bulletsToRemove.Add(bullet);
                            break;
                        }
                    }
                }
                
                // 移除标记的子弹
                foreach (Bullet bullet in bulletsToRemove)
                {
                    Bullets.Remove(bullet);
                }
                
                // 检查游戏是否结束
                CheckGameOver();
                
                // 更新可动画的障碍物（如河流）
                foreach (Obstacle obstacle in Obstacles)
                {
                    if (obstacle is River)
                    {
                        ((River)obstacle).Update();
                    }
                }
            }
        }
        
        // 检查游戏是否结束
        private void CheckGameOver()
        {
            foreach (Player player in Players)
            {
                if (player.Health <= 0)
                {
                    State = GameState.GameOver;
                    WinnerPlayerNumber = (player.PlayerNumber == 1) ? 2 : 1;
                    break;
                }
            }
        }
        
        // 处理玩家射击
        public void PlayerShoot(int playerNumber)
        {
            if (State != GameState.Playing || playerNumber < 1 || playerNumber > Players.Count)
                return;
                
            Player player = Players[playerNumber - 1];
            Bullet bullet = player.Shoot();
            
            if (bullet != null)
            {
                Bullets.Add(bullet);
            }
        }
        
        // 绘制游戏
        public void Draw(Graphics g)
        {
            // 设置高质量绘图
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            // 绘制背景
            DrawBackground(g);
            
            // 根据游戏状态绘制不同的屏幕
            switch (State)
            {
                case GameState.Start:
                    DrawStartScreen(g);
                    break;
                case GameState.Playing:
                    DrawGameScreen(g);
                    break;
                case GameState.GameOver:
                    DrawGameScreen(g); // 先绘制游戏屏幕
                    DrawGameOverScreen(g); // 再绘制游戏结束覆盖层
                    break;
            }
        }
        
        // 绘制背景
        private void DrawBackground(Graphics g)
        {
            // 绘制深色渐变背景
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                Bounds, Color.FromArgb(20, 20, 40), Color.FromArgb(10, 10, 30), 
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bgBrush, Bounds);
            }
            
            // 绘制星星
            foreach (Star star in stars)
            {
                star.Draw(g);
            }
            
            // 绘制网格线（可选）
            DrawGrid(g);
        }
        
        // 绘制网格线
        private void DrawGrid(Graphics g)
        {
            int gridSize = 50;
            using (Pen gridPen = new Pen(Color.FromArgb(30, 100, 100, 150), 1))
            {
                // 绘制水平线
                for (int y = 0; y < Bounds.Height; y += gridSize)
                {
                    g.DrawLine(gridPen, 0, y, Bounds.Width, y);
                }
                
                // 绘制垂直线
                for (int x = 0; x < Bounds.Width; x += gridSize)
                {
                    g.DrawLine(gridPen, x, 0, x, Bounds.Height);
                }
            }
        }
        
        // 绘制开始屏幕
        private void DrawStartScreen(Graphics g)
        {
            string title = "双人射击游戏";
            string instruction = "按空格键开始游戏";
            string controls = "玩家1: WASD移动, Enter射击\n玩家2: 方向键移动, F射击";
            
            // 创建标题面板
            Rectangle titlePanel = new Rectangle(
                Bounds.Width / 2 - 200,
                Bounds.Height / 3 - 40,
                400,
                80
            );
            
            using (GraphicsPath path = CreateRoundedRectangle(titlePanel, 15))
            using (LinearGradientBrush panelBrush = new LinearGradientBrush(
                titlePanel, Color.FromArgb(180, 60, 20, 120), Color.FromArgb(180, 20, 60, 120), LinearGradientMode.ForwardDiagonal))
            {
                g.FillPath(panelBrush, path);
                
                // 绘制边框
                using (Pen borderPen = new Pen(Color.FromArgb(200, 150, 150, 250), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }
            
            using (Font titleFont = new Font("Arial", 28, FontStyle.Bold))
            using (Font instructionFont = new Font("Arial", 16))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                // 绘制标题
                RectangleF titleRect = new RectangleF(
                    titlePanel.X,
                    titlePanel.Y,
                    titlePanel.Width,
                    titlePanel.Height
                );
                
                g.DrawString(title, titleFont, Brushes.White, titleRect, format);
                
                // 绘制指令
                RectangleF instructionRect = new RectangleF(
                    Bounds.Width / 2 - 150,
                    Bounds.Height / 2,
                    300,
                    40
                );
                
                g.DrawString(instruction, instructionFont, Brushes.White, instructionRect, format);
                
                // 绘制控制说明
                RectangleF controlsRect = new RectangleF(
                    Bounds.Width / 2 - 200,
                    Bounds.Height / 2 + 50,
                    400,
                    80
                );
                
                g.DrawString(controls, instructionFont, Brushes.White, controlsRect, format);
            }
        }
        
        // 绘制游戏屏幕
        private void DrawGameScreen(Graphics g)
        {
            // 绘制障碍物
            foreach (Obstacle obstacle in Obstacles)
            {
                obstacle.Draw(g);
            }
            
            // 绘制子弹
            foreach (Bullet bullet in Bullets)
            {
                bullet.Draw(g);
            }
            
            // 绘制玩家
            foreach (Player player in Players)
            {
                player.Draw(g);
            }
            
            // 绘制UI
            DrawUI(g);
        }
        
        // 绘制游戏结束屏幕
        private void DrawGameOverScreen(Graphics g)
        {
            // 绘制半透明覆盖层
            using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 30)))
            {
                g.FillRectangle(overlayBrush, Bounds);
            }
            
            string gameOver = "游戏结束";
            string winner = $"玩家 {WinnerPlayerNumber} 获胜!";
            string restart = "按空格键重新开始";
            
            // 创建游戏结束面板
            Rectangle gameOverPanel = new Rectangle(
                Bounds.Width / 2 - 200,
                Bounds.Height / 3 - 40,
                400,
                200
            );
            
            using (GraphicsPath path = CreateRoundedRectangle(gameOverPanel, 20))
            using (LinearGradientBrush panelBrush = new LinearGradientBrush(
                gameOverPanel, 
                Color.FromArgb(200, 40, 0, 80), 
                Color.FromArgb(200, 80, 0, 40), 
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillPath(panelBrush, path);
                
                // 绘制边框
                using (Pen borderPen = new Pen(Color.FromArgb(200, 150, 150, 250), 2))
                {
                    g.DrawPath(borderPen, path);
                }
            }
            
            // 获取获胜者颜色
            Color winnerColor = Players[WinnerPlayerNumber - 1].Color;
            
            using (Font gameOverFont = new Font("Arial", 28, FontStyle.Bold))
            using (Font winnerFont = new Font("Arial", 24, FontStyle.Bold))
            using (Font restartFont = new Font("Arial", 16))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                // 绘制游戏结束文本
                RectangleF gameOverRect = new RectangleF(
                    gameOverPanel.X,
                    gameOverPanel.Y + 20,
                    gameOverPanel.Width,
                    40
                );
                g.DrawString(gameOver, gameOverFont, Brushes.White, gameOverRect, format);
                
                // 绘制获胜者文本
                RectangleF winnerRect = new RectangleF(
                    gameOverPanel.X,
                    gameOverPanel.Y + 80,
                    gameOverPanel.Width,
                    40
                );
                using (SolidBrush winnerBrush = new SolidBrush(winnerColor))
                {
                    g.DrawString(winner, winnerFont, winnerBrush, winnerRect, format);
                }
                
                // 绘制重新开始文本
                RectangleF restartRect = new RectangleF(
                    gameOverPanel.X,
                    gameOverPanel.Y + 140,
                    gameOverPanel.Width,
                    40
                );
                g.DrawString(restart, restartFont, Brushes.White, restartRect, format);
            }
        }
        
        // 绘制UI（生命值和分数）
        private void DrawUI(Graphics g)
        {
            int padding = 15;
            int barHeight = 20;
            int barWidth = 200;
            
            // 绘制玩家1的UI（左上角）
            DrawPlayerUI(g, Players[0], new Point(padding, padding), barWidth, barHeight);
            
            // 绘制玩家2的UI（右上角）
            DrawPlayerUI(g, Players[1], new Point(Bounds.Width - barWidth - padding, padding), barWidth, barHeight);
        }
        
        // 绘制单个玩家的UI
        private void DrawPlayerUI(Graphics g, Player player, Point position, int barWidth, int barHeight)
        {
            // 创建UI面板
            Rectangle panelRect = new Rectangle(
                position.X - 10,
                position.Y - 10,
                barWidth + 20,
                90
            );
            
            // 绘制半透明面板背景
            using (GraphicsPath path = CreateRoundedRectangle(panelRect, 10))
            using (SolidBrush panelBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 30)))
            {
                g.FillPath(panelBrush, path);
                
                // 绘制面板边框
                using (Pen borderPen = new Pen(Color.FromArgb(150, player.Color), 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }
            
            // 绘制玩家标签
            string playerLabel = $"玩家 {player.PlayerNumber}";
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(player.Color))
            {
                g.DrawString(playerLabel, font, textBrush, position.X, position.Y);
            }
            
            // 绘制生命值条背景
            Rectangle healthBarBg = new Rectangle(position.X, position.Y + 25, barWidth, barHeight);
            using (GraphicsPath path = CreateRoundedRectangle(healthBarBg, 5))
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(100, 50, 50, 50)))
            {
                g.FillPath(bgBrush, path);
            }
            
            // 绘制生命值条
            int healthWidth = (int)(barWidth * (player.Health / 100.0f));
            if (healthWidth > 0)
            {
                Rectangle healthBar = new Rectangle(position.X, position.Y + 25, healthWidth, barHeight);
                using (GraphicsPath path = CreateRoundedRectangle(healthBar, 5))
                using (LinearGradientBrush healthBrush = new LinearGradientBrush(
                    healthBar, player.Color, GetLighterColor(player.Color), LinearGradientMode.Vertical))
                {
                    g.FillPath(healthBrush, path);
                }
            }
            
            // 绘制生命值文本
            string healthText = $"生命: {player.Health}";
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                RectangleF textRect = new RectangleF(
                    position.X, position.Y + 25, barWidth, barHeight);
                g.DrawString(healthText, font, textBrush, textRect, format);
            }
            
            // 绘制分数
            string scoreText = $"分数: {player.Score}";
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(scoreText, font, textBrush, position.X, position.Y + 50);
            }
        }
        
        // 创建圆角矩形路径
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            // 左上角弧
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // 顶边
            path.AddLine(rect.X + radius, rect.Y, rect.Right - radius, rect.Y);
            // 右上角弧
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // 右边
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom - radius);
            // 右下角弧
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // 底边
            path.AddLine(rect.Right - radius, rect.Bottom, rect.X + radius, rect.Bottom);
            // 左下角弧
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            // 左边
            path.AddLine(rect.X, rect.Bottom - radius, rect.X, rect.Y + radius);
            
            path.CloseFigure();
            return path;
        }
        
        // 获取更亮的颜色
        private Color GetLighterColor(Color color)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + 70),
                Math.Min(255, color.G + 70),
                Math.Min(255, color.B + 70)
            );
        }
    }
    
    // 背景星星类
    public class Star
    {
        public PointF Position { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public float Alpha { get; set; }
        public float AlphaChange { get; set; }
        private Random random;
        
        public Star(PointF position, float size, Color color)
        {
            Position = position;
            Size = size;
            Color = color;
            Alpha = 0.5f + (float)new Random().NextDouble() * 0.5f; // 0.5 - 1.0
            AlphaChange = 0.01f * (new Random().Next(0, 2) * 2 - 1); // -0.01 or 0.01
            random = new Random();
        }
        
        public void Twinkle()
        {
            // 更新透明度以实现闪烁效果
            Alpha += AlphaChange;
            
            // 在0.3和1.0之间反弹
            if (Alpha > 1.0f || Alpha < 0.3f)
            {
                AlphaChange = -AlphaChange;
                Alpha = Math.Max(0.3f, Math.Min(1.0f, Alpha));
            }
            
            // 偶尔改变闪烁速度
            if (random.Next(100) < 5) // 5%的概率
            {
                AlphaChange = 0.01f * (random.Next(0, 2) * 2 - 1); // -0.01 or 0.01
            }
        }
        
        public void Draw(Graphics g)
        {
            // 根据当前Alpha值创建颜色
            Color currentColor = Color.FromArgb((int)(Alpha * 255), Color);
            
            using (SolidBrush brush = new SolidBrush(currentColor))
            {
                g.FillEllipse(brush, Position.X - Size / 2, Position.Y - Size / 2, Size, Size);
            }
        }
    }
}