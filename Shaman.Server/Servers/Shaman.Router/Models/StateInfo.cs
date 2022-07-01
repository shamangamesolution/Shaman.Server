using System;

namespace Shaman.Router.Models;

public class StateInfo
{
    public int ServerId { get; set; }
    public string SerializedState { get; set; }
    public DateTime CreatedOn { get; set; }
}