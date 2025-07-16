using System;

namespace Clingies.Common.CustomEventArgs;

public class PositionChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }

    public int PositionX;
    public int PositionY;

    public PositionChangeRequestedEventArgs(Guid id, int positionX, int positionY)
    {
        ClingyId = id;
        PositionX = positionX;
        PositionY = positionY;
    }

}
