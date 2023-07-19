using System;
using Data_Management_for_Unity.Runtime.Databases.SynchronisedOperations;
using Data_Management_for_Unity.Runtime.Networking.Messaging;
using Data_Management_for_Unity.Runtime.Serializer;

namespace Data_Management_for_Unity.Runtime.Networking.Synchronising.Messages
{
    public class OperationRequest : Request
    {
        //serialize operation since it is an abstract class
        private readonly byte[] _operationValue;
        private readonly string _operationType;

        public OperationRequest(SynchronisedOperation operation)
        {
            //get type of object, allowing the serialization of an abstract class
            Type type = operation.GetType();
            
            //serialize operation
            _operationValue = Serialization.Serialize(type, operation);
            _operationType = type.AssemblyQualifiedName;
        }
        
        public SynchronisedOperation GetOperation()
        {
            return Serialization.Deserialize(_operationValue, Type.GetType(_operationType, true)) as
                SynchronisedOperation;
        }
    }
}