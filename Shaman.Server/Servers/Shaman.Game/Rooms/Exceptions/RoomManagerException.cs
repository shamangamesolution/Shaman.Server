using System;

namespace Shaman.Game.Rooms.Exceptions
{
    public class RoomManagerException:Exception
    {
        public RoomManagerException(string message) : base(message)
        {
        }
    }
}