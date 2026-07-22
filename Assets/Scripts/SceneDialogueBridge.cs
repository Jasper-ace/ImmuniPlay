/// <summary>
/// A plain static class survives a scene load (it's not tied to any GameObject),
/// so it's a simple way to hand a value from one scene to the next.
///
/// PopupPanelManager sets "pendingDialogueIndex" right before loading Scene4.
/// BabyCareManager (in Scene4) should check "hasPendingIndex" in its Start()
/// and, if true, jump straight to that dialogue index instead of starting at 0.
/// See the integration snippet at the bottom of PopupPanelManager.cs.
/// </summary>
public static class SceneDialogueBridge
{
    public static int pendingDialogueIndex = 0;
    public static bool hasPendingIndex = false;
}