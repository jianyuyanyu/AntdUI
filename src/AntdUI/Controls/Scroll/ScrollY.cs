﻿// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
// THE AntdUI PROJECT IS AN WINFORM LIBRARY LICENSED UNDER THE Apache-2.0 License.
// LICENSED UNDER THE Apache License, VERSION 2.0 (THE "License")
// YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE License.
// YOU MAY OBTAIN A COPY OF THE LICENSE AT
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
// DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
// SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
// LIMITATIONS UNDER THE License.
// GITEE: https://gitee.com/antdui/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI
{
    public class ScrollY
    {
        IControl? control;

        public ScrollY(IControl _control)
        {
            Invalidate = () =>
            {
                Change?.Invoke();
                _control.Invalidate();
            };
            control = _control;
        }
        public ScrollY(Control _control)
        {
            Invalidate = () =>
            {
                Change?.Invoke();
                _control.Invalidate();
            };
        }
        public ScrollY(ILayeredForm _form)
        {
            Invalidate = () =>
            {
                Change?.Invoke();
                _form.Print();
            };
            Gap = Back = false;
        }

        Action Invalidate;

        internal Action? Change;

        public bool Back = true;
        public bool Gap = true;


        #region 属性

        bool show = false;
        public bool Show
        {
            get => show;
            set
            {
                if (show == value) return;
                show = value;
                if (!value) val = 0;
            }
        }
        public Rectangle Rect;
        public RectangleF Slider;

        internal float val = 0;
        internal float SetValue(float value)
        {
            if (value < 0) return 0;
            if (value > VrHeightI) return VrHeightI;
            return value;
        }
        /// <summary>
        /// 滚动条进度
        /// </summary>
        public float Value
        {
            get => val;
            set
            {
                var _val = SetValue(value);
                if (val == _val) return;
                val = _val;
                Invalidate();
            }
        }

        /// <summary>
        /// 虚拟高度
        /// </summary>
        public float VrHeight { get; set; } = 0F;
        public float VrHeightI { get; set; } = 0F;
        public int Height { get; set; }

        /// <summary>
        /// 设置容器虚拟高度
        /// </summary>
        /// <param name="len">总Y</param>
        /// <param name="height">容器高度</param>
        public void SetVrSize(float len, int height)
        {
            Height = height;
            if (len > 0 && len > height)
            {
                VrHeightI = len - height;
                VrHeight = len;
                Show = VrHeight > height;
                if (Show)
                {
                    if (val > (len - height)) Value = (len - height);
                }
            }
            else
            {
                VrHeight = VrHeightI = 0F;
                Show = false;
            }
        }

        #endregion

        public virtual void SizeChange(Rectangle rect)
        {
            Rect = new Rectangle(rect.Right - 20, rect.Y, 20, rect.Height);
        }

        /// <summary>
        /// 渲染滚动条竖
        /// </summary>
        /// <param name="g"></param>
        public virtual void Paint(Graphics g)
        {
            if (Show)
            {
                if (Back)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(10, Style.Db.TextBase)))
                    {
                        g.FillRectangle(brush, Rect);
                    }
                }
                float height = (Rect.Height / VrHeight) * Rect.Height;
                if (height < 20) height = 20;
                if (Gap) height -= 12;
                float y = val == 0 ? 0 : (val / (VrHeight - Rect.Height)) * (Rect.Height - height);
                if (Hover) Slider = new RectangleF(Rect.X + 6, Rect.Y + y, 8, height);
                else Slider = new RectangleF(Rect.X + 7, Rect.Y + y, 6, height);
                if (Gap)
                {
                    if (Slider.Y < 6) Slider.Y = 6;
                    else if (Slider.Y > Rect.Height - height - 6) Slider.Y = Rect.Height - height - 6;
                }
                using (var brush = new SolidBrush(Color.FromArgb(141, Style.Db.TextBase)))
                {
                    using (var path = Slider.RoundPath(Slider.Width))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        bool ShowDown = false;
        bool hover = false;
        bool Hover
        {
            get => hover;
            set
            {
                if (hover == value) return;
                hover = value;
                Invalidate();
            }
        }
        public virtual bool MouseDown(Point e)
        {
            if (Show && Rect.Contains(e))
            {
                if (!Slider.Contains(e))
                {
                    float y = (e.Y - Slider.Height / 2F) / Rect.Height;
                    Value = y * VrHeight;
                }
                ShowDown = true;
                return false;
            }
            return true;
        }

        public virtual bool MouseUp(Point e)
        {
            ShowDown = false;
            return true;
        }

        public virtual bool MouseMove(Point e)
        {
            if (ShowDown)
            {
                Hover = true;
                float y = (e.Y - Slider.Height / 2F) / Rect.Height;
                Value = y * VrHeight;
                return false;
            }
            else if (Show && Rect.Contains(e))
            {
                Hover = true;
                control?.SetCursor(false);
                return false;
            }
            else Hover = false;
            return true;
        }

        public bool MouseWheel(int delta)
        {
            if (Show && delta != 0)
            {
                Value -= delta;//120
                return true;
            }
            return false;
        }

        public void Leave()
        {
            Hover = false;
        }
    }
}