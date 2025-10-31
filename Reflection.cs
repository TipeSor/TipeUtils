#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Reflection
    {
        public static object GenericCreator(Type baseType, Type type)
        {
            Type typedType = baseType.MakeGenericType(type);
            return Activator.CreateInstance(typedType)!;
        }
    }
}
