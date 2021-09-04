using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace FixedInput
{
    public class KeySetting : UnityModManager.ModSettings
    {
        public bool useKeyLimit = false;
        public List<int> registerKeys = new List<int>();

        public override void Save(UnityModManager.ModEntry modEntry) {
            var filepath = GetPath(modEntry);
            try {
                using (var writer = new StreamWriter(filepath)) {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            } catch {
            }
        }
       
        public override string GetPath(UnityModManager.ModEntry modEntry) {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }
  
    }
}