using HarmonyLib;
using System.Reflection;

namespace WeaponStatShower.Patches
{
    #pragma warning disable CS8600
    #pragma warning disable CS8618
    #pragma warning disable CS8625
    public abstract class Patch
    {
        public virtual void Initialize() { }

        protected internal Harmony? Harmony { get; set; }

        public virtual string Name { get; }

        public virtual bool Enabled => true;

        public abstract void Execute();

        public void PatchConstructor<TClass>(PatchType patchType, string? prefixMethodName = default, string? postfixMethodName = default) where TClass : class
            => PatchConstructor<TClass>(null, patchType, prefixMethodName, postfixMethodName);

        public void PatchConstructor<TClass>(Type[]? parameters, PatchType patchType, string? prefixMethodName = default, string? postfixMethodName = default) where TClass : class
        {
            var m = AccessTools.Constructor(typeof(TClass), parameters);
            PatchMethod<TClass>(m, patchType, prefixMethodName!, postfixMethodName!);
        }

        #region Generic PatchMethod overloads
        public void PatchMethod<TClass>(string methodName, PatchType patchType, Type[] generics = null, string prefixMethodName = default, string postfixMethodName = default) where TClass : class
            => PatchMethod<TClass>(methodName, null, patchType, generics, prefixMethodName, postfixMethodName);

        public void PatchMethod<TClass>(string methodName, Type[] parameters, PatchType patchType, Type[] generics = null, string prefixMethodName = default, string postfixMethodName = default) where TClass : class
        {
            var m = AccessTools.Method(typeof(TClass), methodName, parameters, generics);
            PatchMethod<TClass>(m, patchType, prefixMethodName, postfixMethodName);
        }

        public void PatchMethod<TClass>(MethodBase methodBase, PatchType patchType, string prefixMethodName = default, string postfixMethodName = default) where TClass : class 
            => PatchMethod(typeof(TClass), methodBase, patchType, prefixMethodName, postfixMethodName);
        #endregion

        #region Non-generic PatchMethod overloads
        public void PatchMethod(Type classType, string methodName, PatchType patchType, Type[] generics = null, string prefixMethodName = default, string postfixMethodName = default)
            => PatchMethod(classType, methodName, null, patchType, generics, prefixMethodName, postfixMethodName);

        public void PatchMethod(Type classType, string methodName, Type[] parameters, PatchType patchType, Type[] generics = null, string prefixMethodName = default, string postfixMethodName = default) 
        {
            var m = AccessTools.Method(classType, methodName, parameters, generics);
            PatchMethod(classType, m, patchType, prefixMethodName, postfixMethodName);
        }
        #endregion

        public void PatchMethod(Type classType, MethodBase methodBase, PatchType patchType, string? prefixMethodName = default, string? postfixMethodName = default) 
        {
            var className = classType.Name.Replace("`", "__");
            var formattedMethodName = methodBase.ToString();
            var methodName = methodBase.IsConstructor ? "ctor" : methodBase.Name;
            MethodInfo postfix = null, prefix = null;

    #pragma warning restore CS8600
    #pragma warning restore CS8618
    #pragma warning restore CS8625

            if ((patchType & PatchType.Prefix) != 0)
            {
                try
                {
                    prefix = AccessTools.Method(GetType(), prefixMethodName ?? $"{className}__{methodName}__Prefix");
                }
                catch (Exception ex)
                {
                    WeaponStatShowerPlugin.LogFatal($"Failed to obtain the prefix patch method for {formattedMethodName}): {ex}");
                }
            }

            if ((patchType & PatchType.Postfix) != 0)
            {
                try
                {
                    postfix = AccessTools.Method(GetType(), postfixMethodName ?? $"{className}__{methodName}__Postfix");
                }
                catch (Exception ex)
                {
                    WeaponStatShowerPlugin.LogFatal($"Failed to obtain the postfix patch method for {formattedMethodName}): {ex}");
                }
            }

            try
            {
                if (prefix != null && postfix != null)
                {
                    Harmony!.Patch(methodBase, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
                    return;
                }

                if (prefix != null)
                {
                    Harmony!.Patch(methodBase, prefix: new HarmonyMethod(prefix));
                    return;
                }

                if (postfix != null)
                {
                    Harmony!.Patch(methodBase, postfix: new HarmonyMethod(postfix));
                    return;
                }
            }
            catch (Exception ex)
            {
                WeaponStatShowerPlugin.LogError($"Failed to patch method {formattedMethodName}: {ex}");
            }
        }
    }
}
