using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ShootGame
{
    public partial class GameForm : Form
    {
        private Game game;
        private System.Windows.Forms.Timer gameTimer;
        private Dictionary<Keys, bool> keyState;
        
        public GameForm()
        {
            InitializeComponent();
            
            // 设置窗体属性
            this.Text = "双人射击游戏";
            this.DoubleBuffered = true; // 启用双缓冲，减少闪烁
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ClientSize = new Size(800, 600);
            
            // 初始化游戏
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            // 创建游戏对象
            game = new Game(this.ClientRectangle);
            
            // 初始化按键状态字典
            keyState = new Dictionary<Keys, bool>();
            
            // 创建游戏计时器
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 20; // 50 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
            
            // 注册键盘事件
            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 更新游戏状态
            UpdatePlayerMovement();
            game.Update();
            
            // 重绘窗体
            this.Invalidate();
        }
        
        private void UpdatePlayerMovement()
        {
            if (game.State != GameState.Playing)
                return;
                
            // 玩家1移动（方向键）
            Player player1 = game.Players[0];
            player1.IsMovingUp = IsKeyPressed(Keys.Up);
            player1.IsMovingDown = IsKeyPressed(Keys.Down);
            player1.IsMovingLeft = IsKeyPressed(Keys.Left);
            player1.IsMovingRight = IsKeyPressed(Keys.Right);
            
            // 玩家2移动（WASD）
            Player player2 = game.Players[1];
            player2.IsMovingUp = IsKeyPressed(Keys.W);
            player2.IsMovingDown = IsKeyPressed(Keys.S);
            player2.IsMovingLeft = IsKeyPressed(Keys.A);
            player2.IsMovingRight = IsKeyPressed(Keys.D);
        }
        
        private bool IsKeyPressed(Keys key)
        {
            return keyState.ContainsKey(key) && keyState[key];
        }
        
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 更新按键状态
            keyState[e.KeyCode] = true;
            
            // 处理特殊按键
            HandleSpecialKeys(e.KeyCode);
        }
        
        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            // 更新按键状态
            keyState[e.KeyCode] = false;
        }
        
        private void HandleSpecialKeys(Keys keyCode)
        {
            // 空格键：开始游戏或重新开始
            if (keyCode == Keys.Space)
            {
                if (game.State == GameState.Start || game.State == GameState.GameOver)
                {
                    game.StartNewGame();
                }
            }
            
            // Enter键：玩家1射击
            else if (keyCode == Keys.Enter)
            {
                game.PlayerShoot(1);
            }
            
            // F键：玩家2射击
            else if (keyCode == Keys.F)
            {
                game.PlayerShoot(2);
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // 绘制游戏
            game.Draw(e.Graphics);
        }
    }
}
