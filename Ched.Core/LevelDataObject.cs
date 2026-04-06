using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class LevelDataEntity
    {
        public string archetype { get; set; } = "";
        public List<LevelDataData> data { get; set; } = new List<LevelDataData>();

        public LevelDataEntity()
        {

        }
        public LevelDataEntity(string archetype, List<LevelDataData> data)
        {
            this.archetype = archetype;
            this.data = data;
        }
    }
    public class NamedLevelDataEntity : LevelDataEntity
    {
        public string name { get; set; }
        public NamedLevelDataEntity(string archetype, List<LevelDataData> data, string name)
        {
            this.archetype = archetype;
            this.data = data;
            this.name = name;
        }
    }

    public class LevelDataData
    {
        public string name { get; set; } = "";
    }

    public class LevelDataValue : LevelDataData
    {
        public double value { get; set; } = 0;
        public LevelDataValue(string name, double value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public class LevelDataRef : LevelDataData
    {
        public string value { get; set; } = "";
        public LevelDataRef(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }

}
