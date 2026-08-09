using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS2ConfigSaver.Helpers
{
    public static class AppPaths
    {
        public static readonly string FindSteamUserSubKey = @"Software\Valve\Steam";

        public static readonly string DefaultSteamPath = @"Software\Valve\Steam";

        public static readonly string MachineConvarsFile = "cs2_machine_convars.vcfg";

        public static readonly string UserConvarsFile = "cs2_user_convars_0_slot0.vcfg";

        public static readonly string UserConvarsFileAlt = "cs2_user_convars.vcfg";

        public static readonly string UserKeysFile = "cs2_user_keys_0_slot0.vcfg";

        public static readonly string UserKeysFileAlt = "cs2_user_keys.vcfg";

        public static readonly string SteamConfigInjectionFolder = @"steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg";
    }
}