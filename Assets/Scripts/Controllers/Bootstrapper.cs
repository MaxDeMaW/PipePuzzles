using Zenject;

namespace Pipes
{
    public sealed class Bootstrapper : IInitializable
    {
        private readonly GridModel _gridModel;
        private readonly IBoardView _boardView;
        private readonly GameController _gameController;

        public Bootstrapper(
            GridModel gridModel,
            IBoardView boardView,
            GameController gameController)
        {
            _gridModel = gridModel;
            _boardView = boardView;
            _gameController = gameController;
        }

        public void Initialize()
        {
            _gridModel.InitializeGrid();
            _boardView.Build();
            _gameController.StartGame();
        }
    }
}
