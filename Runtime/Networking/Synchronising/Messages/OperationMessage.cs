using System;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class OperationMessage
    {
        //serialize operation since it is an abstract class
        private readonly byte[] _operationValue;
        private readonly string _operationType;

        public OperationMessage(SynchronisedOperation operation)
        {
            _operationValue = Serialization.Serialize(operation, out Type type);
            _operationType = type.AssemblyQualifiedName;
        }

        public SynchronisedOperation GetOperation()
        {
            return Serialization.Deserialize(_operationValue, Type.GetType(_operationType, true)) as
                SynchronisedOperation;
        }
    }
}