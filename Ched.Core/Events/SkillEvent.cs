using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Events
{
    /// <summary>
    /// スキルイベントを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    [DebuggerDisplay("Tick = {Tick}, Value = {Skill}")]
    public class SkillEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private SkillTypes skill;
        [Newtonsoft.Json.JsonProperty]
        private int level;

        public SkillTypes Skill
        {
            get { return skill; }
            set { skill = value; }
        }
        public int Level
        {
            get { return level; }
            set { level = value; }
        }


        public int Type = -1;

    }
    public enum SkillTypes
    {
        none,
        heal,
        score,
        judgment
    }
}
