using System;
using Data_Management_for_Unity.Runtime.Networking.Synchronising.Client;

namespace Data_Management_for_Unity.Runtime.Databases
{
    public partial class Database
    {
        /// <summary>
        /// Client managing the synchronisation of the client. Per default, only one client exists locally.
        /// However, multiple may be used for testing.
        /// </summary>
        public SynchronisedClient Client;

        public bool IsSynchronised
        {
            get => _isSynchronised;
            set
            {
                //no update is necessary
                if(value == _isSynchronised) return;
                
                //invoke logic depending on new state
                if (value) OnSynchronisationEnabled();
                else OnSynchronisationDisabled();

                //update local value
                _isSynchronised = value;
            }
        }
        private bool _isSynchronised;

        private void OnSynchronisationEnabled()
        {
            //set a reference to synchronised client, if necessary
            if (Client == null) Client = SynchronisedClient.Instance;
            
            //add database to list of local databases
            Client.AddDatabase(this);
            
            //send currently known values to remote
            throw new NotImplementedException();
        }

        private void OnSynchronisationDisabled()
        {
            //remove database from list of local databases
            Client.RemoveDatabase(this);
        }
        
        /// <summary>
        /// Called when a remote client sets a value of this database
        /// </summary>
        protected internal void OnRemoteSet(string id, byte[] value, Type type, int modCount)
        {
            throw new NotImplementedException();
        }

    }
}