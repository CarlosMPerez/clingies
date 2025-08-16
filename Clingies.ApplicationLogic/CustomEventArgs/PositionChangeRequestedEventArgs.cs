using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class PositionChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public int PositionX;
    public int PositionY;

    public PositionChangeRequestedEventArgs(int id, int positionX, int positionY)
    {
        ClingyId = id;
        PositionX = positionX;
        PositionY = positionY;
    }

}
