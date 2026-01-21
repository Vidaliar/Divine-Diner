public static class GlobalLoadContext
{
    public static bool HasPendingRequest { get; private set; }
    public static string ProfileName { get; private set; }
    public static int SlotIndex { get; private set; }

    public static void Request(string profileName, int slotIndex)
    {
        ProfileName = profileName;
        SlotIndex = slotIndex;
        HasPendingRequest = true;
    }

    public static void Clear()
    {
        HasPendingRequest = false;
    }
}
