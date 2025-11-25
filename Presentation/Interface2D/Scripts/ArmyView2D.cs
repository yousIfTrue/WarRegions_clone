public class PlayerManager : MonoBehaviour
{
    public void InitializePlayer(PlayerType playerType)
    {
        switch(playerType)
        {
            case PlayerType.humanPlay:
                SetupHumanPlayer();
                break;
            case PlayerType.AI:
                SetupAIPlayer();
                break;
        }
    }
    
    private void SetupHumanPlayer()
    {
        // تهيئة اللاعب البشري
    }
}