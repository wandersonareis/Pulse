using System;
using System.Windows;
using Pulse.Core;
using Pulse.UI.Interaction;

namespace Pulse.UI
{
    public static class InteractionService
    {
        public static FFXIIIGamePart GamePart { get; private set; }
        public static ApplicationConfigProviders Configuration { get; }
        public static AudioSettingsProviders AudioSettings { get; }
        public static GameLocationProviders GameLocation { get; }
        public static WorkingLocationProviders WorkingLocation { get; }
        public static TextEncodingProviders TextEncoding { get; }
        public static LocalizatorEnvironmentProviders LocalizatorEnvironment { get; }

        public static event Action<IUiLeaf> SelectedLeafChanged;

        static InteractionService()
        {
            Configuration = new();
            AudioSettings = new();
            GameLocation = new();
            WorkingLocation = new();
            TextEncoding = new();
            LocalizatorEnvironment = new();

            GameLocation.InfoProvided += Configuration.GameLocationProvided;
            WorkingLocation.InfoProvided += Configuration.WorkingLocationProvided;
            LocalizatorEnvironment.InfoProvided += Configuration.LocalizatorEnvironmentProvided;
        }

        public static void SetGamePart(FFXIIIGamePart result)
        {
            GamePart = result;
        }

        public static void RaiseSelectedNodeChanged(UiNode node)
        {
            try
            {
                SelectedLeafChanged.NullSafeInvoke(node as IUiLeaf);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(Application.Current.MainWindow, ex);
            }
        }
    }
}