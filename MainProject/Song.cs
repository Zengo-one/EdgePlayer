using System.Runtime.Serialization;


namespace MainProject
{
    [DataContract]
    internal class Song 
    {
        internal Song() { }

        internal Song(string _ref, string _name, string _duration)
        {
            Ref = _ref;
            Name = _name;
            Duration = _duration;
        }
        [DataMember]
        public string Ref { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string Duration { get; private set; }
    }
}
