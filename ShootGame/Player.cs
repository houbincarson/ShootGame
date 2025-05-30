using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShootGame
{
    public class Player
    {
        // 玩家属性
        public PointF Position { get; set; }
        public float Speed { get; private set; }
        public float BaseSpeed { get; private set; } // 基础速度
        public float SpeedModifier { get; private set; } = 1.0f; // 速度修改器
        public int Health { get; set; }
        public int Score { get; set; }
        public Color Color { get; set; }
        public int PlayerNumber { get; set; }
        public Size Size { get; set; }
        
        // 移动状态
        public bool IsMovingUp { get; set; }
        public bool IsMovingDown { get; set; }
        public bool IsMovingLeft { get; set; }
        public bool IsMovingRight { get; set; }
        
        // 面向方向
        public PointF FacingDirection { get; private set; }
        
        // 上次射击时间（用于限制射击频率）
        private DateTime LastShootTime = DateTime.MinValue;
        
        // 动画相关
        private float animationTime = 0;
        private const float ANIMATION_SPEED = 0.1f;
        
        // 构造函数
        public Player(PointF position, float speed, Color color, int playerNumber, Size size)
        {
            Position = position;
            BaseSpeed = speed;
            Speed = speed;
            Health = 100;
            Score = 0;
            Color = color;
            PlayerNumber = playerNumber;
            Size = size;
            FacingDirection = new PointF(0, -1); // 默认朝上
            SpeedModifier = 1.0f;
        }
        
        // 应用速度修改器（由障碍物调用）
        public void ApplySpeedModifier(float modifier)
        {
            SpeedModifier = modifier;
            Speed = BaseSpeed * SpeedModifier;
        }
        
        // 重置速度修改器
        public void ResetSpeedModifier()
        {
            SpeedModifier = 1.0f;
            Speed = BaseSpeed;
        }
        
        // 移动玩家
        public void Move(Rectangle bounds)
        {
            float newX = Position.X;
            float newY = Position.Y;
            bool hasMoved = false;
            float dirX = 0;
            float dirY = 0;
            
            if (IsMovingUp)
            {
                newY -= Speed;
                dirY = -1;
                hasMoved = true;
            }
            if (IsMovingDown)
            {
                newY += Speed;
                dirY = 1;
                hasMoved = true;
            }
            if (IsMovingLeft)
            {
                newX -= Speed;
                dirX = -1;
                hasMoved = true;
            }
            if (IsMovingRight)
            {
                newX += Speed;
                dirX = 1;
                hasMoved = true;
            }
            
            // 更新面向方向
            if (hasMoved)
            {
                FacingDirection = new PointF(dirX, dirY);
                // 归一化方向向量
                float length = (float)Math.Sqrt(FacingDirection.X * FacingDirection.X + FacingDirection.Y * FacingDirection.Y);
                if (length > 0)
                {
                    FacingDirection = new PointF(FacingDirection.X / length, FacingDirection.Y / length);
                }
                
                // 更新动画时间
                animationTime += ANIMATION_SPEED;
            }
            
            // 边界检查
            if (newX < 0) newX = 0;
            if (newY < 0) newY = 0;
            if (newX > bounds.Width - Size.Width) newX = bounds.Width - Size.Width;
            if (newY > bounds.Height - Size.Height) newY = bounds.Height - Size.Height;
            
            Position = new PointF(newX, newY);
        }
        
        // 获取玩家的边界矩形，用于碰撞检测
        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Size.Width, Size.Height);
        }
        
        // 绘制玩家
        public void Draw(Graphics g)
        {
            // 设置抗锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // 获取玩家矩形
            Rectangle playerRect = GetBounds();
            
            // 计算人形各部分位置
            int headSize = Size.Width / 2;
            Point headCenter = new Point(
                (int)(Position.X + Size.Width / 2),
                (int)(Position.Y + headSize / 2 + 5)
            );
            
            Rectangle bodyRect = new Rectangle(
                (int)(Position.X + Size.Width / 4),
                headCenter.Y + headSize / 2,
                Size.Width / 2,
                Size.Height - headSize - 10
            );
            
            // 计算手臂和腿部位置（根据移动状态和方向调整）
            Point leftShoulder = new Point(bodyRect.Left, bodyRect.Top + 5);
            Point rightShoulder = new Point(bodyRect.Right, bodyRect.Top + 5);
            Point leftHip = new Point(bodyRect.Left, bodyRect.Bottom);
            Point rightHip = new Point(bodyRect.Right, bodyRect.Bottom);
            
            // 计算手臂和腿部终点（带动画）
            float armAngle = (float)Math.Sin(animationTime * 5) * 0.3f;
            float legAngle = (float)Math.Sin(animationTime * 5) * 0.2f;
            
            // 根据面向方向调整手臂角度
            float directionAngle = (float)Math.Atan2(FacingDirection.Y, FacingDirection.X);
            
            // 手臂长度和腿长
            int armLength = Size.Width / 2;
            int legLength = Size.Height / 3;
            
            // 计算手臂终点
            Point leftArm = new Point(
                (int)(leftShoulder.X - Math.Cos(directionAngle - armAngle) * armLength),
                (int)(leftShoulder.Y + Math.Sin(directionAngle - armAngle) * armLength)
            );
            
            Point rightArm = new Point(
                (int)(rightShoulder.X + Math.Cos(directionAngle + armAngle) * armLength),
                (int)(rightShoulder.Y + Math.Sin(directionAngle + armAngle) * armLength)
            );
            
            // 计算腿部终点
            Point leftLeg = new Point(
                (int)(leftHip.X - Math.Cos(legAngle) * legLength),
                (int)(leftHip.Y + Math.Sin(legAngle) * legLength)
            );
            
            Point rightLeg = new Point(
                (int)(rightHip.X + Math.Cos(legAngle) * legLength),
                (int)(rightHip.Y + Math.Sin(legAngle) * legLength)
            );
            
            // 绘制人形
            using (SolidBrush bodyBrush = new SolidBrush(Color))
            using (Pen outlinePen = new Pen(Color.FromArgb(200, 50, 50, 50), 2))
            {
                // 绘制身体
                using (LinearGradientBrush bodyGradient = new LinearGradientBrush(
                    bodyRect, Color, GetGradientColor(Color), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bodyGradient, bodyRect);
                    g.DrawRectangle(outlinePen, bodyRect);
                }
                
                // 绘制头部
                g.FillEllipse(bodyBrush, 
                    headCenter.X - headSize / 2, 
                    headCenter.Y - headSize / 2, 
                    headSize, headSize);
                g.DrawEllipse(outlinePen, 
                    headCenter.X - headSize / 2, 
                    headCenter.Y - headSize / 2, 
                    headSize, headSize);
                
                // 绘制手臂
                g.DrawLine(outlinePen, leftShoulder, leftArm);
                g.DrawLine(outlinePen, rightShoulder, rightArm);
                
                // 绘制腿部
                g.DrawLine(outlinePen, leftHip, leftLeg);
                g.DrawLine(outlinePen, rightHip, rightLeg);
                
                // 绘制玩家编号
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    
                    g.DrawString(PlayerNumber.ToString(), font, textBrush, 
                        new RectangleF(headCenter.X - headSize / 2, headCenter.Y - headSize / 2, 
                                      headSize, headSize), format);
                }
            }
            
            // 绘制方向指示器
            DrawDirectionIndicator(g);
        }
        
        // 绘制方向指示器
        private void DrawDirectionIndicator(Graphics g)
        {
            // 计算指示器起点（玩家中心）
            PointF center = new PointF(
                Position.X + Size.Width / 2,
                Position.Y + Size.Height / 2
            );
            
            // 计算指示器终点
            float indicatorLength = Math.Max(Size.Width, Size.Height);
            PointF end = new PointF(
                center.X + FacingDirection.X * indicatorLength,
                center.Y + FacingDirection.Y * indicatorLength
            );
            
            // 计算箭头点
            float arrowSize = 10;
            float arrowAngle = (float)Math.Atan2(FacingDirection.Y, FacingDirection.X);
            PointF arrowPoint1 = new PointF(
                end.X - (float)Math.Cos(arrowAngle - Math.PI / 6) * arrowSize,
                end.Y - (float)Math.Sin(arrowAngle - Math.PI / 6) * arrowSize
            );
            PointF arrowPoint2 = new PointF(
                end.X - (float)Math.Cos(arrowAngle + Math.PI / 6) * arrowSize,
                end.Y - (float)Math.Sin(arrowAngle + Math.PI / 6) * arrowSize
            );
            
            // 绘制指示线和箭头
            using (Pen indicatorPen = new Pen(Color.FromArgb(150, Color), 2))
            {
                // 设置虚线样式
                indicatorPen.DashStyle = DashStyle.Dash;
                
                // 绘制指示线
                g.DrawLine(indicatorPen, center, end);
                
                // 绘制箭头
                indicatorPen.DashStyle = DashStyle.Solid;
                g.DrawLine(indicatorPen, end, arrowPoint1);
                g.DrawLine(indicatorPen, end, arrowPoint2);
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
        
        // 获取渐变颜色
        private Color GetGradientColor(Color baseColor)
        {
            // 创建一个比基础颜色更亮的颜色
            int r = Math.Min(255, baseColor.R + 70);
            int g = Math.Min(255, baseColor.G + 70);
            int b = Math.Min(255, baseColor.B + 70);
            
            return Color.FromArgb(baseColor.A, r, g, b);
        }
        
        // 射击方法
        public Bullet Shoot()
        {
            // 限制射击频率（每0.5秒最多射击一次）
            if ((DateTime.Now - LastShootTime).TotalMilliseconds < 500)
                return null;
                
            LastShootTime = DateTime.Now;
            
            // 子弹从玩家中心发射
            PointF bulletPos = new PointF(
                Position.X + Size.Width / 2,
                Position.Y + Size.Height / 2
            );
            
            // 创建子弹并设置方向
            Bullet bullet = new Bullet(bulletPos, 10, 10.0f, PlayerNumber);
            bullet.Direction = FacingDirection;
            
            return bullet;
        }
    }
}