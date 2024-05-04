using System;
using System.Threading.Tasks;

namespace Data_Management_for_Unity.Runtime.Persistence
{
    public static partial class PersistentData2
    {
        public static Task Save(string databaseId, string valueId, byte[] value, Type type, int modCount)
        {
            string commandText = $"insert or replace into '{databaseId}'(id, value, type, modCount) " +
                                 $"values('{valueId}', @value, '{type.AssemblyQualifiedName}', {modCount})";
            
            return CreateCommand(commandText,  command =>
            {
                //add value
                command.Parameters.AddWithValue("@value", value);
                
                //execute command
                return command.ExecuteNonQueryAsync();
            });
        }
    }
}