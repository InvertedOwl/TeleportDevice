using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace TpDevice
{
#if DEBUG
	[EnableReloading]
#endif
    public static class Main
    {
        static Harmony harmony;
        static Device lastWireframed;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            harmony = new Harmony(entry.Info.Id);

            entry.OnToggle = OnToggle;
#if DEBUG
			entry.OnUnload = OnUnload;
#endif

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry entry, bool active)
        {
            if (active)
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(entry.Info.Id);
            }

            return true;
        }

#if DEBUG
		static bool OnUnload(UnityModManager.ModEntry entry) {
			return true;
		}
#endif

        [HarmonyPatch(typeof(WorldController), "AttachComponent")]
        class NewD
        {
            public static void Postfix(WorldController __instance)
            {
                Device d = lastWireframed.rootComponent.device;

                UnityModManager.Logger.Log("Just created " + d.name);
                Main.lastWireframed = d;
            }
        }

        [HarmonyPatch(typeof(Device), "MakeWireframe")]
        class Wireframe
        {
            public static void Postfix(Device __instance)
            {
                UnityModManager.Logger.Log("Just wireframed into " + __instance.name);
                Main.lastWireframed = __instance;
            }
        }


        [HarmonyPatch(typeof(WorldController), "Update")]

        class Patch
        {
            private static void Postfix()
            {
                WorldController wc = Controllers.worldController;
                RigidbodyCharacter character = wc.character;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (lastWireframed)
                    {
                        if (!lastWireframed.isWireframe)
                        {
                            lastWireframed.MakeWireframe();
                        }

                        Vector3 pos = wc.playerPosition;
                        lastWireframed.rootComponent.baseSubComponent.rigidbody.transform.position = pos + (wc.character.camera.transform.forward * 1.5f);
                        
                    }
                }
            }
        }
    }
}
