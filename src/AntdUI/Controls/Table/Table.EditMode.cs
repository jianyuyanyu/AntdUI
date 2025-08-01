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

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI
{
    partial class Table
    {
        #region 编辑模式

        /// <summary>
        /// 进入编辑模式
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        public bool EnterEditMode(int row, int column)
        {
            if (rows != null)
            {
                try
                {
                    var _row = rows[row];
                    var item = RealCELL(_row.cells[column], rows, row, column, out var crect);
                    EditModeClose();
                    if (CanEditMode(item))
                    {
                        ScrollLine(row, rows);
                        if (showFixedColumnL && fixedColumnL != null && fixedColumnL.Contains(column)) OnEditMode(_row, item, crect, row, column, item.COLUMN, 0, ScrollBar.ValueY);
                        else if (showFixedColumnR && fixedColumnR != null && fixedColumnR.Contains(column)) OnEditMode(_row, item, crect, row, column, item.COLUMN, sFixedR, ScrollBar.ValueY);
                        else OnEditMode(_row, item, crect, row, column, item.COLUMN, ScrollBar.ValueX, ScrollBar.ValueY);
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        bool inEditMode = false;
        /// <summary>
        /// 关闭编辑模式
        /// </summary>
        public void EditModeClose()
        {
            if (inEditMode)
            {
                ScrollBar.OnInvalidate = null;
                if (!focused)
                {
                    if (InvokeRequired) Invoke(Focus);
                    else Focus();
                }
                inEditMode = false;
            }
        }

        bool CanEditMode(CELL cell)
        {
            if (rows == null) return false;
            if (cell.COLUMN.Editable)
            {
                if (cell is TCellText) return true;
                else if (cell is TCellSelect) return true;
                else if (cell is Template templates)
                {
                    foreach (var template in templates.Value)
                    {
                        if (template is CellText text) return true;
                    }
                }
            }
            return false;
        }
        void OnEditMode(RowTemplate it, CELL cell, Rectangle rect, int i_row, int i_col, Column? column, int sx, int sy)
        {
            if (rows == null) return;
            if (it.AnimationHover)
            {
                it.ThreadHover?.Dispose();
                it.ThreadHover = null;
            }
            bool multiline = cell.COLUMN.LineBreak;
            if (cell is TCellText cellText)
            {
                object? value = null;
                if (cell.PROPERTY != null && cell.VALUE != null) value = cell.PROPERTY.GetValue(cell.VALUE);
                else if (cell.VALUE is AntItem item) value = item.value;
                else value = cell.VALUE;

                bool isok = true;
                if (CellBeginEdit != null) isok = CellBeginEdit(this, new TableEventArgs(value, it.RECORD, i_row, i_col, column));
                if (!isok) return;
                inEditMode = true;

                ScrollBar.OnInvalidate = () => EditModeClose();
                BeginInvoke(() =>
                {
                    for (int i = 0; i < rows.Length; i++) rows[i].hover = i == i_row;
                    var tmp_input = CreateInput(cell, sx, sy, multiline, value, rect);
                    if (cellText.COLUMN.Align == ColumnAlign.Center) tmp_input.TextAlign = HorizontalAlignment.Center;
                    else if (cellText.COLUMN.Align == ColumnAlign.Right) tmp_input.TextAlign = HorizontalAlignment.Right;
                    var arge = new TableBeginEditInputStyleEventArgs(value, it.RECORD, i_row, i_col, column, tmp_input);
                    CellBeginEditInputStyle?.Invoke(this, arge);
                    ShowInput(arge.Input, (cf, _value) =>
                    {
                        var e = new TableEndEditEventArgs(_value, it.RECORD, i_row, i_col, column);
                        arge.Call?.Invoke(e);
                        bool isok_end = CellEndEdit?.Invoke(this, e) ?? true;
                        if (isok_end && !cf)
                        {
                            if (GetValue(value, _value, out var o))
                            {
                                cellText.value = _value;
                                if (it.RECORD is DataRow datarow) cellText.VALUE = cellText.value = _value;
                                SetValue(cell, o);
                                if (multiline) LoadLayout();
                            }
                            CellEditComplete?.Invoke(this, new ITableEventArgs(it.RECORD, i_row, i_col, column));
                        }
                    });
                    Controls.Add(arge.Input);
                    arge.Input.Focus();
                });
            }
            else if (cell is TCellSelect cellSelect)
            {
                object? value = cellSelect.value?.Tag;
                bool isok = true;
                if (CellBeginEdit != null) isok = CellBeginEdit(this, new TableEventArgs(value, it.RECORD, i_row, i_col, column));
                if (!isok) return;
                inEditMode = true;

                ScrollBar.OnInvalidate = () => EditModeClose();
                BeginInvoke(() =>
                {
                    for (int i = 0; i < rows.Length; i++) rows[i].hover = i == i_row;
                    var tmp_input = CreateInput(cell, sx, sy, multiline, cellSelect.value, rect);
                    if (cellSelect.COLUMN.Align == ColumnAlign.Center) tmp_input.TextAlign = HorizontalAlignment.Center;
                    else if (cellSelect.COLUMN.Align == ColumnAlign.Right) tmp_input.TextAlign = HorizontalAlignment.Right;
                    var arge = new TableBeginEditInputStyleEventArgs(value, it.RECORD, i_row, i_col, column, tmp_input);
                    CellBeginEditInputStyle?.Invoke(this, arge);
                    if (arge.Input is Select select)
                    {
                        ShowSelect(select, (cf, _value) =>
                        {
                            bool isok_end = CellEndValueEdit?.Invoke(this, new TableEndValueEditEventArgs(_value, it.RECORD, i_row, i_col, column)) ?? true;
                            if (isok_end && !cf)
                            {
                                cellSelect.value = cellSelect.COLUMN[_value];
                                SetValue(cell, _value);
                                if (multiline) LoadLayout();
                                CellEditComplete?.Invoke(this, new ITableEventArgs(it.RECORD, i_row, i_col, column));
                            }
                        });
                        Controls.Add(arge.Input);
                        arge.Input.Focus();
                    }
                    else arge.Input.Dispose();
                });
            }
            else if (cell is Template templates)
            {
                foreach (var template in templates.Value)
                {
                    if (template is CellText text)
                    {
                        object? value = null;
                        if (cell.PROPERTY != null && cell.VALUE != null) value = cell.PROPERTY.GetValue(cell.VALUE);
                        else if (cell.VALUE is AntItem item) value = item.value;
                        else value = cell.VALUE;
                        bool isok = true;
                        if (CellBeginEdit != null) isok = CellBeginEdit(this, new TableEventArgs(value, it.RECORD, i_row, i_col, column));
                        if (!isok) return;
                        inEditMode = true;

                        ScrollBar.OnInvalidate = () => EditModeClose();
                        BeginInvoke(() =>
                        {
                            for (int i = 0; i < rows.Length; i++) rows[i].hover = i == i_row;
                            var tmp_input = CreateInput(cell, sx, sy, multiline, value, rect);
                            if (template.PARENT.COLUMN.Align == ColumnAlign.Center) tmp_input.TextAlign = HorizontalAlignment.Center;
                            else if (template.PARENT.COLUMN.Align == ColumnAlign.Right) tmp_input.TextAlign = HorizontalAlignment.Right;
                            var arge = new TableBeginEditInputStyleEventArgs(value, it.RECORD, i_row, i_col, column, tmp_input);
                            CellBeginEditInputStyle?.Invoke(this, arge);
                            ShowInput(arge.Input, (cf, _value) =>
                            {
                                var e = new TableEndEditEventArgs(_value, it.RECORD, i_row, i_col, column);
                                arge.Call?.Invoke(e);
                                bool isok_end = CellEndEdit?.Invoke(this, e) ?? true;
                                if (isok_end && !cf)
                                {
                                    if (value is CellText text2)
                                    {
                                        text2.Text = _value;
                                        SetValue(cell, text2);
                                    }
                                    else
                                    {
                                        text.Text = _value;
                                        if (GetValue(value, _value, out var o)) SetValue(cell, o);
                                    }
                                    CellEditComplete?.Invoke(this, new ITableEventArgs(it.RECORD, i_row, i_col, column));
                                }
                            });
                            Controls.Add(arge.Input);
                            arge.Input.Focus();
                        });
                        return;
                    }
                }
            }
        }

        bool GetValue(object? value, string _value, out object read)
        {
            if (value is int)
            {
                if (int.TryParse(_value, out var v))
                {
                    read = v;
                    return true;
                }
            }
            else if (value is double)
            {
                if (double.TryParse(_value, out var v))
                {
                    read = v;
                    return true;
                }
            }
            else if (value is decimal)
            {
                if (decimal.TryParse(_value, out var v))
                {
                    read = v;
                    return true;
                }
            }
            else if (value is float)
            {
                if (float.TryParse(_value, out var v))
                {
                    read = v;
                    return true;
                }
            }
            else
            {
                read = _value;
                return true;
            }
            read = _value;
            return false;
        }

        Input CreateInput(CELL cell, int sx, int sy, bool multiline, object? value, Rectangle rect)
        {
            switch (EditInputStyle)
            {
                case TEditInputStyle.Full:
                    var inputFull = CreateInput(multiline, value, cell.COLUMN, RectInput(cell, rect, sx, sy, 1F, 4));
                    inputFull.Radius = 0;
                    return inputFull;
                case TEditInputStyle.Excel:
                    var inputExcel = CreateInput(multiline, value, cell.COLUMN, RectInput(cell, rect, sx, sy, 2.5F, 0));
                    inputExcel.WaveSize = 0;
                    inputExcel.Radius = 0;
                    inputExcel.BorderWidth = 2.5F;
                    return inputExcel;
                case TEditInputStyle.Default:
                default:
                    return CreateInput(multiline, value, cell.COLUMN, RectInputDefault(cell, rect, sx, sy, 1F, 4));
            }
        }

        Rectangle RectInput(CELL cell, Rectangle rect, int sx, int sy, float borwidth, int wavesize)
        {
            int bor = (int)((wavesize + borwidth / 2F) * Config.Dpi), bor2 = bor * 2, ry = rect.Y, rh = rect.Height;
            if (EditAutoHeight)
            {
                int texth = Helper.GDI(g => g.MeasureString(Config.NullText, Font).Height), sps = (int)(texth * .4F), sps2 = sps * 2, h = texth + sps2 + bor2;
                if (h > rect.Height)
                {
                    rh = h - bor2;
                    ry = rect.Y + (rect.Height - rh) / 2;
                    if ((ry + h) - sy > rect_read.Bottom) ry = rect_read.Bottom + sy - rh - bor;
                }
            }
            return new Rectangle(rect.X - sx - bor, ry - sy - bor, rect.Width + bor2, rh + bor2);
        }
        Rectangle RectInputDefault(CELL cell, Rectangle rect, int sx, int sy, float borwidth, int wavesize)
        {
            int bor = (int)((wavesize + borwidth / 2F) * Config.Dpi), bor2 = bor * 2, ry = rect.Y, rh = rect.Height;
            if (EditAutoHeight)
            {
                int texth = Helper.GDI(g => g.MeasureString(Config.NullText, Font).Height), sps = (int)(texth * .4F), sps2 = sps * 2, h = texth + sps2 + bor2;
                if (h > rect.Height)
                {
                    rh = h;
                    ry = rect.Y + (rect.Height - rh) / 2;
                    if ((ry + h) - sy > rect_read.Bottom) ry = rect_read.Bottom + sy - rh;
                }
            }
            return new Rectangle(rect.X - sx, ry - sy, rect.Width, rh);
        }
        Input CreateInput(bool multiline, object? value, Column column, Rectangle rect)
        {
            Input input;
            if (column is ColumnSelect columnSelect)
            {
                Select edit = new Select
                {
                    Multiline = multiline,
                    Location = rect.Location,
                    Size = rect.Size,
                    List = true,
                    IconRatio = 1f,
                    ReadOnly = column.ReadOnly,
                };
                if (value is SelectItem select)
                {
                    edit.Text = select.Text;
                    edit.SelectedValue = select.Tag;
                    edit.ShowIcon = select.Icon != null || select.IconSvg != null;
                }
                input = edit;
                edit.Items.AddRange(columnSelect.Items.ToArray());
                edit.SelectedValue = value;
            }
            else if (value is CellText text)
            {
                input = new Input
                {
                    Multiline = multiline,
                    Location = rect.Location,
                    Size = rect.Size,
                    Text = text.Text ?? "",
                    ReadOnly = column.ReadOnly
                };
            }
            else
            {
                input = new Input
                {
                    Multiline = multiline,
                    Location = rect.Location,
                    Size = rect.Size,
                    Text = column.GetDisplayText(value) ?? string.Empty,
                    ReadOnly = column.ReadOnly
                };
            }
            if (input.ReadOnly) input.BackColor = Style.Db.BorderSecondary;
            if (EditSelection == TEditSelection.All) input.SelectAll();
            return input;
        }
        void ShowInput(Input input, Action<bool, string> call)
        {
            string old = input.Text;
            bool isone = true;
            input.KeyPress += (a, b) =>
            {
                if (a is Input input && isone)
                {
                    if (b.KeyChar == 13)
                    {
                        isone = false;
                        b.Handled = true;
                        ScrollBar.OnInvalidate = null;
                        call(old == input.Text, input.Text);
                        inEditMode = false;
                        input.Dispose();
                    }
                }
            };
            input.LostFocus += (a, b) =>
            {
                if (a is Input input && isone)
                {
                    isone = false;
                    input.Visible = false;
                    ScrollBar.OnInvalidate = null;
                    call(old == input.Text, input.Text);
                    inEditMode = false;
                    Focus();
                    if (Modal.ModalCount > 0)
                    {
                        ITask.Run(() =>
                        {
                            System.Threading.Thread.Sleep(200);
                            BeginInvoke(() => input.Dispose());
                        });
                        return;
                    }
                    input.Dispose();
                }
            };
        }
        void ShowSelect(Select select, Action<bool, object?> call)
        {
            var old = select.SelectedValue;
            bool isone = true;
            select.SelectedValueChanged += (a, b) =>
            {
                if (isone)
                {
                    isone = false;
                    ScrollBar.OnInvalidate = null;
                    call(old == select.SelectedValue, select.SelectedValue);
                    inEditMode = false;
                    select.Dispose();
                }
            };
            select.LostFocus += (a, b) =>
            {
                if (isone)
                {
                    isone = false;
                    select.Visible = false;
                    ScrollBar.OnInvalidate = null;
                    call(old == select.SelectedValue, select.SelectedValue);
                    inEditMode = false;
                    Focus();
                    if (Modal.ModalCount > 0)
                    {
                        ITask.Run(() =>
                        {
                            System.Threading.Thread.Sleep(200);
                            BeginInvoke(() => select.Dispose());
                        });
                        return;
                    }
                    select.Dispose();
                }
            };
        }

        #endregion
    }
}