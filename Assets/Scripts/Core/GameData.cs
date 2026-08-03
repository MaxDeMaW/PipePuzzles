namespace Pipes
{
    public struct DropData
    {
        public CellModel Model;
        public int FromY;
        public int ToY;
        public int X;

        public DropData(CellModel model, int fromY, int toY, int x)
        {
            Model = model;
            FromY = fromY;
            ToY = toY;
            X = x;
        }
    }
}
