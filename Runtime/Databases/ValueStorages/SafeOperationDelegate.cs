namespace Data_Management_for_Unity.Runtime.Databases.ValueStorages
{
    public delegate TOut SafeOperation<in T, out TOut>(T data);
}