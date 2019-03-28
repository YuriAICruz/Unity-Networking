using System.CodeDom;
using UnityEngine.Networking;

namespace Graphene.Networking
{
    public class NullMessage : MessageBase { }

    public class DirectedBoolMessage : MessageBase
    {
        public bool value;
        public int Id;

        public DirectedBoolMessage()
        {
            
        }

        public DirectedBoolMessage(bool value, int id)
        {
            this.value = value;
            Id = id;
        }
    }
}