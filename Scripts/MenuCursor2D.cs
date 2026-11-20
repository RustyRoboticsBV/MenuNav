using System;

namespace Rusty.MenuNav;

/// <summary>
/// A controller for a two-dimensional menu cursor. The coordinate (0, 0) represents the top-left coordinate.
/// Contains methods and properties for moving the cursor, resizing the menu and controlling edge wrapping behavior.
/// </summary>
public sealed class MenuCursor2D
{
    /* Private types. */
    /// <summary>
    /// A controller for a one-dimensional menu cursor.
    /// </summary>
    private struct MenuCursor1D
    {
        /* Fields. */
        private int length;
        private int position;

        /* Public properties. */
        public int Length
        {
            get => length;
            set
            {
                length = Math.Max(1, value);

                // Re-apply position to ensure it is not out of range.
                Position = position;
            }
        }
        public int Position
        {
            get => position;
            set
            {
                if (Wrap)
                {
                    int remainder = value % Length;
                    position = remainder < 0 ? remainder + Length : remainder;
                }
                else
                    position = Math.Clamp(value, 0, Length - 1);
            }
        }
        public bool Wrap { get; set; }

        /* Constructors. */
        public MenuCursor1D(int length, int position, bool wrap)
        {
            Wrap = wrap;
            Length = length;
            Position = position;
        }
    }

    /* Fields. */
    private MenuCursor1D x;
    private MenuCursor1D y;

    /* Public properties. */
    /// <summary>
    /// The menu's current dimensions.
    /// </summary>
    public (int width, int height) Dimensions
    {
        get => (Width, Height);
        set => Resize(value.width, value.height);
    }
    /// <summary>
    /// The number of columns in the menu.
    /// </summary>
    public int Width
    {
        get => x.Length;
        set => Resize(value, Height);
    }
    /// <summary>
    /// The number of rows in the menu.
    /// </summary>
    public int Height
    {
        get => y.Length;
        set => Resize(Width, value);
    }

    /// <summary>
    /// The cursor's current position.
    /// </summary>
    public (int x, int y) Position
    {
        get => (X, Y);
        set => MoveTo(value.x, value.y);
    }
    /// <summary>
    /// The cursor's horizontal position.
    /// </summary>
    public int X
    {
        get => x.Position;
        set => MoveTo(value, Y);
    }
    /// <summary>
    /// The cursor's vertical position.
    /// </summary>
    public int Y
    {
        get => y.Position;
        set => MoveTo(X, value);
    }

    /// <summary>
    /// Whether horizontal positions wrap around instead of being clamped.
    /// </summary>
    public bool WrapH
    {
        get => x.Wrap;
        set => x.Wrap = value;
    }
    /// <summary>
    /// Whether vertical positions wrap around instead of being clamped.
    /// </summary>
    public bool WrapV
    {
        get => y.Wrap;
        set => y.Wrap = value;
    }
    /// <summary>
    /// Whether moving horizontally past an edge moves the cursor to the next or previous row.
    /// Requires <see cref="WrapH"/> to be enabled.
    /// </summary>
    public bool WrapToNextRow { get; set; }
    /// <summary>
    /// Whether moving vertically past an edge moves the cursor to the next or previous column.
    /// Requires <see cref="WrapV"/> to be enabled.
    /// </summary>
    public bool WrapToNextColumn { get; set; }


    /* Public events. */
    /// <summary>
    /// An event that gets invoked when the menu dimensions change.
    /// </summary>
    public event Action<(int width, int height)> Resized;
    /// <summary>
    /// An event that gets invoked when the cursor position changes.
    /// </summary>
    public event Action<(int x, int y)> Moved;

    /* Constructors. */
    public MenuCursor2D(int width, int height, int x, int y, bool wrapH, bool wrapV, bool wrapToNextRow, bool wrapToNextColumn)
    {
        this.x = new(width, x, wrapH);
        WrapToNextRow = wrapToNextRow;

        this.y = new(height, y, wrapV);
        WrapToNextColumn = wrapToNextColumn;
    }

    public MenuCursor2D(int width, int height, bool wrapH, bool wrapV, bool wrapToNextRow, bool wrapToNextColumn)
        : this(width, height, 0, 0, wrapH, wrapV, wrapToNextRow, wrapToNextColumn) { }

    public MenuCursor2D(int width, int height, bool wrapH, bool wrapV)
        : this(width, height, wrapH, wrapV, false, false) { }

    public MenuCursor2D(int width, int height, int x, int y, bool wrapH, bool wrapV)
        : this(width, height, x, y, wrapH, wrapV, false, false) { }

    public MenuCursor2D(int width, int height, int x, int y) : this(width, height, x, y, false, false) { }

    public MenuCursor2D(int width, int height) : this(width, height, 0, 0) { }

    /* Public methods. */
    public override string ToString() => $"({X}, {Y}) in ({Width}, {Height})";

    /// <summary>
    /// Move the cursor down one position.
    /// </summary>
    public void MoveDown()
    {
        if (WrapV && WrapToNextColumn && Y == Height - 1)
        {
            if (X < Width - 1 || WrapH)
                MoveTo(X + 1, 0);
            else
                MoveTo(Width - 1, Height - 1);
        }
        else
            Y++;
    }

    /// <summary>
    /// Move the cursor up one position.
    /// </summary>
    public void MoveUp()
    {
        if (WrapV && WrapToNextColumn && Y == 0)
        {
            if (X > 0 || WrapH)
                MoveTo(X - 1, Height - 1);
            else
                MoveTo(0, 0);
        }
        else
            Y--;
    }

    /// <summary>
    /// Move the cursor left one position.
    /// </summary>
    public void MoveLeft()
    {
        if (WrapH && WrapToNextRow && X == 0)
        {
            if (Y > 0 || WrapV)
                MoveTo(Width - 1, Y - 1);
            else
                MoveTo(0, 0);
        }
        else
            X--;
    }

    /// <summary>
    /// Move the cursor right one position.
    /// </summary>
    public void MoveRight()
    {
        if (WrapH && WrapToNextRow && X == Width - 1)
        {
            if (Y < Height - 1 || WrapV)
                MoveTo(0, Y + 1);
            else
                MoveTo(Width - 1, Height - 1);
        }
        else
            X++;
    }

    /// <summary>
    /// Change the cursor position.
    /// </summary>
    public void MoveTo(int x, int y) => Transform(Width, Height, x, y);

    /// <summary>
    /// Change the menu dimensions, adjusting the cursor position if necessary.
    /// </summary>
    public void Resize(int width, int height) => Transform(width, height, X, Y);

    /// <summary>
    /// Change the cursor position and menu dimensions.
    /// </summary>
    public void Transform(int width, int height, int x, int y)
    {
        int oldWidth = Width;
        int oldHeight = Height;
        int oldX = X;
        int oldY = Y;

        this.x.Length = width;
        this.y.Length = height;
        if (oldWidth != Width || oldHeight != Height)
            Resized?.Invoke((Width, Height));

        this.x.Position = x;
        this.y.Position = y;
        if (oldX != X || oldY != Y)
            Moved?.Invoke((X, Y));
    }

    /// <summary>
    /// Copy the dimensions, position and wrap settings of another cursor.
    /// </summary>
    public void CopyFrom(MenuCursor2D other)
    {
        ArgumentNullException.ThrowIfNull(other);

        WrapH = other.WrapH;
        WrapV = other.WrapV;
        WrapToNextRow = other.WrapToNextRow;
        WrapToNextColumn = other.WrapToNextColumn;
        Transform(other.Width, other.Height, other.X, other.Y);
    }
}
