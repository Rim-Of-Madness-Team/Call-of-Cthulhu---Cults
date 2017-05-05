using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Cthulhu.NoCCL
{
    [StaticConstructorOnStartup]
    internal static class DetourInjector
    {
        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(DetourInjector));
            }
        }

        private static string AssemblyName
        {
            get
            {
                return DetourInjector.Assembly.FullName.Split(new char[]
                {
                    ','
                }).First<string>();
            }
        }

        static DetourInjector()
        {
            LongEventHandler.QueueLongEvent(new Action(DetourInjector.Inject), "Initializing", true, null);
        }

        private static void Inject()
        {
            Cthulhu_SpecialInjector cthulhu_SpecialInjector = new Cthulhu_SpecialInjector();
            bool flag = cthulhu_SpecialInjector.Inject();
            if (flag)
            {
                //Log.Messag(DetourInjector.AssemblyName + " injected.");
            }
            else
            {
                Log.Error(DetourInjector.AssemblyName + " failed to get injected properly.");
            }
        }
    }

    public class SpecialInjector
    {
        public virtual bool Inject()
        {
            Log.Error("This should never be called.");
            return false;
        }
    }

    public static class Detours
    {
        private static List<string> detoured = new List<string>();

        private static List<string> destinations = new List<string>();

        public unsafe static bool TryDetourFromTo(MethodInfo source, MethodInfo destination)
        {
            bool flag = source == null;
            bool result;
            if (flag)
            {
                Log.Error("Source MethodInfo is null: Detours");
                result = false;
            }
            else
            {
                bool flag2 = destination == null;
                if (flag2)
                {
                    Log.Error("Destination MethodInfo is null: Detours");
                    result = false;
                }
                else
                {
                    string item = string.Concat(new string[]
                    {
                        source.DeclaringType.FullName,
                        ".",
                        source.Name,
                        " @ 0x",
                        source.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString())
                    });
                    string item2 = string.Concat(new string[]
                    {
                        destination.DeclaringType.FullName,
                        ".",
                        destination.Name,
                        " @ 0x",
                        destination.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString())
                    });
                    Detours.detoured.Add(item);
                    Detours.destinations.Add(item2);
                    bool flag3 = IntPtr.Size == 8;
                    if (flag3)
                    {
                        long num = source.MethodHandle.GetFunctionPointer().ToInt64();
                        long num2 = destination.MethodHandle.GetFunctionPointer().ToInt64();
                        byte* ptr = (byte*)num;
                        long* ptr2 = (long*)(ptr + 2);
                        *ptr = 72;
                        ptr[1] = 184;
                        *ptr2 = num2;
                        ptr[10] = 255;
                        ptr[11] = 224;
                    }
                    else
                    {
                        int num3 = source.MethodHandle.GetFunctionPointer().ToInt32();
                        int num4 = destination.MethodHandle.GetFunctionPointer().ToInt32();
                        byte* ptr3 = (byte*)num3;
                        int* ptr4 = (int*)(ptr3 + 1);
                        int num5 = num4 - num3 - 5;
                        *ptr3 = 233;
                        *ptr4 = num5;
                    }
                    result = true;
                }
            }
            return result;
        }
    }

    public class Cthulhu_SpecialInjector : SpecialInjector
    {
        private static readonly BindingFlags[] bindingFlagCombos = new BindingFlags[]
        {
            BindingFlags.Instance | BindingFlags.Public,
            BindingFlags.Static | BindingFlags.Public,
            BindingFlags.Instance | BindingFlags.NonPublic,
            BindingFlags.Static | BindingFlags.NonPublic
        };

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(DetourInjector));
            }
        }

        public override bool Inject()
        {
            Type[] types = Cthulhu_SpecialInjector.Assembly.GetTypes();
            bool result;
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                BindingFlags[] array = Cthulhu_SpecialInjector.bindingFlagCombos;
                for (int j = 0; j < array.Length; j++)
                {
                    BindingFlags bindingFlags = array[j];
                    MethodInfo[] methods = type.GetMethods(bindingFlags);
                    for (int k = 0; k < methods.Length; k++)
                    {
                        MethodInfo methodInfo = methods[k];
                        object[] customAttributes = methodInfo.GetCustomAttributes(typeof(DetourAttribute), true);
                        for (int l = 0; l < customAttributes.Length; l++)
                        {
                            DetourAttribute detourAttribute = (DetourAttribute)customAttributes[l];
                            BindingFlags bindingFlags2 = (detourAttribute.bindingFlags != BindingFlags.Default) ? detourAttribute.bindingFlags : bindingFlags;
                            MethodInfo method = detourAttribute.source.GetMethod(methodInfo.Name, bindingFlags2);
                            bool flag = method == null;
                            if (flag)
                            {
                                Log.Error(string.Format("Cthulhu :: Detours :: Can't find source method '{0} with bindingflags {1}", methodInfo.Name, bindingFlags2));
                                result = false;
                                return result;
                            }
                            bool flag2 = !Detours.TryDetourFromTo(method, methodInfo);
                            if (flag2)
                            {
                                result = false;
                                return result;
                            }
                        }
                    }
                }
            }
            result = true;
            return result;
        }
    }
}
