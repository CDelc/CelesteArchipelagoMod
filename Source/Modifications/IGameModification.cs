using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    public abstract class IGameModification
    {
        public abstract void Load();
        public abstract void Unload();

    }
}
