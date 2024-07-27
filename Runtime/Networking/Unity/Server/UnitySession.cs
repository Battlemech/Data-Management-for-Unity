using Data_Management_for_Unity.Runtime.Networking.Synchronising.Server;
using Data_Management_for_Unity.Runtime.Networking.Unity.Messages;

namespace Data_Management_for_Unity.Runtime.Networking.Unity.Server
{
    public class UnitySession : SynchronisedSession
    {
        public UnitySession(AbstractUnityServer<UnitySession> server) : base(server)
        {
            AddCallback<ShowGameObjectOperation>((operation =>
            {
                server.MulticastToOthers(operation, this);
            }));
            AddCallback<DestroyGameObjectOperation>((operation =>
            {
                server.MulticastToOthers(operation, this);
            }));
        }
    }
}