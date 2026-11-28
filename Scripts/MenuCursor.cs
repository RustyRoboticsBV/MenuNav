using System;

namespace Rusty.MenuNav;

/// <summary>
/// A 2D grid menu cursor controller. Supports horizontal and vertical movement, configurable wrapping and positioning by index or
/// by grid coordinates.
/// </summary>
public sealed class MenuCursor
{
    /* Fields. */
    private int length = 1;
    private int width = 1;
    private int index;

    /* Public properties. */
    /// <summary>
    /// The number of items in the menu (with a minimum of 1). Changing the length preserves the cursor's grid position when
    /// possible.
    /// </summary>
    public int Length
    {
        get => length;
        set
        {
            int x = X;
            int y = Y;
            length = Math.Max(value, 1);
            MoveTo(x, y);
        }
    }
    /// <summary>
    /// The number of items per row (with a minimum of 1). Changing the width preserves the cursor's grid position when
    /// possible.
    /// </summary>
    public int Width
    {
        get => width;
        set
        {
            int x = X;
            int y = Y;
            width = Math.Max(value, 1);
            MoveTo(x, y);
        }
    }
    /// <summary>
    /// The number of rows in the menu (with a minimum of 1). Changing the height preserves the cursor's grid position when
    /// possible. Setting the height adjusts <see cref="Length"/> to fill the specified number of rows.
    /// </summary>
    public int Height
    {
        get => (int)Math.Ceiling(Length / (float)Width);
        set => Length = Width * Math.Max(value, 1);
    }
    /// <summary>
    /// The row-wise index of the selected menu item. The value is clamped to the valid range of menu indices.
    /// </summary>
    public int Index
    {
        get => index;
        set => index = Math.Clamp(value, 0, Length - 1);
    }
    /// <summary>
    /// The horizontal grid position of the selected menu item.
    /// </summary>
    public int X
    {
        get => Index % Width;
        set => MoveTo(value, Y);
    }
    /// <summary>
    /// The vertical grid position of the selected menu item.
    /// </summary>
    public int Y
    {
        get => Index / Width;
        set => MoveTo(X, value);
    }

    /// <summary>
    /// Whether horizontal movement wraps around when reaching either end of a row.
    /// </summary>
    public bool WrapHorizontal { get; set; }
    /// <summary>
    /// Whether vertical movement wraps around when reaching either end of a column.
    /// </summary>
    public bool WrapVertical { get; set; }
    /// <summary>
    /// Whether horizontal wrapping advances the cursor to the next or previous row instead of staying in the current row.
    /// </summary>
    public bool WrapToNextRow { get; set; }
    /// <summary>
    /// Whether vertical wrapping advances the cursor to the next or previous column instead of staying in the current column.
    /// </summary>
    public bool WrapToNextColumn { get; set; }

    /// <summary>
    /// When enabled, attempting downwards movement to a position >= <see cref="Length"/> will move the cursor to the final item,
    /// instead of wrapping or doing nothing.
    /// 
    /// <br/><br/><b>Note</b>: this leads to asymmetric behavior with upward movement when both <see cref="WrapVertical"/> and
    /// <see cref="WrapToNextColumn"/> are also enabled!
    /// </summary>
    public bool ClampDownwardToLength { get; set; }

    /* Public methods. */
    /// <summary>
    /// Moves the cursor one position to the right. Respects <see cref="WrapHorizontal"/> and <see cref="WrapToNextRow"/>.
    /// </summary>
    public void MoveRight()
    {
        if (Index == Length - 1)
        {
            if (WrapHorizontal)
            {
                if (WrapToNextRow)
                    Index = 0;
                else
                    Index -= X;
            }
        }
        else if (X == Width - 1)
        {
            if (WrapHorizontal)
            {
                if (WrapToNextRow)
                    Index = Index - X + Width;
                else
                    Index -= X;
            }
        }
        else
            Index++;
    }

    /// <summary>
    /// Moves the cursor one position to the left. Respects <see cref="WrapHorizontal"/> and <see cref="WrapToNextRow"/>.
    /// </summary>
    public void MoveLeft()
    {
        if (Index == 0)
        {
            if (WrapHorizontal)
            {
                if (WrapToNextRow)
                    Index = Length - 1;
                else
                    Index = Width - 1;
            }
        }
        else if (X == 0)
        {
            if (WrapHorizontal)
            {
                if (WrapToNextRow)
                    Index--;
                else
                    Index += Width - 1;
            }
        }
        else
            Index--;
    }

    /// <summary>
    /// Moves the cursor down one row. Respects <see cref="WrapVertical"/> and <see cref="WrapToNextColumn"/>.
    /// </summary>
    public void MoveDown()
    {
        if (Index + Width >= Length)
        {
            if (ClampDownwardToLength && (!WrapVertical || Y < Height - 1))
                Index = Length - 1;
            else if (WrapVertical)
            {
                if (WrapToNextColumn)
                {
                    if (X == Width - 1)
                    {
                        if (WrapHorizontal)
                            Index = 0;
                    }
                    else
                        Index = X + 1;
                }
                else
                    Index = X;
            }
        }
        else
            Index += Width;
    }

    /// <summary>
    /// Moves the cursor up one row. Respects <see cref="WrapVertical"/> and <see cref="WrapToNextColumn"/>.
    /// </summary>
    public void MoveUp()
    {
        if (Y == 0)
        {
            if (WrapVertical)
            {
                int x = X;
                int y = Height - 1;

                if (WrapToNextColumn)
                {
                    if (X == 0)
                    {
                        if (WrapHorizontal)
                            x = Width - 1;
                        else
                            y = 0;
                    }
                    else
                        x--;
                }

                if (x + y * Width >= Length)
                    y--;

                Index = x + y * Width;
            }
        }
        else
            Index -= Width;
    }

    /// <summary>
    /// Moves the cursor to the specified grid coordinates. Coordinates are clamped to the menu's valid bounds.
    /// </summary>
    public void MoveTo(int x, int y)
    {
        x = Math.Clamp(x, 0, Width - 1);
        y = Math.Clamp(y, 0, Height - 1);

        Index = x + y * Width;
    }
}
