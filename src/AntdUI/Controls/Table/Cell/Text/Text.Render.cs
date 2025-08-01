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
// GITCODE: https://gitcode.com/AntdUI/AntdUI
// GITEE: https://gitee.com/AntdUI/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System.Drawing;

namespace AntdUI
{
    partial class CellText
    {
        public override void PaintBack(Canvas g)
        {
            if (Back.HasValue) g.Fill(Back.Value, PARENT.RECT);
        }

        public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
        {
            if (Fore.HasValue) g.DrawText(Text, Font ?? font, Fore.Value, Rect, Table.StringFormat(PARENT.COLUMN));
            else g.DrawText(Text, Font ?? font, fore, Rect, Table.StringFormat(PARENT.COLUMN));
            if (PrefixSvg != null) g.GetImgExtend(PrefixSvg, RectL, Fore ?? fore.Color);
            else if (Prefix != null) g.Image(Prefix, RectL);

            if (SuffixSvg != null) g.GetImgExtend(SuffixSvg, RectR, Fore ?? fore.Color);
            else if (Suffix != null) g.Image(Suffix, RectR);
        }

        public override Size GetSize(Canvas g, Font font, TableGaps gap)
        {
            var size = g.MeasureText(Text, Font ?? font);
            bool has_prefix = HasPrefix, has_suffix = HasSuffix;
            if (has_prefix && has_suffix)
            {
                int icon_size = (int)(size.Height * IconRatio);
                return new Size((icon_size * 2) + gap.x2 + size.Width, size.Height);
            }
            else if (has_prefix || has_suffix)
            {
                int icon_size = (int)(size.Height * IconRatio);
                return new Size(icon_size + gap.x + size.Width, size.Height);
            }
            return new Size(size.Width, size.Height);
        }

        Rectangle RectL, RectR;
        public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int maxwidth, TableGaps gap)
        {
            int width = rect.Width;
            if (width > maxwidth) width = maxwidth;
            bool has_prefix = HasPrefix, has_suffix = HasSuffix;
            if (has_prefix && has_suffix)
            {
                int icon_size = (int)(size.Height * IconRatio);
                RectL = new Rectangle(rect.X, rect.Y + (rect.Height - icon_size) / 2, icon_size, icon_size);
                RectR = new Rectangle(rect.Right - icon_size, RectL.Y, icon_size, icon_size);

                Rect = new Rectangle(RectL.Right + gap.x, rect.Y + (rect.Height - size.Height) / 2, width - (icon_size * 2 + gap.x2), size.Height);
            }
            else if (has_prefix)
            {
                int icon_size = (int)(size.Height * IconRatio);
                RectL = new Rectangle(rect.X, rect.Y + (rect.Height - icon_size) / 2, icon_size, icon_size);
                Rect = new Rectangle(RectL.Right + gap.x, rect.Y + (rect.Height - size.Height) / 2, width - icon_size - gap.x, size.Height);
            }
            else if (has_suffix)
            {
                int icon_size = (int)(size.Height * IconRatio);
                RectR = new Rectangle(rect.Right - icon_size, rect.Y + (rect.Height - icon_size) / 2, icon_size, icon_size);
                Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, width - icon_size - gap.x2, size.Height);
            }
            else Rect = new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, width, size.Height);
        }
    }
}