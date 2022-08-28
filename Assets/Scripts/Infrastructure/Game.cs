using Services.Input;

namespace Infrastructure
{
    public class Game
    {
        private IInputService inputService;

        public Game()
        {
            inputService = new InputService();
        }
    }
}