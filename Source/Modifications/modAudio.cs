using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modAudio : IGameModification
    {
        public override void Load()
        {
            On.Celeste.SoundSource.Play += OnAudioPlay;
        }

        public override void Unload()
        {
            On.Celeste.SoundSource.Play -= OnAudioPlay;
        }

        private static SoundSource OnAudioPlay(On.Celeste.SoundSource.orig_Play orig, SoundSource self, string path, string param, float value)
        {
            CelesteArchipelagoModule.Log(path);
            if (path == "event:/sj21_jamjar-blue" || path == "event:/sj21_jamjar-red" || path == "event:/sj21_jamjar-yellow" || path == "event:/sj21_jamjar-orange" || path == "event:/sj21_jamjar-purple")
            {
                return self;
            }
            return orig(self, path, param, value);
        }
    }
}
