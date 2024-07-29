using Data_Management_for_Unity.Submodules.NetCoreServer;

namespace Data_Management_for_Unity.Runtime.Networking.Unity.Server
{
    /// <summary>
    /// Default implementation of the unity server wrapper.
    /// Default session class is SynchronisedSession, unless changed by the user.
    /// </summary>
    public class UnityServer : AbstractUnityServer<UnitySession>
    {
        protected override TcpSession CreateSession()
        {
            return new UnitySession(this);
        }
    }
}