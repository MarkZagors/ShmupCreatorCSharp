namespace Editor
{
    public enum ScrollDirection
    {
        FORWARD,
        BACKWARD
    }

    public enum PageType
    {
        EDITOR,
        PLAY
    }

    public enum PlayState
    {
        ENTERING,
        MAIN,
        LOOP_TRANSITION,
        PHASE_TRANSITION,
        END_WIN,
        END_LOSE,
    }
}
