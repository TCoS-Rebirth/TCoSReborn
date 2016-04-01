namespace Database.Dynamic.Internal
{
    public class DBPersistentVar
    {
        public int ContextId;
        public int VarId;
        public int Value;

        public DBPersistentVar(int contextID, int varID, int value)
        {
            ContextId = contextID;
            VarId = varID;
            Value = value;
        }
    }
}