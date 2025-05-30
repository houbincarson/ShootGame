using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShootGame
{
    public class Bullet
    {
        // 子弹属性
        public PointF Position { get; set; }
        public float Speed { get; set; }
        public int Size { get; set; }
        public int PlayerNumber { get; set; } // 发射子弹的玩家编号
        public PointF Direction { get; set; } // 子弹方向
        private PointF[] trailPositions; // 存储子弹尾迹位置
        private const int TrailLength = 5; // 尾迹长度
        
        // 构造函数
        public Bullet(PointF startPosition, int size, float speed, int playerNumber)
        {
            Position = startPosition;
            Size = size;
            Speed = speed;
            PlayerNumber = playerNumber;
            
            // 设置默认方向（将在Player.Shoot方法中被覆盖）
            // 玩家1的子弹向右，玩家2的子弹向左
            Direction = (playerNumber == 1) 
                ? new PointF(1, 0) 
                : new PointF(-1, 0);
                
            // 初始化尾迹位置数组
            trailPositions = new PointF[TrailLength];
            for (int i = 0; i < TrailLength; i++)
            {
                trailPositions[i] = startPosition;
            }
        }
        
        // 移动子弹
        public void Move()
        {
            // 更新尾迹位置（从后向前移动）
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
            }
            trailPositions[0] = Position;
            
            // 更新子弹位置
            Position = new PointF(
                Position.X + Direction.X * Speed,
                Position.Y + Direction.Y * Speed
            );
        }
        
        // 检查子弹是否超出边界
        public bool IsOutOfBounds(Rectangle bounds)
        {
            return Position.X < 0 || 
                   Position.X > bounds.Width || 
                   Position.Y < 0 || 
                   Position.Y > bounds.Height;
        }
        
        // 获取子弹的边界矩形，用于碰撞检测
        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - Size / 2),
                (int)(Position.Y - Size / 2),
                Size,
                Size
            );
        }
        
        // 绘制子弹
        public void Draw(Graphics g)
        {
            // 设置高质量绘图
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 确定子弹颜色
            Color bulletColor = (PlayerNumber == 1) ? Color.Red : Color.Blue;
            Color glowColor = (PlayerNumber == 1) ? Color.OrangeRed : Color.RoyalBlue;
            Color trailColor = (PlayerNumber == 1) ? Color.Orange : Color.LightBlue;
            
            // 绘制尾迹
            DrawTrail(g, trailColor);
            
            // 绘制发光效果
            DrawGlow(g, glowColor);
            
            // 绘制子弹主体
            using (SolidBrush brush = new SolidBrush(bulletColor))
            {
                g.FillEllipse(brush, GetBounds());
            }
            
            // 绘制子弹高光
            DrawHighlight(g);
        }
        
        // 绘制尾迹
        private void DrawTrail(Graphics g, Color trailColor)
        {
            // 创建尾迹路径
            if (TrailLength > 1)
            {
                using (GraphicsPath trailPath = new GraphicsPath())
                {
                    // 添加尾迹点
                    for (int i = 0; i < TrailLength; i++)
                    {
                        float size = Size * (1 - (float)i / TrailLength); // 尾迹逐渐变小
                        RectangleF rect = new RectangleF(
                            trailPositions[i].X - size / 2,
                            trailPositions[i].Y - size / 2,
                            size,
                            size
                        );
                        trailPath.AddEllipse(rect);
                    }
                    
                    // 使用渐变画刷绘制尾迹
                    using (PathGradientBrush pgBrush = new PathGradientBrush(trailPath))
                    {
                        pgBrush.CenterColor = Color.FromArgb(150, trailColor);
                        pgBrush.SurroundColors = new Color[] { Color.FromArgb(0, trailColor) };
                        g.FillPath(pgBrush, trailPath);
                    }
                }
            }
        }
        
        // 绘制发光效果
        private void DrawGlow(Graphics g, Color glowColor)
        {
            // 创建比子弹稍大的矩形用于发光效果
            Rectangle glowRect = new Rectangle(
                (int)(Position.X - Size * 1.5 / 2),
                (int)(Position.Y - Size * 1.5 / 2),
                (int)(Size * 1.5),
                (int)(Size * 1.5)
            );
            
            // 使用径向渐变画刷创建发光效果
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(glowRect);
                using (PathGradientBrush pgBrush = new PathGradientBrush(path))
                {
                    pgBrush.CenterColor = Color.FromArgb(180, glowColor);
                    pgBrush.SurroundColors = new Color[] { Color.FromArgb(0, glowColor) };
                    g.FillPath(pgBrush, path);
                }
            }
        }
        
        // 绘制高光
        private void DrawHighlight(Graphics g)
        {
            // 创建小的高光点
            float highlightSize = Size / 4;
            RectangleF highlightRect = new RectangleF(
                Position.X - Size / 4 + highlightSize / 2,
                Position.Y - Size / 4 + highlightSize / 2,
                highlightSize,
                highlightSize
            );
            
            using (SolidBrush highlightBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(highlightBrush, highlightRect);
            }
        }
    }
}