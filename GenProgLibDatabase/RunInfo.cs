namespace GenProgLibDatabase
{
    public class RunInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public RunInfo(string name, string filename) {
            Name = name;
            FileName = filename;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RunInfo objAsPart = obj as RunInfo;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode() + FileName.GetHashCode();
        }
        public bool Equals(RunInfo other)
        {
            if (other == null) return false;
            return (this.Name.Equals(other.Name));
        }
    }
}