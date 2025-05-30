using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShootGame
{
    // 障碍物类型枚举
    public enum ObstacleType
    {
        // 不可穿过的障碍物
        Mountain,  // 假山
        River,     // 河流
        Tree,      // 树木
        
        // 可穿过的障碍物
        Sand,      // 沙场
        Grass      // 草地
    }
    
    // 障碍物基类
    public abstract class Obstacle
    {
        // 基本属性
        public PointF Position { get; set; }
        public SizeF Size { get; set; }
        public ObstacleType Type { get; protected set; }
        public bool IsPassable { get; protected set; } // 是否可穿过
        protected Random random;
        
        // 构造函数
        public Obstacle(PointF position, SizeF size)
        {
            Position = position;
            Size = size;
            random = new Random();
        }
        
        // 获取障碍物的边界矩形，用于碰撞检测
        public virtual Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X, 
                (int)Position.Y, 
                (int)Size.Width, 
                (int)Size.Height);
        }
        
        // 检查是否与玩家碰撞
        public virtual bool CheckCollision(Player player)
        {
            return GetBounds().IntersectsWith(player.GetBounds());
        }
        
        // 检查是否与子弹碰撞
        public virtual bool CheckCollision(Bullet bullet)
        {
            return GetBounds().IntersectsWith(bullet.GetBounds());
        }
        
        // 应用障碍物效果到玩家
        public virtual void ApplyEffect(Player player)
        {
            // 基类不实现具体效果，由子类覆盖
        }
        
        // 绘制障碍物
        public abstract void Draw(Graphics g);
        
        // 创建圆角矩形路径（通用方法）
        protected GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
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
    }
    
    // 假山障碍物（不可穿过）
    public class Mountain : Obstacle
    {
        private Point[] mountainPoints;
        
        public Mountain(PointF position, SizeF size) : base(position, size)
        {
            Type = ObstacleType.Mountain;
            IsPassable = false;
            
            // 创建山峰形状的点
            GenerateMountainShape();
        }
        
        private void GenerateMountainShape()
        {
            int peakCount = random.Next(3, 6); // 2-5个山峰
            mountainPoints = new Point[peakCount + 2];
            
            // 起点（左下角）
            mountainPoints[0] = new Point((int)Position.X, (int)(Position.Y + Size.Height));
            
            // 生成山峰
            for (int i = 1; i <= peakCount; i++)
            {
                float xPos = Position.X + (Size.Width * i / (peakCount + 1));
                float yPos = Position.Y + Size.Height - (random.Next(50, 100) % (int)Size.Height);
                mountainPoints[i] = new Point((int)xPos, (int)yPos);
            }
            
            // 终点（右下角）
            mountainPoints[peakCount + 1] = new Point((int)(Position.X + Size.Width), (int)(Position.Y + Size.Height));
        }
        
        public override void Draw(Graphics g)
        {
            // 设置抗锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 绘制山体
            using (LinearGradientBrush brush = new LinearGradientBrush(
                GetBounds(), Color.Gray, Color.DarkGray, LinearGradientMode.Vertical))
            {
                g.FillPolygon(brush, mountainPoints);
            }
            
            // 绘制山体轮廓
            using (Pen outlinePen = new Pen(Color.FromArgb(100, 50, 50, 50), 2))
            {
                g.DrawPolygon(outlinePen, mountainPoints);
            }
            
            // 绘制山顶雪
            for (int i = 1; i <= mountainPoints.Length - 2; i++)
            {
                if (mountainPoints[i].Y < Position.Y + Size.Height * 0.4) // 只在较高的山峰上绘制雪
                {
                    using (SolidBrush snowBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                    {
                        g.FillEllipse(snowBrush, 
                            mountainPoints[i].X - 10, 
                            mountainPoints[i].Y - 5, 
                            20, 10);
                    }
                }
            }
        }
    }
    
    // 河流障碍物（不可穿过，但子弹可穿过）
    public class River : Obstacle
    {
        private Point[] wavePoints;
        private float animationOffset = 0;
        
        public River(PointF position, SizeF size) : base(position, size)
        {
            Type = ObstacleType.River;
            IsPassable = false;
            
            // 创建波浪形状
            GenerateWaveShape();
        }
        
        private void GenerateWaveShape()
        {
            int pointCount = (int)(Size.Width / 10) + 1; // 每10像素一个点
            wavePoints = new Point[pointCount * 2];
            
            // 生成上边缘波浪点
            for (int i = 0; i < pointCount; i++)
            {
                float x = Position.X + (i * 10);
                float y = Position.Y + (float)Math.Sin(i * 0.5) * 5;
                wavePoints[i] = new Point((int)x, (int)y);
            }
            
            // 生成下边缘波浪点（反向）
            for (int i = 0; i < pointCount; i++)
            {
                float x = Position.X + Size.Width - (i * 10);
                float y = Position.Y + Size.Height + (float)Math.Sin(i * 0.5) * 5;
                wavePoints[pointCount + i] = new Point((int)x, (int)y);
            }
        }
        
        // 更新波浪动画
        public void Update()
        {
            animationOffset += 0.1f;
            if (animationOffset > Math.PI * 2)
            {
                animationOffset = 0;
            }
            
            // 更新波浪形状
            int pointCount = wavePoints.Length / 2;
            for (int i = 0; i < pointCount; i++)
            {
                // 上边缘
                wavePoints[i] = new Point(
                    wavePoints[i].X,
                    (int)(Position.Y + Math.Sin(i * 0.5 + animationOffset) * 5)
                );
                
                // 下边缘
                wavePoints[pointCount + i] = new Point(
                    wavePoints[pointCount + i].X,
                    (int)(Position.Y + Size.Height + Math.Sin(i * 0.5 + animationOffset) * 5)
                );
            }
        }
        
        public override bool CheckCollision(Bullet bullet)
        {
            // 子弹可以穿过河流
            return false;
        }
        
        public override void Draw(Graphics g)
        {
            // 设置抗锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 绘制河流主体
            using (LinearGradientBrush brush = new LinearGradientBrush(
                GetBounds(), Color.FromArgb(180, 100, 149, 237), Color.FromArgb(180, 0, 105, 148), 
                LinearGradientMode.Vertical))
            {
                g.FillPolygon(brush, wavePoints);
            }
            
            // 绘制波纹
            using (Pen wavePen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
            {
                for (int i = 0; i < 3; i++) // 绘制3条波纹线
                {
                    float yOffset = Size.Height * (i + 1) / 4;
                    Point[] waveLinePoints = new Point[(int)(Size.Width / 10) + 1];
                    
                    for (int j = 0; j < waveLinePoints.Length; j++)
                    {
                        waveLinePoints[j] = new Point(
                            (int)(Position.X + j * 10),
                            (int)(Position.Y + yOffset + Math.Sin(j * 0.5 + animationOffset + i) * 3)
                        );
                    }
                    
                    g.DrawCurve(wavePen, waveLinePoints);
                }
            }
        }
    }
    
    // 树木障碍物（不可穿过）
    public class Tree : Obstacle
    {
        public Tree(PointF position, SizeF size) : base(position, size)
        {
            Type = ObstacleType.Tree;
            IsPassable = false;
        }
        
        public override void Draw(Graphics g)
        {
            // 设置抗锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 计算树干和树冠的尺寸和位置
            float trunkWidth = Size.Width * 0.2f;
            float trunkHeight = Size.Height * 0.4f;
            float trunkX = Position.X + (Size.Width - trunkWidth) / 2;
            float trunkY = Position.Y + Size.Height - trunkHeight;
            
            // 绘制树干
            RectangleF trunk = new RectangleF(trunkX, trunkY, trunkWidth, trunkHeight);
            using (LinearGradientBrush trunkBrush = new LinearGradientBrush(
                trunk, Color.SaddleBrown, Color.FromArgb(139, 69, 19), LinearGradientMode.Horizontal))
            {
                g.FillRectangle(trunkBrush, trunk);
            }
            
            // 绘制树冠（多层）
            int layers = 3;
            for (int i = 0; i < layers; i++)
            {
                float layerSize = Size.Width - (i * Size.Width * 0.2f);
                float layerX = Position.X + (Size.Width - layerSize) / 2;
                float layerY = Position.Y + (Size.Height * 0.6f) * i / layers;
                
                using (SolidBrush leafBrush = new SolidBrush(Color.FromArgb(255, 0, 100 + i * 30, 0)))
                {
                    g.FillEllipse(leafBrush, layerX, layerY, layerSize, Size.Height * 0.4f);
                }
            }
        }
    }
    
    // 沙场障碍物（可穿过，减慢速度）
    public class Sand : Obstacle
    {
        private Point[] sandDots;
        
        public Sand(PointF position, SizeF size) : base(position, size)
        {
            Type = ObstacleType.Sand;
            IsPassable = true;
            
            // 生成沙粒点
            GenerateSandDots();
        }
        
        private void GenerateSandDots()
        {
            int dotCount = (int)(Size.Width * Size.Height / 100); // 每100平方像素一个点
            sandDots = new Point[dotCount];
            
            for (int i = 0; i < dotCount; i++)
            {
                sandDots[i] = new Point(
                    (int)(Position.X + random.Next((int)Size.Width)),
                    (int)(Position.Y + random.Next((int)Size.Height))
                );
            }
        }
        
        public override void ApplyEffect(Player player)
        {
            // 减慢玩家速度（临时效果）
            player.ApplySpeedModifier(0.5f); // 速度减半
        }
        
        public override void Draw(Graphics g)
        {
            // 绘制沙场背景
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(180, 244, 215, 140)))
            {
                g.FillRectangle(bgBrush, GetBounds());
            }
            
            // 绘制沙粒
            using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(150, 210, 180, 140)))
            {
                foreach (Point dot in sandDots)
                {
                    g.FillEllipse(dotBrush, dot.X, dot.Y, 2, 2);
                }
            }
            
            // 绘制纹理线条
            using (Pen texturePen = new Pen(Color.FromArgb(50, 210, 180, 140), 1))
            {
                for (int i = 0; i < 5; i++) // 绘制5条纹理线
                {
                    Point[] curvePoints = new Point[4];
                    for (int j = 0; j < 4; j++)
                    {
                        curvePoints[j] = new Point(
                            (int)(Position.X + (Size.Width * j / 3)),
                            (int)(Position.Y + Size.Height * (0.2f + 0.15f * i) + 
                                 Math.Sin(j * 0.8 + i * 0.5) * 10)
                        );
                    }
                    g.DrawCurve(texturePen, curvePoints);
                }
            }
        }
    }
    
    // 草地障碍物（可穿过，略微减慢速度）
    public class Grass : Obstacle
    {
        private Point[][] grassBlades;
        
        public Grass(PointF position, SizeF size) : base(position, size)
        {
            Type = ObstacleType.Grass;
            IsPassable = true;
            
            // 生成草叶
            GenerateGrassBlades();
        }
        
        private void GenerateGrassBlades()
        {
            int clumpCount = (int)(Size.Width * Size.Height / 500); // 每500平方像素一簇草
            grassBlades = new Point[clumpCount][];
            
            for (int i = 0; i < clumpCount; i++)
            {
                int bladeCount = random.Next(3, 7); // 每簇3-6片草叶
                grassBlades[i] = new Point[bladeCount * 2]; // 每片草叶需要2个点（底部和顶部）
                
                int baseX = (int)(Position.X + random.Next((int)Size.Width));
                int baseY = (int)(Position.Y + random.Next((int)Size.Height));
                
                for (int j = 0; j < bladeCount; j++)
                {
                    // 草叶底部
                    grassBlades[i][j * 2] = new Point(baseX, baseY);
                    
                    // 草叶顶部（随机高度和角度）
                    float angle = (float)(random.NextDouble() * Math.PI - Math.PI / 2); // -90到90度
                    float length = 5 + random.Next(5); // 5-10像素长
                    
                    grassBlades[i][j * 2 + 1] = new Point(
                        (int)(baseX + Math.Cos(angle) * length),
                        (int)(baseY - Math.Sin(angle) * length)
                    );
                }
            }
        }
        
        public override void ApplyEffect(Player player)
        {
            // 略微减慢玩家速度（临时效果）
            player.ApplySpeedModifier(0.8f); // 速度减少20%
        }
        
        public override void Draw(Graphics g)
        {
            // 绘制草地背景
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(150, 144, 238, 144)))
            {
                g.FillRectangle(bgBrush, GetBounds());
            }
            
            // 绘制草叶
            using (Pen darkGrassPen = new Pen(Color.FromArgb(200, 0, 100, 0), 1.5f))
            using (Pen lightGrassPen = new Pen(Color.FromArgb(200, 50, 205, 50), 1.5f))
            {
                foreach (Point[] clump in grassBlades)
                {
                    for (int i = 0; i < clump.Length / 2; i++)
                    {
                        // 交替使用深色和浅色
                        Pen grassPen = (i % 2 == 0) ? darkGrassPen : lightGrassPen;
                        g.DrawLine(grassPen, clump[i * 2], clump[i * 2 + 1]);
                    }
                }
            }
        }
    }
}