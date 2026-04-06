using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class LevelData
    {
        public List<LevelDataEntity> entities;
        public double bgmOffset = 0;


        public LevelData(double offset, List<LevelDataEntity> entities) { 
            this.bgmOffset = offset;
            this.entities = entities;
        }


    }
}
