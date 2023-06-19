using Data_Management_for_Unity.Runtime.Persistence;
using NUnit.Framework;

namespace Data_Management_for_Unity.Tests.EditMode
{
    public class SqliteTests
    {
        [Test]
        public void TableCreation()
        {
            const string id = "SimpleTestTable";
            
            //make sure table doesn't exist
            Assert.IsFalse(PersistentData.DoesDatabaseExists(id));
            
            //create table
            PersistentData.CreateDatabase(id);
            Assert.IsTrue(PersistentData.DoesDatabaseExists(id));
            
            //delete table
            PersistentData.DeleteDatabase(id);
            Assert.IsFalse(PersistentData.DoesDatabaseExists(id));
        }
    }
}