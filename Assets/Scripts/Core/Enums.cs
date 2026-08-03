namespace Pipes
{
    [System.Flags]
    public enum Direction
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }

    public enum PipeType
    {
        I = 0,
        L = 1,
        T = 2,
        X = 3
    }
}
