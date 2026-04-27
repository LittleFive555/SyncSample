public enum InputType
{
    Right = 0,
    Left = 1,
    Up = 2,
    Down = 3,
    A = 4,
}

public static class InputExtensions
{
    public static int SetInput(this int value, InputType inputType, bool enabled)
    {
        var mask = 1 << (int)inputType;
        return enabled ? value | mask : value & ~mask;
    }

    public static bool GetInput(this int value, InputType inputType)
    {
        var mask = 1 << (int)inputType;
        return (value & mask) != 0;
    }

    public static int GetHorizontal(this int value)
    {
        var right = value.GetInput(InputType.Right);
        var left = value.GetInput(InputType.Left);

        if (right == left)
            return 0;

        return right ? 1 : -1;
    }

    public static int GetVertical(this int value)
    {
        var up = value.GetInput(InputType.Up);
        var down = value.GetInput(InputType.Down);

        if (up == down)
            return 0;

        return up ? 1 : -1;
    }
}